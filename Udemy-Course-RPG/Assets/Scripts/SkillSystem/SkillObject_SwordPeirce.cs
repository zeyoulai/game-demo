using UnityEngine;

public class SkillObject_SwordPeirce : SkillObject_Sword
{
    private int amountToPeirce;
    public override void SetupSword(Skill_SwordThrow swordManager, Vector2 direction)
    {
        base.SetupSword(swordManager, direction);
        amountToPeirce = swordManager.amountToPeirce;
        Debug.Log(direction);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        bool groundHit = collision.gameObject.layer == LayerMask.NameToLayer("Ground");
        if (amountToPeirce <= 0  || groundHit)
        {
            DamageEnemiesInRadius(transform, .3f);//半径过大可能会导致撞到多个被集中在一起的敌人的时候多次扣血
            StopSword(collision);
            return;
        }
        amountToPeirce--;
        DamageEnemiesInRadius(transform, .3f);

    }
}
