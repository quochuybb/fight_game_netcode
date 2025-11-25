using System.Collections;
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
    private readonly NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>();
    private float lastServerSyncTime;
    private float lastNetworkUpdate;
    private const float NetworkUpdateInterval = 0.001f;
    [SerializeField] private Transform spawnTransform;
    [FormerlySerializedAs("statsHandlerReWork")] [SerializeField] private StatsHandlerReWork statsHandlerReWork;

    // --- reconciliation settings (mới) ---
    [Header("Reconciliation")]
    [Tooltip("Nếu lệch nhỏ hơn giá trị này thì không cần correction")]
    [SerializeField] private float reconciliationIgnoreThreshold = 0.05f;
    [Tooltip("Nếu lệch lớn hơn snap thì nhảy thẳng về server")]
    [SerializeField] private float reconciliationSnapThreshold = 0.8f;
    [Tooltip("Số giây để sửa mượt về vị trí server (nếu không snap)")]
    [SerializeField] private float reconciliationDuration = 0.12f;

    private Coroutine reconcileCoroutine;

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
        _networkPosition.OnValueChanged += OnNetworkPositionChanged;
        characterController.OnMoveEvent.AddListener(OnMove);
        characterController.OnDash.AddListener(RequestDashServerRpc);
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
        }
        else if (IsClient)
        {
            var t = GameObject.FindGameObjectWithTag("SpawnClient" + map);
            if (t != null) spawnTransform = t.transform;
            if (spawnTransform != null) transform.position = spawnTransform.position;
        }


    }

    private void OnNetworkPositionChanged(Vector2 oldValue, Vector2 newValue)
    {
        // Nếu không phải owner (remote), set trực tiếp để render (interpolation khác đang dùng)
        if (!IsOwner)
        {
            transform.position = newValue;
            return;
        }

        // --- Owner: reconcile prediction với authoritative server position ---
        // compute distance between local predicted pos and server pos
        float dist = Vector2.Distance(transform.position, newValue);

        // nhỏ hơn ngưỡng: bỏ qua (local prediction đủ tốt)
        if (dist <= reconciliationIgnoreThreshold)
        {
            return;
        }

        // quá lớn: snap ngay
        if (dist >= reconciliationSnapThreshold)
        {
            // hủy coroutine nếu đang chạy
            if (reconcileCoroutine != null)
            {
                StopCoroutine(reconcileCoroutine);
                reconcileCoroutine = null;
            }

            transform.position = newValue;
            if (rb != null) rb.position = newValue;
            return;
        }

        // trung bình: sửa mượt dần (smooth correction)
        if (reconcileCoroutine != null) StopCoroutine(reconcileCoroutine);
        reconcileCoroutine = StartCoroutine(SmoothReconcile(newValue, reconciliationDuration));
    }

    private IEnumerator SmoothReconcile(Vector2 targetPos, float duration)
    {
        float elapsed = 0f;
        Vector2 startPos = transform.position;

        // If using rb for movement, we set rb.position each frame to avoid physics conflicts
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector2 pos = Vector2.Lerp(startPos, targetPos, t);
            transform.position = pos;
            if (rb != null) rb.position = pos;
            yield return null;
        }

        // ensure final
        transform.position = targetPos;
        if (rb != null) rb.position = targetPos;
        reconcileCoroutine = null;
    }

    private void HandleOwnerMovement(Vector2 movementVel)
    {
        movementVel = movementVel * statsHandlerReWork.currentStats.Value.speedMove;
        rb.velocity = movementVel;
        if (Time.time - lastNetworkUpdate >= NetworkUpdateInterval)
        {

            lastNetworkUpdate = Time.time;
            UpdatePositionServerRpc(rb.position);
        }

    }
    private void HandleClientMovement()
    {
        transform.position = Vector2.Lerp(
            transform.position,
            _networkPosition.Value,
            Time.deltaTime * 15f
        );
    }

    private void OnMove(Vector2 movementInput)
    {

        this.movement = movementInput;
        if (IsOwner)
        {
            HandleOwnerMovement(movementInput);
        }
        else
        {
            HandleClientMovement();
        }

    }
    [ServerRpc]
    private void UpdatePositionServerRpc(Vector2 newPosition)
    {
        _networkPosition.Value = newPosition;
        transform.position = newPosition;
        UpdatePositionClientRpc(newPosition);
    }
    [ClientRpc]
    private void UpdatePositionClientRpc(Vector2 newPosition)
    {
        // nếu là owner ta đã xử lý reconciliation trong NetworkVariable callback
        if (!IsOwner)
        {
            transform.position = newPosition;
        }
    }


    [ServerRpc (RequireOwnership = false)]
    private void RequestDashServerRpc()
    {
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

    // Optional: allow external system to force immediate sync (debug)
    public void ForceSnapToServer()
    {
        var sv = _networkPosition.Value;
        transform.position = sv;
        if (rb != null) rb.position = sv;
        if (reconcileCoroutine != null) { StopCoroutine(reconcileCoroutine); reconcileCoroutine = null; }
    }
}
