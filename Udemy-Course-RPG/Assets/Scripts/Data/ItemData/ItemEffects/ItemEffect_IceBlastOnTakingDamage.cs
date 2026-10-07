using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item effect/ Ice blast", fileName = "Item effect data - Ice blast on taking damage")]
public class ItemEffect_IceBlastOnTakingDamage : ItemEffect_DataSO
{
    [SerializeField] private ElementalEffectData effectData;
    [SerializeField] private float iceDamage;
    [SerializeField] private LayerMask whatIsEnemy;
    [Space]
    [SerializeField] private float healPercentTrigger = 0.25f;
    [SerializeField] private float cooldown ;
    [SerializeField]
    private float lastTimeUsed = -999;
    [Header("Vfx Object")]
    [SerializeField] private GameObject iceBlastVfx;
    [SerializeField] private GameObject onHitVfx;

    private void OnEnable()
    {
        lastTimeUsed = -999;
    }

    

    public override void ExecuteEffect()
    {
        if (player == null || player.health == null || player.vfx == null)
            return;

        bool noCooldown = Time.time >= lastTimeUsed + cooldown;
        bool reachedThreshold = player.health.GetHealthPercent() <= healPercentTrigger;

        Debug.Log("noCooldown:"+ lastTimeUsed);
        Debug.Log("reachedThreshold:" + player.health.GetHealthPercent());
        if (noCooldown && reachedThreshold)
        {
            player.vfx.CreateEffectOf(iceBlastVfx, player.transform);
            lastTimeUsed = Time.time;
            DamageEnemiesWithIce();
        }
    }
    private void DamageEnemiesWithIce()
    {
        if (player == null)
            return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(player.transform.position, 1.5f, whatIsEnemy);
        foreach (var target in enemies)
        {
            IDamagable damagable = target.GetComponent<IDamagable>();

            if (damagable == null) continue;

            bool targetGoHit = damagable.TakeDamage(0,iceDamage,ElementType.Ice,player.transform);
            Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();
            statusHandler?.ApplyStausEffect(ElementType.Ice, effectData);

            if (targetGoHit)
            {
                player.vfx.CreateEffectOf(onHitVfx, target.transform);
            }
        }
    }

    public override void Subscribe(Player player)
    {
        if (player == null || player.health == null)
            return;

        base.Subscribe(player);
        player.health.OnTakingDamage += ExecuteEffect;
        
    }

    public override void Unsubscribe()
    {
        if (player == null)
            return;

        if (player.health == null)
        {
            player = null;
            return;
        }

        base.Unsubscribe();
        player.health.OnTakingDamage -= ExecuteEffect;
        player = null;
    }
}
