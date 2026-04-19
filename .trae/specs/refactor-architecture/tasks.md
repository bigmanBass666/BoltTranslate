# Tasks

- [x] Task 1: 创建 `NativeInterop/Win32Api.cs` — 统一 Win32 P/Invoke 层
  - [x] 从 SelectionService、WindowManager、TranslationPopup 提取所有 P/Invoke 声明
  - [x] 统一 POINT 结构体（删除 tagPOINT 重复）
  - [x] 按功能分组：窗口操作、光标、剪贴板、热键、键盘模拟
- [x] Task 2: 拆分 SelectionService — 创建 IHotkeyService + HotkeyService
  - [x] 热键注册/注销、WndProc 消息循环、冷却时间管理
  - [x] 回调接口改为 `Action<string>`（纯文本，不含坐标）
- [x] Task 3: 创建 IClipboardService + ClipboardService
  - [x] 剪贴板保存/恢复、Ctrl+C 模拟、序列号检测
  - [x] STA 线程封装、重试逻辑
- [x] Task 4: 重构 ITextSelectionService — 协调 UIA 和 ClipboardService
  - [x] UIA 优先，ClipboardService 兜底
  - [x] 移除对 Win32 API 的直接依赖（通过 ClipboardService）
- [x] Task 5: 清理 App.xaml.cs 和 MainWindow
  - [x] 构造函数注入替代 SetServices()
  - [x] HandleTranslateAsync 中的错误处理统一化
  - [x] 翻译 prompt 提取为常量或配置项
- [x] Task 6: 提取常量和配置
  - [x] 弹窗偏移量、冷却时间等提取到 AppConstants
  - [x] 魔法数字替换为命名常量
- [x] Task 7: 编译测试验证
  - [x] 编译 0 错误 0 警告

---

# Task Dependencies
- [Task 2, 3] depends on [Task 1]
- [Task 4] depends on [Task 2, 3]
- [Task 5] depends on [Task 4]
- [Task 6] 可与 [Task 5] 并行
- [Task 7] depends on [Task 4, 5, 6]
