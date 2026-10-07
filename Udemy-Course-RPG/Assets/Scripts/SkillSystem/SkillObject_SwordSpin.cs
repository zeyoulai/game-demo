using UnityEngine;

public class SkillObject_SwordSpin : SkillObject_Sword
{
    private int maxDistance;
    private float attacksPerSecound;
    private float attackTimer;

    public override void SetupSword(Skill_SwordThrow swordManager, Vector2 direction)
    {
        base.SetupSword(swordManager, direction);

        anim?.SetTrigger("spin");
        maxDistance = swordManager.maxDistance;
        attacksPerSecound = swordManager.attacksPerSecound;

        Invoke(nameof(GetSwordBackToPlayer), swordManager.maxSpinDuration);
    }

    protected override void Update()
    {
        HandleStopping();
        HandleComeback();
        HanldeAttack();
    }

    private void HandleStopping()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        Debug.Log(transform == null);
        Debug.Log(playerTransform == null);
        if (distanceToPlayer > maxDistance && rb.simulated == true)
            rb.simulated = false;
    }

    private void HanldeAttack()
    {
        attackTimer -= Time.deltaTime;

        if(attackTimer < 0)
        {
            DamageEnemiesInRadius(transform, 1);
            attackTimer = 1 / attacksPerSecound;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        rb.simulated = false;
    }
}
