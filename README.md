# BoltTranslate

基于大模型的划词翻译工具。

## 功能特性

- **划词翻译**：选中文字后按快捷键即可翻译
- **全局快捷键**：默认 `Ctrl+Shift+T`，支持自定义
- **智能翻译**：自动识别中英文互译，英文单词显示美式音标
- **代理支持**：可配置 HTTP 代理
- **自动启动**：支持开机自启
- **静默运行**：后台运行，不占任务栏

## 快速开始

### 1. 配置

复制配置文件：

```
copy Bolt.json.example Bolt.json
```

编辑 `Bolt.json`：

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

### 2. 运行

```bash
dotnet run
```

### 3. 发布

功能验证完成后，可发布独立 exe：

```bash
dotnet publish -r win-x64 --self-contained true -o publish -p:PublishSingleFile=true
```

## 项目结构

```
BoltTranslate/
├── Services/              # 核心服务
│   ├── TranslationService.cs   # 翻译服务
│   ├── HotkeyService.cs        # 快捷键服务
│   ├── TextSelectionService.cs # 文本选择服务
│   └── ClipboardService.cs     # 剪贴板服务
├── Windows/                # 窗口
│   ├── SettingsWindow.xaml     # 设置窗口
│   └── TranslationPopup.xaml   # 翻译弹窗
├── Config/
│   └── AppConfig.cs       # 配置管理
└── MainWindow.xaml        # 主窗口
```

## 技术栈

- .NET 8.0 + WPF
- HTTP API 调用
- Win32 API (全局快捷键、窗口管理)
