# 快捷键配置兼容性验证 Spec

## Why
用户反馈配置文件仍是旧格式（HotkeyModifiers + HotkeyKey），虽然代码已支持向后兼容且 publish/config.json 已更新为新格式，但需要验证完整流程：用户运行新版 exe → 读取配置 → 注册快捷键 → 一切正常工作。

## What Changes
- 验证 AppConfig 向后兼容逻辑正确（已实现）
- 确保 publish/config.json 为新格式（已完成）
- 重新 publish 并启动应用让用户测试
- 确认托盘菜单显示的快捷键格式友好

## Impact
- Affected code: Config/AppConfig.cs, Services/SelectionService.cs, MainWindow.xaml.cs
- Affected files: publish/config.json, publish/TranslateSharp.exe

## Requirements

### Requirement: 旧格式配置自动迁移
系统 SHALL 在检测到旧格式配置（HotkeyModifiers + HotkeyKey）时：
- 自动合并为新的 Hotkey 字段
- 保存为新格式覆盖旧文件
- 后续读取使用新格式

#### Scenario: 用户首次用新版打开旧格式配置
- **WHEN** 用户 config.json 包含 `HotkeyModifiers: "Ctrl+Shift"` 和 `HotkeyKey: "T"` 但没有 `Hotkey` 字段
- **THEN** 系统自动设置 `Hotkey = "Ctrl+Shift+T"` 并保存，托盘显示 "Ctrl+Shift+T"

### Requirement: 新格式配置直接生效
- **WHEN** 用户 config.json 已包含 `"Hotkey": "Ctrl+Shift+T"`
- **THEN** 直接使用该值注册全局快捷键，无需迁移

### Requirement: 托盘菜单显示友好格式
- **WHEN** 用户右键托盘图标查看配置信息
- **THEN** 显示 "快捷键: Ctrl+Shift+T" 而非原始 JSON 格式
