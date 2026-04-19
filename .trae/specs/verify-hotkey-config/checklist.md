# Checklist

- [x] AppConfig.cs 包含 EffectiveHotkey 计算属性，正确处理新旧格式
- [x] ConfigManager.Load() 包含 NeedsMigration 检测和自动迁移逻辑
- [x] 所有服务层代码引用 _config.EffectiveHotkey 而非 _config.Hotkey
- [x] publish/config.json 使用新格式：`"Hotkey": "Ctrl+Shift+T"`
- [x] publish/config.json 的 ApiKey 与用户提供的值一致
- [x] dotnet build 编译无错误
- [x] dotnet publish 成功生成新 exe
- [x] 应用可正常启动，快捷键 Ctrl+Shift+T 可触发翻译
