# 悬浮窗定位 — 试错记录与调研发现

> **重要**：此文件记录所有探索经历和试错过程，避免后续 AI 重复探索！

---

## 坐标系基础知识

Windows 存在两套坐标系：

| 坐标系 | 说明 | 适用场景 |
|--------|------|----------|
| **逻辑坐标（DIP）** | 与 DPI 无关的虚拟像素，96 DIP = 1 英寸 = 1 DIP | WPF Window.Left/Top、GetCursorPos（PerMonitorV2） |
| **物理坐标（设备像素）** | 屏幕实际像素数 | SetWindowPos、UI Automation BoundingRectangles |

---

## 关键调研发现

### 来源
- Microsoft Learn "LogicalToPhysicalPoint function (winuser.h)" — **官方文档**
- Microsoft Learn "GetPhysicalCursorPos function" — **官方文档**
- dotnet/wpf GitHub Issues #3105, #4127, #11335
- CSDN / StackOverflow 社区讨论

### 发现 1: GetCursorPos 在 PerMonitorV2 下的行为
> "typically, the GetCursorPos function returns the **logical coordinates**"

**结论**：PerMonitorV2 模式下，`GetCursorPos` 返回**逻辑坐标（DIP）**。

### 发现 1.5: GetPhysicalCursorPos — ⚠️⚠️ 关键！
> Microsoft Learn: "Retrieves the position of the cursor in **physical coordinates**."

**结论**：`GetPhysicalCursorPos()` 直接返回**物理坐标**，无需任何转换！这是最简洁的方案。

### 发现 2: WPF Window.Left/Top 的坐标系 — ⚠️ 关键！
> dotnet/wpf #3105: "Left/Top is expressed in WPF's **local** device independent 1/96" coordinate space"
> dotnet/wpf #4127: "**If the window is not opened, then WPF will have to guess on which monitor WPF position is.**"

**结论**：WPF 的 `Window.Left/Top` **不是屏幕逻辑坐标**！而是 WPF 内部坐标空间！WPF 需要知道窗口在哪个显示器上才能正确转换。窗口未 Show 时，WPF 会**猜测**显示器，可能猜错！

### 发现 3: SetWindowPos 的坐标系
Win32 文档：SetWindowPos 接受**物理坐标（设备像素）**

**结论**：`SetWindowPos` 使用物理坐标，与 WPF `Window.Left/Top` 不同坐标系！

### 发现 4: UI Automation BoundingRectangles 的坐标系
> "The UI Automation API does not use logical coordinates. CurrentBoundingRectangle returns **physical coordinates**"

**结论**：UIA 的 `GetBoundingRectangles()` 返回**物理坐标**。

### 发现 5: ⚠️⚠️⚠️ LogicalToPhysicalPoint 在 PerMonitorV2 下是空操作！！！
> **Microsoft Learn 官方文档原文**：
> "In Windows 8.1, **PhysicalToLogicalPoint and LogicalToPhysicalPoint no longer transform points**. The system returns all points to an application in its own coordinate space."
> "In those cases, use **PhysicalToLogicalPointForPerMonitorDPI** and **LogicalToPhysicalPointForPerMonitorDPI**."

**结论**：
- **`LogicalToPhysicalPoint` 和 `PhysicalToLogicalPoint` 在 PerMonitorV2 模式下是 NO-OP（空操作）！**
- 它们直接返回输入值，不做任何转换！
- 这就是尝试 7「偏上 + 偏中间」的根因：逻辑坐标被原封不动地传给了需要物理坐标的 SetWindowPos！
- 必须改用 `LogicalToPhysicalPointForPerMonitorDPI` 或 `GetPhysicalCursorPos` + 手动 DPI 缩放

### 发现 6: 「偏上 + 偏中间」症状的数学解释
假设屏幕 1920x1080 物理像素，125% DPI 缩放：
- 物理尺寸：1920 x 1080
- 逻辑尺寸：1536 x 864（1920/1.25, 1080/1.25）
- GetCursorPos 返回逻辑坐标（如右侧边缘 = 1536）
- LogicalToPhysicalPoint 空操作 → 还是 1536
- SetWindowPos(1536, ...) → 窗口出现在 1536/1920 = 80% 处 → **比预期更靠左（=偏中间）！**
- Y 轴同理 → **比预期更靠上！**

---

## 试错历史（按时间顺序）

### 尝试 1: GetCursorPos 直接赋值给 Window.Left/Top（无 DPI 转换）
- **做法**: `Window.Left = cursorX; Window.Top = cursorY;`
- **结果**: 弹窗固定在屏幕上方
- **失败原因**: 当时没意识到是 DPI/WPF 内部坐标系问题

### 尝试 2: DPI 缩放 `cursorX * dpiScaleX`
- **做法**: 用 `GetDeviceCaps(LOGPIXELSX)/96` 计算 DPI 缩放因子，乘以光标坐标
- **结果**: 弹窗乱飞（随机位置）
- **失败原因**: 如果 GetCursorPos 已返回逻辑坐标，再乘以 DPI 就会放大导致乱飞

### 尝试 3: 除以 DPI `cursorX / dpiScaleX`
- **做法**: 光标坐标除以 DPI 缩放因子
- **结果**: 固定在左上角边界处
- **失败原因**: 逻辑坐标除以 DPI 会变小，跑到左上角

### 尝试 4: 全部 Win32 API（GetCursorPos + GetSystemMetrics + SetWindowPos）
- **做法**: 移除所有 WPF 坐标操作，全部用 Win32
- **结果**: 还是固定在左上角
- **失败原因**: **关键 bug** — ShowPopupAtSelection 方法签名没接收光标参数，`_popupPos` 一直是默认值 (100,100)！

### 尝试 5: 修复 ShowPopupAtSelection 传入光标参数 + SetWindowPos
- **做法**: 修复方法签名传入 cursorX/cursorY，用 SetWindowPos 定位
- **结果**: 有所好转但位置仍不准确
- **失败原因**: 可能坐标系仍有细微差异

### 尝试 6: 纯 WPF 方式（GetCursorPos → Window.Left/Top）+ PhysicalToLogicalPoint 转 UIA
- **做法**: 移除 SetWindowPos，改用 WPF Left/Top；UIA 物理坐标用 PhysicalToLogicalPoint 转换
- **结果**: 有所好转，但弹窗出现在鼠标**很上方且偏右**
- **失败原因**: **WPF Window.Left/Top 是 WPF 内部坐标空间，不是屏幕坐标！WPF 在 Show 时猜测显示器 DPI，可能猜错！**

### 尝试 7: Show() → SetWindowPos 覆盖 + LogicalToPhysicalPoint 转换
- **做法**: Left=0 → Show() → 获取 HWND → LogicalToPhysicalPoint 转→ SetWindowPos
- **结果**: **偏上 + 偏中间！（左边选字偏右，右边选字偏左）**
- **失败原因**: **⚠️ LogicalToPhysicalPoint 在 PerMonitorV2 下是空操作！不转换坐标！逻辑坐标被直接当物理坐标用！**
- **数学验证**: 125% DPI 下逻辑值 1536 当物理值用 → 出现在 80% 位置而非 100% → 视觉上就是「偏中间」

---

## 正确方案（基于调研）

### 方案 A: GetPhysicalCursorPos 直接获取物理坐标（推荐 ✅✅✅）
```
SelectionService 中：
  用 GetPhysicalCursorPos 替代 GetCursorPos → 直接获得物理坐标
  传给 WindowManager 时已经是物理坐标

WindowManager 中：
  Show() → SetWindowPos(hwnd, physicalX, physicalY, ...)
  无需任何坐标转换！
  UIA 坐标本身就是物理坐标，也无需转换！
✅ 最简单、最可靠、零转换
```

### 方案 B: GetDpiForWindow 手动缩放
```
GetCursorPos → 逻辑坐标 → GetDpiForWindow(hwnd) → physical = logical * dpi / 96 → SetWindowPos
✅ 可行但多一步
```

### 方案 C: LogicalToPhysicalPointForPerMonitorDPI
```
替换 LogicalToPhysicalPoint 为 LogicalToPhysicalPointForPerMonitorDPI（Windows 10 1703+）
✅ 但需要检查最低 Windows 版本支持
```

---

## 最终结论

**核心原则：全程使用物理坐标，不做任何转换。**

数据源 → 坐标类型 → 处理方式
- GetPhysicalCursorPos → 物理坐标 → 直接用于 SetWindowPos ✅
- UIA BoundingRectangles → 物理坐标 → 直接用于 SetWindowPos ✅
- GetSystemMetrics(SM_CXSCREEN) → 物理坐标 → 直接用于边界检测 ✅
