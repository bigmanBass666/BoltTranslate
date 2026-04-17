# TranslateSharp - 极简划词翻译工具

## 项目概述

Windows 全局快捷键翻译工具，基于大模型 API（支持智谱 GLM/OpenAI 等），悬浮窗展示结果，极轻量、纯净、无广告。

## 技术栈

- **语言/框架**：C# + .NET 8 + WPF
- **项目类型**：WinForms + WPF 混合（WinForms 仅用于 NotifyIcon 系统托盘）
- **发布模式**：自包含单文件（可打包成单个 exe）

## 项目结构

```
TranslateSharp/
├── Config/
│   └── AppConfig.cs          # 配置管理（JSON文件读写）
├── Services/
│   ├── TranslationService.cs  # LLM API 调用（HttpClient）
│   ├── SelectionService.cs    # 全局热键 + 模拟Ctrl+C获取选中文字
│   └── WindowManager.cs       # 悬浮窗显示/隐藏控制
├── Windows/
│   ├── TranslationPopup.xaml      # 悬浮窗UI（透明、圆角、只读TextBox）
│   └── TranslationPopup.xaml.cs
├── App.xaml / App.xaml.cs     # 启动入口，组装所有模块
├── MainWindow.xaml/.cs        # 系统托盘图标 + 右键菜单
└── config.json                # 配置文件（程序目录）
```

## 核心模块说明

### 1. SelectionService（关键机制）
- 注册全局热键 `Ctrl+Shift+T`
- 热键触发时，自动：保存剪贴板 → 模拟Ctrl+C → 读取选中文字 → 恢复剪贴板
- 使用 Win32 API：`RegisterHotKey`、`keybd_event`、`GetForegroundWindow`

### 2. TranslationService
- 直接 HttpClient 调用，不依赖 SDK
- 智能 URL 拼接（避免 `/chat/completions` 重复拼接）
- 支持代理（ProxyUrl）
- 30秒超时，详细的错误信息

### 3. WindowManager + TranslationPopup
- WPF 分层窗口（透明背景 + 圆角）
- 只显示译文，不显示原文
- 只读 TextBox，支持文字选择和复制
- 失焦自动关闭、ESC关闭
- 智能定位（屏幕边缘检测）

## 配置说明（config.json）

```json
{
  "ApiUrl": "https://open.bigmodel.cn/api/paas/v4/chat/completions",
  "ApiKey": "你的API密钥",
  "Model": "glm-4-flash",
  "ProxyUrl": "http://127.0.0.1:7890",
  "HotkeyModifiers": "Ctrl+Shift",
  "HotkeyKey": "T"
}
```

| 字段 | 说明 | 默认值 |
|------|------|--------|
| ApiUrl | API端点URL | https://api.openai.com/v1/chat/completions |
| ApiKey | API密钥 | （空）|
| Model | 模型名称 | gpt-4o-mini |
| ProxyUrl | HTTP代理地址 | （空，不使用代理）|
| HotkeyModifiers | 修饰键 | Ctrl+Shift |
| HotkeyKey | 触发键 | T |

## 使用方法

### 开发运行
```bash
cd d:\Working\programming_projects\TranslateSharp
dotnet run
```

### 发布打包
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 已完成功能

- ✅ 全局热键 Ctrl+Shift+T 触发翻译
- ✅ 自动获取当前选中的文字（模拟Ctrl+C）
- ✅ 调用大模型API翻译
- ✅ 悬浮窗只显示译文，支持文字选择
- ✅ 系统托盘常驻，右键菜单
- ✅ 配置文件热重载（重启生效）
- ✅ 智谱GLM API 支持
- ✅ HTTP代理支持

## 待完成/改进项

- [ ] 开机自启动
- [ ] 翻译历史记录
- [ ] 多语言互译（目前固定英译中）
- [ ] 自定义快捷键（目前写死 Ctrl+Shift+T）
- [ ] 打包成单文件 exe 安装包
- [ ] 深色模式支持
- [ ] 划词图标触发（暂不支持，方案过于复杂）

## 已知限制

- 快捷键方案依赖模拟Ctrl+C，少数应用（如某些游戏、安全软件）可能不兼容
- 悬浮窗使用 WPF，在极少数不支持 WPF 的环境下可能异常

## Git 操作

```bash
git init
git add -A
git commit -m "init: project scaffold"
```
