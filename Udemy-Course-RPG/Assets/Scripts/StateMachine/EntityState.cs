using UnityEngine;

public abstract class EntityState 
{
    protected StateMachine stateMachine;
    protected string animBoolName;

    protected Animator anim;
    protected Rigidbody2D rb;
    protected Entity_Stats stats;

    protected float stateTimer;
    protected bool triggerCalled;

    public EntityState(StateMachine stateMachine,string animBoolName)
    {
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }

    public virtual void Enter()
    {
        // everytime state will be changed , enter will be called
        anim.SetBool(animBoolName, true);
        triggerCalled = false;

    }

    public virtual void Update()
    {
        // we going to run logic of the state here
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();

    }

    public virtual void Exit()
    {
        // this will be called ,everytime we exit state and chage to a new one
        anim.SetBool(animBoolName, false);
    }

    public void AnimationTrigger()
    {
        triggerCalled = true;
    }

    public virtual void UpdateAnimationParameters()
    {

    }

    public void SyncAttackSpeed()
    {
        float attackSpeed = stats.offense.attackSpeed.GetValue();
        anim.SetFloat("attackSpeedMultiplier", attackSpeed);
    }
}
