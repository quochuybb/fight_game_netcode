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
    private Vector2 direction = Vector2.zero;
    
    private CharacterController player;

    private void Awake()
    {
        player = GetComponent<CharacterController>();
    }
    

    private void Start()
    {
        player.onLookEvent.AddListener(OnLookMouse);
    }

    private void OnLookMouse(Vector2 direction)
    {
        RotateAim(direction);
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
