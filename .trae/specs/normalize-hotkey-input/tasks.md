# Tasks

- [x] Task 1: 创建 HotkeyParser 工具类，统一快捷键解析逻辑
  - [x] 创建 Services/HotkeyParser.cs
  - [x] 实现 Parse() 方法返回 (uint modifiers, byte vk, string normalized)
  - [x] 实现 NormalizeHotkey() 方法：规范化显示格式（Ctrl+Shift+T）
  - [x] 将 ParseVkByName 移入 HotkeyParser
- [x] Task 2: 替换 SelectionService 和 HotkeyService 中的重复代码
  - [x] SelectionService.cs 删除本地 ParseHotkey/ParseVkByName，改用 HotkeyParser.Parse()
  - [x] HotkeyService.cs 删除本地 ParseHotkey/ParseVkByName，改用 HotkeyParser.Parse()
- [x] Task 3: 配置加载时规范化快捷键并回写
  - [x] ConfigManager.Load() 中调用 HotkeyParser 规范化 Hotkey 字段
  - [x] 若规范化结果与原始值不同，自动保存回 config.json
- [x] Task 4: 编译验证 + publish + 启动测试
  - [x] dotnet build 无错误
  - [x] dotnet publish
  - [x] 启动应用验证快捷键功能正常

# Task Dependencies
- Task 2 depends on Task 1
- Task 3 depends on Task 1
- Task 4 depends on Task 2, Task 3
