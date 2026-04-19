# Checklist

- [x] SelectionService 使用 GetPhysicalCursorPos 替代 GetCursorPos
- [x] WindowManager 移除 LogicalToPhysicalPoint / PhysicalToLogicalPoint 调用
- [x] WindowManager.ShowPopupAtSelection: UIA 物理坐标直接使用，无转换
- [x] WindowManager.ShowPopup: Show() → SetWindowPos(hwnd, physicalX, physicalY) 流程
- [x] 边界检测使用 GetSystemMetrics 获取屏幕物理尺寸
- [x] 编译通过，无警告
- [ ] 实际测试：选中文字 → Ctrl+Shift+T → 弹窗在正确位置（不偏上、不偏中间）
