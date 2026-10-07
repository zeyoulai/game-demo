using System;
using UnityEngine;

public class Entity_Combat : MonoBehaviour
{
    public event Action<float> OnDoingPhysicalDamage;
    private Entity_SFX sfx;
    private Entity_VFX vfx;
    private Entity_Stats stats;

    public DamageScaleData basicAttackScale;

    [Header("Target detection")]
    [SerializeField] private Transform targetCheck;
    [SerializeField] private float targetCheckRadius;
    [SerializeField] private LayerMask whatIsTarget;


    private void Awake()
    {
        vfx = GetComponent<Entity_VFX>();
        sfx = GetComponent<Entity_SFX>();
        stats = GetComponent<Entity_Stats>();
    }

    public void PerformAttack()
    {

        bool hitAnyTarget = false;

        foreach (var target in GetDectedColliders(whatIsTarget))
        {


            IDamagable damagable = target.GetComponentInParent<IDamagable>();



            if (damagable == null)
            {

                continue;
            }

            AttackData attackData = stats.GetAttackData(basicAttackScale);
            Entity_StatusHandler statusHandler = target.GetComponentInParent<Entity_StatusHandler>();



            bool targetGotHit = damagable.TakeDamage(
                attackData.physicalDamage,
                attackData.elementDamage,
                attackData.element,
                transform
            );



            if (attackData.element != ElementType.None)
                statusHandler?.ApplyStausEffect(attackData.element, attackData.effectData);

            if (targetGotHit)
            {
                hitAnyTarget = true;

                OnDoingPhysicalDamage?.Invoke(attackData.physicalDamage);

                vfx.CreateOnHitVFX(target.transform, attackData.isCrit, attackData.element);

                sfx?.PlayAttackHit();

            }
        }

        if (hitAnyTarget == false)
        {
            sfx?.PlayAttackMiss();
        }

        //bool targetGotHit = false;

        //foreach (var target in GetDectedColliders())
        //{
        //    IDamagable damagable = target.GetComponent<IDamagable>();


        //    Debug.Log(
        //        $"检测到 collider: {target.name}, " +
        //        $"collider对象ID: {target.gameObject.GetInstanceID()}, " +
        //        $"damagable: {damagable}, " +
        //        $"root: {target.transform.root.name}"
        //    );

        //    if (damagable == null)
        //        continue;

        //    AttackData attackData = stats.GetAttackData(basicAttackScale);
        //    Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();

        //    float physicalDamage = attackData.physicalDamage;
        //    float elementalDamage = attackData.elementDamage;
        //    ElementType element = attackData.element;//声明element的同时传入到函数里面
        //    bool isCrit = attackData.isCrit;

        //    targetGotHit = damagable.TakeDamage(physicalDamage, elementalDamage, element, transform);


        //    if (element != ElementType.None)
        //        statusHandler?.ApplyStausEffect(element,attackData.effectData);
        //    //    ApplyStatusEffect(target.transform, element);

        //    if (targetGotHit)
        //    {
        //        OnDoingPhysicalDamage?.Invoke(physicalDamage);
        //        vfx.CreateOnHitVFX(target.transform, isCrit, element);
        //        sfx?.PlayAttackHit();
        //    }

        //}
        //if(targetGotHit == false)
        //{
        //    sfx?.PlayAttackMiss();
        //}

    }

    public void PerformAttackOnTarget(Transform target,DamageScaleData damageScaleData = null)
    {
        bool targetGotHit = false;

        IDamagable damageable = target.GetComponent<IDamagable>();

        if (damageable == null)
            return; // skip target, go to next target
        DamageScaleData damageScale = damageScaleData == null ? basicAttackScale : damageScaleData;
        AttackData attackData = stats.GetAttackData(basicAttackScale);
        Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();

        float physicalDamage = attackData.physicalDamage;
        float elementalDamage = attackData.elementDamage;
        ElementType element = attackData.element;

        targetGotHit = damageable.TakeDamage(physicalDamage, elementalDamage, element, transform);

        if (element != ElementType.None)
            statusHandler?.ApplyStausEffect(element, attackData.effectData);

        if (targetGotHit)
        {
            OnDoingPhysicalDamage?.Invoke(physicalDamage);
            vfx.CreateOnHitVFX(target.transform, attackData.isCrit, element);
            //sfx?.PlayAttackHit();
        }

        if (targetGotHit == false)
        {

            //sfx?.PlayAttackMiss();
        }
    }



    protected Collider2D[] GetDectedColliders(LayerMask whatToDetect)
    {
        //return Physics2D.OverlapCircleAll(targetCheck.position, targetCheckRadius, whatIsTarget);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
    targetCheck.position,
    targetCheckRadius,
    whatToDetect
);



        //foreach (var col in colliders)
        //{
        //    Debug.Log(
        //        $"检测到: {col.name}, " +
        //        $"位置: {col.transform.position}, " +
        //        $"Layer: {LayerMask.LayerToName(col.gameObject.layer)}, " +
        //        $"enabled: {col.enabled}"
        //    );
        //}

        return colliders;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(targetCheck.position, targetCheckRadius);
    }

}
