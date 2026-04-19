# 重新设计 SelectionService — 摆脱剪贴板依赖

## 问题分析

当前架构的**根本缺陷**：用「模拟 Ctrl+C + 读写剪贴板」来获取选中文字。

这带来两个顽固 bug：

1. **剪贴板污染**：模拟 Ctrl+C 后恢复剪贴板，恢复失败时选中文字留在剪贴板里；恢复成功但 `SetText("")` 空字符串也可能抛异常
2. **空选中也触发翻译**：没选中文字时 Ctrl+C 可能复制当前行（IDE）或不改变剪贴板，导致 `selectedText == savedText`（旧剪贴板内容），被当作"选中文字"去翻译

## 方案：UI Automation 优先 + 剪贴板兜底

### 核心思路

```
获取选中文字的优先级：
1. UI Automation (TextPattern) → 大多数现代应用支持，完全不碰剪贴板
2. 剪贴板方式（仅作为兜底）→ 修复现有 bug
```

### UI Automation 原理

* Windows 自带的辅助功能 API，`System.Windows.Automation` 命名空间

* 通过 `TextPattern.GetSelection()` 直接获取选中文字，**不需要剪贴板**

* 浏览器（Chrome/Edge/Firefox）、Office、记事本、VS Code 等都支持

* 不支持的场景：部分游戏、UWP 应用、管理员权限应用

### 剪贴板兜底修复

当 UIA 失败时才用剪贴板方式，同时修复两个 bug：

1. **空选中检测**：`selectedText == savedText` 时返回空字符串（说明没有新内容被复制）
2. **剪贴板恢复**：用 `Clipboard.SetDataObject(text, true, 3, 100)` 替代 `SetText`，自带重试机制

## 实现步骤

### Step 1: 新建 `UiaSelectionService.cs`

* 使用 `System.Windows.Automation` 命名空间

* `AutomationElement.FocusedElement` 获取当前焦点元素

* 尝试获取 `TextPattern`，调用 `GetSelection()`

* 如果获取成功且非空，直接返回选中文字

* 如果失败，返回 null 表示需要兜底

### Step 2: 重构 `SelectionService.cs`

* `GetSelectedText()` 改为：先调 UIA → 失败则用剪贴板兜底

* 剪贴板兜底增加空选中检测：`selectedText == savedText` → 返回 ""

* 剪贴板恢复改用 `Clipboard.SetDataObject` 带重试

* 添加 `_lastText` 缓存，避免重复翻译同一段文字

### Step 3: 修改 `App.xaml.cs`

* `HandleTranslateAsync` 增加去重逻辑：如果新文字和上次一样，不重复翻译

## 文件变更清单

| 文件                                | 操作                          |
| --------------------------------- | --------------------------- |
| `Services/UiaSelectionService.cs` | 新建 — UI Automation 获取选中文字   |
| `Services/SelectionService.cs`    | 修改 — UIA 优先 + 剪贴板兜底 + 空选中检测 |
| `App.xaml.cs`                     | 修改 — 去重逻辑                   |

## 风险评估

* UIA 在少数应用中不可用 → 有剪贴板兜底，不影响功能

* UIA 调用可能耗时 → 加超时控制（500ms）

* 不需要额外 NuGet 包 → `System.Windows.Automation` 是 .NET Framework/Core 内置的

