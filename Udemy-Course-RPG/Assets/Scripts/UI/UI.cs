using UnityEngine;

public class UI : MonoBehaviour
{
    public static UI instance;

    [SerializeField] private GameObject[] uiElements;
    public bool alternativeInput { get; private set; }
    private PlayerInputSet input;

    #region UI_Components
    public UI_SkillToolTip skillTooTip { get; private set; }
    public UI_ItemToolTip itemToolTip { get; private set; }
    public UI_StatToolTip statToolTip { get; private set; }
    public UI_SkillTree skillTreeUI { get; private set; }
    public UI_Inventory inventoryUI { get; private set; }
    public UI_Storage storageUI { get; private set; }
    public UI_Craft craftUI { get; private set; }
    public UI_Merchant merchantUI { get; private set; }
    public UI_InGame inGameUI { get; private set; }
    public UI_Options optionsUI { get; private set; }
    public UI_DeathScreen deathScreenUI { get; private set; }
    public UI_FadeScreen fadeScreenUI { get; private set; }
    public UI_Quest questUI { get; private set; }
    public UI_Dialogue dialogueUI { get; private set; }

    #endregion
    private bool skillTreeEnabled = false;
    private bool inventoryEnabled = false;

    private void Awake()
    {
        instance = this;

        skillTooTip = GetComponentInChildren<UI_SkillToolTip>();
        itemToolTip = GetComponentInChildren<UI_ItemToolTip>();
        statToolTip = GetComponentInChildren<UI_StatToolTip>();
        skillTreeUI = GetComponentInChildren<UI_SkillTree>(true);
        inventoryUI = GetComponentInChildren<UI_Inventory>(true);
        storageUI = GetComponentInChildren<UI_Storage>(true);
        craftUI = GetComponentInChildren<UI_Craft>(true);
        merchantUI = GetComponentInChildren<UI_Merchant>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        optionsUI = GetComponentInChildren<UI_Options>(true);
        deathScreenUI = GetComponentInChildren<UI_DeathScreen>(true);
        fadeScreenUI = GetComponentInChildren<UI_FadeScreen>(true);
        questUI = GetComponentInChildren<UI_Quest>(true);
        dialogueUI = GetComponentInChildren<UI_Dialogue>(true);

        skillTreeEnabled = skillTreeUI.gameObject.activeSelf;
        inventoryEnabled = inventoryUI.gameObject.activeSelf;
    }

    private void Start()
    {
        skillTreeUI.UnlockDefaultSkills();
    }
    public void SetupControlsUI(PlayerInputSet inputset)
    {
        input = inputset;
        input.UI.SkillTreeUI.performed += ctx => ToggleSkillTreeUI();
        input.UI.InventoryUI.performed += ctx => ToggleInventoryUI();
        input.UI.AlternativeInput.performed += ctx => alternativeInput = true;
        input.UI.AlternativeInput.canceled += ctx => alternativeInput = false;
        input.UI.OptionsUI.performed += ctx =>
        {
            foreach (var element in uiElements)
            {
                if (element.activeSelf)
                {
                    Time.timeScale = 1;
                    SwitchToInGameUI();
                    return;
                }
            }
            Time.timeScale = 0.01f;
            OpenOptionsUI();
        };

        input.UI.DialogueInteraction.performed += ctx =>
        {
           
            if (dialogueUI.gameObject.activeInHierarchy)
            {
                Debug.Log("F interaction");
                dialogueUI.DialogueInteraction();
            }
        };

        input.UI.DialogueNevigation.performed += ctx =>
        {
            int direction = Mathf.RoundToInt(ctx.ReadValue<float>());

            if (dialogueUI.gameObject.activeInHierarchy)
                dialogueUI.NavigationChoice(direction);
        };
    }

    public void OpenDeathScreenUI()
    {
        SwitchTo(deathScreenUI.gameObject);
        StopPlayerControls(true);
        input.Disable();
    }

    public void OpenOptionsUI()
    {
        SwitchTo(optionsUI.gameObject);

        HideAllToolTips();
        StopPlayerControls(true);

    }

    public void SwitchToInGameUI()
    {
        SwitchTo(inGameUI.gameObject);
        HideAllToolTips();
        StopPlayerControls(false);
        inGameUI.gameObject.SetActive(true);

        skillTreeEnabled = false;
        inventoryEnabled = false;
    }

    private void SwitchTo(GameObject objectToSwitchOn)
    {
        foreach (var element in uiElements)
            element.gameObject.SetActive(false);

        objectToSwitchOn.SetActive(true);
    }

    private void StopPlayerControls(bool stopControls)
    {
        if (stopControls)
            input.Player.Disable();
        else input.Player.Enable();
    }
    private void StopPlayerControlsIfNeeded()
    {
        foreach (var element in uiElements)
        {
            if (element.activeSelf)
            {
                StopPlayerControls(true);
                return;
            }
        }
        StopPlayerControls(false);
    }



    public void ToggleSkillTreeUI()
    {
        skillTreeUI.transform.SetAsLastSibling();
        SetToolTipAsLastSibling();
        fadeScreenUI.transform.SetAsLastSibling();

        skillTreeEnabled = !skillTreeEnabled;
        skillTreeUI.gameObject.SetActive(skillTreeEnabled);
        HideAllToolTips();

        StopPlayerControlsIfNeeded();
    }

    public void ToggleInventoryUI()
    {
        inventoryUI.transform.SetAsLastSibling();
        SetToolTipAsLastSibling();
        fadeScreenUI.transform.SetAsLastSibling();

        inventoryEnabled = !inventoryEnabled;
        inventoryUI.gameObject.SetActive(inventoryEnabled);
        HideAllToolTips();

        StopPlayerControlsIfNeeded();
    }

    public void OpenDialogueUI(DialogueLineSO firstLine,DialogueNPCData npcData)
    {
        StopPlayerControls(true);
        HideAllToolTips();

        dialogueUI.gameObject.SetActive(true);
        dialogueUI.SetupNpcData(npcData);
        dialogueUI.PlayDialogueLine(firstLine);
    }

    public void OpenQuestUI(QuestDataSO[] questsToShow)
    {
        StopPlayerControls(true);
        HideAllToolTips();
        questUI.gameObject.SetActive(true);
        questUI.SetupQuestUI(questsToShow);
    }

    public void OpenStorageUI(bool openStorageUI)
    {
        storageUI.gameObject.SetActive(openStorageUI);
        StopPlayerControls(openStorageUI);

        if (openStorageUI == false)
        {
            craftUI.gameObject.SetActive(false);
            HideAllToolTips();
        }
    }

    public void OpenCraftUI(bool openStorageUI)
    {
        craftUI.gameObject.SetActive(openStorageUI);
        StopPlayerControls(openStorageUI);

        if (openStorageUI == false)
        {
            storageUI.gameObject.SetActive(false);
            HideAllToolTips();
        }
    }

    public void OpenMerchantUI(bool openMerchantUI)
    {
        merchantUI.gameObject.SetActive(openMerchantUI);
        StopPlayerControls(openMerchantUI);

        if (openMerchantUI == false)
        {
            HideAllToolTips();
        }
    }

    public void HideAllToolTips()
    {
        itemToolTip.ShowToolTip(false, null);
        skillTooTip.ShowToolTip(false, null);
        statToolTip.ShowToolTip(false, null);
    }

    private void SetToolTipAsLastSibling()
    {
        itemToolTip.transform.SetAsLastSibling();
        skillTooTip.transform.SetAsLastSibling();
        statToolTip.transform.SetAsLastSibling();
    }
}
