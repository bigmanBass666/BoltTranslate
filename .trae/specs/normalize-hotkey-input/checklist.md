# Checklist

- [x] HotkeyParser.cs 存在，包含 Parse() 和 NormalizeHotkey() 方法
- [x] Parse() 返回 (modifiers, vk, normalizedString) 三元组
- [x] NormalizeHotkey() 将任意大小写输入转为标准格式（如 Ctrl+Shift+T）
- [x] SelectionService.cs 不再包含本地 ParseHotkey/ParseVkByName 方法
- [x] HotkeyService.cs 不再包含本地 ParseHotkey/ParseVkByName 方法
- [x] ConfigManager.Load() 在加载配置后自动规范化 Hotkey 字段
- [x] 规范化结果与原始值不同时自动保存回 config.json
- [x] dotnet build 编译无错误
- [x] 应用正常启动，快捷键功能正常
