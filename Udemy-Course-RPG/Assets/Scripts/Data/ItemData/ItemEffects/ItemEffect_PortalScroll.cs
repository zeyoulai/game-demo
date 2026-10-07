using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/ Portal scroll", fileName = "Item effect data - PortalScroll")]
public class ItemEffect_PortalScroll : ItemEffect_DataSO
{
    public override bool CanBeUsed(Player player)
    {
        return SceneManager.GetActiveScene().name != "Level_0" && player != null && Object_Portal.instance != null;
    }

    public override void ExecuteEffect()
    {
        if (SceneManager.GetActiveScene().name == "Level_0")
        {
            Debug.Log("Cannot open portal in town!");
            return;
        }

        Player player = Player.instance;
        if (player == null || Object_Portal.instance == null)
            return;

        Vector3 portalPosition = player.transform.position + new Vector3(player.facingDir * 1.5f, 0);
        Object_Portal.instance.ActivatePortal(portalPosition);
    }
}
