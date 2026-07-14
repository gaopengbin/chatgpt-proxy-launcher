# ChatGPT Proxy Launcher

<p align="center"><a href="README.md">English</a> | <strong>简体中文</strong></p>

## 你是不是遇到了这些问题？

- **Codex 一直 `Reconnecting... 1/5` 到 `5/5`**，要等很久才开始回复。
- **不开全局代理，ChatGPT/Codex Desktop 就无法稳定连接**；但又不想让所有 Windows 程序都走代理。
- **手机无法连接 Windows 上的桌面 Codex 会话**，一直找不到桌面端或停在等待连接。

**ChatGPT Proxy Launcher 就是为这三个问题做的。** 它只给本次启动的 ChatGPT/Codex 及其子进程设置代理，不修改 Windows 全局代理。已在 Windows 上实测，通过这种方式启动后，手机可以连接此前无法连接的桌面 Codex 会话。

<p align="center">
  <img src="assets/app-icon.png" width="128" height="128" alt="ChatGPT Proxy Launcher icon">
</p>

一个轻量的 Windows 小工具：检查本地 HTTP 代理，并在仅影响当前应用的环境变量中启动 ChatGPT/Codex Desktop。单文件不到 100 KB，无需管理员权限。

> 上述手机连接结论针对“桌面端缺少可用代理出口”这一故障场景。如果故障来自账号不一致、局域网、防火墙、TLS 检查或服务端状态，则仍需单独排查。

## 特性

- 自动发现 Microsoft Store/MSIX 安装的 `OpenAI.Codex` 或 `OpenAI.ChatGPT-Desktop` 包
- 优先启动当前的 `ChatGPT.exe`，兼容旧版 `Codex.exe`
- 启动前检查代理端口以及已经运行的 ChatGPT 实例
- 应用界面支持 English / 简体中文完整切换
- 同时设置大小写形式的 `HTTP_PROXY`、`HTTPS_PROXY` 和 `NO_PROXY`
- 不修改 Windows 系统代理，不需要管理员权限

## 构建

推荐构建轻量版（依赖 Windows 自带的 .NET Framework 4.x）：

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

输出为 `output/ChatGPTProxyLauncher.exe`，当前约 70 KB（包含多尺寸应用图标）。构建使用 Windows 自带的 .NET Framework C# 编译器。

## 使用

1. 先完全退出正在运行的 ChatGPT。
2. 启动本地 HTTP 代理。
3. 填写代理主机和端口，点击“启动 ChatGPT”。

在 GitHub Release 中可以直接下载 `ChatGPTProxyLauncher.exe`；ZIP 版本额外包含 README、许可证和桌面快捷方式脚本。

如需桌面快捷方式，请在解压目录中右键 `Create-Desktop-Shortcut.ps1`，选择“使用 PowerShell 运行”。快捷方式会自动使用应用图标。

> 如果本地软件提供的是 SOCKS 端口，请改用其 HTTP/mixed 端口；本工具当前传递的是 HTTP 代理 URL。

## 隐私与安全

本工具不转发或读取网络流量。它只检查指定端口是否可连接，并把代理环境变量传给新启动的 ChatGPT 进程。发布给其他人前建议对二进制进行代码签名，以减少 Windows SmartScreen 警告。

## 许可证

MIT
