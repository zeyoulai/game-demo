# 背包-UI 系统的 MVC 设计详解

## 1. 先看现实：项目现在的架构是什么？

在讲 MVC 之前，先看看项目**实际怎么写的**，因为它的设计已经包含了 MVC 的雏形，只是没有严格分离。

### 当前项目实际架构（类图）

```
┌─────────────────────────────────────┐
│          «ScriptableObject»         │
│          ItemDataSO                 │
├─────────────────────────────────────┤
│ saveId : string                     │
│ itemName : string                   │
│ itemIcon : Sprite                   │
│ itemType : ItemType                 │
│ maxStackSize : int                  │
│ itemPrice : int                     │
│ itemEffect : ItemEffect_DataSO      │
│ craftRecipe : Inventory_Item[]      │
│ dropChance : float                  │
└─────────────────────────────────────┘
         △
         │ 继承
┌────────┴──────────────────────────┐
│       EquipmentDataSO             │
│       (extends ItemDataSO)        │
├───────────────────────────────────┤
│ modifiers : ItemModifier[]        │
└───────────────────────────────────┘
         │
         │ 被运行时包装类引用
         ▼
┌─────────────────────────────────────┐
│     Inventory_Item                  │
│     «纯C# 运行时数据类»              │
├─────────────────────────────────────┤
│ itemId : string  ← GUID唯一标识     │
│ itemData : ItemDataSO  ← 模板引用   │
│ stackSize : int                     │
│ modifiers[] : ItemModifier          │
│ itemEffect : ItemEffect_DataSO      │
│ buyPrice / sellPrice : int/float    │
├─────────────────────────────────────┤
│ + AddStack() / RemoveStack()        │
│ + AddModifiers(stats)               │
│ + RemoveModifiers(stats)            │
│ + AddItemEffect(player)             │
│ + GetItemInfo() : string            │
└─────────────────────────────────────┘

     ┌──────────────────────────────────────────┐
     │    Inventory_Base                        │
     │    «MonoBehaviour — 背包Model基类»        │
     ├──────────────────────────────────────────┤
     │ player : Player                          │
     │ maxInventorySize : int = 10              │
     │ itemList : List<Inventory_Item>          │
     │ itemDataBase : ItemListDataSO            │
     │ event OnInventoryChange                  │
     ├──────────────────────────────────────────┤
     │ + AddItem(item)                          │
     │ + RemoveOneItem(item)                    │
     │ + CanAddItem(item) : bool                │
     │ + FindStackable(item) : Inventory_Item   │
     │ + TryUseItem(item)                       │
     │ + HasItemAmount(data,amount) : bool      │
     │ + RemoveItemAmount(data,amount)          │
     │ + TriggerUpdateUI()  ← 触发View刷新      │
     └──────────────────────────────────────────┘
         △
         │ 继承
    ┌────┴──────────────┬──────────────────────┐
    │                   │                      │
┌───┴───────────┐ ┌────┴────────────┐ ┌───────┴──────────┐
│Inventory_Player│ │Inventory_Storage│ │Inventory_Merchant│
│ «玩家背包»     │ │ «仓库»          │ │ «商人»           │
├────────────────┤ ├─────────────────┤ ├──────────────────┤
│ gold : int     │ │ materialStash   │ │ shopData         │
│ quickItems[2]  │ │ : List<Item>    │ │ minItemAmount    │
│ equipList      │ │                 │ │                  │
│ storage        │ │ + CraftItem()   │ │ + TryBuyItem()   │
│                │ │ + AddMaterial   │ │ + TrySellItem()  │
│ + TryEquipItem │ │   ToStash()     │ │ + FillShopList() │
│ + UnequipItem  │ │ + FromPlayerTo  │ │                  │
│ + SetQuickItem │ │   Storage()     │ │                  │
└────────────────┘ └─────────────────┘ └──────────────────┘
    │ 组合
    │ 1..*
    ▼
┌─────────────────────────────────────┐
│  Inventory_EquipmentSlot            │
│  «纯C# 数据类»                      │
├─────────────────────────────────────┤
│ slotType : ItemType                 │
│ equipedItem : Inventory_Item        │
├─────────────────────────────────────┤
│ + HasItem() : bool                  │
│ + GetEquipedItem() : Inventory_Item │
└─────────────────────────────────────┘


═══════════════ View 层 ═══════════════

┌─────────────────────────────────────────┐
│  UI_Inventory                           │
│  «MonoBehaviour — 背包UI主面板»          │
├─────────────────────────────────────────┤
│ inventory : Inventory_Player            │
│ inventorySlotsParent : UI_ItemSlotParent│
│ equipSlotParent : UI_EquipSlotParent    │
│ goldText : TextMeshProUGUI              │
├─────────────────────────────────────────┤
│ + UpdateUI()  ← 订阅OnInventoryChange   │
└─────────────────────────────────────────┘
         │ 组合
         ▼
┌─────────────────────────────────────────┐
│  UI_ItemSlotParent                      │
│  «MonoBehaviour — 物品槽容器»           │
├─────────────────────────────────────────┤
│ slots : UI_ItemSlot[]                   │
├─────────────────────────────────────────┤
│ + UpdateSlots(List<Inventory_Item>)     │
│   → 遍历slots[i].UpdateSlot(itemList[i])│
└─────────────────────────────────────────┘
         │ 组合 1..*
         ▼
┌─────────────────────────────────────────┐
│  UI_ItemSlot : MonoBehaviour,           │
│  IPointerDownHandler,                   │
│  IPointerEnterHandler,                  │
│  IPointerExitHandler                    │
├─────────────────────────────────────────┤
│ itemInSlot : Inventory_Item             │
│ inventory : Inventory_Player            │
│ ui : UI                                 │
│ itemIcon : Image                        │
│ itemStackSize : TextMeshProUGUI         │
├─────────────────────────────────────────┤
│ + UpdateSlot(item)  ← 刷新单个格子UI    │
│ + OnPointerDown()   ← ⚠️直接调Model!    │
│ + OnPointerEnter()  ← 显示Tooltip       │
│ + OnPointerExit()   ← 隐藏Tooltip       │
└─────────────────────────────────────────┘
```

---

## 2. 这个架构的问题在哪？——离真正的 MVC 差了什么？

注意看 `UI_ItemSlot.OnPointerDown()`（`UI_ItemSlot.cs:27-53`）：

```csharp
public virtual void OnPointerDown(PointerEventData eventData)
{
    // View 直接调 Model！
    inventory.TryUseItem(itemInSlot);     // ← View 直接操作数据
    inventory.TryEquipItem(itemInSlot);   // ← View 直接操作数据
    inventory.RemoveOneItem(itemInSlot);  // ← View 直接操作数据
}
```

**这就是典型的 "View 耦合 Model"**。View 不仅负责显示，还负责**决定"点击后做什么"**。这在小型项目中完全够用，但当你需要：

- 同一个背包数据用不同的 UI 展示（比如快捷键栏 + 背包面板 + 装备栏）
- 点击物品时先弹确认框再执行
- 根据当前游戏状态（战斗中/对话中）决定能不能使用物品
- 做按键快捷键操作（键盘 1/2 使用快捷物品，不需要点 UI）

这时候 View 直接调 Model 就会出问题——**业务逻辑散落在各个 View 里，改一个逻辑要改 N 个 UI 脚本**。

---

## 3. 真正的 MVC 应该怎么设计？

核心思想一句话：**Model 只负责数据，View 只负责显示，Controller 负责"用户操作 → 调用 Model"的转发。**

### 完整 MVC 类图

```
═══════════════════════════════════════════════════════════════════
                        M — Model 层
                    （纯数据 + 数据操作，不依赖Unity UI）
═══════════════════════════════════════════════════════════════════

┌──────────────────────────────────────────┐
│  «ScriptableObject»                     │
│  ItemDataSO  ← 物品静态模板              │
├──────────────────────────────────────────┤
│ itemId : string                         │
│ itemName : string                       │
│ itemIcon : Sprite                       │
│ itemType : ItemType                     │
│ maxStackSize : int                      │
│ itemEffect : ItemEffect_DataSO          │
└──────────────────────────────────────────┘
         │
         │ 被包装
         ▼
┌──────────────────────────────────────────┐
│  InventoryItem  ← 运行时物品实例          │
│  «纯C# 类，不继承MonoBehaviour»           │
├──────────────────────────────────────────┤
│ uid : string  ← 实例唯一ID               │
│ data : ItemDataSO  ← 模板引用            │
│ stackSize : int                         │
├──────────────────────────────────────────┤
│ + CanStack() : bool                     │
│ + AddStack() / RemoveStack()            │
└──────────────────────────────────────────┘

┌──────────────────────────────────────────┐
│  InventoryModel  ← 背包数据核心           │
│  «纯C# 类，不继承MonoBehaviour»           │
├──────────────────────────────────────────┤
│ items : List<InventoryItem>              │
│ maxSlots : int                          │
│ event OnDataChanged  ← 数据变更通知      │
├──────────────────────────────────────────┤
│ + AddItem(item) : bool                  │
│ + RemoveItem(item) : bool               │
│ + CanAddItem(item) : bool               │
│ + GetItemAt(index) : InventoryItem      │
│ + SwapSlots(a, b)                      │
│ + FindStackable(item) : InventoryItem   │
└──────────────────────────────────────────┘

┌──────────────────────────────────────────┐
│  EquipmentModel  ← 装备槽数据             │
│  «纯C# 类»                               │
├──────────────────────────────────────────┤
│ slots : Dictionary<ItemType,InventoryItem>│
│ event OnEquipmentChanged                │
├──────────────────────────────────────────┤
│ + Equip(item, slot) : bool              │
│ + Unequip(slot) : InventoryItem         │
│ + GetEquipped(slot) : InventoryItem     │
└──────────────────────────────────────────┘


═══════════════════════════════════════════════════════════════════
                      V — View 层
              （只负责显示，不包含业务逻辑）
═══════════════════════════════════════════════════════════════════

┌──────────────────────────────────────────┐
│  «interface»                            │
│  IInventoryView                         │
├──────────────────────────────────────────┤
│ + RefreshAll(model)                     │
│ + ShowTooltip(item, position)           │
│ + HideTooltip()                         │
│ + PlayAddAnimation(index)               │
│ + PlayRemoveAnimation(index)            │
└──────────────────────────────────────────┘
         △
         │ 实现
┌────────┴─────────────────────────────────┐
│  UI_InventoryView : MonoBehaviour,       │
│  IInventoryView                          │
├──────────────────────────────────────────┤
│ slotViews : UI_ItemSlotView[]            │
│ goldText : TMP_Text                      │
├──────────────────────────────────────────┤
│ + RefreshAll(model)                      │
│   → 遍历slots，把model数据推到View       │
└──────────────────────────────────────────┘
         │ 组合 1..*
         ▼
┌──────────────────────────────────────────┐
│  UI_ItemSlotView : MonoBehaviour         │
│  «单个格子的纯View — 只管显示»            │
├──────────────────────────────────────────┤
│ icon : Image                             │
│ stackText : TMP_Text                     │
│ highlight : GameObject                   │
│ emptyIcon : GameObject                   │
├──────────────────────────────────────────┤
│ + SetData(item, stackSize)               │
│ + SetEmpty()                             │
│ + SetHighlight(on)                       │
│ + GetRectTransform()                     │
└──────────────────────────────────────────┘


═══════════════════════════════════════════════════════════════════
                      C — Controller 层
               （处理用户输入，调用Model，更新View）
═══════════════════════════════════════════════════════════════════

┌──────────────────────────────────────────┐
│  InventoryController                     │
│  «MonoBehaviour — 桥接View和Model»       │
├──────────────────────────────────────────┤
│ model : InventoryModel                   │
│ view : IInventoryView                    │
│ equipmentModel : EquipmentModel          │
├──────────────────────────────────────────┤
│ + OnSlotClicked(slotIndex)              │
│   → 判断物品类型 → 调用Model对应方法     │
│   → Model执行 → 触发OnDataChanged       │
│   → View.RefreshAll()                   │
│                                          │
│ + OnSlotRightClicked(slotIndex)         │
│ + OnSlotDrag(from, to)                  │
│ + OnQuickKeyPressed(keyIndex)           │
│ + OnEquipSlotClicked(slotType)          │
│ + OnUnequipClicked(slotType)            │
└──────────────────────────────────────────┘

┌──────────────────────────────────────────┐
│  «static»                               │
│  InventoryEvents  ← 全局事件通道          │
├──────────────────────────────────────────┤
│ + OnItemUsed : Action<InventoryItem>     │
│ + OnItemEquipped : Action<InventoryItem> │
│ + OnItemUnequipped : Action<InventoryItem>│
│ + OnInventoryOpened : Action             │
│ + OnInventoryClosed : Action             │
└──────────────────────────────────────────┘
```

### 数据流（点击使用一个药水的完整链路）

```
用户点击格子
     │
     ▼
UI_ItemSlotView                   ← View 层：只负责接收点击事件
(IPointerDownHandler)
     │
     │ 把 "第3个格子被点了" 告诉 Controller
     ▼
InventoryController               ← Controller 层：决策
.OnSlotClicked(slotIndex: 3)
     │
     │ 1. 查 model.GetItemAt(3) → 是个 Consumable
     │ 2. 判断：战斗中？→ 是 → 可以用
     │ 3. 调用 model.UseItem(item)
     ▼
InventoryModel                    ← Model 层：执行数据变更
.UseItem(item)
     │ stackSize--
     │ 触发 OnDataChanged?.Invoke()
     ▼
InventoryController               ← Controller：响应数据变更
(订阅了 model.OnDataChanged)
     │ view.RefreshAll(model)
     ▼
UI_InventoryView                  ← View 层：刷新显示
.RefreshAll(model)
     │ 遍历 slotViews[i].SetData(...)
     ▼
UI_ItemSlotView                   ← 单个格子更新图标和数量
.SetData(item, stackSize)
```

**关键区别**：View 不知道 Item 怎么用、什么时候能用。它只是说"这个格子被点了"，Controller 决定要做什么，Model 执行数据变更，最后 Controller 通知 View 刷新。

---

## 4. 每个类的职责一句话总结

| 层 | 类名 | 一句话职责 |
|----|------|-----------|
| **M** | `ItemDataSO` | 物品的**静态模板**——策划在编辑器里配的图标/名字/类型/价格，永远不变 |
| **M** | `InventoryItem` | 物品的**运行时实例**——包裹 ItemDataSO + 当前堆叠数 + 唯一ID，存在于内存中 |
| **M** | `InventoryModel` | 背包的**核心数据结构**——List\<Item\> + 增删改查 + 变更通知。**不知道 UI 的存在** |
| **M** | `EquipmentModel` | 装备槽的**核心数据结构**——哪个槽位装了哪个物品。**不知道 UI 的存在** |
| **V** | `IInventoryView` | View 的**接口契约**——任何背包 UI 都必须实现 RefreshAll / ShowTooltip |
| **V** | `UI_InventoryView` | 背包**主面板**——拿到 Model 数据后，分发给各子格子。不包含"点了格子该怎么办" |
| **V** | `UI_ItemSlotView` | **单个格子**的渲染——显示图标/数量/高亮/空状态。点击时只通知 Controller |
| **C** | `InventoryController` | **大脑/调度中心**——接收 View 的点击、拖拽、快捷键，决定调 Model 的哪个方法 |
| **C** | `InventoryEvents` | **跨系统通信**的 static event——"物品被使用了"这类事件，让属性系统/音效系统自己订阅 |

---

## 5. 为什么不把 Controller 的逻辑写在 View 里？

**反面例子（View 当 Controller 用）**：

```csharp
// ❌ 初学者写法：View 里直接写业务逻辑
public class UI_ItemSlot : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // View 直接判断物品类型、直接调 Model
        if (item.type == ItemType.Consumable)
            playerInventory.TryUseItem(item);
        else if (item.type == ItemType.Equipment)
            playerInventory.TryEquipItem(item);
        else if (item.type == ItemType.Material)
            ShowDetailInfo(item);
    }
}
```

问题：假设加需求"战斗中不能使用消耗品，对话中不能打开装备栏"。你必须去改 `UI_ItemSlot` 这个 View 脚本。如果你有一个快捷键栏（键盘 1/2 使用物品），需要在另一个脚本里**再写一遍相同判断**。

**正面例子（Controller 集中处理）**：

```csharp
// ✓ Controller 集中处理所有业务逻辑
public class InventoryController : MonoBehaviour
{
    public void OnItemUseRequested(InventoryItem item)
    {
        if (player.IsInCombat && item.data.itemType == ItemType.Consumable)
            model.UseItem(item);
        else if (player.IsInDialogue)
            ShowMessage("对话中不能使用物品");
        else
            model.UseItem(item);
    }
}
```

View 只需要做一件事：**把 "用户想用这个物品" 告诉 Controller**。

---

## 6. View 怎么通知 Controller？——两种常见方案

### 方案 A：直接引用（简单项目推荐）

```csharp
public class UI_ItemSlotView : MonoBehaviour, IPointerDownHandler
{
    public InventoryController controller;  // Inspector 拖拽
    public int slotIndex;

    public void OnPointerDown(PointerEventData eventData)
    {
        controller.OnSlotClicked(slotIndex);  // 只传"哪个格子"
    }
}
```

### 方案 B：事件驱动（复杂项目，完全解耦）

```csharp
// View 触发事件
public class UI_ItemSlotView : MonoBehaviour, IPointerDownHandler
{
    public static event Action<int> OnSlotClicked;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnSlotClicked?.Invoke(slotIndex);
    }
}

// Controller 订阅事件
public class InventoryController : MonoBehaviour
{
    private void OnEnable() => UI_ItemSlotView.OnSlotClicked += HandleSlotClick;
    private void OnDisable() => UI_ItemSlotView.OnSlotClicked -= HandleSlotClick;

    private void HandleSlotClick(int index) { /* 业务逻辑 */ }
}
```

---

## 7. Model 怎么通知 View 刷新？——Observer 模式

```
Model 数据变了 ──event──→ Controller 收到通知 ──调用──→ View.RefreshAll()
```

**永远不要让 Model 直接调 View**。Model 只负责发通知（`OnDataChanged`），Controller 决定怎么刷新。

```csharp
// Model 层 —— 纯 C#，不引用 UnityEngine.UI
public class InventoryModel
{
    public event Action OnDataChanged;

    public bool AddItem(InventoryItem item)
    {
        // ... 数据操作 ...
        OnDataChanged?.Invoke();  // 只通知"变了"，不管谁来响应
        return true;
    }
}

// Controller 层 —— 订阅 Model，驱动 View
public class InventoryController : MonoBehaviour
{
    private void Awake()
    {
        model.OnDataChanged += () => view.RefreshAll(model);
        // 可以注册多个 View：
        // model.OnDataChanged += () => quickSlotView.Refresh(model);
        // model.OnDataChanged += () => equipmentView.Refresh(model);
    }
}
```

---

## 8. 项目实际做法 vs 理想 MVC —— 对比表

| 维度 | 项目实际做法 | 理想 MVC |
|------|------------|---------|
| Model 独立性 | `Inventory_Base` 继承 MonoBehaviour | `InventoryModel` 纯 C# 类，不继承任何东西 |
| View 和 Model 的通信 | View 直接调 Model 方法 | View → Controller → Model |
| 业务逻辑位置 | 分散在 View 和 Model 中 | 集中在 Controller |
| 数据变更通知 | `OnInventoryChange` event → View 直接刷新 | Model 发事件 → Controller 调 View |
| 新增 UI 面板 | 新面板自己订阅 event，自己调 Model | 新面板实现 IInventoryView，Controller 管理 |
| 单元测试 | 几乎无法测试（依赖 MonoBehaviour 和 Scene） | Model 层可独立测试 |
| 代码量 | 少 | 多出 Controller 层和接口 |

---

## 9. Checklist：设计背包-UI 系统需要的类

```
□ Model 层（不依赖 Unity UI，纯C#）
  □ ItemDataSO / ItemConfig      — 物品静态模板（ScriptableObject）
  □ InventoryItem                — 运行时物品实例（纯C#）
  □ InventoryModel               — 背包数据容器 + 增删改查 + 变更通知
  □ EquipmentModel               — 装备槽数据容器（如果需要装备系统）

□ View 层（只负责显示，不含物品使用逻辑）
  □ IInventoryView 接口          — 定义 View 必须实现的方法
  □ UI_InventoryPanel            — 背包主面板，管理所有格子
  □ UI_ItemSlot                  — 单个格子：图标/数量/高亮/空状态
  □ UI_ItemTooltip               — 物品详情浮窗
  □ UI_EquipmentSlot             — 装备槽显示

□ Controller 层（业务逻辑决策）
  □ InventoryController          — 点击/拖拽/快捷键 → 判断 → 调Model → 刷新View
  □ InventoryEvents              — static event 跨系统通信（可选）

□ 辅助
  □ ItemFactory                  — 从 ItemDataSO 创建 InventoryItem（可选）
  □ ISaveable                    — 存档接口（Model 层实现）
```

**最核心的一条原则**：View 永远只做两件事——
1. 把用户操作告诉 Controller
2. 根据 Controller 的指令刷新显示

View 里不应该出现 `if (itemType == Consumable)` 这种业务判断。
