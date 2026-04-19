# Tasks

- [x] Task 1: ~~重写 WindowManager.ShowPopup — 纯 WPF 方式~~（已验证失败，见 research-log 尝试6）
- [x] Task 2: ~~处理 UIA BoundingRectangles 的坐标系转换~~（已验证失败）
- [x] Task 3: ~~重写 WindowManager — Show() → SetWindowPos() 覆盖 + LogicalToPhysicalPoint~~（已验证失败，见 research-log 尝试7）
- [x] Task 5: **修复根因 — 全物理坐标方案**
  - [x] SelectionService: `GetCursorPos` → `GetPhysicalCursorPos`，直接获取物理坐标
  - [x] WindowManager: 移除 LogicalToPhysicalPoint / PhysicalToLogicalPoint 转换
  - [x] WindowManager.ShowPopupAtSelection: UIA 物理坐标直接使用，不再转逻辑坐标再转回物理坐标
  - [x] WindowManager.CalculatePosition: 改为接收物理坐标，直接计算，无转换
- [ ] Task 6: 编译测试验证弹窗位置正确性
  - [ ] 在屏幕不同位置选中文字 → Ctrl+Shift+T → 弹窗在选中文字附近
  - [ ] 靠近屏幕边缘时弹窗自动翻转方向

---

# Task Dependencies
- [Task 6] depends on [Task 5]
