using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoliderController : EnemyController
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

        if (gameObject.tag == "Clean")
        {
            target = FindTelePort();

        }
        if (CanSeeObject(target) && gameObject.tag != "Clean")
        {

            if (DistanceToTarget(target) < 7f && gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                if (lastTimeAttack >= bulletConfig.delay)
                {
                    lastTimeAttack = 0f;
                    useGun = true;
                    onLookEvent.Invoke(DirectionToTarget(target));
                }
                else
                {
                    onLookEvent.Invoke(DirectionToTarget(target));
                }
            } 
            else
            {
                onLookEvent.Invoke(DirectionToTarget(target));
                onMoveEvent.Invoke(DirectionToTarget(target) * 0.5f);

            }
        }
        else
        {
            onMoveEvent.Invoke(DirectionToTarget(target) * 0.3f);
            onLookEvent.Invoke(DirectionToTarget(target));
        }
    }
    
}
