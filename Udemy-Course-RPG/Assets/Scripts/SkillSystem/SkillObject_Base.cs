using UnityEngine;

public class SkillObject_Base : MonoBehaviour
{
    [SerializeField] private GameObject onHitVfx;
    [Space]

    [SerializeField] protected LayerMask whatIsEnemy;
    [SerializeField] protected Transform targetCheck;
    [SerializeField] protected float checkRadius = 1;

    protected Rigidbody2D rb;
    protected Animator anim;
    [Header("Shard element")]
    [SerializeField] protected Entity_Stats playerStats;
    [SerializeField] protected DamageScaleData damageScaleData;
    protected ElementType usedElement;

    protected bool targetGotHit;
    protected Transform lastTarget;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        Debug.Log("Awake" + damageScaleData.burnDuration);

    }

    private void Start()
    {
        Debug.Log("Start" + damageScaleData.burnDuration);
    }


    protected void DamageEnemiesInRadius(Transform t,float radius)
    {
        foreach (var target in GetEnemiesAround(t,radius))
        {

            IDamagable damagable = target.GetComponent<IDamagable>();
            if (damagable == null)
                continue;
 

            AttackData attackData = playerStats.GetAttackData(damageScaleData);
            Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();

            float phsiDamage = playerStats.GetPhyiscalDamage(out bool isCrit,damageScaleData.phsycal);
            float elemDamage = playerStats.GetElementDamage(out ElementType element, damageScaleData.elemental);

            targetGotHit = damagable.TakeDamage(phsiDamage, elemDamage, element, transform);

            if (element != ElementType.None)
                statusHandler.ApplyStausEffect(element, attackData.effectData);
            if (targetGotHit)
            {
                lastTarget = target.transform;
                Instantiate(onHitVfx, target.transform.position, Quaternion.identity);
            }

            usedElement = element;
        }
    }

    protected Transform FindClosestTarget()
    {
        Transform target = null;
        float closestDistance = Mathf.Infinity;

        foreach (var enemy in GetEnemiesAround(transform,10))  
        {
            float distance = Vector2.Distance(transform.position,enemy.transform.position);
            if (distance < closestDistance)
            {
                target = enemy.transform;
                closestDistance = distance;
            }
        }

        return target;
    }

    protected Collider2D[] GetEnemiesAround(Transform t,float  radius)
    {
        return Physics2D.OverlapCircleAll(t.position,radius,whatIsEnemy);
    }

    protected virtual void OnDrawGizmos()
    {
        if (targetCheck == null)
        {
            targetCheck = transform;
        }

        Gizmos.DrawWireSphere(targetCheck.position, checkRadius);
    }
}
