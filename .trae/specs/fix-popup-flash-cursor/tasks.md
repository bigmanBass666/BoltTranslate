# Tasks

- [x] Task 1: 修复弹窗闪烁 — 消除左上角一闪
  - [x] Show() 前设置 Left/Top 为屏幕外坐标（-10000, -10000）
  - [x] Show() 后 SetWindowPos 移到目标位置
- [x] Task 2: 弹窗光标样式分区
  - [x] TranslationPopup.xaml: Border/标题区域 Cursor="SizeAll"
  - [x] TranslationPopup.xaml: TextBox 区域 Cursor="Arrow"（已有）
  - [x] TranslationPopup.xaml: 关闭按钮 Cursor="Hand"（已有）
- [x] Task 3: 编译测试验证
  - [x] 编译通过
  - [ ] 弹窗不再闪烁，直接出现在目标位置
  - [ ] 光标样式正确区分各区域

---

# Task Dependencies
无
