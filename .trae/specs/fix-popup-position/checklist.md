# Checklist

- [x] WindowManager.ShowPopup() 使用 Show() → SetWindowPos() 覆盖方式定位
- [x] Win32 P/Invoke 声明完整（SetWindowPos、LogicalToPhysicalPoint、GetSystemMetrics）
- [x] UIA 物理坐标通过 PhysicalToLogicalPoint 转为逻辑坐标，再由 LogicalToPhysicalPoint 转回物理坐标用于 SetWindowPos
- [x] 光标坐标（逻辑）通过 LogicalToPhysicalPoint 转为物理坐标后再使用
- [x] 边界检测使用 GetSystemMetrics 获取屏幕物理尺寸
- [x] 编译通过，无警告
- [ ] 实际测试：选中文字 → Ctrl+Shift+T → 弹窗在正确位置
