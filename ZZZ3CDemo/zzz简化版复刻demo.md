# Practice_ACTGame_ZZZ 架构分析笔记

## 一、项目概览

| 维度 | 说明 |
|------|------|
| **项目类型** | 第三人称动作游戏角色控制器原型 |
| **参考原型** | 绝区零 (Zenless Zone Zero) |
| **Unity 版本** | 2022.3.62f3 |
| **渲染管线** | URP 12.1.7 |
| **核心玩法** | 多角色切换 + 5段普攻连段 + 前/后闪避 + 必杀技演出 |
| **可操控角色** | 安比(Anbi)、珂琳(Corin)、妮可(Nike) 三人 |
| **项目性质** | 学习项目（无完整游戏循环，无伤害判定，无敌人 AI） |

---

## 二、技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| Unity | 2022.3.62f3 | 引擎 |
| URP | 12.1.7 | 渲染管线 |
| Cinemachine | 2.8.9 | 相机系统 (FreeLook + VirtualCamera) |
| Input System | 1.4.4 | 输入管理 (Player + UI Action Maps) |
| ProBuilder | 5.0.7 | 关卡白盒搭建 |
| AmplifyShaderEditor | (依赖存在) | 自定义 Shader 编辑（代码中未直接引用） |

---

## 三、目录结构与模块划分

```
Assets/
├── Scripts/
│   ├── Base/
│   │   ├── StateBase.cs              # 纯 C# 状态抽象基类
│   │   ├── PlayerStateBase.cs        # 玩家状态基类（重力/计时/角色切换）
│   │   ├── EnemyStateBase.cs         # 敌人状态空壳（MonoBehaviour）
│   │   └── SingleMomoBase.cs        # MonoBehaviour 单例模式基类
│   ├── StateMachine/
│   │   └── StateMachine.cs           # 泛型纯 C# FSM（字典缓存）
│   ├── Player/
│   │   ├── PlayerController.cs       # 玩家入口（单例+IStateMachineOwner）
│   │   ├── PlayerModel.cs            # 角色模型数据（Animator/CC持有者）
│   │   ├── FollowPoint.cs            # 跟随指定 Transform（相机用）
│   │   ├── TargetPoint.cs            # 跟随 PlayerController 位置（场景用）
│   │   └── State/
│   │       ├── Idle/PlayerIdleState.cs           # Idle + Idle_AFK 合一
│   │       ├── Run/PlayerRunState.cs             # Walk + Run + 转身检测 合一
│   │       ├── Run/PlayerRunEndState.cs          # 跑步减速停止
│   │       ├── Run/PlayerTurnBackState.cs        # 180° 急转动画
│   │       ├── NormalAttack/PlayerNormalAttackState.cs      # 连段 + 预输入
│   │       ├── NormalAttack/PlayerNormalAttackEndState.cs   # 收招 + 继续连段
│   │       ├── Evade/PlayerEvadeState.cs         # 闪避（前/后）
│   │       ├── Evade/PlayerEvadeEndState.cs      # 闪避收招
│   │       ├── BigSkill/PlayerBigSkillStartState.cs  # 必杀起手（切镜）
│   │       ├── BigSkill/PlayerBigSkillState.cs       # 必杀进行中
│   │       ├── BigSkill/PlayerBigSkillEndState.cs    # 必杀收招（恢复镜）
│   │       └── SwitchIn/PlayerSwitchInNormalState.cs # 角色入场
│   ├── Manager/
│   │   ├── MonoManager.cs            # 统一 Update 调度中心
│   │   └── CameraManager.cs          # Cinemachine 相机控制
│   └── Config/
│       ├── PlayerConfig.cs           # SO：玩家角色 Prefab 列表
│       └── SkiilConfig.cs            # SO：普攻段数和伤害倍率
│
├── Config/
│   ├── Player Config.asset           # 安比+珂琳+妮可 Prefab 引用
│   └── Skill/
│       ├── Anbi Skill Config.asset   # 4段普攻，倍率全 0（未配置）
│       ├── Corin Skill Config.asset  # 5段普攻，倍率全 0（未配置）
│       └── Nike Skill Config.asset   # 3段普攻，倍率全 0（未配置）
│
├── Perfabs/
│   ├── Anbi.prefab                    # 5748 行，含骨骼+SkinnedMesh+Shots
│   ├── Corin.prefab                   # 7804 行，含骨骼+SkinnedMesh+Shots
│   └── Nostradamus_Model_CN.prefab   # 妮可（Nike）模型 Prefab
│
├── Arts/
│   ├── Anbi/Anbi.controller           # AnimatorController（全状态无Transition）
│   ├── Corin/Corin.controller         # AnimatorController（全状态无Transition）
│   └── Nike/Nike.controller           # AnimatorController
│
├── Settings/
│   └── Input System/
│       ├── Input System.inputactions  # Input Action 定义
│       └── Input System.cs            # 自动生成 C# 封装
│
└── Scenes/
    └── SampleScene.unity             # 主场景
```

---

## 四、场景层级结构

```
SampleScene
├── Environment        ← ProBuilder 白盒（Cube, Plane, Stairs 等）
├── Manager            ← MonoManager 组件【单例】
├── Main Camera        ← Camera + AudioListener + UniversalAdditionalCameraData + CinemachineBrain
│                         Tag: MainCamera, FOV: 40, Position: (0, 1.215, -3.85)
│                         CinemachineBrain: UpdateMethod=SmartUpdate, BlendUpdateMethod=LateUpdate
│                         默认 Blend: Style=Cut, Time=2s
├── Directional Light
├── Cameras            ← CameraManager 组件【单例】
│   │                    cm_brain→MainCamera, freeLookCanmera→CM FreeLook1, freeLook→CM FreeLook1
│   ├── CM FreeLook1   ← CinemachineFreeLook + CinemachineInputProvider + CinemachineCollider
│   │   │                Priority: 10, FOV: 40, LookAt/Follow→TargetPoint
│   │   │                Y Axis: 0.5 (default), MaxSpeed 0.005, Invert
│   │   │                X Axis: 0, MaxSpeed 0.5, Wrap
│   │   │                Orbits: Top(1.79, 3.43) / Mid(0.1, 3.85) / Bot(-1.05, 3.42)
│   │   │                Collider: Radius 0.1, Smoothing 0.372, Strategy=PullCameraForward
│   │   ├── TopRig
│   │   ├── MiddleRig
│   │   └── BottomRig
├── Player             ← PlayerController 组件【单例】
│   │                    rotationSpeed: 8, playerConfig→"Player Config.asset"
│   └── TargetPoint    ← TargetPoint 组件，LocalPosition (0, 1.115, 0)
│                        CM FreeLook 的 Follow/LookAt 目标
├── Global Volume      ← URP Volume（后处理）
└── Plant              ← ProBuilder 植物/装饰
```

---

## 五、角色 Prefab 结构（以 Corin.prefab 为例）

```
Corin (Root GameObject)
├── Animator           ← ApplyRootMotion: ✅ 启用
│                         UpdateMode: Normal
│                         CullingMode: CullUpdateTransforms
│                         Avatar: Corin Avatar
│                         Controller: Corin.controller
├── CharacterController ← Height: 1.6, Radius: 0.2, Center: (0, 0.8, 0)
│                         SlopeLimit: 45°, StepOffset: 0.3, SkinWidth: 0.0001
├── PlayerModel        ← gravity: -9.8, skiilConfig→Corin Skill Config
│                         bigSkillStartShot→BigSkillStart Shot GO
│                         bigSkillShot→BigSkill Shot GO
├── Bip001 (根骨骼)     ← 3ds Max Biped 骨骼命名规范
│   ├── Bip001 Pelvis
│   ├── Bip001 Spine → Spine1 → Spine2 → ...
│   ├── Bip001 L Thigh → L Calf → L Foot → L Toe0
│   ├── Bip001 R Thigh → R Calf → R Foot → R Toe0
│   └── ... (手指、头部等完整骨骼)
├── Corin_body         ← SkinnedMeshRenderer (身体模型)
├── Corin_Weapon       ← 武器模型层级
├── ... (头发、裙子等子物体)
└── Shots              ← 必杀技镜头容器
    ├── BigSkillStart Shot ← CinemachineVirtualCamera, Priority: 10
    │   │   FOV: 60 (Corin) / 38 (Anbi), Dutch: 14° (Corin) / 未设置 (Anbi)
    │   │   FarClipPlane: 500, Follow/LookAt→BigSkillStart target
    │   └── cm ← CinemachineComposer + CinemachineFramingTransposer
    │              FramingTransposer: FollowOffset (0, 0, -0.6), all damping=0
    ├── BigSkillStart target ← FollowPoint 组件，Target→Bip001
    │                           LocalPos (0.281, 0.925, 0.231), 朝向 -212°
    ├── BigSkill Shot    ← CinemachineVirtualCamera, Priority: 10
    │   │   FOV: 50, FarClipPlane: 5000, Follow/LookAt→BigSkill target
    │   └── cm ← CinemachineComposer + CinemachineFramingTransposer
    │              FramingTransposer: FollowOffset (0, 0, -3), ZDamping=1
    ├── BigSkill target  ← FollowPoint 组件，Target→Bip001
    │                       LocalPos (0, 0.85, 0)
    ├── 02               ← 空 Transform 标记点（可能是备用机位）
    └── 03               ← 空 Transform 标记点
```

---

## 六、核心架构

### 6.1 架构总览图

```
┌──────────────────────────────────────────────────────────────────────┐
│                          MonoManager (单例)                           │
│                    Action updatAction / fixedUpdatAction              │
│                    Action lateUpdatAction                             │
│              Update() → updatAction?.Invoke()                        │
│              FixedUpdate() → fixedUpdatAction?.Invoke()              │
└──────┬───────────────────────────────────────────────────────────────┘
       │ 注册/注销 Action 委托
       │
┌──────▼───────────────────────────────────────────────────────────────┐
│                    PlayerController (单例)                             │
│              实现 IStateMachineOwner                                   │
│  ┌─────────────────────┐    ┌─────────────────────┐                  │
│  │   InputSystem        │    │   StateMachine       │                  │
│  │   (Input Actions)    │    │  ┌─────────────────┐│                  │
│  │   Move / Fire        │    │  │ stateDic         ││                  │
│  │   Evade / BigSkill   │    │  │ <Type,StateBase> ││                  │
│  │   SwitchUp/Down      │    │  │ 缓存所有状态实例  ││                  │
│  └─────────────────────┘    │  │ currentState     ││                  │
│                              │  └─────────────────┘│                  │
│  ┌─────────────────────┐    └─────────────────────┘                  │
│  │ controllableModels   │                                             │
│  │ List<PlayerModel>    │──→ PlayerModel[0] (当前控制角色)             │
│  │ [0]=Anbi, [1]=Corin  │    PlayerModel[1] (待机)                    │
│  └─────────────────────┘    PlayerModel[2] (待机)                    │
└──────────────────────────────────────────────────────────────────────┘
       │
       │ 每个 PlayerModel 拥有:
       │   ├── Animator              (动画播放, ApplyRootMotion=1)
       │   ├── CharacterController   (移动 & 重力)
       │   ├── SkiilConfig (SO)      (技能数据)
       │   ├── bigSkillStartShot     (必杀起手镜头 VCam)
       │   └── bigSkillShot          (必杀镜头 VCam)
       │
       ▼
┌──────────────────────────────────────────────────────────────────────┐
│                      PlayerStateBase (抽象)                           │
│  持有 playerController / playerModel 引用                             │
│  Update() 基类处理: 重力 + 计时 + 角色切换输入                          │
│  FixedUpdate() 基类处理: 重力                                          │
│  提供 IsAnimationEnd() / NormalizedTime() 工具方法                     │
└──────┬───────────────────────────────────────────────────────────────┘
       │ 继承
       ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ IdleState    │ │  RunState    │ │ AttackState  │ │  EvadeState  │
│ (含 AFK)     │ │(Walk+Run+    │ │(连段+收招)   │ │(前闪+后闪+   │
│              │ │ TurnBack+    │ │              │ │ 收招)        │
│              │ │ RunEnd)      │ │              │ │              │
└──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘
┌──────────────┐ ┌──────────────┐
│ BigSkill     │ │ SwitchIn     │
│ (起手+进行+  │ │ (入场动画)   │
│  收招)       │ │              │
└──────────────┘ └──────────────┘
```

### 6.2 核心设计决策

#### 决策 1：将所有 Update 统一收敛到 MonoManager

这是这个项目**最核心的设计决策**。状态类（StateBase 的子类）全都是纯 C# 类，不继承 MonoBehaviour。它们不能自己 Update。

**做法**: StateMachine 在 Enter/Exit 状态时，把当前状态的 `Update()`/`FixedUpdate()`/`LateUpdate()` 作为 Action 委托注册/注销到 `MonoManager` 中。MonoManager 的 Update/FixedUpdate/LateUpdate 统一调用所有已注册的委托。

**好处**:
- 状态类是纯 C# 对象，可被 `new()` 创建，不依赖 GameObject
- 所有 Update 集中管理，可统一添加 Profiler、暂停控制
- 避免每个 MonoBehaviour 各自 Update 的分散管理

**代价**:
- 委托调用有轻微性能开销
- 必须严格管理注册/注销，漏注销会导致逻辑错误

#### 决策 2：字典缓存状态实例，懒加载

`StateMachine.LoadState<T>()` 使用 `Dictionary<Type, StateBase>` 缓存。首次进入时 `new T()` 创建并存入字典，之后复用。

**方案对比**:
- 方案 A（本项目）：首次进入时创建，之后缓存复用 → 内存换 GC
- 方案 B：初始化时一次性 new 所有状态 → 零 GC，但需提前知道所有状态类型
- 方案 C：每次切换都 new → 大量 GC，不可取

#### 决策 3：Animator Controller 不作为逻辑控制器

**所有 `m_Transitions: []` 为空！** Animator Controller 中没有任何 Transition，没有任何参数（Bool/Trigger/Float/Int 全无）。动画切换完全由 C# 代码通过 `CrossFadeInFixedTime()` 驱动。

**好处**: 所有切换逻辑在代码中显式可读，不需要在 Animator 窗口中查连线。
**代价**: 完全放弃了 Unity 的 Transition 系统（Has Exit Time、Conditions 等），需要自己管理过渡时长和时机。

#### 决策 4：状态与动画一对多映射

`PlayerIdleState` 同时处理 `PlayerState.Idle` 和 `PlayerState.Idle_AFK`（闲置动画），`PlayerRunState` 同时处理 `PlayerState.Walk` 和 `PlayerState.Run`。减少类数量，避免拆成多个几乎一样的类。代价是状态类内部 switch/if 变多。

#### 决策 5：Root Motion + 代码重力

`m_ApplyRootMotion: 1`（启用），动画驱动角色水平位移。但垂直方向的重力由 `PlayerStateBase` 中通过 `characterController.Move` 单独施加。这是典型的 "用代码接管 Root Motion 的 Y 轴" 做法，确保角色无论动画如何都能正确着地。

---

## 七、系统逐一详解

### 7.1 状态机系统

#### 类关系图

```
┌──────────────────────────┐
│   <<interface>>          │
│   IStateMachineOwner     │
│   (标记接口，无成员)       │
└──────────────────────────┘
            △
            │ 实现
            │
┌───────────┴────────────┐
│   PlayerController      │
│   (同时是单例)           │
└──────────────────────────┘
            ◆
            │ 持有
            ▼
┌──────────────────────────────────────────┐
│              StateMachine                 │
├──────────────────────────────────────────┤
│ - currentState : StateBase               │
│ - owner : IStateMachineOwner             │
│ - stateDic : Dictionary<Type, StateBase> │
├──────────────────────────────────────────┤
│ + EnterState<T>(reLoadState)  ← 切换状态  │
│ - LoadState<T>()              ← 懒加载    │
│ - EnterCurrentState()         ← 注册委托  │
│ - ExitCurrentState()          ← 注销委托  │
│ + Clear()                     ← 全部清理  │
└──────┬───────────────────────────────────┘
       │
       │ 持有
       ▼
┌──────────────────────────────────────────┐
│           StateBase (abstract)            │
├──────────────────────────────────────────┤
│ + Init(owner)          (abstract)         │
│ + UnInit()             (abstract)         │
│ + Enter()              (abstract)         │
│ + Exit()               (abstract)         │
│ + Update()             (abstract)         │
│ + FixedUpdate()        (abstract)         │
│ + LateUpdate()         (abstract)         │
└──────┬───────────────────────────────────┘
       △
       │ 继承
       │
┌──────┴───────────────────────────────────────────┐
│              PlayerStateBase                      │
├───────────────────────────────────────────────────┤
│ + playerController : PlayerController  ← 通过Init注入│
│ + playerModel : PlayerModel                       │
│ + statePlayTime : float            ← 状态已持续时长│
│ - stateInfo : AnimatorStateInfo    ← 当前动画信息  │
├───────────────────────────────────────────────────┤
│ + Enter()         → statePlayTime=0, base.Enter() │
│ + Update()        → 重力 + 计时 + 角色切换输入检测  │
│ + FixedUpdate()   → 重力                           │
│ + IsAnimationEnd()  → normalizedTime>=1 && !IsInTransition│
│ + NormalizedTime()  → 当前动画归一化时间            │
└───────────────────────────────────────────────────┘
```

#### 状态切换流程

1. 各状态类调用 `playerController.SwitchState(PlayerState.XXX)`
2. `SwitchState` 在 `playerModel.currentState` 记录新枚举值
3. 通过 `switch(playerState)` 映射到具体状态类，调用 `stateMachine.EnterState<T>(reLoadState)`
4. `StateMachine.EnterState<T>()`:
   - 如果当前状态类和目标相同且 `reLoadState==false`，直接返回
   - 否则 Exit 当前状态（注销所有 Update 委托），从字典加载/创建目标状态，Enter 新状态（注册所有 Update 委托）

#### `reLoadState` 的使用场景

- `SwitchState(Idle)` → `EnterState<PlayerIdleState>(true)` — Idle 和 Idle_AFK 是同一个类，需强制重载
- `SwitchState(RunEnd)` → `EnterState<PlayerRunEndState>()` — 不需要重载，只有一个类实例

#### 状态生命周期

```
Init(owner) → Enter() → Update()/FixedUpdate()/LateUpdate() 循环 → Exit() → ... → UnInit()
```

- `Init`: 在首次创建时调用，只执行一次，注入 owner 引用
- `Enter`: 每次进入时调用，重置 `statePlayTime = 0`
- `Update/FixedUpdate/LateUpdate`: 每帧由 MonoManager 驱动
- `Exit`: 每次离开时调用（当前实现为空）
- `UnInit`: 在 `StateMachine.Clear()` 时调用（当前实现为空）

#### ⚠️ 已知 Bug：重力被施加两次

`PlayerStateBase.Update()` 和 `FixedUpdate()` 都执行了 `characterController.Move(new Vector3(0, gravity * Time.deltaTime, 0))`。实际下落加速度约为 `-9.8 * 2 = -19.6`。**修复**: 只在 `FixedUpdate` 中施加重力，`Update` 中删除重力逻辑。

---

### 7.2 MonoManager — 统一 Update 调度

#### 实现

```csharp
// MonoManager 核心 (Assets\Scripts\Manager\MonoManager.cs)
public class MonoManager : SingleMomoBase<MonoManager>
{
    public Action updatAction;          // Update 委托链
    public Action fixedUpdatAction;     // FixedUpdate 委托链
    public Action lateUpdatAction;      // LateUpdate 委托链

    void Update()         => updatAction?.Invoke();
    void FixedUpdate()    => fixedUpdatAction?.Invoke();
    void LateUpdate()     => lateUpdatAction?.Invoke();
}
```

注册/注销由 StateMachine 在切换状态时自动完成：

```
EnterCurrentState():  注册 currentState 的 Update/FixedUpdate/LateUpdate
ExitCurrentState():   注销 currentState 的 Update/FixedUpdate/LateUpdate
```

每次切换状态都完整注销旧状态、完整注册新状态，保证 "当前只有一个状态在 Update"。

---

### 7.3 多角色切换系统

#### 数据流

```
PlayerConfig (SO)
  └── models: GameObject[]  ← Inspector 中拖入 3 个角色 Prefab
        │
        ▼
PlayerController.Awake()
  └── 遍历 models[], Instantiate 到自身下, 获取 PlayerModel 组件
  └── 全部 SetActive(false), 只激活第一个
        │
        ▼
controllableModels: List<PlayerModel>
  [0] = Anbi (当前控制)
  [1] = Corin (待机)
  [2] = Nike  (待机)
```

#### 切换流程（SwitchNextModel）

```
输入触发 (SwitchDown/SwitchUp) → PlayerStateBase.Update() 基类检测
  → playerController.SwitchNextModel()
    → ① stateMachine.Clear()                         ← 清空所有状态
    → ② playerModel.Exit()                            ← 播放退场动画
    → ③ currentModelIndex++ (循环)                     ← 选择下一个角色
    → ④ nextModel.gameObject.SetActive(true)          ← 激活新角色
    → ⑤ 保存当前位置/旋转                               ← prevPos, prevRot
    → ⑥ playerModel = nextModel                       ← 切换引用
    → ⑦ playerModel.Enter(prevPos, prevRot)            ← 设置新角色位置(偏移)
        → Enter() 内部:
          - 移除之前的 OnExit 委托
          - 计算偏移: right*0.8 + back*4 → 出现在旧角色右后方
          - characterController.Move(偏移量) → 瞬移至新位置
    → ⑧ SwitchState(SwitchInNormal)                    ← 播放入场动画
```

#### 退场自清理

`PlayerModel.OnExit()` 注册到 MonoManager，每帧检测 `IsAnimationEnd()`，动画播完自动 `SetActive(false)` + 移除自身委托。

#### 切换限制

BigSkillStart 和 BigSkill 状态期间，`PlayerStateBase.Update()` 中的切换输入检测被屏蔽。

---

### 7.4 输入系统

#### Action 映射

| Action | 类型 | 键盘 | 手柄 |
|--------|------|------|------|
| Move | Vector2 | WASD / 方向键 | 左摇杆 |
| Look | Vector2 | 鼠标 Delta | 右摇杆 |
| Fire | Button | 鼠标左键 | X(□) |
| Evade | Button | 左Shift | A(×) |
| BigSkill | Button | Q | RT |
| SwitchUp | Button | (键盘未绑定) | LB |
| SwitchDown | Button | 空格 | RB |

#### 检测方式

- `.triggered` — 绝大多数 Button Action 使用，仅按下瞬间为 true
- `.IsPressed()` — 仅在 `PlayerRunEndState` 闪避检测中使用，持续为 true
- `ReadValue<Vector2>()` — Move Action 每帧在 `PlayerController.FixedUpdate()` 中读取

#### 相机输入

CinemachineFreeLook 通过 `CinemachineInputProvider` 独立处理 Look 输入，不经过 PlayerController 代码。

---

### 7.5 连招系统（普攻）

#### 数据层

SkiilConfig (SO) 每角色独立：
- Corin: 5段，`normalAttackDamageMultiple: [0, 0, 0, 0, 0]`（伤害未配置）
- Anbi: 4段，`normalAttackDamageMultiple: [0, 0, 0, 0]`
- Nike: 3段，`normalAttackDamageMultiple: [0, 0, 0]`

`currentNormalAttackIndex` 标记当前打到第几段，运行时直接修改 SO 上的值（OnDisable 时重置为 1）。

#### 连招流程

```
按 Fire → SwitchState(NormalAttack)
  → Enter(): PlayAnimation("Attack_Normal_1", 0.1f), enterNextAttack=false

Update() 中:
  NormalizedTime >= 0.5 (动画过半)
    如果 Fire.triggered: enterNextAttack = true  ← 预输入缓冲

  IsAnimationEnd():
    若 enterNextAttack:
      currentNormalAttackIndex++ → SwitchState(NormalAttack)  ← 进下一段
    否则:
      SwitchState(NormalAttackEnd)  ← 进收招

NormalAttackEnd 中:
  按 Fire: currentIndex++ → 重新进 NormalAttack  ← 收招中可继续连段
  按移动/闪避/动画结束: currentIndex=1  ← 重置段数
```

#### 设计特点

- **预输入窗口**: 动画 50% 后按攻击排队进入下一段
- **收招可连段**: 收招状态中按攻击继续累加段数
- **段数循环**: 超出数组长度后重置为 1
- **所有退出路径重置段数**: 移动/闪避/动画结束 → currentIndex=1

#### ⚠️ 问题

- `currentNormalAttackIndex` 直接存在 SO 上，运行时修改会污染资源
- 50% 窗口硬编码，不可配置
- 50% 之前按攻击会丢弃输入

---

### 7.6 闪避系统

#### 状态映射

- `Evade_Front` / `Evade_Back` → `PlayerEvadeState`（一个类处理两个方向）
- `Evade_Front_End` / `Evade_Back_End` → `PlayerEvadeEndState`

#### CD 机制

`evadeTimer` 初始为 1（可用），使用后归零，每帧 `+= Time.deltaTime` 恢复，最大 1（即 1 秒 CD）。在 `PlayerController.FixedUpdate()` 中累积。

#### 方向判断

由调用方决定：Idle/RunEnd 状态按闪避 → 后闪；Run 状态按闪避 → 前闪。

---

### 7.7 必杀技系统

#### 三阶段流程

```
BigSkillStart → BigSkill → BigSkillEnd (全由 IsAnimationEnd() 自动推进)
```

#### 镜头切换详解

| 阶段 | BigSkillStart | BigSkill | BigSkillEnd |
|------|---------------|----------|-------------|
| 主相机 Blend | Style.Cut + 0f | (保持) | Style.EaseInOut + 1f |
| FreeLook | SetActive(false) | (保持) | SetActive(true) |
| 起手镜头 | SetActive(true) | SetActive(false) | (off) |
| 必杀镜头 | (off) | SetActive(true) | SetActive(false) |
| 相机重置 | - | - | Y=0.5, X=角色朝向 |

#### 必杀镜头参数（角色差异）

| 参数 | Corin BigSkillStart | Corin BigSkill | Anbi BigSkillStart | Anbi BigSkill |
|------|--------------------|----------------|---------------------|---------------|
| FOV | **60** | 50 | **38** | 50 |
| Dutch | **14°** | 0 | 0 | 0 |
| FarClipPlane | 500 | 5000 | (默认) | (默认) |
| FollowOffset Z | -0.6 | **-3** | -0.6 | **-3** |
| ZDamping | 0 | **1** | 0 | 0 |

每个角色的必杀镜头经过独立调优。Corin 的 BigSkillStart 有 14° Dutch 倾斜 + FOV 60 广角制造紧张感；Anbi 的 BigSkillStart FOV 38 更聚焦、不倾斜。

> **镜头 Follow 机制**: BigSkillStart target 和 BigSkill target 是 FollowPoint 组件（`Assets\Scripts\Player\FollowPoint.cs`），在 `LateUpdate` 中跟随角色 Bip001 根骨骼位置。

#### 必杀技期间的输入限制

BigSkillStart/BigSkill 状态屏蔽角色切换输入。BigSkillEnd 允许攻击/移动/闪避打断收招动画。

---

### 7.8 跑步系统（含转身）

#### Walk → Run 自动过渡

Walk 状态持续超过 3 秒后自动切换为 Run。

#### 脚位追踪

```csharp
// PlayerModel
public ModelFoot foot = ModelFoot.Left;

// PlayerRunState.Enter(): 根据脚位决定动画起始帧
case ModelFoot.Left:
    PlayAnimation("Walk", 0.125f, 0.5f);    // 从动画 50% 处开始
    playerModel.foot = ModelFoot.Right;
    break;
case ModelFoot.Right:
    PlayAnimation("Walk", 0.125f, 0.0f);    // 从动画 0% 处开始
    playerModel.foot = ModelFoot.Left;
    break;
```

确保停步后重走时从正确的脚位继续，自然衔接步态。

#### 180° 转身检测

```csharp
float angles = Mathf.Abs(targetQua.eulerAngles.y - playerModel.transform.eulerAngles.y);
if (angles > 145f && angles < 215f && currentState == PlayerState.Run)
    SwitchState(PlayerState.TurnBack);  // 播放转身动画
```

仅在 Run 状态下触发。Walk 状态下直接 Slerp 旋转。

---

### 7.9 相机系统

#### CameraManager

```csharp
// Assets\Scripts\Manager\CameraManager.cs
public class CameraManager : SingleMomoBase<CameraManager>
{
    public CinemachineBrain cm_brain;
    public GameObject freeLookCanmera;          // CM FreeLook1 的 GameObject
    public CinemachineFreeLook freeLook;

    public void ResetFreeLookCamera()
    {
        freeLook.m_YAxis.Value = 0.5f;
        freeLook.m_XAxis.Value = PlayerController.INSTANCE.playerModel.transform.eulerAngles.y;
    }
}
```

#### CinemachineFreeLook 精确配置

| 参数 | 值 |
|------|-----|
| Priority | 10 |
| FOV | 40 |
| Binding Mode | World Space |
| Spline Curvature | 0.2 |
| **Top Rig** | Height: 1.79, Radius: 3.43 |
| **Middle Rig** | Height: 0.1, Radius: 3.85 |
| **Bottom Rig** | Height: -1.05, Radius: 3.42 |
| Y Axis Speed | 0.005 (非常慢) |
| X Axis Speed | 0.5 |
| X Axis Wrap | ✅ |
| Y Axis Invert | ✅ |
| Collider Radius | 0.1 |
| Collider Smoothing | 0.372s |
| Collider Strategy | PullCameraForward |

#### FollowPoint / TargetPoint

- `TargetPoint` (场景中，y=1.115) — CM FreeLook 的 Follow 和 LookAt 共同指向的目标。LateUpdate 中跟随 `PlayerController.INSTANCE.playerModel.transform.position`。
- `FollowPoint` (Prefab 内，必杀镜头用) — 跟随指定 Target（Bip001 根骨骼），用在 `BigSkill target` 和 `BigSkillStart target` 上。

---

### 7.10 Animator Controller 状态全列表

#### Corin.controller（29个状态，全部 m_Transitions: []）

| 分类 | 状态名称 |
|------|---------|
| 待机 | `Idle`, `Idle_AFK` |
| 移动 | `Walk`, `Run` |
| 停步 | `Run_End_L`, `Run_End_R` |
| 转身 | `TurnBack` |
| 普攻 | `Attack_Normal_1` ~ `Attack_Normal_5` |
| 普攻收 | `Attack_Normal_1_End` ~ `Attack_Normal_5_End` |
| 闪避 | `Evade_Front`, `Evade_Back` |
| 闪避收 | `Evade_Front_End`, `Evade_Back_End` |
| 必杀 | `BigSkill_Start`, `BigSkill`, `BigSkill_End` |
| 切换 | `SwitchIn_Normal`, `SwitchOut_Normal` |
| 未使用 | `New State` |

默认状态: `Idle`

#### Anbi.controller（状态完全同名，仅普攻只有 1~4 段）

---

### 7.11 ScriptableObject 配置体系

#### PlayerConfig

```
[CreateAssetMenu(menuName = "Config/Player Config")]
public class PlayerConfig : ScriptableObject
{
    public GameObject[] models;  // 3 个角色 Prefab
}
```

实际配置引用的 GUID：
- Anbi: `f09e8fc968846474db0227392852147d`
- Corin: `fccf6ae4e20c0994aa7376c5a43513f7`
- Nike: `1e38e5e5b251d484ebc17f0d3e8c0a46`

#### SkiilConfig

```
[CreateAssetMenu(menuName = "Config/Skill")]
public class SkiilConfig : ScriptableObject
{
    [HideInInspector] public int currentNormalAttackIndex = 1;  // 运行时状态（问题）
    public float[] normalAttackDamageMultiple;                    // 每段倍率（当前全 0）
}
```

**设计问题**: 运行时状态存 SO 上，正确做法是分离到纯 C# ReusableData 中。

---

## 八、设计模式总结

| 设计模式 | 出现位置 | 具体形式 | 评价 |
|----------|---------|---------|------|
| **状态模式** | StateMachine + StateBase + 12个具体状态 | 纯 C# FSM，委托驱动 Update | 核心骨架 |
| **单例模式** | SingleMomoBase\<T\> | MonoBehaviour 单例基类 | 三个单例：PlayerController / MonoManager / CameraManager |
| **观察者/发布-订阅** | MonoManager 的 Action 委托链 | 状态→MonoManager 注册 Update | 统一调度中心 |
| **ScriptableObject** | PlayerConfig, SkiilConfig | SO 存储角色列表和技能参数 | 基础用法 |
| **模板方法模式** | StateBase → PlayerStateBase | 基类定义骨架，子类覆写 | 重力/计时/切换在基类统一 |
| **标记接口** | IStateMachineOwner | 空接口，类型约束 | 仅约束 StateMachine 的 owner 类型 |
| **策略模式** | 各具体状态类 | 每个状态封装一种行为 | 新增状态只需新建类 |

---

## 九、优缺点分析

### 优点

1. **MonoManager 统一调度**：纯 C# 状态类脱离 GameObject 运行，架构干净。
2. **状态机懒加载 + 字典缓存**：12 个状态内存压力小，GC 友好。
3. **Enter/Exit 对称性**：注册/注销严格对称，状态清理彻底。
4. **多角色切换位置传递**：继承位置+朝向+合理偏移，实现自然换人效果。
5. **必杀技镜头切换**：三阶段 + Cut/EaseInOut 混合 + 角色独立调优，复刻原作演出感。
6. **脚位追踪**：根据停止时的脚位决定动画起始帧，细节到位。
7. **预输入缓冲**：50% 窗口，手感流畅。
8. **Animator Controller 作为纯动画库**：所有逻辑在代码中，不依赖 Animator 参数和 Transition。

### 缺点和改进建议

1. **重力被施加两次**：Update 和 FixedUpdate 双重力 → 实际加速度约 -19.6。
2. **SkiilConfig 存储运行时状态**：`currentNormalAttackIndex` 应移到纯 C# 运行时数据类。
3. **MonoManager Action 字段 public**：应改为 `event` 或加只读包装。
4. **闪避 CD 在 FixedUpdate 中累积**：应放在 Update（表现逻辑，非物理）。
5. **EnemyStateBase 是空壳**：敌人 AI 状态机完全未实现。
6. **拼写错误**：`SkiilConfig`→SkillConfig, `SingleMomoBase`→SingleMonoBase, `Perfabs`→Prefabs, `freeLookCanmera`→freeLookCamera。
7. **缺少事件系统**：系统间通信依赖直接引用和单例，耦合度较高。
8. **没有对象池**：切换角色只是 SetActive(false)，未池化。
9. **50% 预输入窗口硬编码**：应暴露到 SO 中可配置。
10. **SwitchUp 键盘未绑定**：手柄可切换上一个角色，键盘只能通过空格切下一个。

---

## 十、Animator & 物理配置总表

| 参数 | Anbi | Corin |
|------|------|-------|
| **Animator** | | |
| ApplyRootMotion | 1 (启用) | 1 (启用) |
| UpdateMode | 0 (Normal) | 0 (Normal) |
| CullingMode | 1 (Cull Transforms) | 1 (Cull Transforms) |
| KeepStateOnDisable | 0 (false) | 0 (false) |
| StabilizeFeet | 0 (false) | 0 (false) |
| Controller GUID | `7a6c170d...` | `4823475d...` |
| **CharacterController** | | |
| Height | 1.6 | 1.6 |
| Radius | 0.2 | 0.2 |
| Center | (0, 0.8, 0) | (0, 0.8, 0) |
| SlopeLimit | 45 | 45 |
| StepOffset | 0.3 | 0.3 |
| SkinWidth | 0.0001 | 0.0001 |
| **PlayerModel** | | |
| gravity | -9.8 | -9.8 |
| 普攻段数 | 4 | 5 |

---

## 十一、关键文件索引

| 文件 | 职责 | 重要程度 |
|------|------|---------|
| `StateMachine/StateMachine.cs` | FSM 核心，字典缓存 | ★★★★★ |
| `Base/StateBase.cs` | 状态抽象基类 | ★★★★★ |
| `Base/PlayerStateBase.cs` | 玩家状态基类（重力/计时/切换输入） | ★★★★★ |
| `Player/PlayerController.cs` | 玩家入口，SwitchState，角色切换 | ★★★★★ |
| `Manager/MonoManager.cs` | 统一 Update 调度中心 | ★★★★☆ |
| `Player/PlayerModel.cs` | 角色模型数据，进出场逻辑 | ★★★★☆ |
| `Player/State/NormalAttack/PlayerNormalAttackState.cs` | 连招核心（预输入+段数） | ★★★★☆ |
| `Player/State/BigSkill/PlayerBigSkillStartState.cs` | 必杀技镜头切换 | ★★★☆☆ |
| `Config/SkiilConfig.cs` | 技能数据 SO | ★★★☆☆ |
| `Manager/CameraManager.cs` | 相机控制 | ★★★☆☆ |
| `Base/SingleMomoBase.cs` | MonoBehaviour 单例基类 | ★★★☆☆ |
| 各角色 Prefab (Corin/Anbi) | 完整角色+镜头+碰撞体 | ★★★☆☆ |
| 各角色 .controller | 动画状态列表（纯动画库） | ★★★☆☆ |
