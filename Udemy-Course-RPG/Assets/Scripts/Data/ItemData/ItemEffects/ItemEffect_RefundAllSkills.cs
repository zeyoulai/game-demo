using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/ Refund all skills", fileName = "Item effect data - Refund all skills")]
public class ItemEffect_RefundAllSkills : ItemEffect_DataSO
{
    public override bool CanBeUsed(Player player)
    {
        UI ui = FindAnyObjectByType<UI>();
        return ui != null && ui.skillTreeUI != null && ui.skillTreeUI.HasRefundableSkills();
    }

    public override void ExecuteEffect()
    {
        base.ExecuteEffect();
        UI ui = FindAnyObjectByType<UI>();
        if (ui == null || ui.skillTreeUI == null)
            return;

        ui.skillTreeUI.RefundAllSkills();
    }

}
