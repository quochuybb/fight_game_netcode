using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RotateCharacter : NetworkBehaviour
{
    [SerializeField] private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    [SerializeField] private SpriteRenderer gun;
    [SerializeField] private Transform spawnBullet;
    private NetworkVariable<Vector2> networkAimDirection = new NetworkVariable<Vector2>( Vector2.right, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Owner);
    
    private CharacterController player;

    private void Awake()
    {
        player = GetComponent<CharacterController>();
    }
    

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            player.onLookEvent.AddListener(OnLookMouse);
        }
        networkAimDirection.OnValueChanged += HandleAimDirectionChanged;
    }

    private void OnLookMouse(Vector2 direction)
    {
        networkAimDirection.Value = direction;
    }
    private void HandleAimDirectionChanged(Vector2 previousValue, Vector2 newValue)
    {
        RotateAim(newValue);
    }

    private void RotateAim(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        gun.flipY = Mathf.Abs(angle) > 90f;
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.flipX = gun.flipY;
        }
        spawnBullet.rotation = Quaternion.Euler(0, 0, angle);

    }
}
