# 悬浮窗定位修复 Spec

## Why
经过 7 次尝试（GetCursorPos 直接赋值、DPI 缩放、纯 WPF、SetWindowPos 覆盖、全物理坐标等），弹窗位置始终不准确。决定改用**固定位置方案**：弹窗始终出现在屏幕固定位置（如光标附近或屏幕中央偏右下）。

## What Changes
- WindowManager 改为固定位置显示弹窗，不再依赖光标/选区坐标计算

## Impact
- Affected code: `Services/WindowManager.cs`
- Affected behavior: 悬浮窗显示位置

---

## ADDED Requirements

### Requirement: 悬浮窗固定位置显示
系统 SHALL 在用户按下快捷键后，将翻译弹窗显示在**屏幕固定位置**。

#### Scenario: 固定位置显示
- **WHEN** 用户选中文字并按 Ctrl+Shift+T
- **THEN** 弹窗出现在屏幕右下角区域（或光标右下方固定偏移）
