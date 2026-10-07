using UnityEngine;
using UnityEngine.SceneManagement;

public class Object_Portal : MonoBehaviour, ISaveable
{
    public static Object_Portal instance;

    public bool isActivte { get; private set; }
    [SerializeField] private Vector2 defaultPosition; // where portal appears in town
    [SerializeField] private string townSceneName = "Level_0";

    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool canBeTriggered;


    private string currentSceneName;
    private string returnSceneName;
    private bool returningFromTown;

    private void Awake()
    {
        instance = this;
        currentSceneName = SceneManager.GetActiveScene().name;
        transform.position = new Vector3(9999, 9999);
    }

    public void ActivatePortal(Vector3 position, int facingDir = 1)
    {
        isActivte = true;
        transform.position = position;
        SaveManager.instance.GetGameData().inScenePortals.Clear();


        if (facingDir == -1)
            transform.Rotate(0, 180, 0);
    }

    public void DisableIfNeeded()
    {
        if (returningFromTown == false) return;


        SaveManager.instance.GetGameData().inScenePortals.Remove(currentSceneName);
        isActivte = false;
        transform.position = new Vector3(9999, 9999);
    }

    private void UseTeleport()
    {
        //transfer to another scene
        string destinationScene = InTown() ? returnSceneName : townSceneName;
        GameManager.instance.ChangeScene(destinationScene, RespawnType.Portal);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canBeTriggered == false)
            return;

        UseTeleport();
    }

    private void OnTriggerExit2D(Collider2D collision) => canBeTriggered = true;

    public void SetTrigger(bool trigger) => canBeTriggered = trigger;

    public Vector3 GetPosition() => respawnPoint != null ? respawnPoint.position : transform.position;

    private bool InTown() => currentSceneName == townSceneName;

    public void LoadData(GameData data)
    {
        if (InTown() && data.inScenePortals.Count > 0)
        {
            transform.position = defaultPosition;
            isActivte = true;
        }
        else if (data.inScenePortals.TryGetValue(currentSceneName, out Vector3 portalPosition))
        {
            transform.position = portalPosition;
            isActivte = true;
        }

        returningFromTown = data.returningFromTown;
        returnSceneName = data.portalDestinationSceneName;


    }

    public void SaveData(ref GameData data)
    {
        data.returningFromTown = InTown();
        if (isActivte && InTown() == false)
        {
            data.inScenePortals[currentSceneName] = transform.position;
            data.portalDestinationSceneName = currentSceneName;
        }
        else
        {
            data.inScenePortals.Remove(currentSceneName);
        }

    }
}
