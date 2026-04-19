# TranslateSharp

Windows 全局快捷键翻译工具 — 选中文字，一键翻译。

## 功能特性

- **全局快捷键翻译**：选中任意文字，按 `Ctrl+Shift+T` 即刻翻译
- **悬浮窗显示**：翻译结果以圆角悬浮窗展示在光标附近
- **智能文本获取**：优先使用 UI Automation（不污染剪贴板），自动降级到剪贴板方案
- **托盘常驻**：最小化到系统托盘，不占任务栏
- **配置热更新**：修改配置文件后自动重启生效
- **支持代理**：可配置 HTTP 代理访问 API

## 环境要求

- Windows 10 / 11
- .NET 8 Runtime（或安装 [.NET 8 运行时](https://dotnet.microsoft.com/download/dotnet/8.0)）

## 安装方式

### 从源码编译

```bash
git clone <repo-url>
cd TranslateSharp
dotnet publish -r win-x64 --self-contained true -o publish -p:PublishSingleFile=true
```

运行 `publish\TranslateSharp.exe`。

### 直接运行（需已安装 .NET 8）

```bash
dotnet run
```

## 配置说明

首次启动时会自动生成 `config.json`（位于 exe 同目录），格式如下：

```json
{
  "ApiUrl": "https://open.bigmodel.cn/api/paas/v4/chat/completions",
  "ApiKey": "",
  "Model": "GLM-4-Flash-250414",
  "ProxyUrl": "",
  "Hotkey": "Ctrl+Shift+T"
}
```

| 字段 | 说明 | 默认值 |
|------|------|--------|
| `ApiUrl` | 兼容 OpenAI Chat Completions 格式的 API 地址 | 智谱 GLM-4 |
| `ApiKey` | API 密钥（**必填**） | 空（首次启动会提示配置） |
| `Model` | 模型名称 | GLM-4-Flash |
| `ProxyUrl` | HTTP 代理地址（如 `http://127.0.0.1:7890`） | 空（不使用代理） |
| `Hotkey` | 全局快捷键，用 `+` 连接修饰键和按键 | Ctrl+Shift+T |

### 快捷键格式

`Hotkey` 字段支持以下格式：

**基本格式**：`修饰键+按键`，多个修饰键用 `+` 连接

**可用修饰键**：`Ctrl`、`Shift`、`Alt`、`Win`

**可用按键**：
- 单字母/数字：`A`~`Z`、`0`~`9`
- 功能键：`F1`~`F12`
- 特殊键：`Space`（空格）、`Tab`、`Enter`、`Esc`、`Backspace`、`Delete`、`Insert`、`Home`、`End`、`PgUp`、`PgDn`、`Left`/`Up`/`Right`/`Down`（方向键）

**示例**：
```json
"Hotkey": "Ctrl+Shift+T"        // 默认
"Hotkey": "Alt+Q"              // Alt + Q
"Hotkey": "Ctrl+F1"            // Ctrl + F1
"Hotkey": "Win+A"              // Win 键 + A
"Hotkey": "Ctrl+Alt+Shift+F12" // 三键组合 + F12
```

> ⚠️ 修改 Hotkey 后需重启应用生效（或通过托盘菜单打开配置文件修改后自动重启）。

### 支持的 API 服务

任何兼容 OpenAI `/chat/completions` 接口的服务均可使用：
- [智谱 AI (GLM)](https://open.bigmodel.cn/)
- [OpenAI (GPT)](https://platform.openai.com/)
- [DeepSeek](https://platform.deepseek.com/)
- [Ollama](http://localhost:11434/)（本地模型）
- 其他兼容服务

只需将 `ApiUrl` 和 `ApiKey` 改为对应服务的值即可。

## 使用方法

### 翻译文字

1. 用鼠标**选中**要翻译的文字
2. 按 **`Ctrl+Shift+T`**
3. 翻译结果悬浮窗出现在光标右下方
4. 点击 **✕** 或按 **Esc** 关闭弹窗
5. 弹窗可**拖动**移动位置

### 托盘菜单

右键点击系统托盘图标：

| 菜单项 | 说明 |
|--------|------|
| 显示快捷键 | 当前快捷键（只读） |
| 打开配置文件 | 用记事本打开 config.json（修改保存后自动重启） |
| 重启 | 重启应用 |
| 退出 | 完全退出 |

双击托盘图标显示状态提示。

## 常见问题

### Q：提示「快捷键注册失败」怎么办？

A：当前配置的快捷键可能被其他软件占用。可以：
- 关闭占用该快捷键的程序后重启 TranslateSharp
- 或修改 `config.json` 中的 `Hotkey` 为其他组合（如 `"Alt+Q"`、`"Ctrl+F1"`）

### Q：翻译结果不准确？

A：翻译质量取决于所用的 API 和模型。建议：
- 使用更强的模型（如 `gpt-4o`、`GLM-4-Plus`）
- 在 prompt 中指定更明确的翻译要求（需修改代码中的 SystemPrompt）
- 确保 ApiUrl 指向正确的端点

### Q：部分应用中选中文本无法获取？

A：TranslateSharp 使用 UI Automation 获取选中文本（不污染剪贴板），但部分应用（如终端、特殊编辑器）不支持 UIA，此时会自动降级到模拟 Ctrl+C 方式。如果剪贴板内容发生变化，程序会自动恢复原内容。

### Q：如何更换翻译语言对？

A：当前默认英文→中文。如需更改，修改代码中 `TranslationService.cs` 的 system prompt 即可（位于 `AppConstants.TranslationSystemPrompt` 常量）。后续版本将支持配置化。
