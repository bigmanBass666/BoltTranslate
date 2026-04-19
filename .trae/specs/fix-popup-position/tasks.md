# Tasks

- [x] Task 1: ~~重写 WindowManager.ShowPopup — 纯 WPF 方式~~（已验证失败，见 research-log 尝试6）
- [x] Task 2: ~~处理 UIA BoundingRectangles 的坐标系转换~~（已验证失败）
- [x] Task 3: 重写 WindowManager — 采用 **Show() → SetWindowPos() 覆盖** 方案
  - [x] 添加 Win32 P/Invoke：SetWindowPos、LogicalToPhysicalPoint、GetSystemMetrics(SM_CXSCREEN/SM_CYSCREEN)
  - [x] ShowPopup() 逻辑：设置 Left=0, Top=0 → Show() → 立即调用 SetWindowPos(hwnd, physicalX, physicalY, ...)
  - [x] 统一物理坐标计算：UIA 坐标通过 PhysicalToLogicalPoint 转→逻辑坐标，再由 LogicalToPhysicalPoint 转回物理坐标
  - [x] 边界检测使用 GetSystemMetrics 物理坐标
- [ ] Task 4: 编译测试验证弹窗位置正确性
  - [ ] 在屏幕不同位置选中文字 → Ctrl+Shift+T → 弹窗在选中文字附近
  - [ ] 靠近屏幕边缘时弹窗自动翻转方向

---

# Task Dependencies
- [Task 4] depends on [Task 3]
