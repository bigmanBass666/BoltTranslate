# Checklist

- [x] NativeInterop/Win32Api.cs 包含所有 Win32 P/Invoke 声明，无重复
- [x] SelectionService 已拆分为 HotkeyService + ClipboardService + TextSelectionService
- [x] 各服务通过接口暴露，可测试/可替换
- [x] MainWindow 使用构造函数注入，无 SetServices() 方法
- [x] 魔法数字已提取为命名常量或配置项（AppConstants）
- [x] 翻译 prompt 不再硬编码在方法体内
- [x] 错误处理策略统一（操作层不弹 MessageBox）
- [x] 编译 0 错误 0 警告
- [ ] 功能回归测试通过（快捷键 → 翻译 → 弹窗）
