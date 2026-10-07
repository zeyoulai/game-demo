using System;
using UnityEngine;
using UnityEngine.UI;
public class Entity_Health : MonoBehaviour, IDamagable
{
    public event Action OnTakingDamage;
    public event Action OnHealthUpdate;

    private Slider healthBar;
    private Entity_VFX entityVfx;
    private Entity entity;
    private Entity_Stats entityStats;
    private Entity_DropManager dropManager;

    private bool miniHealthBarActive;
    private bool healthInitialized;

    [SerializeField] protected float currentHealth;
    [SerializeField] public bool isDead;

    [Header("Health Regen")]
    [SerializeField] private float regenInterval = 1;
    [SerializeField] private bool canRegenerateHealth = true;
    public float lastDamageTaken {  get; private set; }

    protected bool canTakenDamage = true;


    [Header("On Damage Knockback")]
    [SerializeField] private Vector2 knockbackPower = new Vector2(1.5f, 2.5f);
    [SerializeField] private Vector2 heavyKnockbackPower = new Vector2(7f, 7f);
    [SerializeField] private float knockbackDuration = .2f;
    [SerializeField] private float heavyKnockbackDuration = .5f;
    [Header("On Heavy Damage")]
    [SerializeField] private float heavyDamageThreshold = .3f; // percentage of health you should lose to consider damage sa heavy


    protected virtual void Awake()
    {
        EnsureComponents();
    }

    protected void EnsureComponents()
    {
        if (entityVfx == null)
            entityVfx = GetComponent<Entity_VFX>();

        if (entity == null)
            entity = GetComponent<Entity>();

        if (healthBar == null)
            healthBar = GetComponentInChildren<Slider>(true);

        if (entityStats == null)
            entityStats = GetComponent<Entity_Stats>();

        if (dropManager == null)
            dropManager = GetComponent<Entity_DropManager>();
    }

    protected virtual void Start()
    {
        SetupHealth();
        
    }

    private void SetupHealth()
    {
        EnsureComponents();

        if (entityStats == null)
            return;
        if (healthInitialized == false)
        {
            currentHealth = entityStats.GetMaxHealth();
            healthInitialized = true;
        }
        OnHealthUpdate -= UpdataHealthBar;
        OnHealthUpdate += UpdataHealthBar;
        UpdataHealthBar();
        CancelInvoke(nameof(RegenerateHealth));
        InvokeRepeating(nameof(RegenerateHealth), regenInterval, regenInterval);//呼吸回血
    }

    public virtual bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        EnsureComponents();

        if (isDead || canTakenDamage == false) return false;

        if (AttackEvaded())
        {
            Debug.Log($"{gameObject.name} evaded the attack!");
            return false;
        }

        Entity_Stats attackStates = damageDealer.GetComponent<Entity_Stats>();
        float armorReduction = attackStates != null ? attackStates.GetArmorReduction() : 0;

        float mitigation = entityStats != null ? entityStats.GetArmorMitigation(armorReduction):0;
        float resistance = entityStats != null ? entityStats.GetElementalResistance(element):0;

        float physicalDamageTaken = damage * (1 - mitigation);
        float elementalDamageTaken = elementalDamage * (1 - resistance);


        TakeKnockback(damageDealer, physicalDamageTaken);
        ReduceHealth(physicalDamageTaken + elementalDamageTaken);
        lastDamageTaken = physicalDamageTaken + elementalDamageTaken;

        OnTakingDamage?.Invoke();

        return true;
    }

    public void SetCanTakenDamage(bool canTakenDamage) =>this.canTakenDamage = canTakenDamage;

    private bool AttackEvaded()
    {
        EnsureComponents();

        if(entityStats == null) 
            return false;
        else
            return UnityEngine.Random.Range(0, 100) < entityStats.GetEvasion();

    }
    private void RegenerateHealth()
    {
        EnsureComponents();

        if (!canRegenerateHealth)
            return;
        if (entityStats == null)
            return;

        float regenAmount = entityStats.resources.healthRegen.GetValue();
        IncreaseHealth(regenAmount);
    }


    public void IncreaseHealth(float healAmount)
    {
        EnsureComponents();

        if (isDead)
            return;
        if (entityStats == null)
            return;

        float newHealth = currentHealth + healAmount;
        float maxHealth = entityStats.GetMaxHealth();

        currentHealth = Mathf.Min(newHealth, maxHealth);
        OnHealthUpdate?.Invoke();
        
    }


    public void ReduceHealth(float damage)
    {
        EnsureComponents();

        currentHealth -= damage;

        entityVfx?.PlayOnDamageVFX();
        OnHealthUpdate?.Invoke();


        if (currentHealth <= 0) Die();
    }

    public float GetHealthPercent()
    {
        EnsureComponents();

        if (entityStats == null || entityStats.GetMaxHealth() <= 0)
            return 1;

        return currentHealth / entityStats.GetMaxHealth();
    }

    public void SetHealthToPercent(float percent)
    {
        EnsureComponents();

        if (entityStats == null)
            return;

        currentHealth = entityStats.GetMaxHealth() * Mathf.Clamp01(percent);
        healthInitialized = true;
        OnHealthUpdate?.Invoke();
    }

    public void SetCurrentHealth(float health)
    {
        EnsureComponents();

        if (entityStats == null)
            return;

        currentHealth = Mathf.Clamp(health, 0, entityStats.GetMaxHealth());
        healthInitialized = true;
        OnHealthUpdate?.Invoke();
    }

    protected virtual void Die()
    {
        EnsureComponents();

        isDead = true;
        entity?.EntityDeath();
        dropManager?.DropItems();
    }

    public float GetCurrentHealth()=>currentHealth;

    private void UpdataHealthBar()
    {
        EnsureComponents();

        if (healthBar == null || entityStats == null)
            return;

        if (healthBar.transform.parent.gameObject.activeSelf == false)
            return;
        healthBar.value = currentHealth / entityStats.GetMaxHealth();

    }

    public void EnableHealthBar(bool enable)=>healthBar?.transform.parent.gameObject.SetActive(enable);

    private void TakeKnockback(Transform damageDealer, float finalDamage)
    {
        EnsureComponents();

        Vector2 knockback = CalculateKnockback(finalDamage, damageDealer);

        float duration = CalculateDuration(finalDamage);

        if(entity == null)
            return ;//技能echo其实没有这个entity脚本
        entity.ReciveKnockback(knockback, duration);

    }

    private Vector2 CalculateKnockback(float damage, Transform damageDealer)
    {
        int direction = transform.position.x > damageDealer.position.x ? 1 : -1;

        Vector2 knockback = isHeavyDamage(damage) ? heavyKnockbackPower : knockbackPower;
        knockback.x = knockback.x * direction;
        return knockback;
    }

    private float CalculateDuration(float damage) => isHeavyDamage(damage) ? heavyKnockbackDuration : knockbackDuration;

    private bool isHeavyDamage(float damage)
    {
        EnsureComponents();

        if(entityStats == null)
            return false;
        else
            return damage / entityStats.GetMaxHealth() > heavyDamageThreshold;

    }
}
