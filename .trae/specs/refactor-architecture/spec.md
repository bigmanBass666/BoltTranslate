# 项目架构质量提升 Spec

## Why
经过 7 次悬浮窗定位迭代和多次功能修改，项目代码虽然能正常运行，但存在以下可维护性问题需要清理：
- SelectionService 承担过多职责（热键管理 + 光标捕获 + 文本提取(UIA+剪贴板) + 剪贴板恢复），是典型的 God Class
- Win32 P/Invoke 声明散落在 4 个文件中，结构体重复定义（POINT/tagPOINT）
- 魔法数字散布各处（OffsetX=20、Cooldown=500ms、Thread.Sleep(50) 等）
- 翻译 prompt 硬编码在方法体内
- 错误处理策略不统一（有的静默吞异常，有的弹 MessageBox）
- MainWindow 使用 SetServices() setter 注入而非构造函数注入

## What Changes
- 拆分 SelectionService 为职责单一的多个服务
- 统一 Win32 P/Invoke 到 NativeInterop 层
- 提取常量和配置项
- 统一错误处理策略
- 清理依赖注入方式

## Impact
- Affected code: `Services/SelectionService.cs`, `Services/WindowManager.cs`, `Windows/TranslationPopup.xaml.cs`, `App.xaml.cs`, `Services/NativeInterop/`
- 新增文件: `Services/HotkeyService.cs`, `Services/ClipboardService.cs`, `NativeInterop/Win32Api.cs`

---

## ADDED Requirements

### Requirement 1: 职责分离 — 拆分 SelectionService
系统 SHALL 将 SelectionService 拆分为以下独立服务：

| 服务 | 职责 |
|------|------|
| **IHotkeyService** | 热键注册/注销、消息循环、冷却时间管理 |
| **IClipboardService** | 剪贴板保存/恢复/Ctrl+C模拟/序列号检测 |
| **ITextSelectionService** | 文本获取（UIA优先 → 剪贴板兜底） |

### Requirement 2: 统一 Win32 P/Invoke 层
系统 SHALL 将所有 Win32 API 声明集中到 `NativeInterop/Win32Api.cs`：
- 删除 SelectionService、WindowManager、TranslationPopup 中的 P/Invoke 声明
- 统一使用一个 POINT 结构体
- 所有服务通过 Win32Api 类调用 Win32 函数

### Requirement 3: 常量与配置提取
系统 SHALL 将魔法数字提取为有意义的命名常量：
- 弹窗偏移量 → 配置项或常量类
- 冷却时间 → 可配置
- 线程睡眠时间 → 有意义的常量
- 翻译 prompt → 可配置的模板

### Requirement 4: 统一错误处理
系统 SHALL 采用统一的错误处理策略：
- 操作层错误：通过回调/事件通知上层，不弹 MessageBox
- 致命错误：抛出异常，由 App.OnStartup 统一处理
- 剪贴板操作失败：静默重试，记录日志（而非吞掉）

### Requirement 5: 构造函数注入
MainWindow 和各服务 SHALL 通过构造函数接收依赖，移除 SetServices() 方法
