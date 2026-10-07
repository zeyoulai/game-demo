using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/grand skill points", fileName = "Item effect data - Grant skill point")]
public class ItemEffect_GrantSkillPoints : ItemEffect_DataSO
{
    [SerializeField] private int pointsToAdd;

    public override void ExecuteEffect()
    {
        UI ui = FindAnyObjectByType<UI>();
        if (ui == null || ui.skillTreeUI == null)
            return;

        ui.skillTreeUI.AddSkillPoints(pointsToAdd);
        Debug.Log("skill point:" + ui.skillTreeUI.skillPoints);
    }
}
