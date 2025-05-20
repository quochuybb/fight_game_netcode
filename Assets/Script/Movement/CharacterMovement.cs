using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

public class CharacterMovement : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    private Vector2 movement = Vector2.zero;
    private CharacterController characterController;
    private bool canDash = true;
    private bool isDashing;
    private float dashDuration = 0.25f;
    private float dashPower = 5f;
    [SerializeField] private TrailRenderer dashTrail;
    private NetworkVariable<Vector2> networkPosition = new NetworkVariable<Vector2>(); 
    private float lastServerSyncTime;
    private float lastNetworkUpdate;
    private const float NETWORK_UPDATE_INTERVAL = 0.001f; 
    [SerializeField] private Transform spawnTransform;
    [SerializeField] private StatsHandler statsHandler;
    
    private void Awake()
    {
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
        networkPosition.OnValueChanged += OnNetworkPositionChanged;
        characterController.onMoveEvent.AddListener(OnMove);
        characterController.onDash.AddListener(RequestDashServerRpc);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        //Random random = new Random();
        //int map = random.Next(1, 3);
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

    private void OnNetworkPositionChanged(Vector2 oldValue, Vector2 newValue)
    {
        transform.position = newValue;
    } 

    private void HandleOwnerMovement(Vector2 movement)
    {

        Vector2 moveVelocity = movement;
        if (IsServer)
        {
            moveVelocity = movement * statsHandler.currentStatsNetworkVariableHost.Value.speedMove;

        }
        else
        {
            moveVelocity = movement * statsHandler.currentStatsNetworkVariableClient.Value.speedMove;
        }
        rb.velocity = moveVelocity;
        if (Time.time - lastNetworkUpdate >= NETWORK_UPDATE_INTERVAL)
        {

            lastNetworkUpdate = Time.time;
            UpdatePositionServerRpc(rb.position);
        }

    }
    private void HandleClientMovement()
    {
        transform.position = Vector2.Lerp(
            transform.position, 
            networkPosition.Value, 
            Time.deltaTime * 15f
        );
    }

    private void OnMove(Vector2 movement)
    {

        this.movement = movement;
        if (IsOwner)
        {
            HandleOwnerMovement(movement);
        }
        else
        {
            HandleClientMovement();
        }  

    }
    [ServerRpc]
    private void UpdatePositionServerRpc(Vector2 newPosition, ServerRpcParams serverRpcParams = default)
    {
        networkPosition.Value = newPosition;
        transform.position = newPosition;
        UpdatePositionClientRpc(newPosition);
    }
    [ClientRpc]
    private void UpdatePositionClientRpc(Vector2 newPosition)
    {

        transform.position = newPosition;
        
    }


    [ServerRpc (RequireOwnership = false)]
    private void RequestDashServerRpc()
    {
        PerformDashClientRpc();
    }

    [ClientRpc]
    private void PerformDashClientRpc(ClientRpcParams rpcParams = default)
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

