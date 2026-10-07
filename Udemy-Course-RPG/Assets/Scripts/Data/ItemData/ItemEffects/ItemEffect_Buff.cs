using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/ Buff effect", fileName = "Item effect data - Buff")]
public class ItemEffect_Buff : ItemEffect_DataSO
{
    [SerializeField] private BuffEffectData[] buffsTpApply;
    [SerializeField] private float duration;
    [SerializeField] private string source = Guid.NewGuid().ToString();



    public override bool CanBeUsed(Player player)
    {
        if (player == null || player.stats == null)
            return false;

        if (player.stats.CanApplyBuffOf(source))
        {
            this.player = player;
            return true;
        }
        else
        {
            Debug.Log("Same Buff effect cannot be applied twice");
            return false;
        }
    }
    public override void ExecuteEffect()
    {
        if (player == null || player.stats == null)
            return;

        player.stats.ApplyBuff(buffsTpApply, duration,source);
        player = null;
    }
}
