using UnityEngine;

public class Player_DomainExpansionState : PlayerState
{
    private Vector2 originalPosition;
    private float originalGravity;
    private float maxDistanceToGoUp;

    private bool isLevitating;
    private bool createdDomain;

    public Player_DomainExpansionState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        originalGravity = rb.gravityScale;
        originalPosition = player.transform.position;
        maxDistanceToGoUp = GetAvaliableRiseDistance();

        player.SetVelocity(0, player.riseSpeed);
        player.health.SetCanTakenDamage(false);
    }

    public override void Update()
    {
        base.Update();

        if (Vector2.Distance(originalPosition, player.transform.position) >= maxDistanceToGoUp && isLevitating == false)
            Levitate();

        if (isLevitating)
        {
            //skill namage cast spells
            skillManager.domainExpansion.DoSpellCasting();

            if(stateTimer < 0)
            {
                rb.gravityScale = originalGravity;
                isLevitating = false;
                stateMachine.ChangeState(player.idleState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        createdDomain = false;
        player.health.SetCanTakenDamage(true);
    }

    private void Levitate()
    {
        isLevitating = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;


        stateTimer = skillManager.domainExpansion.GetDomainDuration();
        //

        if(createdDomain == false)
        {
            createdDomain = true;
            skillManager.domainExpansion.CreateDomain();
        }
    }

    private float GetAvaliableRiseDistance()
    {
        RaycastHit2D hit = Physics2D.Raycast(player.transform.position,Vector2.up,player.riseMaxDistance,player.whatIsGround);
        return hit.collider != null ?hit.distance - 1:player.riseMaxDistance;
    }
}
