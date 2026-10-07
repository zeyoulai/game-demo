using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/ Heal effect", fileName = "Item effect data - heal")]
public class ItemEffect_Heal : ItemEffect_DataSO
{
    [SerializeField] private float healPercent = .1f;

    public override bool CanBeUsed(Player player)
    {
        return player != null && player.health != null && player.health.GetHealthPercent() < 1;
    }

    public override void ExecuteEffect()
    {
        Player player = Player.instance;
        if (player == null)
            return;

        float healAmount = player.stats.GetMaxHealth() * healPercent;

        player.health.IncreaseHealth(healAmount);
    }
}
