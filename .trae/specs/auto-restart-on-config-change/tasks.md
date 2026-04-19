# Tasks

- [x] Task 1: MainWindow.OpenConfigFile() 改为异步等待 + 变化检测
  - [x] 打开前读取当前 config.json 内容（全文比较）
  - [x] WaitForExit 等待 notepad 关闭
  - [x] 关闭后重新读取 config.json 内容并与之前比较
  - [x] 内容不同时调用 RestartApplication()
- [x] Task 2: 编译测试验证
  - [x] 编译通过
  - [ ] 托盘→打开配置→修改→保存→关闭 → 自动重启
  - [ ] 托盘→打开配置→不修改→关闭 → 不重启

---

# Task Dependencies
无
