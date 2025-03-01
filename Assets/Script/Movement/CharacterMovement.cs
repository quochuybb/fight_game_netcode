using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterMovement : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    private Vector2 movement = Vector2.zero;
    private CharacterController characterController;
    private float speed = 5;
    private bool canDash = true;
    private bool isDashing;
    private float dashDuration = 1f;
    private float dashPower = 1f;
    private float dashCooldown = 5f;
    [SerializeField] private TrailRenderer dashTrail;
    private NetworkVariable<Vector2> networkPosition = new NetworkVariable<Vector2>(); 
    private float lastServerSyncTime;
    private float lastNetworkUpdate;
    private const float NETWORK_UPDATE_INTERVAL = 0.05f; 

    
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
        characterController.onDash.AddListener(Dashing);
        
    }
    private void OnNetworkPositionChanged(Vector2 oldValue, Vector2 newValue)
    {
        Debug.Log($"OnNetworkPositionChanged old: {oldValue}, new: {newValue}");
        transform.position = newValue;
    } 
    private void HandleOwnerMovement(Vector2 movement)
    {
        Vector2 moveVelocity = movement * speed;
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
        GameLogger.Instance.LogError("send position to server" + newPosition + ", serverRpcParams: " + serverRpcParams.Receive.SenderClientId);
        networkPosition.Value = newPosition;
        transform.position = newPosition;
        UpdatePositionClientRpc(newPosition);
    }
    [ClientRpc]
    private void UpdatePositionClientRpc(Vector2 newPosition)
    {
        GameLogger.Instance.LogError("send position to client" + newPosition);
        transform.position = newPosition;
        
    }


    private void Dashing()
    {
        if (canDash)
        {
            canDash = false;
            isDashing = true; 
            rb.AddForce(movement * dashPower, ForceMode2D.Force);
            dashTrail.emitting = true;
        }
        dashTrail.emitting = false;
        isDashing = false;
        canDash = true;
    }
    
}

