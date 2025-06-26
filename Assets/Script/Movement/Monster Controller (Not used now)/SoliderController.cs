using UnityEngine;

public class SoliderController : EnemyController
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

        if (gameObject.tag == "Clean")
        {
            target = FindTelePort();

        }
        if (CanSeeObject(target) && gameObject.tag != "Clean")
        {

            if (DistanceToTarget(target) < 7f && gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                if (LastTimeAttack >= 0.5f)
                {
                    LastTimeAttack = 0f;
                    useGun = true;
                    OnLookEvent.Invoke(DirectionToTarget(target));
                }
                else
                {
                    OnLookEvent.Invoke(DirectionToTarget(target));
                }
            } 
            else
            {
                OnLookEvent.Invoke(DirectionToTarget(target));
                OnMoveEvent.Invoke(DirectionToTarget(target) * 0.5f);

            }
        }
        else
        {
            OnMoveEvent.Invoke(DirectionToTarget(target) * 0.3f);
            OnLookEvent.Invoke(DirectionToTarget(target));
        }
    }
    
}
