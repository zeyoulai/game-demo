using UnityEngine;

public class Entity_AnimationTriggers : MonoBehaviour
{
    private Entity entity;
    private Entity_Combat entityCombat;

    protected virtual void Awake()
    {
        entity = GetComponentInParent<Entity>();
        entityCombat = GetComponentInParent<Entity_Combat>();
    }


    private void CurrentStateTrigger()
    {
        // get accec to player and let current state know that we want to exit state
        entity.CurrentStateAnimationTrigger();
    }

    private void AttackTrigger()
    {

        entityCombat.PerformAttack();
    }
}
