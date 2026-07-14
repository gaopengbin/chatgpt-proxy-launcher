# ChatGPT Proxy Launcher

<p align="center">
  <img src="assets/app-icon.png" width="128" height="128" alt="ChatGPT Proxy Launcher icon">
</p>

一个轻量的 Windows 小工具：检查本地 HTTP 代理，并在仅影响当前应用的环境变量中启动 ChatGPT/Codex Desktop。单文件不到 100 KB，无需管理员权限。

## 为什么需要它

ChatGPT/Codex Desktop 在 Windows 上不一定会自动使用代理软件提供的本地 HTTP 代理。本工具通过代理环境启动桌面客户端，因此：

- **无需开启全局代理**：只让本次启动的 ChatGPT/Codex 及其子进程走代理，不影响浏览器和其他程序。
- **减少 Codex 反复重连**：当重连由桌面客户端无法稳定访问服务引起时，为它提供明确的代理出口可以改善连接稳定性。
- **改善手机连接 Codex 失败**：当手机无法连接 Windows 上的 Codex 会话是由 Windows 桌面端网络不通导致时，通过代理启动桌面端可以解决或缓解问题。

> 本工具解决的是代理路由问题。如果故障来自账号、局域网、防火墙、TLS 检查或服务端状态，则仍需单独排查。

## 特性

- 自动发现 Microsoft Store/MSIX 安装的 `OpenAI.Codex` 或 `OpenAI.ChatGPT-Desktop` 包
- 优先启动当前的 `ChatGPT.exe`，兼容旧版 `Codex.exe`
- 启动前检查代理端口以及已经运行的 ChatGPT 实例
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

如需桌面快捷方式，请在解压目录中右键 `Create-Desktop-Shortcut.ps1`，选择“使用 PowerShell 运行”。快捷方式会自动使用应用图标。

> 如果本地软件提供的是 SOCKS 端口，请改用其 HTTP/mixed 端口；本工具当前传递的是 HTTP 代理 URL。

## 隐私与安全

本工具不转发或读取网络流量。它只检查指定端口是否可连接，并把代理环境变量传给新启动的 ChatGPT 进程。发布给其他人前建议对二进制进行代码签名，以减少 Windows SmartScreen 警告。

## 许可证

MIT
