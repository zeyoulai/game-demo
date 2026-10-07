using UnityEngine;

public class Enemy_ArcherElf : Enemy
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_ArcherElfBattleState elfBattleState {  get; set; }

    [Header("Archer Elf specifics")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowStartPoint;
    [SerializeField] private float arrowSpeed = 8.0f;

    protected override void Awake()
    {
        base.Awake();
        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        attackState = new Enemy_AttackState(this, stateMachine, "attack");
        deadState = new Enemy_DeadState(this, stateMachine, "idle"); //参数不重要，因为不用具体动画
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");

        elfBattleState = new Enemy_ArcherElfBattleState(this, stateMachine, "battle");
        battleState = elfBattleState;
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState); 

    }

    public override void SpecialAttack()
    {
        GameObject newArrow = Instantiate(arrowPrefab,arrowStartPoint.position,Quaternion.identity);
        newArrow.GetComponent<Enemy_ArcherElfArrow>().SetupArrow(arrowSpeed * facingDir,combat); 
    }



    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }
}
