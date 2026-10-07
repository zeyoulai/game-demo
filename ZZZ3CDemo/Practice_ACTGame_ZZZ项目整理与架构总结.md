# Practice_ACTGame_ZZZ 项目整理与架构总结

## 1. 项目概览

这是一个 Unity 第三人称 ACT 战斗练习项目，核心目标不是完整游戏循环，而是搭出类似《绝区零》基础操作手感的原型框架：角色移动、待机、普攻连段、闪避、大招镜头、三角色切换。

项目规模偏教学 Demo：`Assets/Scripts` 下有 25 个 C# 脚本；没有发现正式敌人 AI、伤害判定、血量、受击、关卡推进、UI、存档或测试代码。也就是说，这个项目的重点是“角色状态切换和动画驱动”，不是完整战斗系统。

技术栈：

| 项 | 实际内容 |
| --- | --- |
| Unity 版本 | `2022.3.62f3`，见 `ProjectSettings/ProjectVersion.txt` |
| 渲染管线 | URP，`com.unity.render-pipelines.universal` 版本 `12.1.7` |
| 输入 | New Input System `1.4.4`，输入资源在 `Assets/Settings/Input System/` |
| 相机 | Cinemachine `2.8.9`，使用 FreeLook + 大招虚拟镜头 |
| 场景 | `Assets/Scenes/SampleScene.unity` |
| 角色资源 | `Anbi`、`Corin`、`Nostradamus_Model_CN` 三个 Prefab |

## 2. 目录结构

```text
Assets/
|-- Arts/
|   |-- Anbi/                  角色动画、材质、贴图、Animator Controller
|   |-- Corin/
|   |-- Nike/                  实际 Prefab 名是 Nostradamus_Model_CN
|   |-- Environment/           灰盒/场景贴图资源
|   `-- Shader/
|-- Config/
|   |-- Player Config.asset    三角色 Prefab 队伍配置
|   `-- Skill/                 三个角色的普攻段数配置
|-- Perfabs/
|   |-- Anbi.prefab
|   |-- Corin.prefab
|   `-- Nostradamus_Model_CN.prefab
|-- Scenes/
|   `-- SampleScene.unity
|-- Scripts/
|   |-- Base/                  StateBase、PlayerStateBase、单例基类
|   |-- Config/                PlayerConfig、SkiilConfig
|   |-- Manager/               MonoManager、CameraManager
|   |-- Player/                PlayerController、PlayerModel、状态类
|   `-- StateMachine/          通用状态机
|-- Settings/
|   `-- Input System/          InputAction 资源和生成代码
`-- Plug_in/
    `-- Editor/                手动 Reload Domain 编辑器工具
```

## 3. 核心架构

项目采用的是“单状态机 + 纯 C# 状态类 + MonoManager 统一 Update 调度”的架构。

```text
Scene
 |
 |-- PlayerController : SingleMomoBase<PlayerController>, IStateMachineOwner
 |   |-- StateMachine
 |   |   |-- Dictionary<Type, StateBase>
 |   |   `-- currentState
 |   |-- InputSystem
 |   |-- PlayerConfig
 |   `-- current PlayerModel
 |
 |-- MonoManager
 |   |-- updatAction
 |   |-- fixedUpdatAction
 |   `-- lateUpdatAction
 |
 `-- CameraManager
     |-- CinemachineBrain
     |-- FreeLook Camera
     `-- CinemachineFreeLook
```

关键点：

| 模块 | 作用 | 关键文件 |
| --- | --- | --- |
| `StateMachine` | 根据状态类型缓存/切换状态，进入状态时注册 Update，退出时注销 | `Assets/Scripts/StateMachine/StateMachine.cs:44` |
| `StateBase` | 定义 `Init/Enter/Exit/Update/FixedUpdate/LateUpdate` 生命周期 | `Assets/Scripts/Base/StateBase.cs:8` |
| `PlayerStateBase` | 玩家状态公共基类，统一处理重力、计时、切人输入、动画结束判断 | `Assets/Scripts/Base/PlayerStateBase.cs:14` |
| `PlayerController` | 玩家入口、状态分发、输入系统、角色生成/切换 | `Assets/Scripts/Player/PlayerController.cs:8` |
| `PlayerModel` | 单个角色的 Animator、CharacterController、技能配置、大招镜头引用 | `Assets/Scripts/Player/PlayerModel.cs:9` |
| `MonoManager` | 用 Action 委托集中驱动非 MonoBehaviour 状态类 | `Assets/Scripts/Manager/MonoManager.cs:6` |
| `CameraManager` | 管理 FreeLook 和大招镜头切换 | `Assets/Scripts/Manager/CameraManager.cs:10` |

这个设计的好处是学习成本低：每个状态对应一个类，类名基本就是玩法语义。代价也很明显：输入判断分散在大量状态里，状态数量增加后会出现重复代码和分支膨胀。

## 4. 状态机系统

`StateMachine` 的核心是一个 `Dictionary<Type, StateBase>`。第一次进入某个状态时 `new T()`，之后复用同一个状态对象。

```csharp
// Assets/Scripts/StateMachine/StateMachine.cs:70
private StateBase LoadState<T>() where T : StateBase, new()
{
    Type stateType = typeof(T);
    if (!stateDic.TryGetValue(stateType, out StateBase state))
    {
        state = new T();
        state.Init(owner);
        stateDic.Add(stateType, state);
    }
    return state;
}
```

进入状态后，状态的三个帧循环会被注册到 `MonoManager`：

```csharp
// Assets/Scripts/StateMachine/StateMachine.cs:87
private void EnterCurrentState()
{
    currentState.Enter();
    MonoManager.INSTANCE.AddUpdateAction(currentState.Update);
    MonoManager.INSTANCE.AddFixedUpdateAction(currentState.FixedUpdate);
    MonoManager.INSTANCE.AddLateUpdateAction(currentState.LateUpdate);
}
```

为什么这么做：

| 设计选择 | 意义 |
| --- | --- |
| 状态类不继承 `MonoBehaviour` | 可以用普通 C# 类表达状态，不需要给每个状态挂 GameObject |
| 字典缓存状态对象 | 避免反复创建状态对象，减少小规模 GC |
| `MonoManager` 统一调度 | 让纯 C# 状态也能获得 Update 生命周期 |

注意这里有一个潜在风险：`StateMachine.Clear()` 直接调用 `ExitCurrentState()`，但没有判断 `currentState` 是否为空。

```csharp
// Assets/Scripts/StateMachine/StateMachine.cs:105
public void Clear()
{
    ExitCurrentState();
    currentState = null;
    ...
}
```

当前调用路径里，切人前通常已经处于某个状态，所以不一定触发问题。但从通用状态机角度看，`Clear()` 最好先判断 `HasState`，否则未来如果在状态机尚未启动或已清空后再次调用，会有空引用风险。

## 5. 玩家入口与队伍切换

`PlayerController.Awake()` 负责创建状态机、创建输入系统、根据 `PlayerConfig.models` 实例化三名角色，并默认启用第一个角色。

```csharp
// Assets/Scripts/Player/PlayerController.cs:40
stateMachine = new StateMachine(this);
inputSystem = new InputSystem();

// Assets/Scripts/Player/PlayerController.cs:46
for (int i = 0; i < playerConfig.models.Length; i++)
{
    GameObject modle = Instantiate(playerConfig.models[i], transform);
    controllableModels.Add(modle.GetComponent<PlayerModel>());
    controllableModels[i].gameObject.SetActive(false);
}
```

`Player Config.asset` 确认拖了三个 Prefab：

| 顺序 | Prefab |
| --- | --- |
| 1 | `Assets/Perfabs/Anbi.prefab` |
| 2 | `Assets/Perfabs/Corin.prefab` |
| 3 | `Assets/Perfabs/Nostradamus_Model_CN.prefab` |

切人流程：

```text
SwitchNextModel / SwitchLastModel
 -> stateMachine.Clear()
 -> old playerModel.Exit()
 -> 计算下一个角色索引
 -> 激活 nextModel
 -> nextModel.Enter(prevPos, prevRot)
 -> SwitchState(SwitchInNormal)
```

具体代码在 `Assets/Scripts/Player/PlayerController.cs:129` 和 `Assets/Scripts/Player/PlayerController.cs:158`。

这个设计很直接：控制器永远只维护一个当前 `playerModel`。旧角色退场动画由 `PlayerModel.Exit()` 注册到 `MonoManager`，动画播完再隐藏：

```csharp
// Assets/Scripts/Player/PlayerModel.cs:63
public void Exit()
{
    animator.CrossFade("SwitchOut_Normal", 0.1f);
    MonoManager.INSTANCE.AddUpdateAction(OnExit);
}
```

这里有一个好点：`PlayerModel.Enter()` 会先移除旧的 `OnExit` 回调，避免同一个模型被快速切回时，旧退场逻辑把它再次隐藏。

```csharp
// Assets/Scripts/Player/PlayerModel.cs:40
public void Enter(Vector3 pos, Quaternion rot)
{
    MonoManager.INSTANCE.RemoveUpdateAction(OnExit);
    ...
}
```

## 6. 输入系统

输入使用 New Input System，资源在 `Assets/Settings/Input System/Input System.inputactions`，生成代码在 `Assets/Settings/Input System/Input System.cs`。

| Action | 类型 | 主要绑定 |
| --- | --- | --- |
| `Move` | `Value / Vector2` | WASD、方向键、手柄左摇杆 |
| `Look` | `PassThrough / Vector2` | 鼠标 delta、手柄右摇杆 |
| `Fire` | `Button` | 鼠标左键、手柄 West |
| `Evade` | `Button` | 左 Shift、手柄 South |
| `BigSkill` | `Button` | Q、手柄右扳机 |
| `SwitchUp` | `Button` | 手柄左肩键 |
| `SwitchDown` | `Button` | Space、手柄右肩键 |

当前输入读取方式是轮询：状态类里直接查 `triggered` 或 `IsPressed()`，移动输入在 `PlayerController.FixedUpdate()` 中每个 FixedUpdate 更新一次：

```csharp
// Assets/Scripts/Player/PlayerController.cs:205
private void FixedUpdate()
{
    inputMoveVec2 = inputSystem.Player.Move.ReadValue<Vector2>().normalized;
}
```

这个方案简单，但也带来两个问题：

1. 输入判断重复散落在 Idle、Run、AttackEnd、EvadeEnd、BigSkillEnd、SwitchIn 等状态里。
2. 移动输入在 FixedUpdate 更新，但大部分状态逻辑在 Update 判断，极端帧率下会有轻微输入时序不一致。

## 7. 玩家状态系统

状态枚举定义在 `Assets/Scripts/Base/PlayerStateBase.cs:5`：

```csharp
public enum PlayerState
{
    Idle, Idle_AFK,
    Walk, Run, RunEnd, TurnBack,
    Evade_Front, Evade_Back, Evade_Front_End, Evade_Back_End,
    NormalAttack, NormalAttackEnd,
    BigSkillStart, BigSkill, BigSkillEnd,
    SwitchInNormal
}
```

整体转换关系可以理解为：

```text
Idle
 |-- 3 秒无输入 -> Idle_AFK -> 动画结束 -> Idle
 |-- Move -> Walk -> 3 秒持续移动 -> Run
 |-- Fire -> NormalAttack -> NormalAttackEnd -> Idle
 |-- Evade -> Evade_Back -> Evade_Back_End -> Idle/Walk/Attack
 |-- BigSkill -> BigSkillStart -> BigSkill -> BigSkillEnd -> Idle/Walk/Attack/Evade
 |-- Switch -> SwitchInNormal -> Idle/Walk/Attack/Evade/BigSkill

Run
 |-- 无移动输入 -> RunEnd -> Idle
 |-- 大角度反向输入 -> TurnBack -> Run
 |-- Evade -> Evade_Front -> Evade_Front_End
 |-- Fire -> NormalAttack
```

`PlayerController.SwitchState()` 是所有状态转换的集中分发点，见 `Assets/Scripts/Player/PlayerController.cs:73`。这个设计方便新手读项目，但状态越多，switch 就越大。

## 8. 移动系统

移动没有使用复杂 Blend Tree，而是依赖动画命名、脚步标记和播放偏移来衔接。

`PlayerRunState.Enter()` 根据 `playerModel.foot` 决定从动画 0.0 或 0.5 的时间偏移开始播放：

```csharp
// Assets/Scripts/Player/State/Run/PlayerRunState.cs:24
switch (playerModel.foot)
{
    case ModelFoot.Left:
        playerController.PlayAnimation("Walk", 0.125f, 0.5f);
        playerModel.foot = ModelFoot.Right;
        break;
    case ModelFoot.Right:
        playerController.PlayAnimation("Walk", 0.125f, 0.0f);
        playerModel.foot = ModelFoot.Left;
        break;
}
```

动画资源里也能看到 `SetOutLeftFoot`、`SetOutRightFoot` 动画事件，调用的是 `PlayerModel` 里的方法：

```csharp
// Assets/Scripts/Player/PlayerModel.cs:97
public void SetOutLeftFoot()
{
    foot = ModelFoot.Left;
}
```

移动方向由相机 Y 轴旋转修正，玩家朝目标方向 `Slerp`：

```csharp
// Assets/Scripts/Player/State/Run/PlayerRunState.cs:95
Vector3 inputMoveVec3 = new Vector3(playerController.inputMoveVec2.x, 0, playerController.inputMoveVec2.y);
float cameraAxisY = mainCamera.transform.rotation.eulerAngles.y;
Vector3 targetDic = Quaternion.Euler(0, cameraAxisY, 0) * inputMoveVec3;
Quaternion targetQua = Quaternion.LookRotation(targetDic);
```

反向转身阈值是硬编码的 `145f` 到 `215f`：

```csharp
// Assets/Scripts/Player/State/Run/PlayerRunState.cs:103
if (angles > 145f && angles < 215f && playerModel.currentState == PlayerState.Run)
{
    playerController.SwitchState(PlayerState.TurnBack);
}
```

这个系统很适合练习动作衔接，因为它绕开了 Blend Tree 的复杂配置。代价是：速度、转向阈值、动画偏移都硬编码在状态类里，后续调手感需要频繁改代码。

## 9. 普攻连招系统

普攻系统是“段数计数器 + 动画命名约定”的简化实现。

播放攻击动画：

```csharp
// Assets/Scripts/Player/State/NormalAttack/PlayerNormalAttackState.cs:18
playerController.PlayAnimation("Attack_Normal_" + playerModel.skiilConfig.currentNormalAttackIndex, 0.1f);
```

如果动画播放进度超过 50% 后再次按攻击，就记录 `enterNextAttack = true`：

```csharp
// Assets/Scripts/Player/State/NormalAttack/PlayerNormalAttackState.cs:35
if (NormalizedTime() >= 0.5f && playerController.inputSystem.Player.Fire.triggered)
{
    enterNextAttack = true;
}
```

动画结束时，如果记录了下一段输入，就让 `currentNormalAttackIndex++`，超出配置长度则回到 1：

```csharp
// Assets/Scripts/Player/State/NormalAttack/PlayerNormalAttackState.cs:47
playerModel.skiilConfig.currentNormalAttackIndex++;
if (playerModel.skiilConfig.currentNormalAttackIndex > playerModel.skiilConfig.normalAttackDamageMultiple.Length)
{
    playerModel.skiilConfig.currentNormalAttackIndex = 1;
}
```

三个角色的技能配置：

| 角色 | 配置文件 | 普攻段数 | 当前倍率 |
| --- | --- | --- | --- |
| Anbi | `Assets/Config/Skill/Anbi Skill Config.asset` | 4 段 | 全部为 0 |
| Corin | `Assets/Config/Skill/Corin Skill Config.asset` | 5 段 | 全部为 0 |
| Nike/Nostradamus | `Assets/Config/Skill/Nike Skill Config.asset` | 3 段 | 全部为 0 |

注意：`SkiilConfig.currentNormalAttackIndex` 是运行时会修改的 ScriptableObject 字段。

```csharp
// Assets/Scripts/Config/SkiilConfig.cs:12
[HideInInspector] public int currentNormalAttackIndex = 1;
```

这有 SO 污染风险：在编辑器运行时修改 SO 字段，理论上会改到资源本体。项目用 `PlayerModel.OnDisable()` 重置段数：

```csharp
// Assets/Scripts/Player/PlayerModel.cs:110
private void OnDisable()
{
    skiilConfig.currentNormalAttackIndex = 1;
}
```

这能兜住大多数情况，但不是最稳的架构。更合理的做法是把 `currentNormalAttackIndex` 挪到运行时数据类，比如 `PlayerRuntimeData`，SO 只保存配置。

当前还没有真正伤害系统：没有命中盒、LayerMask、Raycast/SphereCast、受击接口、血量扣除或伤害事件。`normalAttackDamageMultiple` 已配置数组长度，但数值全是 0，也没有被用于伤害计算。

## 10. 闪避系统

闪避状态分成前闪、后闪以及各自的结束状态：

```text
Evade_Front -> Evade_Front_End
Evade_Back  -> Evade_Back_End
```

触发时由 `PlayerController.SwitchState()` 统一检查 1 秒冷却：

```csharp
// Assets/Scripts/Player/PlayerController.cs:92
case PlayerState.Evade_Front:
case PlayerState.Evade_Back:
    if (evadeTimer != 1)
    {
        return;
    }
    stateMachine.EnterState<PlayerEvadeState>();
    evadeTimer = 0f;
    break;
```

冷却在 FixedUpdate 中恢复：

```csharp
// Assets/Scripts/Player/PlayerController.cs:210
if (evadeTimer < 1f)
{
    evadeTimer += Time.deltaTime;
    if (evadeTimer > 1f)
    {
        evadeTimer = 1f;
    }
}
```

这个设计能跑，但有两个点要注意：

1. 用 `evadeTimer != 1` 判断是否冷却完成，依赖后面手动夹到 1。当前代码夹值了，所以能工作；如果未来改成更复杂的冷却，建议改为 `evadeTimer < evadeCooldown`。
2. 冷却时长、闪避方向、无敌帧、位移曲线都没有数据化；当前主要是播动画，不是真正完整闪避系统。

## 11. 大招与相机系统

大招被拆成三个状态：

```text
BigSkillStart -> BigSkill -> BigSkillEnd
```

`BigSkillStart` 直接把 Cinemachine Brain 的 Blend 改成 Cut，关闭 FreeLook，打开角色身上的起手大招镜头：

```csharp
// Assets/Scripts/Player/State/BigSkill/PlayerBigSkillStartState.cs:16
CameraManager.INSTANCE.cm_brain.m_DefaultBlend =
    new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
CameraManager.INSTANCE.freeLookCanmera.SetActive(false);
playerModel.bigSkillStartShot.SetActive(true);
```

`BigSkill` 切到主体大招镜头：

```csharp
// Assets/Scripts/Player/State/BigSkill/PlayerBigSkillState.cs:15
playerModel.bigSkillStartShot.SetActive(false);
playerModel.bigSkillShot.SetActive(true);
```

`BigSkillEnd` 恢复 FreeLook，并把 Blend 调成 1 秒 EaseInOut：

```csharp
// Assets/Scripts/Player/State/BigSkill/PlayerBigSkillEndState.cs:16
playerModel.bigSkillShot.SetActive(false);
CameraManager.INSTANCE.cm_brain.m_DefaultBlend =
    new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 1f);
CameraManager.INSTANCE.freeLookCanmera.SetActive(true);
CameraManager.INSTANCE.ResetFreeLookCamera();
```

这个拆法是项目里比较好的设计：大招起手、主体、收尾三个阶段在相机和输入打断规则上都不同，拆成三个状态比塞在一个状态里更清楚。

目前缺少的部分是大招伤害、能量消耗、命中停顿、震屏、特效池和音效池。

## 12. Unity 配置侧结论

从 YAML 和资源命名能确认：

| 配置点 | 实际情况 |
| --- | --- |
| 场景 | `SampleScene.unity` 有 `Manager`、`Player`、`Cameras`、`CM FreeLook1`、`TargetPoint` |
| PlayerController | 场景里的 `Player` 绑定 `Player Config.asset`，见 `Assets/Scenes/SampleScene.unity:2285` 和 `:2291` |
| CameraManager | 场景里绑定了 `cm_brain`、`freeLookCanmera`、`freeLook`，见 `Assets/Scenes/SampleScene.unity:1643` |
| 角色 Prefab | 三个角色都有 Animator、CharacterController、`PlayerModel`、大招镜头引用 |
| CharacterController | 三个角色统一高度 `1.6`、半径 `0.2`、坡度 `45`、台阶 `0.3` |
| Animator Controller | `Anbi.controller`、`Corin.controller`、`Nike.controller` 都有攻击、闪避、大招、入场等状态名 |
| Animation Event | Walk/Run/部分 Evade 资源里能搜到 `SetOutLeftFoot` / `SetOutRightFoot` |

## 13. 设计模式总结

| 模式/思想 | 项目中的体现 | 评价 |
| --- | --- | --- |
| 有限状态机 FSM | `StateMachine` + `StateBase` + 各个 `PlayerXXXState` | 主结构清楚，适合动作状态练习 |
| 单例 | `SingleMomoBase<T>` 派生出 `PlayerController`、`MonoManager`、`CameraManager` | 方便 Demo，但耦合较高 |
| 模板方法 | `StateBase` 定义生命周期，`PlayerStateBase` 统一公共逻辑 | 能减少部分重复，但输入重复仍然明显 |
| 中介者/集中分发 | `PlayerController.SwitchState()` 统一枚举到具体状态类 | 初期可读性强，后期容易变大 |
| 数据配置 SO | `PlayerConfig`、`SkiilConfig` | 配置面很轻，且混入了运行时状态 |
| 委托调度器 | `MonoManager` 的 Action 列表 | 能让普通 C# 状态获得 Update，但调试栈不如 MonoBehaviour 直观 |

## 14. 主要问题与改进建议

### P0：重力被重复施加

`PlayerStateBase.FixedUpdate()` 和 `PlayerStateBase.Update()` 都调用了 `CharacterController.Move()` 施加重力：

```csharp
// Assets/Scripts/Base/PlayerStateBase.cs:40
public override void FixedUpdate()
{
    playerModel.characterController.Move(new Vector3(0, playerModel.gravity * Time.deltaTime, 0));
}

// Assets/Scripts/Base/PlayerStateBase.cs:57
public override void Update()
{
    playerModel.characterController.Move(new Vector3(0, playerModel.gravity * Time.deltaTime, 0));
}
```

这会让下落速度变成“Update + FixedUpdate”双重叠加。建议只保留一处。使用 `CharacterController` 时，一般放在 `Update` 里统一移动和重力更简单。

### P1：ScriptableObject 保存了运行时段数

`currentNormalAttackIndex` 应该是运行时状态，不应该放在 SO 资源里。建议：

```text
SkiilConfig
 `-- normalAttackDamageMultiple  配置数据

PlayerRuntimeData
 `-- currentNormalAttackIndex    运行时数据
```

这样即使编辑器崩溃或对象没有触发 `OnDisable()`，也不会污染资源。

### P1：状态里输入判断重复太多

Idle、Run、RunEnd、AttackEnd、EvadeEnd、BigSkillEnd、SwitchIn 都有类似“检测大招/攻击/移动/闪避”的代码。建议提炼成可复用的优先级输入处理，例如：

```text
PlayerStateBase.TryHandleCommonInput(policy)
```

不同状态传入允许项，比如“大招主体不允许切人和攻击”“攻击后摇允许 0.5 秒后移动”。

### P1：连招窗口硬编码

`NormalizedTime() >= 0.5f` 写在代码里。建议挪到配置：

```text
ComboStepConfig
 |-- animationName
 |-- nextInputStartNormalizedTime
 |-- cancelToMoveTime
 |-- damageMultiplier
```

这样调手感不用改代码。

### P2：缺少伤害/命中系统

现在普通攻击只播动画，`normalAttackDamageMultiple` 没有被消耗。下一步应补：

| 子系统 | 建议 |
| --- | --- |
| 命中检测 | 先用 `Physics.OverlapSphere` 或武器挂点 `SphereCast` |
| 目标接口 | `IDamageable.TakeDamage(DamageInfo info)` |
| 攻击时机 | 由 Animation Event 调 `Hit()`，不要在固定时间里写死 |
| 伤害数据 | 从 `SkiilConfig` 或后续 `ComboStepConfig` 读取 |

### P2：相机系统还缺通用性

大招镜头引用挂在 `PlayerModel` 上，是可运行方案；但如果以后加入更多技能，每个技能都有专属镜头，就需要改成技能数据驱动的镜头表。

### P2：单例基类不处理重复实例销毁

`SingleMomoBase<T>` 检测到重复实例只打错误，不阻止覆盖：

```csharp
// Assets/Scripts/Base/SingleMomoBase.cs:13
if (INSTANCE != null)
{
    Debug.LogError(this + "不符合单例模式");
}
INSTANCE = (T)this;
```

Demo 可接受；正式项目里建议重复实例直接销毁，或者明确保留第一个实例。

## 15. 学习路线建议

如果你是要通过这个项目学习 ACT 架构，建议按这个顺序读：

1. `Assets/Scripts/StateMachine/StateMachine.cs`
2. `Assets/Scripts/Base/StateBase.cs`
3. `Assets/Scripts/Base/PlayerStateBase.cs`
4. `Assets/Scripts/Player/PlayerController.cs`
5. `Assets/Scripts/Player/PlayerModel.cs`
6. `Assets/Scripts/Player/State/Idle/PlayerIdleState.cs`
7. `Assets/Scripts/Player/State/Run/PlayerRunState.cs`
8. `Assets/Scripts/Player/State/NormalAttack/PlayerNormalAttackState.cs`
9. `Assets/Scripts/Player/State/NormalAttack/PlayerNormalAttackEndState.cs`
10. `Assets/Scripts/Player/State/BigSkill/PlayerBigSkillStartState.cs`
11. `Assets/Scripts/Manager/CameraManager.cs`

这个顺序会比较顺：先理解“状态怎么跑”，再看“玩家怎么切状态”，最后看具体玩法状态怎么用动画和输入拼起来。

## 16. 一句话总结

这个副本项目是一个第三人称 ACT 动作状态机练习工程：它已经把“状态机、输入、动画命名约定、角色切换、大招相机”这些动作游戏原型最核心的骨架搭起来了，但还停留在演出与状态流层面。下一步如果要往完整战斗系统走，最值得优先补的是重力修正、运行时数据与 SO 分离、连招配置化、命中/伤害/受击闭环。
