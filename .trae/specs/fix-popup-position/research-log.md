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
- Microsoft Learn "Understanding Screen Scaling Issues"
- dotnet/wpf GitHub Issues #3105, #4127, #11335
- CSDN / StackOverflow 社区讨论
- SciChart "WPF Window Positioning & DPI Handling"

### 发现 1: GetCursorPos 在 PerMonitorV2 下的行为
> "typically, the GetCursorPos function returns the **logical coordinates**"

**结论**：PerMonitorV2 模式下，`GetCursorPos` 返回**逻辑坐标（DIP）**。

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

### 发现 5: WPF 多显示器 DPI 转换的已知 Bug
> dotnet/wpf #4127 完整描述了 WPF 在 PerMonitorV2 下定位窗口的问题：
> - 窗口在不同 DPI 显示器间移动时，Left/Top 会因 DPI 缩放因子变化而"跳跃"
> - 未打开的窗口无法确定所在显示器，WPF 只能猜测

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

---

## 正确方案（基于调研）

### 方案 A: SetWindowPos 覆盖法（推荐 ✅）
```
1. Show() 窗口（WPF 用默认位置显示）
2. 立即用 SetWindowPos(hwnd, x, y, ...) 覆盖位置
3. 全程使用 Win32 物理坐标：
   - GetCursorPos → 物理坐标（PerMonitorV2 下需确认）
   - 或 GetPhysicalCursorPos → 明确物理坐标
   - SetWindowPos → 物理坐标
   - GetSystemMetrics(SM_CXSCREEN) → 物理坐标
✅ 统一坐标系，不依赖 WPF 的 Left/Top 解释
```

### 方案 B: WPF PointToScreen 转换
```
GetCursorPos() → 屏幕坐标 → 通过某 Visual 的 PointToScreen() 转换为 WPF 坐标 → Window.Left/Top
⚠️ 需要一个已显示的 Visual 作为参考，可能复杂
```

### 方案 C: 先 Show 再 SetWindowPos
```
Window.Left = 0, Top = 0 (或任意初始值) → Show() → SetWindowPos(hwnd, 正确坐标)
✅ 让 WPF 先完成初始化，然后用 Win32 API 覆盖位置
```

---

## 待验证假设

1. **WPF 窗口边框偏移**: Window.Left/Top 可能指的是窗口内容区而非窗口外框
2. **WPF 内部坐标系 vs 屏幕坐标系**: Left/Top 可能需要通过 PointToScreen 转换
3. **多显示器 DPI 猜测**: 未显示的窗口不知道在哪个显示器上，WPF 用 System DPI 猜测
4. **GetCursorPos vs GetPhysicalCursorPos**: PerMonitorV2 下两者可能有不同行为
