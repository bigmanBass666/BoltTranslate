# 弹窗闪烁修复与光标样式优化 Spec

## Why
1. 翻译弹窗显示时先在屏幕左上角（Left=0, Top=0）一闪而过，然后才跳到光标右下角 — 用户体验差
2. 弹窗内所有区域都是箭头光标，无法区分可拖动区域和文字区域

## What Changes
- 修复弹窗闪烁：Show() 前设置窗口位置到屏幕外或使用 WS_EX_TOOLWINDOW + SWP_NOSHOWMOVE 隐藏初始位置
- TranslationPopup 光标样式：可拖动区域 → 手型/移动光标，文字区域 → IBeam/默认

## Impact
- Affected code: `Services/WindowManager.cs`, `Windows/TranslationPopup.xaml`, `Windows/TranslationPopup.xaml.cs`

---

## ADDED Requirements

### Requirement 1: 消除弹窗闪烁
系统 SHALL 在 Show() 时不出现左上角闪烁，直接出现在目标位置。

#### 方案
- Show() 前 Left/Top 设为屏幕外的坐标（如 -9999, -9999）
- 或使用 `SWP_NOSHOWMOVE` 标志配合 `SetWindowPos`
- Show() 后立即 SetWindowPos 到目标位置

### Requirement 2: 弹窗光标样式分区
系统 SHALL 在翻译弹窗的不同区域显示不同的鼠标光标样式：
- **标题栏/空白区域**（可拖动）：`Cursor="SizeAll"` 或手型
- **文字内容区**（TextBox）：`Cursor="IBeam"` 或 Arrow
- **关闭按钮**：`Cursor="Hand"`
