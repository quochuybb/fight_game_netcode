using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterMovement : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TrailRenderer dashTrail;
    [FormerlySerializedAs("statsHandlerReWork")] [SerializeField] private StatsHandlerReWork statsHandlerReWork;

    [Header("Movement / Prediction")]
    [SerializeField] private float sendRate = 20f; // inputs per second (owner)
    [SerializeField] private float remoteLerpSpeed = 15f; // non-owner smoothing
    [SerializeField] private float snapThreshold = 0.5f; // snap if reconciliation error too large

    // Network authoritative state (server writes)
    private NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>(
        writePerm: NetworkVariableWritePermission.Server
    );
    private NetworkVariable<int> _lastProcessedInputSeq = new NetworkVariable<int>(
        0, writePerm: NetworkVariableWritePermission.Server
    );

    // Prediction buffers (client only)
    private struct PendingInput { public int seq; public Vector2 dir; public float dt; public bool dash; }
    private List<PendingInput> pendingInputs = new List<PendingInput>();
    private int nextInputSeq = 1;
    private float lastSendTime = 0f;
    private float sendInterval => 1f / Mathf.Max(1f, sendRate);

    // Local input state (updated by PlayerInput)
    private Vector2 localInput = Vector2.zero;

    // Interp target for non-owner
    private Vector2 targetNetworkPosition;

    // spawn
    [SerializeField] private Transform spawnTransform;

    // dash
    private bool canDash = true;
    private bool isDashing = false;
    private readonly float dashDuration = 0.25f;
    private readonly float dashPower = 5f;

    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        rb = rb ?? GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("Rigidbody2D not found!");
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.None;
    }

    private void Start()
    {
        // subscribe to move events so PlayerInput -> CharacterController -> CharacterMovement works
        characterController.OnMoveEvent.AddListener(OnMoveLocal);
        characterController.OnDash.AddListener(OnDashLocal);

        _networkPosition.OnValueChanged += OnNetworkPositionChanged;
        _lastProcessedInputSeq.OnValueChanged += OnLastProcessedInputSeqChanged;

        targetNetworkPosition = transform.position;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        int map = 1;
        if (IsServer)
        {
            var t = GameObject.FindGameObjectWithTag("SpawnHost" + map);
            if (t != null) spawnTransform = t.transform;
            if (spawnTransform != null) transform.position = spawnTransform.position;

            // init server authoritative pos
            _networkPosition.Value = transform.position;
            _lastProcessedInputSeq.Value = 0;
        }
        else if (IsClient)
        {
            var t = GameObject.FindGameObjectWithTag("SpawnClient" + map);
            if (t != null) spawnTransform = t.transform;
            if (spawnTransform != null) transform.position = spawnTransform.position;
        }

        targetNetworkPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            // Apply local prediction using Rigidbody for feel
            float speed = statsHandlerReWork.currentStats.Value.speedMove;
            Vector2 vel = localInput * speed;
            rb.velocity = vel;

            // Send input to server at sendRate
            if (Time.time - lastSendTime >= sendInterval)
            {
                lastSendTime = Time.time;
                var inp = new PendingInput { seq = nextInputSeq++, dir = localInput, dt = Time.fixedDeltaTime, dash = false };
                pendingInputs.Add(inp);
                SubmitInputServerRpc(inp.seq, inp.dir, inp.dt, inp.dash);
            }
        }
        else
        {
            // Non-owner: smooth towards target network position
            Vector2 newPos = Vector2.Lerp(rb.position, targetNetworkPosition, 1f - Mathf.Exp(-remoteLerpSpeed * Time.fixedDeltaTime));
            rb.MovePosition(newPos);
        }
    }

    // Called by CharacterController.OnMoveEvent -> PlayerInput -> OnMovement
    private void OnMoveLocal(Vector2 dir)
    {
        localInput = dir;
    }

    // Local dash trigger (player pressed dash)
    private void OnDashLocal()
    {
        if (!IsOwner) return;
        if (!canDash) return;
        // perform local dash immediately for responsiveness
        StartCoroutine(LocalDashCoroutine());
        // inform server to validate and broadcast dash
        RequestDashServerRpc();
    }

    private IEnumerator LocalDashCoroutine()
    {
        canDash = false;
        isDashing = true;
        rb.AddForce(localInput * dashPower, ForceMode2D.Force);
        yield return new WaitForSeconds(dashDuration);
        rb.velocity = Vector2.zero;
        isDashing = false;
        canDash = true;
    }

    // ServerRpc: clients send inputs (unreliable)
    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void SubmitInputServerRpc(int seq, Vector2 dir, float dt, bool dash, ServerRpcParams rpcParams = default)
    {
        // Basic validation
        if (dt <= 0f || dt > 0.2f) dt = Time.fixedDeltaTime;
        float speed = statsHandlerReWork != null ? statsHandlerReWork.currentStats.Value.speedMove : 5f;

        // update server authoritative position
        Vector2 newPos = _networkPosition.Value + dir * speed * dt;

        // clamp big jumps (anti-cheat)
        if (Vector2.Distance(_networkPosition.Value, newPos) > 10f)
        {
            newPos = Vector2.MoveTowards(_networkPosition.Value, newPos, 10f);
        }

        _networkPosition.Value = newPos;
        _lastProcessedInputSeq.Value = seq;

        // update server transform for server-side view
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            transform.position = newPos;
        }
    }

    // ServerRpc for dash: server can validate and then broadcast to clients
    [ServerRpc(RequireOwnership = false)]
    private void RequestDashServerRpc(ServerRpcParams rpcParams = default)
    {
        // (Optional) server validation here
        PerformDashClientRpc();
    }

    [ClientRpc]
    private void PerformDashClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // play dash effect only on owner to mirror local dash
        if (!IsOwner) return;
        StartCoroutine(LocalDashCoroutine());
    }

    // NetworkVariable callbacks
    private void OnNetworkPositionChanged(Vector2 oldValue, Vector2 newValue)
    {
        targetNetworkPosition = newValue;
    }

    private void OnLastProcessedInputSeqChanged(int oldSeq, int newSeq)
    {
        if (!IsOwner) return;
        Reconcile(newSeq);
    }

    // Reconciliation: called on owner when server reports last processed seq
    private void Reconcile(int confirmedSeq)
    {
        Vector2 serverPos = _networkPosition.Value;
        float error = Vector2.Distance((Vector2)transform.position, serverPos);

        // 1) reset to server position
        transform.position = serverPos;
        rb.position = serverPos;

        // 2) remove confirmed inputs
        pendingInputs.RemoveAll(p => p.seq <= confirmedSeq);

        // 3) replay remaining pending inputs
        foreach (var p in pendingInputs)
        {
            float speed = statsHandlerReWork.currentStats.Value.speedMove;
            Vector2 delta = p.dir * speed * p.dt;
            transform.position += (Vector3)delta;
            rb.position = transform.position;
        }

        // 4) if still far, snap and clear pending to avoid runaway
        if (error > snapThreshold)
        {
            transform.position = serverPos;
            rb.position = serverPos;
            pendingInputs.Clear();
        }
    }

    // Allow external (ConnectionQualityChecker) to adjust sendRate at runtime
    public void SetSendRate(float rate)
    {
        sendRate = Mathf.Clamp(rate, 1f, 60f);
        Debug.Log($"[CharacterMovement] sendRate set to {sendRate}");
    }

    // Optional helper: expose current pending count for debugging
    public int GetPendingCount() => pendingInputs.Count;
}
