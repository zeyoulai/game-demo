using System.Collections;
using UnityEngine;

public class Enemy_Mage : Enemy, ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_MageRetreatState mageRetreatState { get; private set; }
    public Enemy_MageBattleState mageBattleState { get; private set; }
    public Enemy_MageSpellCastState mageSpellCastState { get; private set; }

    [Header("Mage specifucs")]
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spellStartPosition;
    [SerializeField] private int amountToCast = 3;
    [SerializeField] private float spellCastCooldown = 0.1f;

    public bool spellCastPerformed { get; private set; }
    [Space]
    public float retreatCooldown = 2;
    public float retreatMaxDistance = 8;
    public float retreatSpeed = 15;
    [SerializeField] private Transform behindCollsionCheck;
    [SerializeField] private bool hasRecoveryAnimation = true;

    protected override void Awake()
    {
        base.Awake();
        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        attackState = new Enemy_AttackState(this, stateMachine, "attack");

        deadState = new Enemy_DeadState(this, stateMachine, "idle"); //参数不重要，因为不用具体动画
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");

        mageRetreatState = new Enemy_MageRetreatState(this, stateMachine, "battle");
        mageBattleState = new Enemy_MageBattleState(this, stateMachine, "battle");
        mageSpellCastState = new Enemy_MageSpellCastState(this, stateMachine, "spellCast");

        battleState = mageBattleState;

        anim.SetBool("hasStunRecovery", hasRecoveryAnimation);

    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);

    }
    public void SetSpellCastPerformed(bool perform) => spellCastPerformed = perform;

    public override void SpecialAttack()
    {

        StartCoroutine(CastSpellCo());

    }

    private IEnumerator CastSpellCo()
    {
        for (int i = 0; i < amountToCast; i++)
        {
            Enemy_MageProjectile projectile = Instantiate(spellPrefab, spellStartPosition.position, Quaternion.identity).GetComponent<Enemy_MageProjectile>();
            projectile.SetupProjectile(player.transform, combat);
            yield return new WaitForSeconds(spellCastCooldown);
        }
        SetSpellCastPerformed(true);


    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }

    public bool CanMoveBackwards()
    {
        bool detectWall = Physics2D.Raycast(behindCollsionCheck.position, Vector2.right * -facingDir, 1.5f, whatIsGround);
        bool noGround = Physics2D.Raycast(behindCollsionCheck.position, Vector2.down, 1.5f, whatIsGround) == false;

        return detectWall || noGround;

    }

    protected virtual void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.DrawLine(behindCollsionCheck.position,
            new Vector3(behindCollsionCheck.position.x + (1.5f * -facingDir), behindCollsionCheck.position.y));

        Gizmos.DrawLine(behindCollsionCheck.position,
            new Vector3(behindCollsionCheck.position.x , behindCollsionCheck.position.y-1.5f));

    }
}
