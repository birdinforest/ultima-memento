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
