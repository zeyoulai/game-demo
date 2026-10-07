using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private FileDataHandler dataHandler;
    private GameData gameData;
    private List<ISaveable> allSaveables;

    private void Awake()
    {
        instance = this;
    }


    [SerializeField] private string fileName = "unitylzydev.json";
    [SerializeField ] private bool encryData = true;
    private IEnumerator Start()
    {
        Debug.Log(Application.persistentDataPath);
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName,encryData);
        yield return null;
        LoadGame();
    }




    private void LoadGame()
    {
        allSaveables = FindISaveables();
        gameData = dataHandler.LoadData();

        if (gameData == null)
        {
            Debug.Log("No save data found, creating new save!");
            gameData = new GameData();
            return;
        }

        foreach (var saveable in GetLoadOrder())
        {
            try
            {
                saveable.LoadData(gameData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load data for {saveable}: {e}");
            }
        }
    }


    public void SaveGame()
    {
        allSaveables = FindISaveables();

        if (gameData == null)
            gameData = new GameData();

        foreach (var saveable in allSaveables)
        {
            try
            {
                saveable.SaveData(ref gameData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save data for {saveable}: {e}");
            }
        }
        dataHandler.SaveData(gameData);
    }

    public GameData GetGameData() => gameData;

    [ContextMenu("*** Delete Save Data ***")]
    public void DeleteSaveData()
    {
        Debug.Log("BTN DELETE");
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName,encryData);
        dataHandler.Delete();

        LoadGame();
        ResetGameManagerData();
        ResetSkillTreeData();
        dataHandler.SaveData(gameData);
    }

    private void ResetGameManagerData()
    {
        if (allSaveables == null)
            return;

        foreach (var gameManager in allSaveables.OfType<GameManager>())
        {
            gameManager.LoadData(gameData);
        }
    }

    private void ResetSkillTreeData()
    {
        if (allSaveables == null)
            return;

        foreach (var skillTree in allSaveables.OfType<UI_SkillTree>())
        {
            skillTree.LoadData(gameData);
            skillTree.SaveData(ref gameData);
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }


    private List<ISaveable> FindISaveables()
    {
        return FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .OfType<ISaveable>()
            .Where(IsValidSaveable)
            .ToList();

    }

    private bool IsValidSaveable(ISaveable saveable)
    {
        Component component = saveable as Component;

        if (component == null)
            return true;

        if (component.GetComponentInParent<Player>(true) != null && component.gameObject.activeInHierarchy == false)
            return false;

        return true;
    }

    private IEnumerable<ISaveable> GetLoadOrder()
    {
        return allSaveables.OrderBy(saveable => saveable is Player_Health ? 1 : 0);
    }
}
