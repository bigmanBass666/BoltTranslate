# BoltTranslate

一款简洁高效的 Windows 翻译工具，选中文本后按下快捷键即可获得翻译结果。

## 功能特性

- **全局快捷键翻译**：默认 `Ctrl+Shift+T`，支持自定义配置
- **智能选文捕获**：优先使用 UI Automation 获取选中文字，不污染剪贴板
- **悬浮翻译结果**：简洁的气泡弹窗显示翻译内容，失焦自动隐藏
- **多 API 支持**：兼容所有 OpenAI 格式的翻译 API（Azure、SiliconFlow 等）
- **配置管理**：图形化设置窗口，支持快捷键捕获、API Key、模型名称、代理设置
- **开机自启**：可配置开机自动启动
- **翻译日志**：自动记录翻译历史，方便回溯

## 快速开始

### 配置

启动应用后，托盘图标右键 → 设置，填写以下信息：

| 配置项 | 说明 |
|--------|------|
| API Key | 翻译 API 的密钥 |
| 模型 | API 模型名称（如 `gpt-4o-mini`） |
| 代理地址 | 可选，如 `http://127.0.0.1:7890` |

### 使用

1. 选中文本
2. 按下快捷键 `Ctrl+Shift+T`
3. 翻译结果以悬浮窗显示

## 快捷操作

| 操作 | 说明 |
|------|------|
| `Ctrl+Shift+T` | 翻译当前选中文本 |
| ESC | 关闭悬浮翻译窗 |
| 双击托盘图标 | 显示运行状态 |

## 项目结构

```
BoltTranslate/
├── Config/                 # 配置管理
│   └── AppConfig.cs
├── Services/               # 核心服务
│   ├── NativeInterop/      # Win32 API 封装
│   ├── SelectionService.cs # 选中文本服务
│   ├── TranslationService.cs # 翻译 API 调用
│   ├── HotkeyService.cs    # 全局热键管理
│   └── ...
├── Windows/                # 窗口界面
│   ├── TranslationPopup.xaml    # 翻译悬浮窗
│   ├── SettingsWindow.xaml      # 设置窗口
│   └── StartupTipWindow.xaml   # 启动提示
├── MainWindow.xaml         # 主窗口（托盘管理）
└── App.xaml                # 应用入口
```

## 技术栈

- .NET 8 + WPF
- Win32 API（全局热键、窗口定位）
- UI Automation（选中文本获取）
- OpenAI 格式 API 兼容

## 构建

```bash
# 开发调试
dotnet build
dotnet run

# 发布独立exe
dotnet publish -r win-x64 --self-contained true -o publish -p:PublishSingleFile=true
```

## 配置说明

配置文件 `Bolt.json` 位于应用目录：

```json
{
  "ApiKey": "your-api-key",
  "Model": "gpt-4o-mini",
  "Proxy": "",
  "Hotkey": "Ctrl+Shift+T",
  "AutoStart": false
}
```
