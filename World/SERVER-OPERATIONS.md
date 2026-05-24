# Server operations（WorldLinux.exe）

本文件与 `WorldLinux.exe` 同目录，说明在本机启动、停止与重启 Ultima Memento 游戏服时常用的 shell 命令。

## 运行前说明

- **工作目录：** 请在 **`World/`** 下执行 `mono WorldLinux.exe`，使 `Core.BaseDirectory` 与进程当前目录一致（`.env`、存档路径等按设计相对于本目录解析）。不要在未确认 BaseDirectory 行为的情况下从仓库根目录随意启动。
- **运行时：** Linux / macOS 使用 **Mono** 运行本程序；Windows 使用项目内的 Windows 编译产物（此处不展开）。
- **二进制位置：** `World/WorldLinux.exe`（由 `World/Source` 下对应编译脚本生成）。

## 启动服务器

前台运行（输出在终端）：

```bash
cd /path/to/ultima-memento/World
mono WorldLinux.exe
```

将标准输出与错误写入日志文件（与常见 `server-start.log` 用法一致）：

```bash
cd /path/to/ultima-memento/World
mono WorldLinux.exe > server-start.log 2>&1
```

后台运行（关闭终端后仍保留进程，示例使用 `nohup`）：

```bash
cd /path/to/ultima-memento/World
nohup mono WorldLinux.exe > server-start.log 2>&1 &
```

可选环境变量与密钥：参见同目录下的 `.env.example`；不要将含密钥的 `.env` 提交到版本库。

## 停止服务器（含崩溃后清理）

先查看是否仍有 `WorldLinux` 相关进程：

```bash
pgrep -fl WorldLinux
```

或使用：

```bash
ps aux | grep -i WorldLinux | grep -v grep
```

**正常结束（建议先试）：** 向进程发送 `TERM`，便于托管代码做收尾：

```bash
pkill -TERM -f WorldLinux.exe
```

等待数秒；若进程仍在，**强制结束：**

```bash
pkill -KILL -f WorldLinux.exe
```

若系统上进程名不同，可用上面 `pgrep` / `ps` 得到的 **PID** 执行：

```bash
kill -TERM <PID>
# 仍不退出时：
kill -9 <PID>
```

## 崩溃后重启（典型流程）

1. 确认旧进程已结束（`pgrep -fl WorldLinux` 无输出或只剩 grep 自身）。
2. 在 `World/` 下重新启动（见上文「启动服务器」）。

示例（前台 + 日志）：

```bash
cd /path/to/ultima-memento/World
pkill -KILL -f WorldLinux.exe 2>/dev/null
mono WorldLinux.exe > server-start.log 2>&1
```

## 服务器命令
### Reload 中文化
[locreload          # 重读全部 locale JSON + quest-composite 表
[locreload opl      # 同上，并刷新双语物品 tooltip 缓存
[lr                 # 别名

### 查看角色装备
[viewequip

### 查看&修改角色属性增强

常用语修复角色身上挂着孤儿装备，导致虽然没有穿装备，但是保留属性和技能增强的bug。
可尝试使用下面两个直接重置数值：
[FixStatMods
[FixSkillMods
TODO: 确认重启服务器是否可以修复这个问题。因为在启动服务器后，角色登录时，所有装备增益都是会重新计算的。

DumpStatMods [DumpStatMods → 点目标
列出所有 StatMod（name / type / offset），并显示 Raw 与 Effective 属性

DumpSkillMods
[DumpSkillMods → 点目标
列出所有 SkillMod（类型、技能、数值、relative）

AddStatMod / SetStatMod
[AddStatMod Dex -12 0x40001234Dex 或 [AddStatMod Dex 5
Str|Dex|Int、offset、可选 modName、可选 minutes（0=永久）

RemoveStatMod
[RemoveStatMod 0x40001234Dex → 点目标
按精确名称删除

ClearStatMods
[ClearStatMods / [ClearStatMods Dex / [ClearStatMods all
清除全部或指定属性类型的 mod

FixStatMods
[FixStatMods → 点目标
清除全部 StatMod，再按当前身上装备重新加（修 orphan mod）

AddSkillMod / SetSkillMod
[AddSkillMod Magery 10 或 [AddSkillMod Magery 5 false
添加 DefaultSkillMod；第三参数 relative，默认 true

RemoveSkillMod
[RemoveSkillMod Magery → 点目标
移除该技能的所有 SkillMod

ClearSkillMods
[ClearSkillMods / [ClearSkillMods Magery
清除全部或指定技能

FixSkillMods
[FixSkillMods → 点目标
清除全部 SkillMod，再从装备重新应用 AOS 技能加成

## 其它常用 CLI 标志（简述）

本地化回归自检会在加载流程中执行并退出，不进入长时间游戏循环。仓库内权威说明见：

`World/Documentation/localization-regression-testing.md`

示例：

```bash
cd /path/to/ultima-memento/World
mono WorldLinux.exe -localization-regression
# 等价别名：
mono WorldLinux.exe -locreg
```

也可用仓库根目录脚本（以你环境中的路径为准）：

```bash
bash World/Source/Tools/run_localization_regression.sh
```

## 文档与工程约定

更完整的构建、本地化与 agent 边界说明见仓库根目录 **`AGENTS.md`**。
