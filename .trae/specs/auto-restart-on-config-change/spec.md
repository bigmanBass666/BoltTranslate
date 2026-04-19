# 配置修改后自动重启 Spec

## Why
用户通过托盘菜单打开 config.json 修改配置后，需要手动重启才能生效。应改为保存后自动重启。

## What Changes
- MainWindow.OpenConfigFile() 中，notepad.exe 退出后检测配置是否变化
- 如果配置有变化，自动调用 RestartApplication()

## Impact
- Affected code: `MainWindow.xaml.cs`

---

## ADDED Requirements

### Requirement: 配置修改后自动重启
系统 SHALL 在用户通过托盘菜单编辑 config.json 后，如果检测到配置内容变化则自动重启应用。

#### Scenario: 用户修改配置并保存
- **WHEN** 用户点击托盘「打开配置文件」→ 编辑 → 保存 → 关闭 notepad
- **THEN** 检测 config.json 内容是否变化 → 若变化则自动重启

#### Scenario: 用户未修改直接关闭
- **WHEN** 用户打开配置文件后未做任何修改直接关闭
- **THEN** 不重启
