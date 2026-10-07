using UnityEngine;

public class SkillObject_Health : Entity_Health
{
    public override bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        return base.TakeDamage(damage, elementalDamage, element, damageDealer);
    }
    protected override void Die()
    {
        SkillObject_TimeEcho timeEcho = GetComponent<SkillObject_TimeEcho>();
        timeEcho.HandleDeath();
    }

}
