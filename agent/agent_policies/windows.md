# Windows-Specific Policies

This document outlines policies and constraints specific to the Windows environment and the `win32` operating system.

## Shell Interaction
- **PowerShell Compatibility**: When using `run_shell_command`, prioritize compatibility with `powershell.exe` (Windows PowerShell 5.1).
- **Avoid Control Operators**: Do NOT use `&&` or `||` in shell commands. Use `;` to separate commands or split them into multiple tool calls.
- **Path Normalization**: While Windows uses backslashes (`\`), Unreal Engine and git tools often expect forward slashes (`/`). 
  - ALWAYS use `PathHelpers.ToUnixPath()` or `PathHelpers.NormalizePath()` before passing paths to build arguments or shell commands.
  - Assert that paths in unit tests handle both separator types or are normalized.
- **Quoting**: Use double quotes (`"`) for paths containing spaces. Avoid single quotes (`'`) as they can behave differently in PowerShell.
- **Numeric Formatting**: Use `multiplier.ToString(CultureInfo.InvariantCulture)` for all numeric arguments passed to shell commands to avoid locale-specific decimal separators (e.g., `,` vs `.`) which can crash Windows batch files.

## Build System & Tooling
- **MSBuild/dotnet**: Use `dotnet` CLI for building the project whenever possible. If `MSBuild.exe` is required, it MUST be located via the `VisualStudioConfigurations` class rather than hardcoded paths.
- **UNC Paths**: Be cautious with UNC paths; some legacy Windows tools may not support them correctly. Map them to a drive letter if necessary.

## OS-Specific APIs
- **Win32 Interop**: Direct Win32 API calls should be wrapped in `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)` checks. Favor `IPlatformService` for cross-platform abstraction of OS-level tasks.
