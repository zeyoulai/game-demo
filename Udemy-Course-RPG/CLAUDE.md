# CLAUDE.md

## Skill 调用规则（最高优先级）

当用户使用 `/<skill-name>` 命令或明确说"使用 XX skill"时，**必须先调用 Skill 工具加载该 skill**，再执行分析或回复。不要跳过 Skill 工具自行处理。

当前项目可用的 skill：
- `/unity-game-study` — Unity 项目架构分析（最全面）
- `/unity-code-analyzer` — Unity C# 代码架构分析

## 项目信息

- 类型：Unity 2D 类银河恶魔城 RPG（Udemy 课程 Demo）
- 引擎版本：Unity 2022.3+
- 渲染管线：Built-in（默认）
- 核心系统：Entity-Component 角色架构、三层状态机、Stat+Modifier 属性系统、Inventory 背包系统、ISaveable 存档接口
