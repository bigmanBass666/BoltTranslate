# 配置文件格式化 Spec

## Why
用户（纯小白编程新手）打开 config.json 发现 JSON 被压缩成一行，可读性差。需要确保 config.json 始终以缩进格式保存。

## What Changes
- 将 publish/config.json 重新写入为带缩进的格式
- 确认 ConfigManager.Save() 的 WriteIndented = true 已生效

## Impact
- Affected files: publish/config.json

## Requirements

### Requirement: 配置文件必须格式化输出
- **WHEN** 用户打开 config.json
- **THEN** 看到 JSON 带有 proper 缩进和换行，每个字段一行，易于阅读编辑
