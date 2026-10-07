using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public enum RewardType { Merchant, Blackmisth, None }
public enum QuestType { Kill,Talk,Delivery}
[CreateAssetMenu(menuName = "RPG Setup/Quest Data/New Quest", fileName = " Quest - ")]
public class QuestDataSO : ScriptableObject
{
    public string questSaveId;
    [Space]
    public QuestType questType;
    public string questName;
    [TextArea] public string description;
    [TextArea] public string questGoal;

    [FormerlySerializedAs("quuestTargetId")]
    public string questTargetId;//enemy name npc name item name
    public int requiredAmount;
    public ItemDataSO itemToDeliver;

    [Header("Reward")]
    public RewardType rewardType;
    public Inventory_Item[] rewardItems;

    private void OnValidate()
    {
#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(this);
        questSaveId = AssetDatabase.AssetPathToGUID(path);
#endif
    }
}
