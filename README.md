# ChatGPT Proxy Launcher

<p align="center"><strong>English</strong> | <a href="README.zh-CN.md">简体中文</a></p>

## Are you experiencing any of these problems?

- **Codex repeatedly shows `Reconnecting... 1/5` through `5/5`**, making every response slow to start.
- **ChatGPT/Codex Desktop is unstable unless global proxy mode is enabled**, but you do not want every Windows application routed through the proxy.
- **Your phone cannot connect to a Codex session running on Windows**, remaining stuck while looking or waiting for the desktop.

**ChatGPT Proxy Launcher was built for these three problems.** It applies a local HTTP proxy only to the ChatGPT/Codex process and its children, without changing the Windows global proxy. This launch method has been tested on Windows and allowed a phone to connect to a desktop Codex session that was previously unreachable.

<p align="center">
  <img src="assets/app-icon.png" width="128" height="128" alt="ChatGPT Proxy Launcher icon">
</p>

A tiny Windows utility that checks a local HTTP proxy and launches ChatGPT/Codex Desktop with process-scoped proxy environment variables. The executable is under 100 KB and does not require administrator privileges.

> The verified mobile connection result applies when the desktop remote-control process lacks a usable proxy route. Account mismatch, LAN restrictions, firewalls, TLS inspection, and service-side incidents require separate diagnosis.

## Features

- Automatically discovers the `OpenAI.Codex` or `OpenAI.ChatGPT-Desktop` Microsoft Store/MSIX package
- Starts the current `ChatGPT.exe` entry point and remains compatible with the older `Codex.exe`
- Checks the proxy port and detects an already-running ChatGPT instance before launch
- Sets uppercase and lowercase forms of `HTTP_PROXY`, `HTTPS_PROXY`, and `NO_PROXY`
- Does not modify the Windows system proxy and does not require administrator privileges

## Download

Download `ChatGPTProxyLauncher.exe` directly from the latest [GitHub Release](https://github.com/gaopengbin/chatgpt-proxy-launcher/releases/latest). The ZIP asset additionally includes this README, the license, and a desktop-shortcut helper.

## Usage

1. Fully quit any running ChatGPT instance.
2. Start your local HTTP proxy.
3. Enter its host and HTTP or mixed port, then select **Launch ChatGPT through proxy**.

To create a desktop shortcut, right-click `Create-Desktop-Shortcut.ps1` from the extracted ZIP and select **Run with PowerShell**. The shortcut automatically uses the application icon.

> If your proxy software exposes a SOCKS port, use its HTTP or mixed port instead. The launcher currently supplies an HTTP proxy URL.

## Build

The lightweight build uses the .NET Framework C# compiler included with Windows:

```powershell
powershell -ExecutionPolicy Bypass -File .\build.ps1
```

The result is written to `output/ChatGPTProxyLauncher.exe` and is approximately 90 KB including the multi-size application icon.

## Privacy and security

The launcher does not forward or inspect network traffic. It checks whether the specified port accepts a connection and passes proxy environment variables to the newly started ChatGPT process. Public binaries should ideally be code-signed to reduce Windows SmartScreen warnings.

## License

MIT
