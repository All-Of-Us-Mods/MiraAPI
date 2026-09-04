[![](https://dcbadge.limes.pink/api/server/AEfHJGwggC)](https://discord.gg/AEfHJGwggC)

[English](../README.md) | [简体中文]

> 此模组与 Among Us 或 Innersloth LLC 无关，其中包含的内容未得到 Innersloth LLC 的认可或赞助。本文所含部分材料为 Innersloth LLC 的财产。© Innersloth LLC。

> 由[太空果](https://github.com/TAIKgroup)开启了Pr并提交了初版翻译，由[林梅](https://github.com/HayashiUme)维护并提供wiki翻译。

# Mira API

一个全面、易上手的 Among Us 模组 API 及实用工具库，内含:

- 角色
- 选项
- 附加职业
- 按钮
- 自定义颜色
- 事件
- 投票
- 资源
- 快捷键
- 本地设置
- 自定义游戏结束条件
- 强兼容性
- ~~游戏模式~~（敬请期待）

Mira API 力求全面而易于上手，同时尽可能复用原版游戏元素。
它是一个更简洁、覆盖常见用例的模组 API，减少了对游戏源码的修改。

**加入 [Discord](https://discord.gg/FYYqJU2bvp) 获取支持并了解最新版本**

# 使用方法

要开始使用 Mira API，你需要：

1. 通过 [DLL](https://github.com/All-Of-Us-Mods/MiraAPI/releases)、项目引用或 [NuGet 包](https://www.nuget.org/packages/AllOfUs.MiraAPI) 添加对 Mira API 的引用。
2. 在你的插件类上添加 BepInDependency，如下所示：`[BepInDependency(MiraApiPlugin.Id)]`
3. 在你的插件类上实现 `IMiraPlugin` 接口。

Mira API 还依赖 [Reactor](https://github.com/NuclearPowered/Reactor) 才能正常运行！
记得将其作为引用和 `BepInDependency` 包含进来！

完整示例请参见 [此文件](https://github.com/All-Of-Us-Mods/MiraAPI/blob/master/MiraAPI.Example/ExamplePlugin.cs)。

## 推荐的项目结构

使用 Mira API 时，强烈建议遵循以下项目结构，以保持代码整洁有序。
你也可以查看本仓库中的示例模组以获得一些指导。

```
MyMiraMod/
├── Buttons/
│   └── MyCoolButton.cs
├── Options/
│   ├── Roles/
│   │   └── CoolCustomRoleOptions.cs
│   └── MainOptionGroup.cs
├── Patches/
│   ├── Roles/
│   │   └── CoolCustomRole/
│   │       ├── PlayerControlPatches.cs
│   │       └── ExileControllerPatches.cs
│   └── General/
│       └── HudManagerPatches.cs
├── Resources/
│   ├── CoolButton.png
│   └── myAssets-win-x86.bundle
├── Roles/
│   └── CoolCustomRole.cs
├── MyMiraModPlugin.cs
└── MyModAssets.cs
```

# 文档

Mira API 每个功能的完整文档可在 **[wiki](https://github.com/All-Of-Us-Mods/MiraAPI/wiki)** 上找到：

- [入门 / IMiraPlugin](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Home)
- [自定义角色](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Custom-Roles)
- [选项](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Options)
- [附加职业](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Modifiers)
- [自定义按钮](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Custom-Buttons)
- [自定义颜色](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Colors)
- [事件](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Events)
- [会议与投票](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Meetings-and-Voting)
- [资源](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Assets)
- [快捷键](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Keybinds)
- [本地设置](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Local-Settings)
- [游戏模式](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Game-Modes)
- [自定义游戏结束](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Game-Over)
- [实用工具](https://github.com/All-Of-Us-Mods/MiraAPI/wiki/Utilities)