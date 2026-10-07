using System.Collections;
using UnityEngine;

public class Enemy_Slime : Enemy, ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_SlimeDeadState slimeDeadState { get; set; }

    [Header("Slime specifucs")]
    [SerializeField] private GameObject slimeToCreatePrefab;
    [SerializeField] private int amountOfSlimsToCreate = 2;
    [SerializeField] private Vector2 newSlimeVelocity;
    [SerializeField] private bool hasRecoveryAnimation = true;


    protected override void Awake()
    {
        base.Awake();
        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        attackState = new Enemy_AttackState(this, stateMachine, "attack");
        battleState = new Enemy_BattleState(this, stateMachine, "battle");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");
        slimeDeadState = new Enemy_SlimeDeadState(this, stateMachine, "idle");

        anim.SetBool("hasStunRecovery", hasRecoveryAnimation);
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);

    }

    public override void EntityDeath()
    {
        stateMachine.ChangeState(slimeDeadState);
    }



    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }

    public void CreateSlimeOnDeath()
    {
        if (slimeToCreatePrefab == null) return;

        float spawnRadius = 0.6f;

        for (int i = 0; i < amountOfSlimsToCreate; i++)
        {
            float angle = i * Mathf.PI * 2f / amountOfSlimsToCreate;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle),
                Mathf.Sin(angle),
                0
            ) * spawnRadius;

            Vector3 spawnPosition = transform.position + offset;

            GameObject newSlime = Instantiate(slimeToCreatePrefab, spawnPosition, Quaternion.identity);

            Enemy_Slime slimeScript = newSlime.GetComponent<Enemy_Slime>();
            slimeScript.stats.AdjustStatSetup(stats.resources, stats.offense, stats.defense, .6f, 1.2f);
            slimeScript.ApplyRespawnVelocity();
            slimeScript.StartCoroutine(slimeScript.StartBattleStateCheckCo(player));
        }

    }

    public void ApplyRespawnVelocity()
    {
        Vector2 velocity = new Vector2(newSlimeVelocity.x * Random.Range(-10f, 10f), newSlimeVelocity.y * Random.Range(1f, 10f));
        SetVelocity(velocity.x, velocity.y);
    }

    public void StartBattleStateCheck(Transform player)
    {

        Debug.Log("进入 StartBattleStateCheck");
        Debug.Log("传入的 player = " + player);

        TryEnterBattleState(player);

        Debug.Log("TryEnterBattleState 完成");

        InvokeRepeating(nameof(ReEnterBattleState), 0, .3f);

        Debug.Log("InvokeRepeating 完成");

    }

    public void SetupSlime(Vector2 velocity, Entity_Stats newStats)
    {
        rb.linearVelocity = new Vector2(velocity.x * Random.Range(-2, 2), velocity.y * Random.Range(1, 10));
        stats.AdjustStatSetup(stats.resources, stats.offense, stats.defense, 0.6f, 1.2f);
    }

    private void ReEnterBattleState()
    {
        if (stateMachine.currentSate == battleState || stateMachine.currentSate == attackState)
        {
            CancelInvoke(nameof(ReEnterBattleState));
            return;
        }

        stateMachine.ChangeState(battleState);
    }

    public IEnumerator StartBattleStateCheckCo(Transform player)
    {
        yield return null;

        StartBattleStateCheck(player);
    }

}
