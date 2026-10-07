using UnityEngine;

public class Enemy_VFX : Entity_VFX
{
    [Header("Counter Attack Window")]
    [SerializeField] private GameObject attackAlert;
    private Enemy_Health health;

    protected override void Awake()
    {
        base.Awake();
        health = GetComponent<Enemy_Health>();
    }

    public void EnableAttackAlert(bool enable)
    {
        if(attackAlert == null)
            return;
        attackAlert.SetActive(enable);
    }

    private void Update()//不然敌人在危的时候死去头上感叹号不会掉
    {
        if (health == null) return;
        else if (health.isDead) ;
            //attackAlert?.SetActive(false);
    }



}
