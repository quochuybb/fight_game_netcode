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
    private const float NetworkUpdateInterval = 0.05f; 
    [SerializeField] private Transform spawnTransform;
    [FormerlySerializedAs("statsHandlerReWork")] [SerializeField] private StatsHandlerReWork statsHandlerReWork;

    
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
        int map = 1;
        if (IsServer)
        {
            spawnTransform = GameObject.FindGameObjectWithTag("SpawnHost" + map).transform;
            transform.position = spawnTransform.position;
        }
        else if (IsClient)
        {
            spawnTransform = GameObject.FindGameObjectWithTag("SpawnClient"+ map).transform;
            transform.position = spawnTransform.position;
        }

        
    }

    private void HandleOwnerMovement(Vector2 movementVel)
    {
        Debug.LogError("Calculate Movement");
        movementVel = movementVel * statsHandlerReWork.currentStats.Value.speedMove;
        rb.velocity = movementVel;
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
        canDash = false;
        isDashing = true;

        rb.AddForce(movement * dashPower, ForceMode2D.Force);
        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector2.zero;
        isDashing = false;
        canDash = true;
    }
    
}

