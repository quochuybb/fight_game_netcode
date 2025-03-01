using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostMovement : EnemyController
{
    public override void FixedUpdate()
    {
        lastTimeAttack += Time.fixedDeltaTime;
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
            
            onLookEvent.Invoke(DirectionToTarget(target));
            onMoveEvent.Invoke(DirectionToTarget(target) * 0.5f);
                
        }
        else
        {
            target = FindTelePort();
            onMoveEvent.Invoke(DirectionToTarget(target) * 0.3f);
            onLookEvent.Invoke(DirectionToTarget(target));
        }
    }
}
