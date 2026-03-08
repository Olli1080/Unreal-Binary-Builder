# Windows-Specific Policies

This document outlines policies and constraints specific to the Windows environment and the `win32` operating system.

## Shell Interaction
- **PowerShell Compatibility**: When using `run_shell_command`, prioritize compatibility with `powershell.exe` (Windows PowerShell 5.1).
- **Avoid Control Operators**: Do NOT use `&&` or `||` in shell commands. These operators are not supported in PowerShell 5.1. Use `;` to separate commands or split them into multiple tool calls.
- **Path Separators**: Always use backslashes (`\`) for file paths in shell commands and when interacting with Windows-native tools (e.g., MSBuild, AutomationTool).
- **Quoting**: Use double quotes (`"`) for paths containing spaces. Avoid single quotes (`'`) as they can behave differently in PowerShell depending on the context.

## Build System & Tooling
- **MSBuild/dotnet**: Use `dotnet` CLI for building the project whenever possible. If `MSBuild.exe` is required, ensure it is located via standard environment variables or common Visual Studio installation paths.
- **UNC Paths**: Be cautious with UNC paths; some legacy Windows tools may not support them correctly. Map them to a drive letter if necessary.

## OS-Specific APIs
- **Win32 Interop**: Direct Win32 API calls should be wrapped in `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)` checks to maintain the cross-platform integrity of the Avalonia application.
