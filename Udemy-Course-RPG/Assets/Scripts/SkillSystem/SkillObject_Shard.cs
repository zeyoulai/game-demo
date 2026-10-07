using System;
using UnityEngine;

public class SkillObject_Shard : SkillObject_Base
{
    public event Action OnExplode;
    private Skill_Shard shardManager;
    [SerializeField] private GameObject vfxPrefab;

    private Transform target;
    private float speed;

    private void Awake()
    {
        Debug.Log("shard 减速:" + damageScaleData.chillSlowMultiplier);
    }

    private void Update()
    {
        MoveTowardsClosestTarget(speed); //这样的话即使出来的时候没有移动后续也会移动，并且得放前面
        if (target == null)
            return;

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }

    public void MoveTowardsClosestTarget(float speed,Transform newTarget = null)
    {
        target = newTarget == null ? FindClosestTarget():newTarget;
        this.speed = speed;
    }

    public void SetupShard(Skill_Shard shardManager)
    {

        this.shardManager = shardManager;

        playerStats = shardManager.player.stats;
        damageScaleData = shardManager.damageScaleData;

        float detonationTime = shardManager.GetDetonateTime();
        Invoke(nameof(Explode), detonationTime);
    }

    public void SetupShard(Skill_Shard shardManager,float detonationTime,bool canMove,float shardSpeed,Transform target = null)
    {
        this.shardManager = shardManager;

        playerStats = shardManager.player.stats;
        damageScaleData = shardManager.damageScaleData;
        Invoke(nameof(Explode), detonationTime);
        if (canMove)
            MoveTowardsClosestTarget(shardSpeed,target);
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Enemy>() == null)
        {
            return;
        }
        Explode();

    }

    public void Explode()
    {
        DamageEnemiesInRadius(transform, checkRadius);
        GameObject vfx = Instantiate(vfxPrefab, transform.position, Quaternion.identity);
        vfx.GetComponentInChildren<SpriteRenderer>().color = shardManager.player.vfx.GetElementColor(usedElement);
        OnExplode?.Invoke();
        Destroy(gameObject);
    }
}
