using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInput : CharacterController
{
    [SerializeField] private Camera _camera;
    private void Awake()
    {
        _camera = Camera.main;
    }
    
    public void OnMovement(InputValue value)
    {
        Vector2 direction = value.Get<Vector2>();
        onMoveEvent.Invoke(direction);
        
    }

    public void OnLook(InputValue value)
    {
        Vector2 direction = value.Get<Vector2>();
        if (direction.normalized != direction)
        {
            Vector2 pointerPosition = _camera.ScreenToWorldPoint(direction);
            direction = (pointerPosition - (Vector2)transform.position).normalized;
        }

        if (direction.magnitude >= 0.9f)
        {
            onLookEvent.Invoke(direction);
        }
    }
    public void OnAttackMelee(InputValue value)
    {
        useMelee = value.isPressed;
    }

    public void OnAttackRange(InputValue value)
    {
        useGun = value.isPressed;
    }

    public void OnThrowItem(InputValue value)
    {
        canThrow = value.isPressed;
    }

    public void OnDash(InputValue value)
    {
        onDash.Invoke();
    }
}
