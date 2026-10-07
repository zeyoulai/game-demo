using UnityEngine;

public class UI_DeathScreen : MonoBehaviour
{
    public void GoToCampBTN()
    {
        GameManager.instance.ChangeScene("Level_0", RespawnType.NonSpecific);
    }

    public void GoToCheckpoint()
    {
        GameManager.instance.ReStartGame();
    }

    public void GoToMainMenu()
    {
        GameManager.instance.ChangeScene("MainMenu", RespawnType.NonSpecific);
    }
}
