# 基于 COM IUIAutomation 重构获取选中文字方案

## 背景

基于对 Pot Desktop、selection-hook 等成熟工具的调研，确定以下方案：
- 之前用 `System.Windows.Automation` (.NET 托管 API) 会导致 0xc0000005 崩溃
- Pot Desktop 使用 COM IUIAutomation 原生接口，更稳定
- 剪贴板兜底需要用 `GetClipboardSequenceNumber()` 检测变化

## 实现步骤

### Step 1: 新建 `Services/NativeInterop/UiaNative.cs`

用 P/Invoke 定义 COM IUIAutomation 原生接口：
- `CoInitialize` / `CoUninitialize`
- `CoCreateInstance` + `CUIAutomation` CLSID
- `IUIAutomation` 接口：`GetFocusedElement`
- `IUIAutomationElement` 接口：`GetCurrentPatternAs`
- `IUIAutomationTextPattern` 接口：`GetSelection`
- `IUIAutomationTextRange` 接口：`GetText`

### Step 2: 重写 `Services/UiaSelectionService.cs`

- 用 COM 原生接口替代 .NET 托管 API
- 流程：`CoInitialize` → `CoCreateInstance(CUIAutomation)` → `GetFocusedElement` → `GetCurrentPatternAs(TextPattern)` → `GetSelection` → `GetText`
- 每步检查 HRESULT，失败返回 null
- `CoUninitialize` 清理

### Step 3: 重构 `Services/SelectionService.cs` 剪贴板兜底

关键改进：
1. **GetClipboardSequenceNumber 检测**：模拟 Ctrl+C 前后检查序列号，没变说明没有选中文字
2. **先释放所有修饰键**：避免快捷键残留冲突
3. **恢复剪贴板区分文本/图片/空**：
   - 文本 → `SetText`
   - 空 → `Clipboard.Clear()`
4. **空选中检测**：序列号没变 → 返回空字符串

### Step 4: 删除旧的 .NET 托管 UIA 代码

- `UiaSelectionService.cs` 中不再使用 `System.Windows.Automation`

### Step 5: 编译测试验证

## 文件变更清单

| 文件 | 操作 |
|------|------|
| `Services/NativeInterop/UiaNative.cs` | 新建 — COM IUIAutomation P/Invoke 定义 |
| `Services/UiaSelectionService.cs` | 重写 — 用 COM 原生接口 |
| `Services/SelectionService.cs` | 修改 — 剪贴板兜底加 GetClipboardSequenceNumber |
