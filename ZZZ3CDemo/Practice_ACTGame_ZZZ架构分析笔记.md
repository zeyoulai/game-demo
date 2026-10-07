# Practice_ACTGame_ZZZ 架构分析笔记

## 一、项目概览

这是一个**第三人称动作游戏原型**，模仿《绝区零》（Zenless Zone Zero）的基础战斗框架。项目包含角色移动、普通攻击连招、闪避、大招、三角色切换等核心 ACT 系统。

大小约 25 个 C# 文件，是一个**教学/练习级 Demo**——能跑，但大量系统是空壳或硬编码。

- **引擎**: Unity 2021.3+（URP 12.1.7）
- **渲染管线**: Universal Render Pipeline
- **输入系统**: Unity New Input System 1.4.4
- **摄像机**: Cinemachine 2.8.9（FreeLook）
- **核心玩法**: 第三人称动作战斗 + 三角色切换

---

## 二、技术栈

| 技术 | 版本 | 用途 |
|---|---|---|
| Unity | 2021.3 LTS | 引擎 |
| URP | 12.1.7 | 渲染管线 |
| Cinemachine | 2.8.9 | 第三人称摄像机 |
| Input System | 1.4.4 | 跨平台输入 |
| ProBuilder | 5.0.7 | 关卡灰盒 |
| TextMeshPro | 3.0.6 | UI 文字 |

---

## 三、目录结构与模块划分

```
Assets/
├── Arts/                          ← 美术资源（角色贴图/材质/Shader/场景贴图）
│   ├── Anbi/                      ← 安比（角色1）
│   ├── Corin/                     ← 可琳（角色2）
│   ├── Nike/                      ← 妮可（角色3）
│   └── Environment/               ← 场景材质+贴图
│
├── Config/                        ← ScriptableObject 配置
│   ├── PlayerConfig.asset         ← 玩家配置：挂3个角色 Prefab
│   └── Skill/                     ← 技能配置目录（下面的 SkiilConfig）
│
├── Perfabs/                       ← Prefab（角色/敌人/场景）
├── Scenes/                        ← 场景文件
│
├── Scripts/                       ← ★ 所有代码
│   ├── Base/                      ← 基类
│   │   ├── StateBase.cs           ← 状态抽象基类（整个状态机的地基）
│   │   ├── PlayerStateBase.cs     ← 玩家状态中间基类（输入检测+重力）
│   │   ├── EnemyStateBase.cs      ← 敌人状态（空壳，只有 Start/Update 空方法）
│   │   └── SingleMomoBase.cs      ← 懒人单例基类
│   │
│   ├── StateMachine/
│   │   └── StateMachine.cs        ← 通用状态机（字典缓存+统一生命周期管理）
│   │
│   ├── Config/
│   │   ├── PlayerConfig.cs        ← SO：角色 Prefab 列表
│   │   └── SkiilConfig.cs         ← SO：技能配置（连招段数+伤害倍率）
│   │
│   ├── Manager/
│   │   ├── MonoManager.cs         ← Update/FixedUpdate/LateUpdate 统一调度器
│   │   └── CameraManager.cs       ← Cinemachine 摄像机管理器
│   │
│   └── Player/
│       ├── PlayerController.cs    ← ★ 核心控制器（唯一入口，IStateMachineOwner）
│       ├── PlayerModel.cs         ← 角色模型容器（Animator+CharacterController）
│       ├── TargetPoint.cs         ← 锁定目标点（跟随角色）
│       ├── FollowPoint.cs         ← 摄像机跟随点
│       └── State/                 ← 所有玩家状态
│           ├── Idle/PlayerIdleState.cs
│           ├── Run/PlayerRunState.cs, PlayerRunEndState.cs, PlayerTurnBackState.cs
│           ├── Evade/PlayerEvadeState.cs, PlayerEvadeEndState.cs
│           ├── NormalAttack/PlayerNormalAttackState.cs, PlayerNormalAttackEndState.cs
│           ├── BigSkill/PlayerBigSkillStartState.cs, PlayerBigSkillState.cs, PlayerBigSkillEndState.cs
│           └── SwitchIn/PlayerSwitchInNormalState.cs
│
└── Settings/
    └── Input System/               ← Input Action Asset
```

---

## 四、核心架构

### 4.1 整体架构模式

项目采用 **状态模式 + 单例管理器** 的经典组合：

```
                    ┌──────────────┐
                    │  MonoManager │  ← 统一调度所有 Update
                    │  (单例)       │
                    └──────┬───────┘
                           │ Update/FixedUpdate/LateUpdate
                           │
    ┌──────────────────────┼──────────────────────┐
    │                      │                      │
    ▼                      ▼                      ▼
┌─────────┐    ┌──────────────────┐    ┌──────────────┐
│ Camera  │    │  PlayerController│    │  (其他...)    │
│ Manager │    │  (单例)           │    │              │
│ (单例)   │    │                  │    └──────────────┘
└─────────┘    │ 持有:             │
               │  - StateMachine   │
               │  - PlayerModel    │
               │  - InputSystem    │
               └────────┬─────────┘
                        │
              ┌─────────┴──────────┐
              │    StateMachine    │
              │  stateDic<Type,    │
              │    StateBase>      │
              └─────────┬──────────┘
                        │ currentState
                        ▼
              ┌──────────────────┐
              │  PlayerStateBase  │  ← 中间基类（输入检测+重力+动画结束判断）
              └─────────┬──────────┘
                        │
        ┌───────┬───────┼───────┬───────┬───────┐
        ▼       ▼       ▼       ▼       ▼       ▼
      Idle    Run    Evade   Attack  BigSkill SwitchIn
      State   State   State   State   State    State
```

### 4.2 设计哲学（3 个核心决策）

**决策 1：按动画类型拆分状态，用 Start/End 子状态区分阶段**

这是本项目最核心的设计思想。一个"大招"被拆成三个状态：

```
BigSkillStart → BigSkill → BigSkillEnd
   (进场)        (主体)       (收招)
```

为什么这样拆？因为每个阶段需要不同的行为：
- `BigSkillStart`：切摄像机镜头（硬切 0 秒过渡）、锁玩家输入、播起手动画
- `BigSkill`：切到技能镜头、播主体动画（不允许任何输入打断）
- `BigSkillEnd`：恢复摄像机（1 秒 EaseInOut 过渡）、解锁输入、进入可被打断状态

如果写成一个状态，内部就得用 if-else 判断动画阶段，状态类会迅速膨胀。

**决策 2：MonoManager 统一调度，状态类不是 MonoBehaviour**

状态类（`StateBase`）是纯 C# 类，不是 MonoBehaviour。它们的 Update/FixedUpdate 通过 `MonoManager` 的 Action 委托统一调用。

好处：
- 状态类可以 new，不依赖 GameObject
- 字典缓存复用，避免频繁创建/销毁
- 所有 Update 集中在一个 MonoBehaviour 上，性能更可控

坏处：
- 无法使用协程
- 断点调试时看不到调用栈是哪来的

**决策 3：PlayerModel 作为数据容器，PlayerController 作为中央调度器**

`PlayerModel` 只负责持有引用（Animator、CharacterController、当前状态枚举），不包含任何行为逻辑。所有决策都在 `PlayerController.SwitchState()` 里：

```csharp
// PlayerController.cs:73
public void SwitchState(PlayerState playerState)
{
    playerModel.currentState = playerState;  // 先更新枚举
    switch (playerState)
    {
        case PlayerState.Idle:       stateMachine.EnterState<PlayerIdleState>(true);  break;
        case PlayerState.NormalAttack: stateMachine.EnterState<PlayerNormalAttackState>(true); break;
        // ...
    }
}
```

这种 Centralized Mediator 模式的好处是：**状态切换逻辑集中在一个地方，一眼就能看出什么条件触发什么状态**。代价是 SwitchState 会随着状态增多而膨胀。

---

## 五、各系统详解

### 5.1 状态机系统

#### 设计理念

这是一个**字典缓存 + 统一生命周期管理**的通用状态机。和传统手动 new 状态的方式不同，它用泛型 + 字典自动管理状态的创建、复用和销毁。

#### 核心类

| 类 | 类型 | 职责 |
|---|---|---|
| `IStateMachineOwner` | interface | 标记接口，标识"拥有状态机的对象" |
| `StateBase` | abstract class | 状态基类，定义 Init/UnInit/Enter/Exit/Update/FixedUpdate/LateUpdate |
| `StateMachine` | class | 状态机本体，字典缓存 + 生命周期管理 |

#### 类图

```
┌──────────────────────────────┐
│    <<interface>>              │
│    IStateMachineOwner         │
└──────────────────────────────┘
              △
              │ 实现
              │
┌─────────────┴──────────────┐
│      PlayerController       │
│      : IStateMachineOwner   │
└─────────────────────────────┘
              ◆
              │ 持有
              ▼
┌─────────────────────────────────────────────┐
│              StateMachine                   │
├─────────────────────────────────────────────┤
│ - owner: IStateMachineOwner                 │
│ - currentState: StateBase                   │
│ - stateDic: Dictionary<Type, StateBase>     │
├─────────────────────────────────────────────┤
│ + EnterState<T>(bool reload): void          │
│ + Clear(): void                             │
│ - LoadState<T>(): StateBase  ← 从字典取，   │
│   没有就 new 并缓存                           │
│ - EnterCurrentState(): void   ← 注册到       │
│   MonoManager 的 Update/FixedUpdate/LateUpdate│
│ - ExitCurrentState(): void    ← 从          │
│   MonoManager 注销                           │
└─────────────────────────────────────────────┘
              ◆
              │ 持有当前
              ▼
┌─────────────────────────────────────────────┐
│              StateBase (abstract)           │
├─────────────────────────────────────────────┤
│ + Init(owner): void           ← 获得 owner 引用│
│ + UnInit(): void              ← 清理资源      │
│ + Enter(): void               ← 进入状态      │
│ + Exit(): void                ← 退出状态      │
│ + Update(): void              ← 每帧逻辑      │
│ + FixedUpdate(): void                          │
│ + LateUpdate(): void                           │
└─────────────────────────────────────────────┘
              △
              │ 继承
              │
┌─────────────┴───────────────────────────────┐
│           PlayerStateBase                   │
│  + playerController: PlayerController       │
│  + playerModel: PlayerModel                 │
│  + statePlayTime: float                     │
│  + IsAnimationEnd(): bool                   │
│  + NormalizedTime(): float                  │
│  Update(): 重力+输入检测(切人)+计时          │
│  FixedUpdate(): 重力                         │
└─────────────────────────────────────────────┘
```

#### 核心流程

```
PlayerController.SwitchState(PlayerState.NormalAttack)
  │
  ├── playerModel.currentState = PlayerState.NormalAttack
  │
  └── stateMachine.EnterState<PlayerNormalAttackState>(true)
        │
        ├── 1. 检查是否需要退出
        │     if (currentState 存在 && 类型不同)
        │       ExitCurrentState()
        │         → currentState.Exit()
        │         → MonoManager.RemoveUpdateAction/RemoveFixedUpdate...（注销回调）
        │
        ├── 2. 加载/创建新状态
        │     LoadState<PlayerNormalAttackState>()
        │       → stateDic.TryGetValue(typeof(PlayerNormalAttackState), out state)
        │       → 如果没有: state = new PlayerNormalAttackState()
        │                  state.Init(owner)  ← 传入 PlayerController 引用
        │                  stateDic.Add(type, state)  ← 缓存到字典
        │
        └── 3. 进入新状态
              EnterCurrentState()
                → currentState = loadedState
                → currentState.Enter()
                → MonoManager.AddUpdateAction(currentState.Update)
                → MonoManager.AddFixedUpdateAction(currentState.FixedUpdate)
                → MonoManager.AddLateUpdateAction(currentState.LateUpdate)
```

#### 关键设计细节

**1. 字典缓存 + 懒加载**

```csharp
// StateMachine.cs:70-85
private StateBase LoadState<T>() where T : StateBase, new()
{
    Type stateType = typeof(T);
    if (!stateDic.TryGetValue(stateType, out StateBase state))
    {
        state = new T();              // 第一次访问才创建
        state.Init(owner);
        stateDic.Add(stateType, state);
    }
    return state;
}
```

所有状态只创建一次，后续切换时直接从字典取。这和 ZZZDemo 项目"构造时一次性 new 全部状态"不同——字典方式更省内存（懒加载），但 Dictionary 查找有微小开销。

**2. reloadState 参数的设计意图**

```csharp
public void EnterState<T>(bool reLoadState = false) where T : StateBase, new()
{
    if (HasState && currentState.GetType() == typeof(T) && !reLoadState)
        return;  // 相同状态不重复进入
    // ...
}
```

普通攻击连招是会重复进入同一个状态类型的——玩家砍完第 1 段后进入 `NormalAttackEnd`，再按键时又进入 `NormalAttack`。这时 `reLoadState=true` 强制重新触发 Enter（重播动画、重置计时器）。



### 5.2 输入系统

#### 概述

使用 Unity New Input System，键盘键位：

| 按键 | 功能 |
|---|---|
| WASD / 方向键 | 移动 |
| 鼠标左键 | 普通攻击（Fire） |
| 左 Shift | 闪避（Evade） |
| Q | 大招（BigSkill） |
| 空格 / 上下方向键（手柄 LB/RB）| 切人（SwitchDown/SwitchUp） |

#### 设计分析

项目没有用 Input System 的 C# 事件回调（`started/performed/canceled`），而是在每个状态的 `Update()` 里**直接轮询** `inputSystem.Player.Fire.triggered`。

```csharp
// PlayerIdleState.cs:39
if (playerController.inputSystem.Player.Fire.triggered)
{
    playerController.SwitchState(PlayerState.NormalAttack);
}
```

这种方式优缺点明显：
- 优点：代码简单直观，每个状态自己决定响应什么按键
- 缺点：每个状态的 Update 都有大量重复的按键检测代码

对比 ZZZDemo 的做法（在 Enter/Exit 中注册/注销回调），轮询方式避免了订阅泄漏的风险，但代码重复度高。

#### Input Action 配置

```json
Move:     Value, Vector2   ← 持续读取摇杆值
Fire:     Button           ← 攻击键（triggered = 按下瞬间的那一帧）
Evade:    Button           ← 闪避
BigSkill: Button           ← 大招
SwitchUp/Down: Button      ← 切人
```

注意：项目把攻击键命名为 `Fire`（射击），说明很可能是从 Unity 的第三人称模板改过来的。



### 5.3 玩家状态系统

#### 状态枚举

```csharp
// PlayerStateBase.cs:6-13
public enum PlayerState
{
    Idle, Idle_AFK,           // 待机 / 待机AFK（发呆动画）
    Walk, Run, RunEnd, TurnBack,  // 移动状态组
    Evade_Front, Evade_Back, Evade_Front_End, Evade_Back_End,  // 闪避状态组
    NormalAttack, NormalAttackEnd,  // 普通攻击状态组
    BigSkillStart, BigSkill, BigSkillEnd,  // 大招状态组
    SwitchInNormal            // 切人入场
}
```

注意 Front/Back、Start/End 都被拆成了**独立枚举值**，而不是用额外的字段标识方向或阶段。这样做的好处是 `SwitchState` 里的分支更清晰。

#### 状态转换图

```
                      ┌───────────┐
         动画结束      │           │ 3秒无操作
    ┌─────────────────→│   Idle    │──────────────→ Idle_AFK
    │                  │           │  动画结束
    │                  └─────┬─────┘←──────────────
    │       摇杆/攻击/闪避    │
    │         ┌──────────────┼──────────────┐
    │         ▼              ▼              ▼
    │    ┌─────────┐  ┌───────────┐  ┌───────────┐
    │    │  Walk   │  │NormalAtk  │  │  Evade    │
    │    │  /Run   │  │  Start    │  │ Front/Back│
    │    └────┬────┘  └─────┬─────┘  └─────┬─────┘
    │    无输入│        攻击结束│        动画结束│
    │         ▼              ▼              ▼
    │    ┌─────────┐  ┌───────────┐  ┌───────────┐
    │    │ RunEnd  │  │NormalAtk  │  │EvadeEnd   │
    │    │(刹车动画)│  │  End      │  │           │
    │    └────┬────┘  └─────┬─────┘  └─────┬─────┘
    │    动画结束│    动画结束/攻击│    动画结束/攻击│
    │         └──────────────┼──────────────┘
    │                        │
    │         Q键(任意状态)   │
    │         ┌──────────────┘
    │         ▼
    │    ┌──────────────┐
    │    │ BigSkillStart│ → BigSkill → BigSkillEnd
    │    └──────────────┘
    │
    │    方向键(可打断状态)
    └───── SwitchInNormal ──→ Idle
```

#### 连招系统（简化版）

连招逻辑极其简单，核心就是一个 `currentNormalAttackIndex` 计数器：

```csharp
// PlayerNormalAttackState.cs:18-19
playerController.PlayAnimation("Attack_Normal_" +
    playerModel.skiilConfig.currentNormalAttackIndex, 0.1f);
```

```csharp
// PlayerNormalAttackEndState.cs:36-45
if (playerController.inputSystem.Player.Fire.triggered)
{
    playerModel.skiilConfig.currentNormalAttackIndex++;
    if (currentIndex > normalAttackDamageMultiple.Length)
        currentIndex = 1;        // 超出段数归零
    playerController.SwitchState(PlayerState.NormalAttack);
}
```

**动画命名约定驱动连招**——动画文件按 `Attack_Normal_1`、`Attack_Normal_2`、`Attack_Normal_3` 命名，代码只负责拼接字符串。和 ZZZDemo 的 ScriptableObject 数据管线相比，这个方案配置成本几乎为零（不需要额外创建 SO），但完全依赖动画命名规范，没有类型安全。

**连招窗口判断**是硬编码的 `0.5f`：

```csharp
// PlayerNormalAttackState.cs:35
if (NormalizedTime() >= 0.5f && playerController.inputSystem.Player.Fire.triggered)
{
    enterNextAttack = true;  // 动画播到50%后才能预输入
}
```

这意味着策划改连招手感只能改代码，不能在编辑器里配置。

#### 移动状态（左右脚交替）

`PlayerRunState` 有一个精妙的小设计——**左右脚交替系统**。Walk 和 Run 动画分左脚步入和右脚步入两个版本，状态用 `playerModel.foot` 标记当前是哪只脚在前：

```csharp
// PlayerRunState.cs:24-51
switch (playerModel.foot)
{
    case ModelFoot.Left:
        playerController.PlayAnimation("Walk", 0.125f, 0.5f);   // 从动画50%处开始播
        playerModel.foot = ModelFoot.Right;
        break;
    case ModelFoot.Right:
        playerController.PlayAnimation("Walk", 0.125f, 0.0f);   // 从动画0%处开始播
        playerModel.foot = ModelFoot.Left;
        break;
}
```

这样从 Idle 进入 Walk 时，动画能无缝衔接——左脚进的动画播完正好是右脚在前，下次就播右脚进的动画。这是不依赖 Animator 复杂的 Blend Tree 配置，用纯代码实现的低成本方案。

#### 转向检测

```csharp
// PlayerRunState.cs:103
if (angles > 145f && angles < 215f && currentState == PlayerState.Run)
{
    playerController.SwitchState(PlayerState.TurnBack);
}
```

跑步方向与摇杆方向夹角超过 145° 时触发急停转身动画。阈值固定 145°-215°，不是从 ScriptableObject 读取。



### 5.4 角色切换系统

#### 数据流

```
PlayerConfig.asset (ScriptableObject)
  └── models: GameObject[]     ← 3个角色 Prefab 的引用
        │
        ▼ (Awake 时 Instantiate)
  controllableModels: List<PlayerModel>
        │
        ▼ (currentModelIndex 控制)
  playerModel: PlayerModel     ← 当前激活的角色
```

#### 切换流程

```csharp
// PlayerController.cs:129-153
public void SwitchNextModel()
{
    stateMachine.Clear();                      // 1. 清空当前状态机的全部状态
    playerModel.Exit();                        // 2. 当前角色播退场动画
    currentModelIndex++;                        // 3. 索引+1
    PlayerModel nextModel = controllableModels[currentModelIndex];
    nextModel.gameObject.SetActive(true);      // 4. 激活下一个模型
    playerModel = nextModel;
    playerModel.Enter(prevPos, prevRot);       // 5. 新角色定位+入场
    SwitchState(PlayerState.SwitchInNormal);   // 6. 进入入场状态
}
```

`PlayerModel.Enter()` 会计算新角色的出生位置（在当前角色右侧 0.8 单位、后方 4 单位）：

```csharp
// PlayerModel.cs:46-53
Vector3 rightDirection = rot * Vector3.right;
pos += rightDirection * 0.8f;      // 右侧偏移
Vector3 backDirection = rot * Vector3.back;
pos += backDirection * 4f;         // 后方偏移
characterController.Move(pos - transform.position);
```

这个硬编码的偏移值是一个潜在问题——不同角色的体型不同，固定的 0.8f/4f 偏移可能导致体型大的角色穿模或重叠。

**切人过程中旧角色的退场**是通过在 `Exit()` 里给 `MonoManager` 注册一个 Update 回调来检测退场动画是否播完：

```csharp
// PlayerModel.cs:63-67
public void Exit()
{
    animator.CrossFade("SwitchOut_Normal", 0.1f);
    MonoManager.INSTANCE.AddUpdateAction(OnExit);
}
public void OnExit()
{
    if (IsAnimationEnd())
    {
        gameObject.SetActive(false);             // 播完隐藏
        MonoManager.INSTANCE.RemoveUpdateAction(OnExit);  // 注销回调
    }
}
```



### 5.5 MonoManager 统一调度器

#### 设计动机

MonoBehaviour 的 Update 会在每个 GameObject 上独立调用，大量 MonoBehaviour 会导致性能问题。`MonoManager` 把所有的 Update 集中到一个 GameObject 上：

```csharp
// MonoManager.cs
public class MonoManager : SingleMomoBase<MonoManager>
{
    public Action updatAction;         // 所有 Update 的集合
    public Action fixedUpdatAction;
    public Action lateUpdatAction;

    void Update()
    {
        updatAction?.Invoke();         // 一次调用执行所有注册的回调
    }
}
```

状态进入时注册：
```csharp
MonoManager.INSTANCE.AddUpdateAction(currentState.Update);
```

状态退出时注销：
```csharp
MonoManager.INSTANCE.RemoveUpdateAction(currentState.Update);
```

这个设计的本质是一个**手写的 Update 管理器**，避免了每个状态挂一个 MonoBehaviour 的开销。但 Action 委托链过长（几百个回调）会有性能问题——好在状态数量少（同一时间只有一个状态在跑），实际影响可以忽略。



### 5.6 ScriptableObject 配置

#### PlayerConfig

```csharp
// PlayerConfig.cs
[CreateAssetMenu(menuName = "Config/Player Config")]
public class PlayerConfig : ScriptableObject
{
    public GameObject[] models;  // 拖入3个角色 Prefab
}
```

只存了一个 Prefab 数组，极其简单。

#### SkiilConfig

```csharp
// SkiilConfig.cs
[CreateAssetMenu(menuName = "Config/Skill")]
public class SkiilConfig : ScriptableObject
{
    [HideInInspector] public int currentNormalAttackIndex = 1;
    public float[] normalAttackDamageMultiple;  // 每段攻击的伤害倍率
}
```

⚠️ **潜在的严重 bug**：`[HideInInspector]` 的字段在运行时被修改（`currentNormalAttackIndex++`），这个修改会**写入 ScriptableObject 资源本身**！在编辑器中运行后，如果退出播放模式前没有把值重置回 1，它会永久保存修改后的值。

`PlayerModel.OnDisable()` 里做了重置：
```csharp
private void OnDisable()
{
    skiilConfig.currentNormalAttackIndex = 1;  // 退出播放模式前重置
}
```

这是针对这个 Unity SO bug 的标准修复方式，但依赖 `OnDisable` 被调用——如果编辑器崩溃或强制停止，重置不会执行。

---

### 5.7 摄像机系统

```csharp
// CameraManager.cs
public class CameraManager : SingleMomoBase<CameraManager>
{
    public CinemachineBrain cm_brain;
    public GameObject freeLookCanmera;
    public CinemachineFreeLook freeLook;

    public void ResetFreeLookCamera()
    {
        freeLook.m_YAxis.Value = 0.5f;
        freeLook.m_XAxis.Value = PlayerController.INSTANCE.playerModel.transform.eulerAngles.y;
    }
}
```

摄像机管理工作非常简单：
- 平时：Cinemachine FreeLook 摄像机跟随角色
- 大招开始：关掉 FreeLook，切到预先摆好的技能镜头（`bigSkillStartShot`/`bigSkillShot` GameObject），过渡风格设为 Cut（0 秒硬切）
- 大招结束：恢复 FreeLook，过渡风格设为 EaseInOut（1 秒淡入），同时 Reset 摄像机角度



### 5.8 重力系统

重力在 `PlayerStateBase.FixedUpdate()` 和 `Update()` 中**两处**都会施加：

```csharp
// PlayerStateBase.cs:40-43
public override void FixedUpdate()
{
    playerModel.characterController.Move(new Vector3(0, playerModel.gravity * Time.deltaTime, 0));
}

public override void Update()
{
    playerModel.characterController.Move(new Vector3(0, playerModel.gravity * Time.deltaTime, 0));
}
```

在 FixedUpdate 和 Update 里**同时**施加重力是错误的——FixedUpdate 的 `Time.deltaTime` 等于 `Time.fixedDeltaTime`，Update 的 `Time.deltaTime` 是实际帧间隔。两处都加意味着重力会被**双倍施加**。正确的做法是只在 FixedUpdate 中处理物理。

---

## 六、设计模式总结

| 模式 | 应用位置 | 说明 |
|---|---|---|
| **状态模式** | StateMachine + StateBase | 核心骨架 |
| **单例模式** | SingleMomoBase → MonoManager/CameraManager/PlayerController | 全局访问点 |
| **中介者模式** | PlayerController.SwitchState() | 集中管理状态切换 |
| **模板方法** | StateBase 定义生命周期，PlayerStateBase 填充公共逻辑 | 复用代码 |
| **对象池（变体）** | StateMachine.stateDic 字典缓存 | 状态对象复用 |

---

## 七、优缺点分析

### 优点

1. **学习曲线平缓**：状态模式 + 单例管理器，没有任何高级技巧，适合初学者理解 FSM 结构
2. **代码即文档**：每个状态类就是一个状态，类名直接说明功能，不需要额外注释
3. **状态切换一目了然**：`PlayerController.SwitchState()` 集中了全部状态切换逻辑
4. **动画命名约定简洁**：`Attack_Normal_{index}` 的命名规则避免了额外配置
5. **左右脚交替系统**：用最小代码量解决了移动动画衔接问题

### 问题

1. **代码重复严重**：每个状态的 Update 都有几乎一样的按键检测代码块（攻击/闪避/大招/切人）
2. **硬编码过多**：连招窗口 `0.5f`、转向角度 `145°-215°`、摄像机重置值 `0.5f`、角色出生偏移 `0.8f/4f`
3. **重力重复施加**：Update 和 FixedUpdate 都调了重力，会导致下落速度翻倍
4. **SO 运行时污染**：`SkiilConfig.currentNormalAttackIndex` 在运行时被修改，依赖 OnDisable 重置
5. **没有伤害判定**：按了攻击键只是播动画，没有任何伤害检测逻辑
6. **没有敌人 AI**：`EnemyStateBase` 是空壳
7. **切人无 CD**：可以无限切人，没有冷却限制
8. **摄像机未处理碰撞**：FreeLook 没有配置碰撞检测，穿墙时摄像机会跑到场景外

---

## 八、与参考项目的架构对比

| 维度 | Practice_ACTGame | ZZZDemo（参考B） |
|---|---|---|
| 文件数 | ~25 | 174 |
| 状态机 | 单层，字典缓存 | 双状态机并行（移动+战斗） |
| 状态切换 | 代码轮询 `IsAnimationEnd()` | Animation Event 驱动 + StateMachineBehaviour |
| 输入 | Update 中轮询 `triggered` | Enter/Exit 中注册/注销回调 |
| 连招 | int 计数器 + 动画命名约定 | ScriptableObject 完整数据管线 + 预输入缓冲 |
| 数据架构 | SO 直接存运行时状态（有 bug） | ReusableData 纯 C# 分离运行时数据 |
| 伤害系统 | 无 | SphereCast + 事件广播 + 顿帧 + 震屏 |
| 摄像机 | Cinemachine FreeLook | 自定义 CameraHitFeel（震动/顿帧/慢动作） |
| 对象池 | StateMachine.stateDic（状态复用） | VFX/SFX 独立对象池系统 |
| 通信 | 单例直接引用 | 事件总线 + BindableProperty + 黑板系统 |

Practice_ACTGame 是 ZZZDemo 的**前期练习版本**——有一些共同的设计思想（状态模式、SO 配置、角色切换），但实现深度差了一个数量级。

---

## 九、关键文件索引

| 文件 | 用途 | 建议先读 |
|---|---|---|
| `Scripts/Base/StateBase.cs` | 状态基类定义 | ★ |
| `Scripts/StateMachine/StateMachine.cs` | 状态机核心 | ★ |
| `Scripts/Player/PlayerController.cs` | 中央调度器 | ★ |
| `Scripts/Base/PlayerStateBase.cs` | 玩家状态基类 | ★ |
| `Scripts/Player/PlayerModel.cs` | 角色模型 | |
| `Scripts/Manager/MonoManager.cs` | Update 调度器 | |
| `Scripts/Manager/CameraManager.cs` | 摄像机 | |
| `Scripts/Player/State/Idle/PlayerIdleState.cs` | 最简单状态的示例 | |
| `Scripts/Player/State/NormalAttack/PlayerNormalAttackState.cs` | 攻击状态（含预输入窗口） | |
| `Scripts/Player/State/Run/PlayerRunState.cs` | 移动状态（含转向+左右脚） | |
| `Scripts/Player/State/BigSkill/PlayerBigSkillStartState.cs` | 大招（含摄像机切换） | |
| `Scripts/Config/SkiilConfig.cs` | SO 配置（注意 bug） | |
| `Scripts/Base/SingleMomoBase.cs` | 单例基类 | |

---

## 十、无法确认的部分

以下内容无法从代码中确认，需要在 Unity 编辑器中查看：
1. Animator Controller 的具体结构和 Transition 连线（Has Exit Time 设置、条件参数）
2. 每个动画片段的具体时长和 Animation Event 放置
3. Cinemachine FreeLook 的 Rig 配置（轨道半径、高度、阻尼）
4. Prefab 的层级结构（`bigSkillStartShot`、`bigSkillShot` 子物体的位置）
5. 角色模型的骨骼结构和 Avatar 配置
6. `PlayerConfig` 和 `SkiilConfig` 资产在 Inspector 中的实际引用值
