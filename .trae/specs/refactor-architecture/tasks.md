# Tasks

- [ ] Task 1: 创建 `NativeInterop/Win32Api.cs` — 统一 Win32 P/Invoke 层
  - [ ] 从 SelectionService、WindowManager、TranslationPopup 提取所有 P/Invoke 声明
  - [ ] 统一 POINT 结构体（删除 tagPOINT 重复）
  - [ ] 按功能分组：窗口操作、光标、剪贴板、热键、键盘模拟
- [ ] Task 2: 拆分 SelectionService — 创建 IHotkeyService + HotkeyService
  - [ ] 热键注册/注销、WndProc 消息循环、冷却时间管理
  - [ ] 回调接口改为 `Action<string>`（纯文本，不含坐标）
- [ ] Task 3: 创建 IClipboardService + ClipboardService
  - [ ] 剪贴板保存/恢复、Ctrl+C 模拟、序列号检测
  - [ ] STA 线程封装、重试逻辑
- [ ] Task 4: 重构 ITextSelectionService — 协调 UIA 和 ClipboardService
  - [ ] UIA 优先，ClipboardService 兜底
  - [ ] 移除对 Win32 API 的直接依赖（通过 ClipboardService）
- [ ] Task 5: 清理 App.xaml.cs 和 MainWindow
  - [ ] 构造函数注入替代 SetServices()
  - [ ] HandleTranslateAsync 中的错误处理统一化
  - [ ] 翻译 prompt 提取为常量或配置项
- [ ] Task 6: 提取常量和配置
  - [ ] 弹窗偏移量、冷却时间等提取到 PopupConfig 或常量类
  - [ ] 魔法数字替换为命名常量
- [ ] Task 7: 编译测试验证
  - [ ] 编译 0 错误 0 警告
  - [ ] 功能回归测试：快捷键 → 选中文本 → 翻译 → 弹窗显示

---

# Task Dependencies
- [Task 2, 3] depends on [Task 1]
- [Task 4] depends on [Task 2, 3]
- [Task 5] depends on [Task 4]
- [Task 6] 可与 [Task 5] 并行
- [Task 7] depends on [Task 4, 5, 6]
