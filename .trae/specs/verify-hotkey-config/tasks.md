# Tasks

- [x] Task 1: 验证代码中所有 Hotkey 引用都使用 EffectiveHotkey
  - [x] 检查 SelectionService.cs 使用 EffectiveHotkey
  - [x] 检查 MainWindow.xaml.cs 使用 EffectiveHotkey
  - [x] 检查 HotkeyService.cs 使用 EffectiveHotkey
- [x] Task 2: 确保 publish/config.json 为新格式且 ApiKey 与用户一致
  - [x] 更新 ApiKey 为用户实际值（015518e0bc86498dafdc42bf88b1572a.dioiOVESgV9mUg5i）
  - [x] 确认 Hotkey 字段为 "Ctrl+Shift+T"
- [x] Task 3: 重新 publish 并启动应用
  - [x] dotnet build 验证编译通过
  - [x] dotnet publish 发布新版本
  - [x] 启动 publish/TranslateSharp.exe 供用户测试
