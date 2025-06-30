using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInput : CharacterController
{
    [SerializeField] private Camera _camera;
    [SerializeField] private MenuTransition _transition;
    
    private void Awake()
    {
        _camera = Camera.main;
        _transition = MenuTransition.instance;
    }
    
    public void OnMovement(InputValue value)
    {
        Vector2 direction = value.Get<Vector2>();
        OnMoveEvent.Invoke(direction);
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
            OnLookEvent.Invoke(direction);
        }
    }

    public void OnAttackRange(InputValue value)
    {
        useGun = value.isPressed;
    }

    public void OnThrowItem(InputValue value)
    {
        //canThrow = value.isPressed;
    }

    public void OnDash(InputValue value)
    {
        base.OnDash.Invoke();
    }

    public void OnSetting(InputValue value)
    {
        if (value.isPressed)
        {
            _transition.onOpenSettings.Invoke();

        }
    }
}
