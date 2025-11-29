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
    private readonly float dashPower = 0.2f;
    [SerializeField] private TrailRenderer dashTrail;
    private readonly NetworkVariable<Vector2> _networkPosition = new NetworkVariable<Vector2>(); 
    private float lastServerSyncTime;
    private float lastNetworkUpdate;
    private const float NetworkUpdateInterval = 0.05f; 
    [SerializeField] private Transform spawnTransform;
    [SerializeField] private StatsHandler statsHandler;
    private Vector2 preDashVelocity;
    private Vector2 preDashInput; 
    [SerializeField] private bool useImpulseDash = true;
    [SerializeField] private int countMap = 3;
    private NetworkVariable<int> selectedMap = new NetworkVariable<int>(
        writePerm: NetworkVariableWritePermission.Server);

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
        characterController.OnMoveEvent.AddListener(OnMove);
        characterController.OnDash.AddListener(RequestDashServerRpc);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (IsServer)
        {
            if (selectedMap.Value == 0) 
            {
                int map = Random.Range(1, countMap + 1);
                selectedMap.Value = map;
                Debug.Log($"[Server] selected map: {map}");
            }
        }

        selectedMap.OnValueChanged += OnSelectedMapChanged;

        if (selectedMap.Value != 0)
        {
            ApplySelectedMap(selectedMap.Value);
        }    
    }
    private void OnSelectedMapChanged(int oldValue, int newValue)
    {
        ApplySelectedMap(newValue);
    }

    private void ApplySelectedMap(int map)
    {
        string tagToFind;
        if (IsServer)
            tagToFind = "SpawnHost" + map;
        else
            tagToFind = "SpawnClient" + map;

        GameObject spawnObj = GameObject.FindGameObjectWithTag(tagToFind);
        if (spawnObj != null)
        {
            spawnTransform = spawnObj.transform;
            transform.position = spawnTransform.position;
        }
        else
        {
            Debug.LogWarning($"Spawn tag {tagToFind} not found in scene. Map={map}");
        }
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        selectedMap.OnValueChanged -= OnSelectedMapChanged;
    }



    private void HandleOwnerMovement(Vector2 movementVel)
    {
        Debug.LogError("Calculate Movement");
        movementVel = movementVel * statsHandler.currentStats.Value.speedMove;
        if (!isDashing)
        {
            rb.velocity = movementVel;
        }
        if (Time.time - lastNetworkUpdate >= NetworkUpdateInterval)
        {
            Debug.LogError("Call update Server");
            lastNetworkUpdate = Time.time;
            UpdatePositionServerRpc(rb.position);
        }

    }
    private void HandleClientMovement()
    {
        Debug.LogError("Update NotOwner");
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
            Debug.LogError("IsOwner");
            HandleOwnerMovement(movementInput);
        }
        else
        {
            Debug.LogError("NotOwner");
            HandleClientMovement();
        }  

    }
    [ServerRpc]
    private void UpdatePositionServerRpc(Vector2 newPosition)
    {
        Debug.LogError("Update on Server");
        _networkPosition.Value = newPosition;
        transform.position = newPosition;
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
        if (!canDash) yield break;

        canDash = false;
        isDashing = true;

        preDashVelocity = rb.velocity;
        preDashInput = movement; 

        if (useImpulseDash)
        {
            rb.AddForce(movement.normalized * dashPower, ForceMode2D.Impulse);
        }
        else
        {
            rb.velocity = movement.normalized * dashPower;
        }

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        Vector2 desiredMovementVelocity = preDashInput.normalized * statsHandler.currentStats.Value.speedMove;
        if (preDashInput != Vector2.zero)
        {
            rb.velocity = desiredMovementVelocity;
        }
        else
        {
            rb.velocity = Vector2.zero; 
        }

        yield return new WaitForSeconds(0.0f); 
        canDash = true;
    }
    
}

