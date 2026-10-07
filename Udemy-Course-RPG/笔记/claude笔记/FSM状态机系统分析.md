# FSM 有限状态机系统 — 深度分析

## 一、什么是 FSM，以及为什么动作游戏需要它

### 1.1 问题的起源

想象你在做一个横版动作游戏的 Player 角色。它需要能：

- 站在地上不动（Idle）
- 左右跑动（Move）
- 跳跃，并且在空中可以左右控制方向（Jump / Air）
- 下落，落到地面时自动切回 Idle（Fall）
- 蹭墙滑行（WallSlide）
- 蹬墙跳（WallJump）
- 冲刺，冲刺期间无敌且不能被击退（Dash）
- 普攻，普攻期间不能移动、不能跳跃（BasicAttack）
- 跳劈，一个从空中向下砸的攻击（JumpAttack）
- 防反，格挡敌人攻击并反击（CounterAttack）
- 死亡（Dead）

这些行为之间有严格的互斥关系：**你不能在普攻挥刀的同时起跳，你不能在死亡的同时冲刺，你不能一边蹭墙下滑一边蹲下（当然这个游戏没有蹲下）**。

### 1.2 不用状态机的反面教材

```csharp
// 反面案例：用 if-else 和 bool 标志位管理所有行为
public class BadPlayer : MonoBehaviour
{
    private bool isGrounded;
    private bool isJumping;
    private bool isAttacking;
    private bool isDashing;
    private bool isDead;
    private bool isWallSliding;
    private bool isCounterAttacking;
    // ... 随着功能增加，bool 越来越多

    void Update()
    {
        if (isDead) return;

        if (isAttacking)
        {
            // 攻击逻辑...
            if (attackAnimationFinished) { isAttacking = false; }
            return; // 攻击期间什么都不能做
        }

        if (isDashing)
        {
            // 冲刺逻辑...
            return;
        }

        if (isCounterAttacking)
        {
            // 防反逻辑...
            return;
        }

        if (isWallSliding)
        {
            // 蹭墙逻辑...
            if (Input.GetKeyDown(KeyCode.Space)) { /* 蹬墙跳 */ }
            if (!wallDetected) { isWallSliding = false; /* 进入下落 */ }
            return;
        }

        // ... 每加一个状态，就要在所有相关地方加判断
        // 很快这段代码就变得像意大利面条
    }
}
```

**这种写法的灾难性后果：**

1. **每个新功能都要修改主 Update**：你无法在不碰现有代码的情况下加新状态
2. **状态互斥靠手动 return**：忘写一个 `return` 就出 bug
3. **过渡条件散落各处**：无法一眼看出"从 A 状态在什么条件下会切换到 B 状态"
4. **bool 变量暴涨**：5 个状态就需要好几个 bool，20 个状态时你根本记不住它们之间的互斥关系
5. **无法复用**：Player 写一套，Enemy 再写一套，两套代码 90% 相似但无法共享

### 1.3 状态机解决的核心问题

状态机把一个对象的**行为**建模为一组离散的**状态（State）**，每个状态封装：

- **进入时做什么**（Enter）
- **每帧做什么**（Update）
- **在什么条件下切换到哪个状态**（过渡逻辑）
- **退出时做什么**（Exit）

这样，你每加一个新行为，只需要新建一个状态类——**不碰已有代码**。

---

## 二、整体类图

```
┌─────────────────────────────────────────┐
│           StateMachine                   │
│  ─────────────────────────────           │
│  + currentState : EntityState            │
│  + canChangeState : bool                 │
│  ─────────────────────────────           │
│  + Initialize(startState)                │
│  + ChangeState(newState)                 │
│  + UpdateActiveState()                   │
│  + SwitchOffStateMachine()               │
└────────────┬────────────────────────────┘
             │ 持有并驱动
             ▼
┌─────────────────────────────────────────┐
│           EntityState  (abstract)        │
│  ─────────────────────────────           │
│  # stateMachine : StateMachine           │
│  # animBoolName : string   ◄── 状态对应  │
│  # anim : Animator           的动画参数  │
│  # rb : Rigidbody2D                     │
│  # stats : Entity_Stats                  │
│  # stateTimer : float       ◄── 通用计时器│
│  # triggerCalled : bool     ◄── 动画事件  │
│  ─────────────────────────────           │
│  + Enter()   (virtual)                   │
│  + Update()  (virtual)                   │
│  + Exit()    (virtual)                   │
│  + AnimationTrigger()                    │
│  + SyncAttackSpeed()                     │
└────┬──────────────────┬─────────────────┘
     │                  │
     ▼                  ▼
┌──────────────┐  ┌──────────────┐
│ PlayerState  │  │ EnemyState   │
│ (abstract)   │  │              │
│ ────────     │  │ ────────     │
│ + player     │  │ + enemy      │
│ + input      │  │              │
│ + skillMgr   │  │              │
│              │  │ Update 中    │
│ Update 中    │  │ 处理动画     │
│ 处理全局技能 │  │ 速度参数     │
│ 输入(Dash/   │  │              │
│ Ultimate)    │  │              │
└────┬─────────┘  └────┬─────────┘
     │                  │
     ▼                  ▼
┌──────────────┐  ┌──────────────┐
│ Player_      │  │ Enemy_       │
│ GroundedState│  │ GroundedState│
│ ────────     │  │ ────────     │
│ Update 中    │  │ Update 中    │
│ 检测起跳/    │  │ 检测玩家 →   │
│ 攻击/防反    │  │ 进入Battle   │
└──┬──┬──┬──┬─┘  └──┬──┬──┬──┬─┘
   │  │  │  │        │  │  │  │
   ▼  ▼  ▼  ▼        ▼  ▼  ▼  ▼
 Idle Move ...等    Idle Move ...等
 (具体状态)         (具体状态)
```

---

## 三、StateMachine 核心——只有 32 行代码

```csharp
public class StateMachine
{
    public EntityState currentSate { get; private set; }
    public bool canChangeState = true;

    public void Initialize(EntityState startState)
    {
        canChangeState = true;
        currentSate = startState;
        currentSate.Enter();
    }

    public void ChangeState(EntityState newState)
    {
        if (canChangeState == false)
            return;

        currentSate.Exit();    // 1. 旧状态清理
        currentSate = newState; // 2. 指向新状态
        currentSate.Enter();    // 3. 新状态初始化
    }

    public void UpdateActiveState()
    {
        currentSate.Update();   // 委托给当前状态
    }

    public void SwitchOffStateMachine() => canChangeState = false;
}
```

### 3.1 设计要点分析

**极简主义**：这个类没有泛型、没有状态栈、没有子状态机的概念。它就是做三件事——Enter、Update、Change。在动作游戏的语境下这完全够用，因为：

- 角色**每次只处于一个状态**（你不可能同时 Idle 和 Jump）
- 状态图是一个**有向图**，不需要历史栈（行为树才需要）
- 所有状态对象在 Player/Enemy 初始化时**一次创建**，之后只做引用切换，不涉及 new/GC

**canChangeState 的作用**：防止在状态切换过程中再次切换。比如 Enemy 死亡时调用 `SwitchOffStateMachine()` 将 `canChangeState` 设为 false，之后任何 `ChangeState` 调用都被忽略——死亡就是终点。

**Initialize 和 ChangeState 的区别**：Initialize 只调 Enter 不调 Exit（因为没有"旧状态"），ChangeState 先 Exit 再 Enter。这很自然但很多 FSMs 实现会忘记这个区别。

### 3.2 状态对象的生命周期

```csharp
// 所有状态在 Player.Awake() 中一次性创建（不是 new 出来的，是构造函数）
idleState = new PlayerIdleState(this, stateMachine, "idle");
moveState = new Player_MoveState(this, stateMachine, "move");
jumpState = new Player_JumpState(this, stateMachine, "jumpFall");
// ... 一共 13 个状态

// 然后在 Start() 中初始化
stateMachine.Initialize(idleState);  // 进入 Idle
```

**关键洞察**：13 个状态对象在游戏开始时就全部存在了，之后只是 `stateMachine.currentSate` 的引用在变。状态对象本身不会被创建/销毁，所以**零 GC 压力**。

---

## 四、EntityState 基类——状态设计的基础设施

```csharp
public abstract class EntityState
{
    protected StateMachine stateMachine;
    protected string animBoolName;       // ← 核心：一个状态 = 一个动画参数

    protected Animator anim;
    protected Rigidbody2D rb;
    protected Entity_Stats stats;

    protected float stateTimer;          // ← 通用计时器
    protected bool triggerCalled;        // ← 动画事件通知

    public EntityState(StateMachine stateMachine, string animBoolName)
    {
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }

    public virtual void Enter()
    {
        anim.SetBool(animBoolName, true);  // 进入状态 → 播放对应动画
        triggerCalled = false;              // 重置动画触发器
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;       // 自动倒计时
        UpdateAnimationParameters();        // 每帧更新动画参数
    }

    public virtual void Exit()
    {
        anim.SetBool(animBoolName, false);  // 离开状态 → 关闭动画参数
    }

    public void AnimationTrigger()
    {
        triggerCalled = true;               // 被 Animation Event 调用
    }
}
```

### 4.1 animBoolName——状态和动画的桥梁

**一个状态**对应 Animator Controller 中的**一个 Bool 参数**。状态的 Enter 把这个 Bool 设为 true（触发动画过渡），Exit 把它设回 false。

注意：**一个状态可能对应多种动画**——比如 `Player_BasicAttackState` 对应的 animBool 是 `"basicAttack"`，但 Animator 内部根据 `basicAttackIndex` 这个 Integer 参数来播放 Combo1/Combo2/Combo3 三段不同的动画。Bool 参数只在"是否处于 BasicAttack 状态"这个层面，具体播放哪一段动画由 Animator 内部的 Blend Tree 或 Sub-State Machine 决定。

换句话说：**代码状态是"行为层面的状态"，动画状态是"表现层面的状态"，前者是后者的超集**。

### 4.2 stateTimer——为什么每个状态自带计时器

很多状态有时间限制：

- Dash 只能持续 0.25 秒
- 敌人 Idle 只能在原地待 2 秒就移动
- 敌人受击硬直只有 1 秒
- CounterAttack 判定窗口只有 0.1 秒

`stateTimer` 在 `Enter()` 时被子状态赋值（例如 `stateTimer = enemy.idleTime`），然后在基类的 `Update()` 中每帧递减。子状态只需要检查 `if (stateTimer < 0)` 就知道时间到了。

**不用协程的原因**：协程依赖 MonoBehaviour，而 EntityState 不是 MonoBehaviour——它是一个纯 C# 类。用 `stateTimer` 保持状态类的纯粹性。

### 4.3 triggerCalled——Animation Event 到状态机的桥梁

这是 FSM 和 Unity 动画系统集成的关键：

```
Unity Animation Clip (攻击动画)
    │
    ├─ 帧 0: 起手
    ├─ 帧 5: 伤害判定帧 → Animation Event → AttackTrigger()
    ├─ 帧 10: 可取消帧  → Animation Event → CurrentStateTrigger()
    └─ 帧 15: 收招结束
                     │
                     ▼
Entity_AnimationTriggers.cs (挂在 Animator 的 GameObject 上)
    │
    ├─ CurrentStateTrigger() → entity.CurrentStateAnimationTrigger()
    │                              → stateMachine.currentSate.AnimationTrigger()
    │                                  → triggerCalled = true
    │
    └─ AttackTrigger() → entityCombat.PerformAttack()
```

**triggerCalled 解决了什么问题？**

假设 BasicAttack 动画长度是 0.5 秒，你怎么知道攻击动画播完了可以切回 Idle？

- **错误做法**：`stateTimer = 0.5f`，然后 `if (stateTimer < 0)` 切状态。问题：如果 Animator 的 speed 变了（比如攻击速度属性 buff），动画实际只需要 0.3 秒播完，但你的代码还在等 0.5 秒。
- **正确做法**：在动画的最后一帧加 Animation Event 调用 `CurrentStateTrigger()`，代码检查 `triggerCalled`。无论动画播多快，都是在真正播完的那一帧触发。

这就是 `SyncAttackSpeed()` 方法存在的理由——`anim.SetFloat("attackSpeedMultiplier", attackSpeed)` 让 Animator 按攻速属性加速/减速动画，而 `triggerCalled` 保证状态切换和动画的实际结束同步。

---

## 五、三层继承的意义——以 Player 侧为例

### 5.1 第一层：EntityState

提供所有状态都需要的基础设施：anim、rb、stats、stateTimer、triggerCalled。

### 5.2 第二层：PlayerState

```csharp
public abstract class PlayerState : EntityState
{
    protected Player player;
    protected PlayerInputSet input;
    protected Player_SkillManager skillManager;

    public override void Update()
    {
        base.Update();
        player.dashCoolDown -= Time.deltaTime;

        // ★ 全局技能输入检测——所有 Player 状态都能 Dash 和使用终极技
        if (input.Player.Dash.WasPressedThisFrame() && CanDash())
        {
            skillManager.dash.SetSkillOnCooldown();
            stateMachine.ChangeState(player.dashState);
        }
        if (input.Player.UltimateSpell.WasPressedThisFrame() && ...)
        {
            // 进入 DomainExpansion 状态
        }
    }
}
```

**这一层存在的理由**：Dash 和 DomainExpansion（终极技）是"全局技能"——无论在 Idle、Move、Jump、WallSlide 还是 BasicAttack（部分情况下），只要条件允许就能触发。

如果没有 PlayerState 这一层，你需要在 **13 个状态类的每一个** 的 `Update()` 中重复这段 Dash 检测代码。现在只需要写一次。

同样，`dashCoolDown` 的递减也放在这一层——Dash 冷却在所有状态下都应该持续减少。

### 5.3 第二层（子类）：Player_GroundedState / Player_AirState

这两层进一步分组：

```csharp
// Player_GroundedState —— 所有地面状态的共同逻辑
public class Player_GroundedState : PlayerState
{
    public override void Update()
    {
        base.Update();
        if (rb.linearVelocityY < 0 && !player.groundDetected)
            stateMachine.ChangeState(player.fallState);  // 踩空了 → 下落

        if (input.Player.Jump.WasPressedThisFrame())
            stateMachine.ChangeState(player.jumpState);   // 按了跳 → 起跳

        if (input.Player.Attack.WasPressedThisFrame())
            stateMachine.ChangeState(player.basicAttackState); // 按了攻击 → 普攻

        if (input.Player.CounterAttack.WasPressedThisFrame())
            stateMachine.ChangeState(player.counterAttackState); // Q → 防反

        if (input.Player.RangeAttack.WasPerformedThisFrame() && ...)
            stateMachine.ChangeState(player.swordThrowState); // 右键 → 投掷飞剑
    }
}

// Player_AirState —— 所有空中状态的共同逻辑
public class Player_AirState : PlayerState
{
    public override void Update()
    {
        base.Update();
        if (player.moveInput.x != 0)
            player.SetVelocity(player.moveInput.x * player.moveSpeed * player.inAirMoveMultiplier, rb.linearVelocity.y);
            // 空中也能左右移动，但有空气阻力系数

        if (input.Player.Attack.WasPressedThisFrame())
            stateMachine.ChangeState(player.jumpAttackState); // 空中攻击 → 跳劈
    }
}
```

这样 Idle 和 Move 状态就不需要各自处理"检测跳跃输入"，Jump 和 Fall 也不需要各自处理"空中移动"。

### 5.4 第三层：具体状态

```
                     EntityState
                         │
                    PlayerState ──────── Dash/Ultimate 全局检测
                    ┌────┴────────┐
           GroundedState       AirState ──── 空中移动
           ┌───┴───┐          ┌──┴──┐
          Idle    Move       Jump  Fall   WallSlide  WallJump
                                        (继承PlayerState)
```

```
Player 状态继承树（13 个叶状态）：

EntityState
 └─ PlayerState
     ├─ Player_GroundedState
     │   ├─ PlayerIdleState          ← "idle"
     │   └─ Player_MoveState         ← "move"
     │
     ├─ Player_AirState
     │   ├─ Player_JumpState         ← "jumpFall"
     │   └─ Player_FallState         ← "jumpFall"
     │
     ├─ Player_WallSlideState        ← "wallSlide"
     ├─ Player_WallJumpState         ← "jumpFall"
     ├─ Player_DashState             ← "dash"
     ├─ Player_BasicAttackState      ← "basicAttack"
     ├─ Player_JumpAttackState       ← "jumpAttack"
     ├─ Player_DeadState             ← "dead"
     ├─ Player_CounterAttackState    ← "counterAttack"
     ├─ Player_SwordThrowState       ← "swordThrow"
     └─ Player_DomainExpansionState  ← "jumpFall"
```

注意 `JumpState` 和 `FallState` 共享同一个 animBool `"jumpFall"`——它们都处于"在空中"的动画状态，区别只是上升还是下降。这再次说明：**一个动画状态可以对应多个代码状态**。

---

## 六、状态迁移实例分析

### 6.1 最简单的：Idle ↔ Move

```csharp
// PlayerIdleState.Update()
public override void Update()
{
    base.Update();  // → PlayerState.Update() → Dash/Ultimate 检测
                     // → Player_GroundedState.Update() → 起跳/攻击/防反 检测

    if (player.moveInput.x == player.facingDir && player.wallDetected)
        return;  // 面朝墙壁且贴着墙 → 不能往前走

    if (player.moveInput.x != 0)
        stateMachine.ChangeState(player.moveState);  // 有水平输入 → Move
}

// Player_MoveState.Update()
public override void Update()
{
    base.Update();  // 同样的上层检测链

    if (player.moveInput.x == 0 || player.wallDetected)
        stateMachine.ChangeState(player.idleState);  // 松手或碰墙 → Idle

    player.SetVelocity(player.moveInput.x * player.moveSpeed, rb.linearVelocity.y);
}
```

单个状态类的 Update 方法通常只有 3-10 行。核心逻辑是：**检查条件 → 满足就切换状态 → 否则执行本状态行为**。

### 6.2 带时序的：Dash

```csharp
public class Player_DashState : PlayerState
{
    private float orginalGravityScale;
    private int dashDir;

    public override void Enter()
    {
        base.Enter();
        skillManager.dash.OnStartEffect();     // 技能层的回调（生成残影等）
        player.vfx.DoImageEchoEffect(player.dashDuration); // 残影特效

        stateTimer = player.dashDuration;       // 冲刺时长 0.25秒
        orginalGravityScale = rb.gravityScale;
        rb.gravityScale = 0;                    // 冲刺时无视重力
        dashDir = player.moveInput.x != 0 ? (int)player.moveInput.x : player.facingDir;

        player.health.SetCanTakenDamage(false); // 无敌帧
        player.gameObject.layer = LayerMask.NameToLayer("Untargetable"); // 不可被锁定
    }

    public override void Update()
    {
        base.Update();
        cancelDashIfNeeded();                   // 撞墙提前终止
        player.SetVelocity(player.dashSpeed * dashDir, 0);

        if (stateTimer < 0)                     // 时间到
        {
            if (player.groundDetected)
                stateMachine.ChangeState(player.idleState);
            else
                stateMachine.ChangeState(player.fallState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        skillManager.dash.OnEndEffect();        // 技能层回调
        player.SetVelocity(0, 0);
        rb.gravityScale = orginalGravityScale;  // 恢复重力
        player.dashCoolDown = player.dashLimitDuration; // 开始冷却
        player.health.SetCanTakenDamage(true);  // 恢复可受伤
        player.gameObject.layer = LayerMask.NameToLayer("Player");
    }
}
```

这个状态展示了 **Enter/Update/Exit 的完美对称**：

| 阶段 | 做的事情 |
|------|---------|
| **Enter** | 设置冲刺参数（重力=0、速度、方向、无敌） |
| **Update** | 维持冲刺速度，检测时间/碰撞结束条件 |
| **Exit** | **恢复 Enter 中修改的每一个状态**（重力、速度、图层、无敌、冷却） |

注意 Exit 中的恢复是**精确对称的**——Enter 改了 6 个东西，Exit 全部恢复。这种对称性保证了一个状态退出后，不会留下任何"脏状态"影响下一个状态。这也是为什么状态机比 if-else 可靠——**Enter 和 Exit 强制你把"进入前"和"退出后"想清楚**。

### 6.3 复杂连招：BasicAttack

```csharp
public class Player_BasicAttackState : PlayerState
{
    private int comboIndex = 1;          // 当前是第几段
    private const int comboLimit = 3;   // 最多三段
    private bool comboAttackQueued;      // 是否排队了下一段攻击

    public override void Enter()
    {
        base.Enter();
        comboAttackQueued = false;
        ResetComboIndexIfNeeded();       // 超过 combo 重置时间就回到第一段

        SyncAttackSpeed();               // 同步攻击速度属性到 Animator
        anim.SetInteger("basicAttackIndex", comboIndex);
        ApplyAttackVelocity();           // 施加攻击位移（每段攻击有不同的位移）
    }

    public override void Update()
    {
        base.Update();
        HandleAttackVelocity();          // 攻击位移计时

        if (input.Player.Attack.WasPressedThisFrame())
            QueueNextAttack();           // 在连招中再次按攻击 → 排队下一段

        if (triggerCalled)               // ← 动画播完的帧触发
            HandleStateExit();
    }

    private void HandleStateExit()
    {
        if (comboAttackQueued)           // 玩家在上一段动画期间按了攻击
        {
            anim.SetBool(animBoolName, false);
            player.EnterAttaclStateWithDelay(); // 延迟一帧再进入（自己）
        }
        else
            stateMachine.ChangeState(player.idleState); // 没有排队 → 回 Idle
    }

    public override void Exit()
    {
        base.Exit();
        comboIndex++;
        lastTimeAttacked = Time.time;
    }
}
```

连招系统的精妙之处：

- **Combo 排队**：玩家可以在第一段攻击动画播放期间按下攻击键，`comboAttackQueued = true`，动画结束时自动进入第二段——这就是手感好的"预输入"
- **Combo 重置**：如果距离上次攻击超过 `comboResetTime`（1秒），comboIndex 回到 1，重新从第一段开始——防止你打一拳跑两步再回来还是第二段
- **延迟一帧进入**：`EnterAttaclStateWithDelay()` 使用 `yield return new WaitForEndOfFrame()`，确保在同一帧内先让 Animator 处理 `animBoolName = false` 的过渡，下一帧再重新设为 true。如果不延迟，同一帧内设为 false 又立即设为 true，Animator 可能会忽略这次变化

---

## 七、Enemy 侧的状态机——相同的模式，不同的中间层

### 7.1 Enemy 状态继承树

```
EntityState
 └─ EnemyState
     ├─ Enemy_GroundedState   ← 检测到玩家 → 进入 Battle
     │   ├─ Enemy_IdleState   ← 待机倒计时结束 → Move
     │   └─ Enemy_MoveState   ← 碰墙/踩空 → Idle
     │
     ├─ Enemy_BattleState     ← 战斗追逐 + 距离/冷却判断
     ├─ Enemy_AttackState     ← 播放攻击动画 → triggerCalled → Battle
     ├─ Enemy_DeadState       ← 关闭碰撞、施加死亡物理、定时销毁
     └─ Enemy_StunnedState    ← 受击硬直 → 计时结束 → Idle
```

### 7.2 EnemyState 中间层的设计

```csharp
public class EnemyState : EntityState
{
    protected Enemy enemy;

    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();
        float battleAnimSpeedMultiplier = (enemy.battleMoveSpeed / enemy.moveSpeed) * enemy.moveAnimSpeedMultiplier;
        anim.SetFloat("battleAnimSpeedMultiplier", battleAnimSpeedMultiplier);
        anim.SetFloat("moveAnimSpeedMultiplier", enemy.moveAnimSpeedMultiplier);
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
    }
}
```

EnemyState 中间层做的事情和 PlayerState 不同但目的相同：**把多个状态共有的逻辑提到中间层**。Enemy 的所有状态都需要把移动速度、战斗速度同步到 Animator，所以重写了 `UpdateAnimationParameters()`。

### 7.3 Enemy_GroundedState——统一的"发现玩家"检测

```csharp
public class Enemy_GroundedState : EnemyState
{
    public override void Update()
    {
        base.Update();
        if (enemy.PlayerDetected() == true)
            stateMachine.ChangeState(enemy.battleState);
    }
}
```

无论是 Idle 还是 Move 状态，只要检测到玩家就进入 Battle。这个逻辑放在 `Enemy_GroundedState` 中间层，Idle 和 Move 自动继承。

### 7.4 敌人 AI 的完整状态流转

```
        ┌────────────┐
        │   Idle     │ ← 待机 timer 结束
        └──┬─────────┘
           │ timer < 0
           ▼
        ┌────────────┐
        │   Move     │ ← 碰墙/踩空返回 Idle
        └──┬─────────┘
           │ PlayerDetected (在 GroundedState 层自动检测)
           ▼
        ┌────────────┐
        │  Battle    │ ← 追逐玩家，维持战斗 timer
        └──┬─────────┘
           │ 进入攻击范围 + 攻击冷却完毕
           ▼
        ┌────────────┐
        │  Attack    │ ← triggerCalled → 返回 Battle
        └────────────┘
           │
           │ 受击可被眩晕 → 进入 Stunned → 返回 Idle
           │ 死亡 → Dead (关闭状态机)
```

---

## 八、和 Unity Animator 状态机的关系——两个状态机共存

这是一个经常被问到的问题：**既然 Unity 的 Animator 本身就是状态机，为什么还要在代码里再写一个状态机？**

### 8.1 它们解决不同的问题

```
Unity Animator 状态机（.controller 文件）
├─ 管理：动画片段的播放/过渡/混合
├─ 关心：这个动画 clip 播完要不要循环？过渡到下一个 clip 要 Blend 多久？
└─ 它不知道：伤害判定、碰撞检测、冷却计算、无敌帧、技能效果

代码 FSM
├─ 管理：游戏逻辑的状态（能不能移动、能不能受伤、重力是否生效）
├─ 关心：从空中落地后应该进入 Idle 还是 Fall？Dash 能不能被普攻打断？
└─ 它不管：实际播放哪个动画 clip（它只设置 Animator 的参数）
```

### 8.2 它们怎么通信

```
代码 FSM                          Unity Animator
────────                          ──────────────
Enter() ──→ anim.SetBool("dash", true) ──→ 播放 Dash 动画
                                          │
Update() ← 检查 triggerCalled ←──────── Animation Event (动画最后一帧)
```

- **代码 → 动画**：通过 `anim.SetBool/SetFloat/SetInteger/SetTrigger`
- **动画 → 代码**：通过 Animation Event（在动画 clip 的特定帧绑定回调函数）

### 8.3 为什么不能全用 Animator 来做

Unity 的 Animator State Machine 有一个致命问题：**过渡条件（Transitions）只能在 Animator Controller 中可视化编辑，无法在代码中灵活管理**。

如果你试图把所有逻辑都放在 Animator 里：
- 你需要为 Dash → Idle、Dash → Fall 分别连线
- 你需要用 Animator 参数来表示 "dashTimer < 0"
- 你不能在代码中做复杂判断（比如 `CanDash()` 要检查冷却、墙面、当前状态）
- 策划和程序需要在 Animator 的图形界面上合作，而那个界面在复杂时极其痛苦

Animator 适合做**表现层**的事情（"从走路过渡到跑步要花 0.2 秒，用这个 Blend Curve"），不适合做逻辑层的事情。

### 8.4 一个 Animator 状态 ≠ 一个代码状态

正如前面提到的：

- `JumpState` 和 `FallState` 两个代码状态共享同一个 Animator Bool `"jumpFall"`——它们都在空中，只是上升和下降的物理不同
- `BasicAttackState` 一个代码状态可能在 Animator 中对应三段不同的攻击动画（Combo1/2/3），通过 `basicAttackIndex` 整数参数区分

所以是**多对多**的关系。

---

## 九、Animation Triggers——沟通两个状态机的桥梁

```csharp
// Entity_AnimationTriggers.cs —— 挂在 Animator 所在的 GameObject 上
public class Entity_AnimationTriggers : MonoBehaviour
{
    private Entity entity;
    private Entity_Combat entityCombat;

    // ★ 这两个方法由 Animation Event 调用 ★
    private void CurrentStateTrigger()
    {
        entity.CurrentStateAnimationTrigger();
        // → stateMachine.currentSate.AnimationTrigger()
        //   → triggerCalled = true
    }

    private void AttackTrigger()
    {
        entityCombat.PerformAttack();
        // → OverlapCircleAll 检测目标 → IDamagable.TakeDamage()
    }
}

// Player_AnimationTriggers.cs —— Player 特有的动画事件
public class Player_AnimationTriggers : Entity_AnimationTriggers
{
    private Player player;

    private void ThrowSword()
    {
        player.skillManager.swordThrow.ThrowSword();
        // 在投掷飞剑动画的特定帧，生成实际的飞剑物体
    }
}

// Enemy_AnimationTriggers.cs —— Enemy 特有的动画事件
public class Enemy_AnimationTriggers : Entity_AnimationTriggers
{
    private void EnableCounterWindow()
    {
        enemy.EnableCounterWindoow(true);   // 敌人攻击的前摇帧 → 开启防反窗口
        enemyVfx.EnableAttackAlert(true);   // 显示"可防反"提示
    }
    private void DisableCounterWindow()
    {
        enemy.EnableCounterWindoow(false);  // 攻击后摇帧 → 关闭防反窗口
    }
}
```

**Animation Triggers 的设计原则**：

1. **挂在 Animator 的 GameObject 上**：因为 Animation Event 只能调用**挂在同一 GameObject 上的脚本**的方法
2. **通过 GetComponentInParent 获取 Entity/Player/Enemy**：因为 Animator 通常是角色的子物体
3. **只做转发**：Triggers 自己没有任何业务逻辑，只是把动画事件转发给对应的系统——做单一职责

---

## 十、这个 FSM 设计的优点总结

### 10.1 每条优点都跟反面做法对比

| 优点 | 这个项目怎么做 | 反面做法 |
|------|--------------|---------|
| **状态互斥内建** | 每个状态是一个类，同时只能有一个 active | 用一堆 bool 标志位，手动保证互斥 |
| **开闭原则** | 加新状态 = 新建一个类，不改已有代码 | 在 Update 里加新的 if-else 分支 |
| **逻辑局部化** | Dash 的所有逻辑在 `Player_DashState` 一个文件里 | Dash 逻辑散落在 Update 的不同 if 块中 |
| **Enter/Exit 对称** | Enter 改了什么，Exit 恢复什么，一目了然 | bool 标志模式下你不知道哪个 if 块改了什么 |
| **代码复用（中间层）** | Dash 检测写在 PlayerState，13 个状态自动继承 | 每个状态都要写一遍 Dash 检测 |
| **动画同步（triggerCalled）** | 动画最后一帧发事件，代码在真正播完时才切换 | 用固定秒数等动画结束，攻速变化就出错 |
| **零 GC** | 13 个状态对象在 Awake 时一次性创建 | 每次切状态 `new` 一个新对象 |
| **纯 C# 类** | EntityState 不继承 MonoBehaviour，可以 new | 状态必须是 MonoBehaviour 组件，挂在 GameObject 上 |

### 10.2 可测试性

因为 EntityState 是纯 C# 类，不依赖 GameObject，理论上可以这样单元测试：

```csharp
// 伪代码——实际上需要 mock animator 和 rigidbody，但至少状态逻辑是可隔离的
[Test]
public void DashState_ShouldTransitionToFall_WhenTimerExpiresInAir()
{
    var state = new Player_DashState(player, sm, "dash");
    state.Enter();
    // 模拟 stateTimer 耗尽 && 不在地面
    // 断言当前状态变为 fallState
}
```

虽然这个项目没有写测试，但架构上确实支持。

---

## 十一、局限性及改进空间

### 11.1 没有状态栈——无法做"暂停并临时做某事然后回来"

假设你要实现"被敌人抓住 → 播放被抓动画 → 挣脱后回到之前的状态"。当前设计做不到，因为你只有一个 `currentSate`，切换后就丢失了"之前是什么状态"。

**解决方案**：加一个状态栈，或者在这种情况下让状态本身记住"回来时应该进入什么状态"（已经在部分场景用到——Dash Exit 时根据 `groundDetected` 决定回 Idle 还是 Fall）。

### 11.2 状态对象需要在构造函数中传入所有引用

```csharp
idleState = new PlayerIdleState(this, stateMachine, "idle");
moveState = new Player_MoveState(this, stateMachine, "move");
// ... 13 行
```

这种写法没什么问题，但如果状态数量到达 30+，可以考虑用反射或工厂模式自动创建。

### 11.3 Enemy 变体需要写全新的状态类

`Enemy_ArcherElf` 要覆盖 Battle 行为，就需要新建 `Enemy_ArcherElfBattleState` 类，覆盖 `Enemy_BattleState` 的距离判断逻辑。如果变体多了，状态类的数量会膨胀。

**替代方案**：可以用**行为树**或**GOAP（Goal-Oriented Action Planning）**。但对于一个 2D 动作游戏 Demo，状态机已经足够好。

### 11.4 PlayerState.Update 中 Dash 冷却递减的位置

```csharp
// PlayerState.Update()
player.dashCoolDown -= Time.deltaTime;
```

这行代码让 Dash 冷却在所有状态下都递减，这看起来合理——但如果未来有一个"时间暂停"状态（比如打开背包时），Dash 冷却不应该继续减少。放在这里是合理的默认行为，但如果需要更精细的控制，应该让各状态自己决定是否递减。

---

## 十二、关键设计决策回顾

| 决策 | 选择 | 备选方案 | 为什么选这个 |
|------|------|---------|------------|
| 状态是类还是枚举 | 类（每个状态一个 class） | `enum PlayerStateType` + switch | 类可以封装 Enter/Update/Exit 逻辑，switch 会导致几千行的巨型 Update |
| 状态是不是 MonoBehaviour | 不是，纯 C# 类 | 每个状态挂一个组件 | 纯 C# 类轻量、可 new、不依赖 GameObject |
| 状态何时创建 | Awake 时一次性创建 | 每次切换时 lazy new | 避免 GC，避免构造时的开销 |
| 技能输入在哪层检测 | PlayerState（中间层） | 每个状态自己检测 | 13 个状态都需要 Dash，写 13 次不如写 1 次 |
| 动画同步机制 | Animation Event → triggerCalled | 用定时器等动画结束 | 攻速 buff 会让动画时长变化，定时器不准 |
| Animator 参数类型 | Bool 参数表示"处于某状态" | Trigger 参数触发一次 | Bool 可以表示"持续处于这个状态"，Enter=SetTrue, Exit=SetFalse 语义清晰 |

---

## 十三、如果你想在别的项目复现这个 FSM

**你只需要拷贝的文件**（不依赖其他游戏系统）：

1. `StateMachine.cs` — 32 行，零依赖，任何项目直接用
2. `EntityState.cs` — 去掉 `Entity_Stats` 引用，改成你自己的依赖即可

然后按照这个模板创建你的状态体系：

```csharp
// 1. 创建你的"Entity"基类
public class MyCharacter : MonoBehaviour
{
    protected StateMachine stateMachine;
    public MyIdleState idleState;
    // ... 其他状态

    protected virtual void Awake()
    {
        stateMachine = new StateMachine();
        idleState = new MyIdleState(this, stateMachine, "Idle");
        // ...
    }
    protected virtual void Start() { stateMachine.Initialize(idleState); }
    protected virtual void Update() { stateMachine.UpdateActiveState(); }
}

// 2. 创建你的 State 基类
public abstract class MyCharacterState
{
    protected StateMachine sm;
    protected string animBool;
    protected Animator anim;
    protected float stateTimer;
    protected bool triggerCalled;
    // ... Enter/Update/Exit 模板方法
}

// 3. 创建具体状态
public class MyIdleState : MyCharacterState
{
    public override void Update()
    {
        base.Update();
        if (/* 条件 */) sm.ChangeState(character.moveState);
    }
}
```

核心思想就三步：**状态机管理器 → 状态基类 → 具体状态**。无论什么类型的游戏，这个模式都适用。
