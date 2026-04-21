# TranslateSharp 开发规范

## 一、代码管理

### 1.1 提交规范
- **每次修改后**都要执行 `git add` + `git commit`
- 提交信息使用中文，简短描述本次改动
- 同时更新 `progress.txt` 简要记录进度

### 1.2 进度记录
- 每次提交后更新 `progress.txt`
- 记录格式：`### 日期` + 分项列举已完成和待办内容

### 1.3 开发测试流程
**日常开发测试**：
1. `dotnet build` — 验证编译
2. `dotnet run` — 直接运行最新代码（bin/Debug），无需 publish

⚠️ **每次 build 后必须启动应用验证！** 这是最重要的开发习惯——不启动就不知道改动有没有问题。

**正式发布**（仅在功能验证 OK 后做一次）：
1. `dotnet publish -r win-x64 --self-contained true -o publish -p:PublishSingleFile=true`

⚠️ **不要每次 build 都 publish！** `dotnet run` 比 publish 快得多，且代码改动立即生效。只有当 `dotnet run` 验证 OK 后，才 publish 一次生成独立 exe。

---

## 二，开发流程

### 2.1 新功能 / 复杂问题
开发新功能或修复复杂问题前，**先去上网深度全方位搜索调研现有方案**，站在巨人的肩膀上。

**搜索要点**：
- 目标技术的官方文档和 GitHub issues
- 成熟开源项目（Pot Desktop、selection-hook 等）的实现方式
- Stack Overflow / CSDN / 博客园等社区的解决方案
- 关键技术点：`site:github.com`、`site:stackoverflow.com` 等限定搜索

### 2.2 疑难杂症
遇到顽固 bug、崩溃、兼容性问题等，**第一时间搜索调研**，了解其他人是如何解决的，不要闭门造车。

**调研维度**：
- 错误信息的准确含义
- 问题是否是已知问题（GitHub issues / Stack Overflow）
- 主流工具/框架如何解决同类问题
- 不同方案的优缺点对比

---

## 三、代码风格

- 不添加注释（除非业务逻辑复杂需要说明）
- 方法/类名使用有意义的英文命名
- 保持代码简洁，避免过度封装
