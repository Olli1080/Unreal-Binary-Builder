# Blueprint: Dependency Injection & Platform Agnosticism

## Objective
Adopt a standard DI container to decouple components and support platform-specific implementations.

## Current Problems
- Manual constructor chaining (`new ProcessExecutor()`, `new UBBUpdater()`).
- Hard to substitute mock services in unit tests.
- Platform-specific code (`OperatingSystem.IsWindows()`) is scattered in ViewModels.

## Target Architecture
1. **Microsoft.Extensions.DependencyInjection**: Standard DI container.
2. **IServiceProvider**: The main container for resolving dependencies.
3. **Platform abstractions**: Interfaces like `IPlatformService` (to handle `Process.Start` differences).

## Execution Steps

### Step 1: Library Integration
- Add the `Microsoft.Extensions.DependencyInjection` NuGet package to `UnrealBinaryBuilder.Avalonia.csproj`.

### Step 2: Service Registration
- Implement a `ServiceCollection` in `App.axaml.cs`.
- Register all services (`IProcessExecutor`, `IUnrealEngineProvider`, `IUBBLogger`, etc.).
- Register ViewModels as transient or singleton.

### Step 3: Platform Service
- Define `IPlatformService` in `UnrealBinaryBuilder.Avalonia.Classes.Interfaces`:
  ```csharp
  public interface IPlatformService {
      void OpenFolder(string path);
      void OpenUrl(string url);
      void ShutdownPC(int seconds);
  }
  ```
- Implement `WindowsPlatformService`, `LinuxPlatformService`, etc.

### Step 4: Component Integration
- Inject dependencies into `MainWindowViewModel` via constructor.
- Replace `OperatingSystem.IsWindows()` checks with `IPlatformService` calls.

## Validation
- Ensure unit tests for `MainWindowViewModel` can use a mock service collection.
- Verify platform-specific actions (like opening folders) still work on Windows.
