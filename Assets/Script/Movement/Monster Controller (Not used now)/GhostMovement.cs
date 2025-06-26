using UnityEngine;

public class GhostMovement : EnemyController
{
    public override void FixedUpdate()
    {
        LastTimeAttack += Time.fixedDeltaTime;
        if (DistanceToTarget(FindClean()) > DistanceToTarget(FindPlayer()) && gameObject.tag != "Clean")
        {
            target = FindPlayer();
        }
        else
        {
            target = FindClean();
        }
        if (CanSeeObject(target) && gameObject.tag != "Clean")
        {
            
            OnLookEvent.Invoke(DirectionToTarget(target));
            OnMoveEvent.Invoke(DirectionToTarget(target) * 0.5f);
                
        }
        else
        {
            target = FindTelePort();
            OnMoveEvent.Invoke(DirectionToTarget(target) * 0.3f);
            OnLookEvent.Invoke(DirectionToTarget(target));
        }
    }
}
