using UnityEngine;

public class SkillObject_AnimationTriggers : MonoBehaviour
{

    private SkillObject_TimeEcho timeEcho;

    private void Awake()
    {
        timeEcho = GetComponentInParent<SkillObject_TimeEcho>();
    }

    private void AttackTrigger()
    {
        timeEcho.PerformAttack();
    }
    private void TryTerminate(int currentAtttackIndex)
    {
        if(currentAtttackIndex == timeEcho.maxAttacks)
        {
            timeEcho.HandleDeath();
        }
    }
}
