using System;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Player : Entity
{

    public static Player instance;
    public static Action OnPlayerDeath;

    public UI ui;

    public PlayerInputSet input { get; private set; }
    public Player_SkillManager skillManager { get; private set; }
    public Player_VFX vfx { get; private set; }
    public Entity_Health health { get; private set; }
    public Entity_StatusHandler statusHandler { get; private set; }
    public Player_Combat combat { get; private set; }
    public Inventory_Player inventory { get; private set; }
    public Player_Stats stats { get; private set; }
    public Player_QuestManager questManager { get; private set; }

    #region State Variables
    public PlayerIdleState idleState { get; private set; }
    public Player_MoveState moveState { get; private set; }
    public Player_JumpState jumpState { get; private set; }
    public Player_FallState fallState { get; private set; }
    public Player_WallSlideState wallSlideState { get; private set; }
    public Player_WallJumpState wallJumpdeState { get; private set; }
    public Player_DashState dashState { get; private set; }
    public Player_BasicAttackState basicAttackState { get; private set; }
    public Player_JumpAttackState jumpAttackState { get; private set; }
    public Player_DeadState deadState { get; private set; }
    public Player_CounterAttackState counterAttackState { get; private set; }
    public Player_SwordThrowState swordThrowState { get; private set; }
    public Player_DomainExpansionState domainExpansionState { get; private set; }
    #endregion


    [Header("Cheat things")]
    public bool wallJumpINF = false;
    public bool dashINF = false;
    public float dashCoolDown = -0.1f;     //目前冷却时长
    public float dashLimitDuration = 0.25f;//总时长

    [Header("Ultimate ability details")]
    public float riseSpeed = 25;
    public float riseMaxDistance = 3;

    [Header("Attack details")]
    public Vector2[] attackVelocity;
    public Vector2 jumpAttackVelocity;
    public float attackVelocityDuration = 0.1f;
    public float comboResetTime = 1f;
    private Coroutine queueAttackCo; //写成一般都需要monobehavior,所以需要写在具有mono的类上

    [Header("Movement details")]
    public float moveSpeed;
    public float jumpForce = 5;
    public Vector2 wallJumpForce;
    [Range(0, 1)] public float inAirMoveMultiplier = 0.7f;
    [Range(0, 1)] public float wallSlideSlowMultiplier = 0.7f;
    [Space]
    public float dashDuration = 0.25f;
    public float dashSpeed = 20f;

    public Vector2 moveInput { get; private set; }
    public Vector2 mousePosition { get; private set; }


    protected override void Awake()
    {
        base.Awake();
        instance = this;

        ui = FindAnyObjectByType<UI>();
        skillManager = GetComponent<Player_SkillManager>();
        vfx = GetComponent<Player_VFX>();
        statusHandler = GetComponent<Entity_StatusHandler>();
        health = GetComponent<Entity_Health>();
        combat = GetComponent<Player_Combat>();
        inventory = GetComponent<Inventory_Player>();
        stats = GetComponent<Player_Stats>();
        questManager = GetComponent<Player_QuestManager>();

        input = new PlayerInputSet();
        ui.SetupControlsUI(input);

        idleState = new PlayerIdleState(this, stateMachine, "idle");
        moveState = new Player_MoveState(this, stateMachine, "move");
        jumpState = new Player_JumpState(this, stateMachine, "jumpFall");
        fallState = new Player_FallState(this, stateMachine, "jumpFall");
        wallSlideState = new Player_WallSlideState(this, stateMachine, "wallSlide");
        wallJumpdeState = new Player_WallJumpState(this, stateMachine, "jumpFall");
        dashState = new Player_DashState(this, stateMachine, "dash");
        basicAttackState = new Player_BasicAttackState(this, stateMachine, "basicAttack");
        jumpAttackState = new Player_JumpAttackState(this, stateMachine, "jumpAttack");
        deadState = new Player_DeadState(this, stateMachine, "dead");
        counterAttackState = new Player_CounterAttackState(this, stateMachine, "counterAttack");
        swordThrowState = new Player_SwordThrowState(this, stateMachine, "swordThrow");
        domainExpansionState = new Player_DomainExpansionState(this, stateMachine, "jumpFall");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public void TeleportPlayer(Vector3 position) => transform.position = position;

    protected override IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
    {
        float originalMoveSpeed = moveSpeed;
        float originalJumpForce = jumpForce;
        float originalAnimSpeed = anim.speed;
        Vector2 originalWallJump = wallJumpForce;
        Vector2 originalJumpAttack = jumpAttackVelocity;
        Vector2[] originalAttackVelocity = (Vector2[])attackVelocity.Clone(); //这里虽然是引用类型，会影响原来的值,但是好像效果上没影响。

        float speedMultiplier = 1 - slowMultiplier;

        moveSpeed = moveSpeed * speedMultiplier;
        jumpForce = jumpForce * speedMultiplier;
        anim.speed = anim.speed * speedMultiplier;
        wallJumpForce = wallJumpForce * speedMultiplier;
        jumpAttackVelocity = jumpAttackVelocity * speedMultiplier;

        for (int i = 0; i < attackVelocity.Length; i++)
        {
            attackVelocity[i] = attackVelocity[i] * speedMultiplier;

        }

        yield return new WaitForSeconds(duration);

        moveSpeed = originalMoveSpeed;
        jumpForce = originalJumpForce;
        anim.speed = originalAnimSpeed;
        wallJumpForce = originalWallJump;
        jumpAttackVelocity = originalJumpAttack;

        for (int i = 0; i < attackVelocity.Length; i++)
        {
            attackVelocity[i] = originalAttackVelocity[i];
        }
    }



    public override void EntityDeath()
    {
        base.EntityDeath();
        OnPlayerDeath?.Invoke();
        stateMachine.ChangeState(deadState);
    }


    public void EnterAttaclStateWithDelay()
    {
        if (queueAttackCo != null)
        {
            StopCoroutine(queueAttackCo);
        }

        queueAttackCo = StartCoroutine(EnterAttackStateWithDelayCo());
    }

    private IEnumerator EnterAttackStateWithDelayCo()
    {
        yield return new WaitForEndOfFrame();
        stateMachine.ChangeState(basicAttackState);
    }

    private void TryInteract()
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;
        Collider2D[] objectAround = Physics2D.OverlapCircleAll(transform.position, 1.5f);

        foreach (var target in objectAround)
        {
            IInteractable interactable = target.GetComponent<IInteractable>();

            if (interactable == null) continue;

            float distance = Vector2.Distance(transform.position, target.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = target.transform;
            }
        }

        if (closest == null) return;
        closest.GetComponent<IInteractable>().Interact();
    }


    private void OnEnable()
    {
        input.Enable();

        //input.Player.Movement.started - input just begun
        //input.Player.Movement.performed - imput is performed
        //input.Player.Movement.canceled - input stops , when you release the key 

        input.Player.Mouse.performed += ctx => mousePosition = ctx.ReadValue<Vector2>();

        input.Player.Movement.performed += context => moveInput = context.ReadValue<Vector2>();
        input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Spell.performed += ctx => skillManager.shard.TryUseSkill();
        input.Player.Spell.performed += ctx => skillManager.timeEcho.TryUseSkill();

       
        input.Player.Interact.performed += ctx => TryInteract();

        input.Player.QuickSlot_1.performed += ctx => inventory.TryUseQuickItemInSlot(1);
        input.Player.QuickSlot_2.performed += ctx => inventory.TryUseQuickItemInSlot(2);
    }

    private void OnDisable()
    {
        input.Disable();
    }


}
