# 悬浮窗定位修复 Spec

## Why
悬浮窗位置不准确，出现在鼠标很上方且偏右，无法跟随选中文字/光标显示。

## What Changes
- 重写 WindowManager 窗口定位逻辑：采用 **Show() → SetWindowPos() 覆盖** 方式
- 统一使用 Win32 物理像素坐标系，绕过 WPF Left/Top 的内部坐标空间问题

## Impact
- Affected code: `Services/WindowManager.cs`
- Affected behavior: 悬浮窗显示位置

---

## ADDED Requirements

### Requirement: 悬浮窗正确定位 — SetWindowPos 覆盖方案
系统 SHALL 使用「先 Show 再 SetWindowPos 覆盖」方式定位翻译弹窗，全程使用 Win32 物理坐标。

#### Scenario: 光标定位成功
- **WHEN** 用户选中文字并按 Ctrl+Shift+T
- **THEN** 弹窗出现在光标/选中文字下方附近（误差 < 50px）

#### Scenario: UIA 坐标可用时优先使用
- **WHEN** UI Automation 成功获取选中文字边界框（物理坐标）
- **THEN** 弹窗以选中文字边界框底部中心为基准定位（直接用物理坐标）

#### Scenario: UIA 不可用时使用光标位置兜底
- **WHEN** UI Automation 无法获取选中文字坐标
- **THEN** 弹窗以 GetCursorPos 获取的光标位置为基准（需转为物理坐标）

#### Scenario: 屏幕边缘检测
- **WHEN** 弹窗超出屏幕右边缘或下边缘
- **THEN** 弹窗自动翻转方向（左侧或上方显示）
