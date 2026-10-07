using System.Collections;
using UnityEngine;

public class Enemy_Reaper : Enemy, ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_ReaperAttackState reaperAttackState { get; private set; }
    public Enemy_ReaperBattleState reaperBattleState { get; private set; }
    public Enemy_ReaperTeleportState reaperTeleportState { get; private set; }
    public Enemy_ReaperSpellCastState reaperSpellCastState { get;private set; }

    [Header("Reaper specifics")]
    [SerializeField] public float maxBattleIdelTime = 5f;

    [Header("Reaper Spellcast")]
    [SerializeField] private DamageScaleData spellScaleData;
    [SerializeField] private GameObject spellCastPrefab;
    [SerializeField] private int amountToCast = 6;
    [SerializeField] private float spellCastRate = 1.2f;
    [SerializeField] private float spellCastStateCooldown = 10;
    [SerializeField] private Vector2 playerOffsetPrediction;
    private float lastTimeCastedSpell = float.NegativeInfinity;
    public bool spellCastPerformed { get; private set; }
    private Player playerScript;

    [Header("Reaper Teleport")]
    [SerializeField] private BoxCollider2D arenaBounds;
    [SerializeField] private float offsetCenterY = 2.97f;
    [SerializeField] private float chanceToTeleport = 0.3f;
    private float defaultTeleportChance;

    public bool teleportTrigger {  get; private set; }

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        deadState = new Enemy_DeadState(this, stateMachine, "idle");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");

        reaperBattleState = new Enemy_ReaperBattleState(this, stateMachine, "battle");
        reaperAttackState = new Enemy_ReaperAttackState(this, stateMachine, "attack");
        reaperTeleportState = new Enemy_ReaperTeleportState(this, stateMachine, "teleport");
        reaperSpellCastState = new Enemy_ReaperSpellCastState(this, stateMachine, "spellCast");

        battleState = reaperBattleState;


    }

    public bool GetSpellCastPerformed() => this.spellCastPerformed;
    public void SetSpellCastPerformed(bool spellCastStatus) => spellCastPerformed = spellCastStatus;
    public bool CanDoSpellCast() => Time.time > lastTimeCastedSpell + spellCastStateCooldown;
    public void SetSpellCastOnCooldown()=>lastTimeCastedSpell = Time.time;
    public override void SpecialAttack()
    {

        StartCoroutine(CastSpellCo());

    }

    private IEnumerator CastSpellCo()
    {
        if (playerScript == null)
            playerScript = player.GetComponent<Player>();
        for (int i = 0; i < amountToCast; i++)
        {
            bool playerMoving = playerScript.rb.linearVelocity.magnitude > 0;
            float xOffset = playerMoving ? playerOffsetPrediction.x * playerScript.facingDir : 0;
            Vector3 spellPosition = player.transform.position +new Vector3( xOffset,playerOffsetPrediction.y);


            Enemy_ReaperSpell spell = Instantiate(spellCastPrefab, spellPosition, Quaternion.identity)
                .GetComponent<Enemy_ReaperSpell>();

            spell.SetupSpell(combat,spellScaleData);

            yield return new WaitForSeconds(spellCastRate);
        }
        SetSpellCastPerformed(true);
        Debug.Log("…Ë÷√performedŒ™true");


    }

    public override void TryEnterBattleState(Transform player)
    {
        if (stateMachine.currentSate == battleState)
            return;

        if (stateMachine.currentSate == attackState)
            return;
        if(stateMachine.currentSate == reaperSpellCastState)
            return;

        this.player = player;
        stateMachine.ChangeState(battleState);
    }


    public bool ShouldTeleport()
    {
        if (Random.value < chanceToTeleport)
        {
            chanceToTeleport = defaultTeleportChance;
            return true;
        }
        chanceToTeleport += 0.05f;
        return false;
    }

    public void SetTeleportTrigger(bool trigger) => this.teleportTrigger = trigger;

    protected override void Start()
    {
        base.Start();

        arenaBounds.transform.parent = null;
        defaultTeleportChance = chanceToTeleport;

        stateMachine.Initialize(idleState);
    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }

    public Vector3 FindTeleportPoint()
    {
        int maxAttampts = 10;
        float bossWithColliderHalf = col.bounds.size.x / 2 + 0.5f;
        for (int i = 0; i < maxAttampts; i++)
        {
            float randomX = Random.Range(arenaBounds.bounds.min.x + bossWithColliderHalf, arenaBounds.bounds.max.x - bossWithColliderHalf);

            Vector2 raycastPoint = new Vector2(randomX, arenaBounds.bounds.max.y);
            RaycastHit2D hit = Physics2D.Raycast(raycastPoint, Vector2.down, Mathf.Infinity, whatIsGround);

            if (hit.collider != null)
                return hit.point + new Vector2(0, offsetCenterY);
        }

        return transform.position;
    }
}
