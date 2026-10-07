using UnityEngine;

public class Enemy_Health : Entity_Health
{
    private Enemy enemy;
    private Player_QuestManager questManager;

    protected override void Start()
    {
        base.Start();
        enemy = GetComponent<Enemy>();
        questManager = Player.instance.questManager;
    }

    public override bool TakeDamage(float damage, float elementalDamage,ElementType element, Transform damageDealer)
    {
        if(canTakenDamage == false)
            return false;

        bool wasHit = base.TakeDamage(damage, elementalDamage,element,damageDealer);

        if (wasHit == false)
            return false;

        if (damageDealer.GetComponent<Player>() != null)
            enemy.TryEnterBattleState(damageDealer);

        return true;

    }

    protected override void Die()
    {
        base.Die();

        if (questManager == null && Player.instance != null)
            questManager = Player.instance.questManager;

        if (questManager != null && enemy != null && string.IsNullOrEmpty(enemy.qurstTargetId) == false)
            questManager.AddProgress(enemy.qurstTargetId);
    }
}
