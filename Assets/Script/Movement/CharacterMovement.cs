using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterMovement : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    private Vector2 movement = Vector2.zero;
    private CharacterController characterController;
    private bool canDash;
    private bool isDashing;
    private readonly float dashDuration = 0.25f;
    private readonly float dashPower = 5f;
    [SerializeField] private TrailRenderer dashTrail;

    // --- Networked state ---
    // Position written only by server (authoritative)
    private NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>(
        writePerm: NetworkVariableWritePermission.Server
    );

    // Last processed input seq on server for this player
    private NetworkVariable<int> _lastProcessedInputSeq = new NetworkVariable<int>(
        0, writePerm: NetworkVariableWritePermission.Server
    );

    // Client-side prediction buffers
    private struct PendingInput { public int seq; public Vector2 dir; public float dt; }
    private List<PendingInput> pendingInputs = new List<PendingInput>();
    private int nextInputSeq = 1;

    // Sending parameters
    [Header("Prediction Settings")]
    [SerializeField] private float sendRate = 20f; // inputs per second
    private float sendInterval => 1f / Mathf.Max(1f, sendRate);
    private float lastSendTime = 0f;

    // Interpolation for non-owner
    private Vector2 targetNetworkPosition;
    [SerializeField] private float remoteLerpSpeed = 15f;

    [SerializeField] private Transform spawnTransform;
    [FormerlySerializedAs("statsHandlerReWork")] [SerializeField] private StatsHandlerReWork statsHandlerReWork;

    // Reconciliation threshold (if deviation too large, snap)
    private const float snapThreshold = 0.5f;

    private void Awake()
    {
        canDash = true;
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found!");
        }
        else
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.None;
        }
    }

    private void Start()
    {
        // subscribe to network changes
        _networkPosition.OnValueChanged += OnNetworkPositionChanged;
        _lastProcessedInputSeq.OnValueChanged += OnLastProcessedInputSeqChanged;

        characterController.OnMoveEvent.AddListener(OnMove);
        characterController.OnDash.AddListener(RequestDashServerRpc);

        targetNetworkPosition = transform.position;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        int map = 1;
        if (IsServer)
        {
            spawnTransform = GameObject.FindGameObjectWithTag("SpawnHost" + map).transform;
            transform.position = spawnTransform.position;
            // initialize server authoritative pos
            _networkPosition.Value = transform.position;
        }
        else
        {
            spawnTransform = GameObject.FindGameObjectWithTag("SpawnClient" + map).transform;
            transform.position = spawnTransform.position;
        }
    }

    private void OnNetworkPositionChanged(Vector2 oldValue, Vector2 newValue)
    {
        // Update target for interpolation (non-owner) OR trigger reconciliation (owner logic handled in OnLastProcessedInputSeqChanged)
        targetNetworkPosition = newValue;

        // If not owner, we'll lerp to target in FixedUpdate
        if (!IsOwner)
        {
            // nothing more here
        }
    }

    private void OnLastProcessedInputSeqChanged(int oldSeq, int newSeq)
    {
        // Called on both client and server; client uses for reconciliation
        if (!IsOwner) return;

        // Only owner runs reconciliation logic
        // Server already applied; client uses this seq when combined with networkPosition
        ReconcileWithServerState();
    }

    private void HandleOwnerMovementPhysics(Vector2 movementDir, float dt)
    {
        // Apply local predicted movement using Rigidbody for feel
        float speed = statsHandlerReWork.currentStats.Value.speedMove;
        Vector2 vel = movementDir * speed;
        rb.velocity = vel;
    }

    // We use FixedUpdate for physics-consistent movement
    private void FixedUpdate()
    {
        if (IsOwner)
        {
            // apply local movement with physics
            HandleOwnerMovementPhysics(movement, Time.fixedDeltaTime);

            // send input at sendRate
            if (Time.time - lastSendTime >= sendInterval)
            {
                lastSendTime = Time.time;
                // Create input and send
                int seq = nextInputSeq++;
                var inp = new PendingInput { seq = seq, dir = movement, dt = Time.fixedDeltaTime };
                // 1) apply locally (we already applied rb.velocity above; but we keep pending for replay)
                pendingInputs.Add(inp);
                // 2) send to server (unreliable)
                SubmitInputServerRpc(seq, inp.dir, inp.dt);
            }
        }
        else
        {
            // Non-owner clients: smooth physics position toward targetNetworkPosition
            Vector2 interp = Vector2.Lerp(rb.position, targetNetworkPosition, 1f - Mathf.Exp(-remoteLerpSpeed * Time.fixedDeltaTime));
            rb.MovePosition(interp);
        }
    }

    private void OnMove(Vector2 movementInput)
    {
        this.movement = movementInput;
        // owner movement will be applied in FixedUpdate
    }

    // ServerRpc: client sends its input (unreliable)
    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void SubmitInputServerRpc(int seq, Vector2 dir, float dt, ServerRpcParams serverRpcParams = default)
    {
        // NOTE: This method runs on server instance of this NetworkBehaviour,
        // so we can update authoritative state here.
        // Basic server-side validation to prevent speed hacking:
        float maxDt = 0.1f;
        if (dt <= 0f || dt > maxDt) dt = Time.fixedDeltaTime;

        float speed = statsHandlerReWork != null ? statsHandlerReWork.currentStats.Value.speedMove : 5f;
        Vector2 newPos = _networkPosition.Value + dir * speed * dt;

        // Optionally clamp movement distance per input to avoid huge teleports
        if (Vector2.Distance(_networkPosition.Value, newPos) > 10f)
        {
            newPos = Vector2.MoveTowards(_networkPosition.Value, newPos, 10f);
        }

        _networkPosition.Value = newPos;
        _lastProcessedInputSeq.Value = seq;

        // Do not call ClientRpc to set transform; clients read NetworkVariable
    }

    // Reconciliation on client owner side
    private void ReconcileWithServerState()
    {
        // Called when _lastProcessedInputSeq changed (client's owner)
        Vector2 serverPos = _networkPosition.Value;
        int confirmedSeq = _lastProcessedInputSeq.Value;

        // Find and remove confirmed inputs
        int removeCount = pendingInputs.RemoveAll(p => p.seq <= confirmedSeq);

        // Compute local predicted position by replaying remaining pending inputs
        // Reset transform to server position, then replay
        Vector3 before = transform.position;
        transform.position = serverPos;
        rb.position = serverPos;

        if (pendingInputs.Count > 0)
        {
            // Replay
            foreach (var p in pendingInputs)
            {
                // simple Euler integration to update position
                float speed = statsHandlerReWork.currentStats.Value.speedMove;
                Vector2 delta = p.dir * speed * p.dt;
                transform.position += (Vector3)delta;
                rb.position = transform.position;
            }
        }

        // If discrepancy still large (more than snapThreshold), snap to server (to prevent runaway)
        float error = Vector2.Distance((Vector2)transform.position, serverPos);
        if (error > snapThreshold)
        {
            transform.position = serverPos;
            rb.position = serverPos;
            // clear pending because huge mismatch
            pendingInputs.Clear();
        }
    }

    // Dash handling (unchanged but ensure server authoritative application if needed)
    [ServerRpc(RequireOwnership = false)]
    private void RequestDashServerRpc()
    {
        // Server may choose to validate dash permission and then tell owner to play dash
        PerformDashClientRpc();
    }

    [ClientRpc]
    private void PerformDashClientRpc()
    {
        if (!IsOwner) return;
        StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false;
        isDashing = true;

        rb.AddForce(movement * dashPower, ForceMode2D.Force);
        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector2.zero;
        isDashing = false;
        canDash = true;
    }
}
