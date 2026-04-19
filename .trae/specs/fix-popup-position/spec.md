# 悬浮窗定位修复 Spec

## Why
悬浮窗位置「偏上 + 偏中间」（左边选字偏右，右边选字偏左）。根因：`LogicalToPhysicalPoint` 在 PerMonitorV2 模式下是**空操作（NO-OP）**，逻辑坐标被直接当物理坐标传给 `SetWindowPos`。

## What Changes
- **SelectionService**: 用 `GetPhysicalCursorPos` 替代 `GetCursorPos`，直接获取物理坐标
- **WindowManager**: 移除所有坐标转换，全程使用物理坐标；UIA 坐标本身已是物理坐标，无需转换

## Impact
- Affected code: `Services/SelectionService.cs`, `Services/WindowManager.cs`
- Affected behavior: 悬浮窗显示位置

---

## ADDED Requirements

### Requirement: 悬浮窗正确定位 — 全物理坐标方案
系统 SHALL 全程使用物理像素坐标系定位翻译弹窗，不做任何逻辑↔物理转换。

#### Scenario: 光标定位成功
- **WHEN** 用户选中文字并按 Ctrl+Shift+T
- **THEN** 弹窗出现在光标/选中文字下方附近（误差 < 50px）

#### Scenario: UIA 坐标可用时优先使用
- **WHEN** UI Automation 成功获取选中文字边界框（物理坐标）
- **THEN** 弹窗以选中文字边界框底部中心为基准定位（直接用物理坐标）

#### Scenario: UIA 不可用时使用光标位置兜底
- **WHEN** UI Automation 无法获取选中文字坐标
- **THEN** 弹窗以 GetPhysicalCursorPos 获取的光标位置为基准（物理坐标，无转换）

#### Scenario: 屏幕边缘检测
- **WHEN** 弹窗超出屏幕右边缘或下边缘
- **THEN** 弹窗自动翻转方向（左侧或上方显示）
