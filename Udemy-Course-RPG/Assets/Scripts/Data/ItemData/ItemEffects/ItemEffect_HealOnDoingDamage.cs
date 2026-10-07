using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/  Heal On doing", fileName = "Item effect data - Heal On doing physical damage")]
public class ItemEffect_HealOnDoingDamage : ItemEffect_DataSO
{
    [SerializeField] private float percentHealedOnAttack = 0.5f;

    private void HealOnDoingDamage(float damage)
    {
        if (player == null || player.health == null)
            return;

        player.health.IncreaseHealth(damage*percentHealedOnAttack);
    }

    public override void Subscribe(Player player)
    {
        if (player == null || player.combat == null)
            return;

        base.Subscribe(player);
        player.combat.OnDoingPhysicalDamage += HealOnDoingDamage;
    }
    public override void Unsubscribe()
    {
        if (player == null)
            return;

        if (player.combat == null)
        {
            player = null;
            return;
        }

        base.Unsubscribe();
        player.combat.OnDoingPhysicalDamage -= HealOnDoingDamage;
        player = null;
    }
}
