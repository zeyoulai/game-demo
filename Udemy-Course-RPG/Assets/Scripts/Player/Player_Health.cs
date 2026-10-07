using UnityEngine;

public class Player_Health : Entity_Health, ISaveable
{
    private Player player;


    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Die();
        }
    }

    protected override void Die()
    {
        base.Die();
        //GameManager.instance.SetLastPlayerPosition(transform.position);
        //GameManager.instance.ReStartGame();
        player.ui.OpenDeathScreenUI();
    }

    public void LoadData(GameData data)
    {
        if (gameObject.activeInHierarchy == false)
            return;

        if (data.hasPlayerHealthData == false)
            return;

        isDead = false;

        if (data.playerCurrentHealth > 0)
            SetCurrentHealth(data.playerCurrentHealth);
        else
            SetHealthToPercent(data.playerHealthPercent);
    }

    public void SaveData(ref GameData data)
    {
        if (gameObject.activeInHierarchy == false)
            return;

        data.hasPlayerHealthData = true;
        data.playerCurrentHealth = GetCurrentHealth();
        data.playerHealthPercent = Mathf.Clamp01(GetHealthPercent());
    }
}
