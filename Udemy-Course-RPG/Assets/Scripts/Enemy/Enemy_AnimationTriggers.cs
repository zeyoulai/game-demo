using UnityEngine;

public class Enemy_AnimationTriggers : Entity_AnimationTriggers
{
    private Enemy enemy;
    private Enemy_VFX enemyVfx; 
    protected override void Awake()
    {
        base.Awake();

        enemy = GetComponentInParent<Enemy>();
        enemyVfx = GetComponentInParent<Enemy_VFX>();
    }


    private void SpecialAttackTrigger()
    {
        enemy.SpecialAttack();
    }

    private void EnableCounterWindow()
    {
        enemy.EnableCounterWindoow(true);
        enemyVfx.EnableAttackAlert(true);
    }

    private void DisableCounterWindow()
    {
        enemy.EnableCounterWindoow(false);
        enemyVfx.EnableAttackAlert(false);
    }
}
