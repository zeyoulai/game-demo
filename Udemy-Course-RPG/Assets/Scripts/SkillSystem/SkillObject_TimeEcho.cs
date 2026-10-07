using UnityEngine;

public class SkillObject_TimeEcho : SkillObject_Base
{
    [SerializeField] private float wispMoveSpeed = 15;
    [SerializeField] private GameObject onDeathVfx;
    [SerializeField] private LayerMask whatIsGround;
    private bool shouldMoveToPlayer = false;

    private Transform playerTransform;
    private Skill_TimeEcho echoManager;
    private TrailRenderer wsipTrail;
    private SkillObject_Health echoHealth;
    private Entity_Health playerHealth;
    private Player_SkillManager skillManager;
    private Entity_StatusHandler statusHandler;

    [Header("Ëæ»úÇúÏß")]
    private float noiseSeed;


    private void Start()
    {
        noiseSeed = Random.Range(0f, 100f);
    }

    public int maxAttacks {  get; private set; }

    public void SetupEcho(Skill_TimeEcho echoManager)
    {
        this.echoManager = echoManager;
        playerStats = echoManager.player.stats;
        damageScaleData = echoManager.damageScaleData;
        maxAttacks = echoManager.GetMaxAttacks();
        playerTransform = echoManager.transform.root;
        playerHealth = echoManager.player.health;
        skillManager  =echoManager.skillManager;
        statusHandler = echoManager.player.statusHandler;
        Invoke(nameof(HandleDeath), echoManager.GetEchoDuration());
        FlipToTarget();

        echoHealth = GetComponent<SkillObject_Health>();
        wsipTrail = GetComponentInChildren<TrailRenderer>();
        wsipTrail.gameObject.SetActive(false);

        anim.SetBool("canAttack", maxAttacks > 0);

    }

    private void HandleWispMovement()
    {
        Vector2 finalDir = getCurveDir();

        transform.position += (Vector3)(finalDir * wispMoveSpeed * Time.deltaTime);


        if (Vector2.Distance(transform.position, playerTransform.position) < 0.2f)
        {
            HanldePlayerTouch();
            Destroy(gameObject);
        }
    }

    private Vector2 getCurveDir()
    {
        Vector2 dir = (playerTransform.position - transform.position).normalized;

        Vector2 perpendicular = new Vector2(-dir.y, dir.x);

        float noise = Mathf.PerlinNoise(Time.time * 2f, noiseSeed) - 0.5f;

        Vector2 finalDir = dir + perpendicular * noise * 2f;
        finalDir.Normalize();
        return finalDir;
    }

    private void HanldePlayerTouch()
    {
        float healAmount = echoHealth.lastDamageTaken * echoManager.GetPercentOfDamageHealed();
        playerHealth.IncreaseHealth(healAmount);

        float amountInSeconds = echoManager.GetCooldownReducedInSeconds();
        skillManager.ReduceAllSkillCooldownBy(amountInSeconds);

        if (echoManager.CanRemoveNegativeEffects())
            statusHandler.RemoveAllNegativeEEfects();

    }

    private void Update()
    {
        if(shouldMoveToPlayer)
            HandleWispMovement();
        else
        {
            anim.SetFloat("yVelocity", rb.linearVelocityY);
            StopHorizontalMovement();

        }


    }

    private void FlipToTarget()
    {
        Transform target = FindClosestTarget();
        if (target != null && target.position.x < transform.position.x)
            transform.Rotate(0, 180, 0);
    }

    public void PerformAttack()
    {
        DamageEnemiesInRadius(targetCheck, 1);

        if(targetGotHit == false)
            return;

        bool canDuplicate = Random.value < echoManager.GetDuplicateChance();
        float xOffset = transform.position.x < lastTarget.position.x ? 1 : -1;

        if(canDuplicate )
            echoManager.CreateTimeEcho(lastTarget.position + new Vector3(xOffset, 0, 0));
    }

    public void HandleDeath()
    {
        Instantiate(onDeathVfx, transform.position, Quaternion.identity);

        if (echoManager.ShouldBeWisp())
        {
            TurnToWisp();
        }
        else
            Destroy(gameObject);
    }

    private void TurnToWisp()
    {
        shouldMoveToPlayer = true;
        anim.gameObject.SetActive(false);
        wsipTrail.gameObject.SetActive(true);
        rb.simulated = false;
    }

    private void StopHorizontalMovement()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f, whatIsGround);
        if(hit.collider != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocityY);
        }
    }
}
