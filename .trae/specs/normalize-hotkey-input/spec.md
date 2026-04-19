# 快捷键输入大小写与规范化 Spec

## Why
用户在 config.json 中输入快捷键时可能用各种大小写格式（如 `ctrl+shift+t`、`Ctrl+Shift+T`、`CTRL+SHIFT+T`），当前解析代码虽然用 `ToUpperInvariant()` 做了内部兼容，但保存回文件时不会规范化显示，且托盘菜单直接展示用户原始输入。对小白用户来说，需要确保无论输入什么大小写，显示和保存都统一为友好格式。

## What Changes
- ParseHotkey 解析成功后，将规范化后的快捷键字符串回写 config（如用户写 `ctrl+shift+t` → 自动保存为 `Ctrl+Shift+T`）
- 统一 SelectionService 和 HotkeyService 中重复的 ParseHotkey/ParseVkByName 代码为共享工具方法
- 托盘菜单始终显示规范化格式

## Impact
- Affected code: Config/AppConfig.cs, Services/SelectionService.cs, Services/HotkeyService.cs, MainWindow.xaml.cs

## ADDED Requirements

### Requirement: 快捷键大小写不敏感解析
系统 SHALL 接受任意大小写格式的快捷键输入，包括但不限于：
- `ctrl+shift+t`
- `Ctrl+Shift+T`
- `CTRL+SHIFT+T`
- `Ctrl + Shift + T`（多余空格）

#### Scenario: 用户输入小写快捷键
- **WHEN** 用户在 config.json 中写入 `"Hotkey": "ctrl+shift+t"`
- **THEN** 系统正确解析并注册快捷键，托盘显示 "Ctrl+Shift+T"

### Requirement: 快捷键规范化回写
系统 SHALL 在加载配置时，将快捷键字符串规范化为标准格式并保存回 config.json。

规范化规则：
- 修饰键首字母大写：`Ctrl`、`Shift`、`Alt`、`Win`
- 普通键大写：`T`、`F1`、`Space`
- 分隔符：`+`（无多余空格）
- 示例：`ctrl+shift+t` → `Ctrl+Shift+T`

#### Scenario: 配置加载时自动规范化
- **WHEN** 用户 config.json 包含 `"Hotkey": "ctrl+alt+f1"`
- **THEN** 系统自动将其规范化为 `"Hotkey": "Ctrl+Alt+F1"` 并保存

### Requirement: 消除重复的快捷键解析代码
SelectionService 和 HotkeyService 中存在完全相同的 ParseHotkey/ParseVkByName 方法，应提取为共享的 HotkeyParser 工具类。

#### Scenario: 统一解析入口
- **WHEN** 任何服务需要解析快捷键字符串
- **THEN** 调用 HotkeyParser.Parse() 获取统一的 (modifiers, vk, normalizedString) 结果
