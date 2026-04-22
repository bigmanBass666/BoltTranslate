# BoltTranslate

基于大模型 API 的划词翻译工具 — 极简、极速、高性能。

## 特性

- **划词翻译**：选中文字后按下快捷键，即可获得翻译结果
- **智能双向翻译**：自动识别中英文，英文译中文，中文译英文
- **音标标注**：单词翻译自动显示美式音标
- **托盘运行**：后台运行，不打扰工作
- **快捷键自定义**：支持配置个性化快捷键
- **代理支持**：可配置 HTTP 代理

## 开始使用

### 前置要求

- Windows 10/11
- .NET 8 Runtime（已内置，无需单独安装）

### 配置

1. 从 [Releases](https://github.com/bigmanBass666/BoltTranslate/releases) 下载最新版本
2. 运行 `Bolt.exe`
3. 首次运行会自动打开配置文件，填入你的 API Key：

```json
{
  "ApiUrl": "https://open.bigmodel.cn/api/paas/v4/chat/completions",
  "ApiKey": "你的API密钥",
  "Model": "GLM-4-Flash-250414",
  "ProxyUrl": "",
  "Hotkey": "Ctrl+Shift+T",
  "AutoStart": false
}
```

### 支持的 API

支持所有兼容 OpenAI 格式的 API 接口，例如：

- 智谱 GLM-4（默认）
- OpenAI GPT 系列
- Claude（需自行配置 API 地址）
- 本地模型（Ollama 等）

### 快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+Shift+T` | 翻译选中文字（默认） |
| `ESC` | 关闭悬浮窗 |

## 项目结构

```
BoltTranslate/
├── Config/              # 配置管理
├── Services/            # 核心服务
│   ├── NativeInterop/   # Win32 API 封装
│   ├── TranslationService.cs   # 翻译服务
│   ├── SelectionService.cs     # 选中文本获取
│   ├── HotkeyService.cs        # 全局热键
│   └── ClipboardService.cs     # 剪贴板操作
├── Windows/              # WPF 窗口
│   ├── MainWindow.xaml   # 主窗口
│   ├── SettingsWindow.xaml  # 设置窗口
│   └── TranslationPopup.xaml # 翻译悬浮窗
└── App.xaml.cs          # 应用入口
```

## 技术栈

- **.NET 8** + **WPF** + **Windows Forms**
- **HttpClient** 连接大模型 API
- **Win32 API** 实现全局热键、无窗口剪贴板读取
- **UI Automation** 跨进程获取选中文字

## 构建

```bash
dotnet build
dotnet run
```

发布独立可执行文件：

```bash
dotnet publish -r win-x64 --self-contained true -o publish -p:PublishSingleFile=true
```

## License

MIT
