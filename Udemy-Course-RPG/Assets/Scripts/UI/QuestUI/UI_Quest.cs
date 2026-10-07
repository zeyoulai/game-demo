using UnityEngine;

public class UI_Quest : MonoBehaviour,ISaveable
{
    private GameData currentGameData;

    [SerializeField] private UI_ItemSlotParent inventorySlots;
    [SerializeField] private UI_QuestPreview questPreview;

    private UI_QuestSlot[] questSlots;
    public Player_QuestManager questManager {  get; private set; }

    private void Awake()
    {
        questSlots = GetComponentsInChildren<UI_QuestSlot>(true);
        questManager = Player.instance.questManager;
    }

    public void SetupQuestUI(QuestDataSO[] questsToSetup)
    {
        foreach (var slot in questSlots)
            slot.gameObject.SetActive(false);

        for (int i = 0; i < questsToSetup.Length; i++)
        {
            questSlots[i].gameObject.SetActive(true);
            questSlots[i].SetupQuestSlot(questsToSetup[i]);
        }

        questPreview.MakeQuestPreviewEmpty();
        inventorySlots.UpdateSlots(Player.instance.inventory.itemList);

        UpdataQuestList();
    }

    public void UpdataQuestList()
    {
        foreach (var slot  in questSlots)
        {
            if (slot.questInSlot == null) continue;

            if(slot.gameObject.activeSelf && CanTakenQuest(slot.questInSlot) == false)
            {
                slot.gameObject.SetActive(false);
            }

        }
    }

    private bool CanTakenQuest(QuestDataSO questToCheck)
    {
        bool questActive = questManager.QuestIsActive(questToCheck);
        if(currentGameData != null)
        {
            bool questCompleted = currentGameData.completedQuests.TryGetValue(questToCheck.questSaveId, out bool isCompleted) && isCompleted;
            return questActive == false && questCompleted == false;

        }

        return questActive == false;
    }

    public UI_QuestPreview GetQuestPreview() => questPreview;

    public void LoadData(GameData data)
    {
        currentGameData = data;
    }

    public void SaveData(ref GameData data)
    {

    }
}
