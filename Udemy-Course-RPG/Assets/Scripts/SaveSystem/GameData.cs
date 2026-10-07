using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData 
{
    public int gold;
    public string quickItemSlot1SaveId;
    public string quickItemSlot2SaveId;
    public bool hasPlayerHealthData;
    public float playerCurrentHealth;
    public float playerHealthPercent;

    public List<Inventory_Item> listItem;
    public SerializableDictionary<string, int> inventory;//itemSaveId -> stackSize
    public SerializableDictionary<string, int> storageItems;
    public SerializableDictionary<string, int> storageMaterials;

    public SerializableDictionary<string, ItemType> equipedItems; //slotTtpe -> itemSaveid

    public int skillPoints;
    public bool hasSkillTreeData;
    public SerializableDictionary<string, bool> skillTreeUI;
    public SerializableDictionary<SkillType, SkillUpgradeType> skillUpgrades;

    public SerializableDictionary<string, bool> unlockedCheckpoints; //check pointid -> ublocked status
    public SerializableDictionary<string,Vector3> inScenePortals;//scene name > portal position

    public SerializableDictionary<string, bool> completedQuests;
    public SerializableDictionary<string, int> activeQuests;


    public string portalDestinationSceneName;
    public bool returningFromTown;

    public string lastScenePlayed;
    public Vector3 lastPlayerPosition;

    public GameData()
    {
        playerHealthPercent = 1;

        inventory = new SerializableDictionary<string, int>();
        storageItems = new SerializableDictionary<string, int>();
        storageMaterials = new SerializableDictionary<string, int>();

        equipedItems = new SerializableDictionary<string, ItemType>();
        skillTreeUI = new SerializableDictionary<string, bool>();
        skillUpgrades = new SerializableDictionary<SkillType, SkillUpgradeType>();

        unlockedCheckpoints = new SerializableDictionary<string, bool>();
        inScenePortals = new SerializableDictionary<string, Vector3>();

        completedQuests = new SerializableDictionary<string, bool>();
        activeQuests = new SerializableDictionary<string, int>(); 
    }
}
