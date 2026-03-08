# Blueprint: Strong-Typed Build Configurations

## Objective
Implement a builder pattern for generating build command-line arguments.

## Current Problems
- `PrepareCommandline` in `MainWindowViewModel` is over 100 lines and uses massive string concatenation.
- High risk of missing spaces or using incorrect flags.
- Manual Boolean to String conversion (`GetBoolStr`).
- Validation for build flags is hard to maintain.

## Target Architecture
1. **BuildGraphArguments**: A class representing all possible BuildGraph flags.
2. **UatArguments**: A class representing all possible UAT flags.
3. **BuildArgumentBuilder**: A builder that generates the final command-line string based on version-specific rules.

## Execution Steps

### Step 1: Definition
- Define the `BuildGraphArguments` and `UatArguments` classes in `UnrealBinaryBuilder.Avalonia.Models`.

### Step 2: Implement Builder
- Implement the `BuildArgumentBuilder` to handle:
  - Adding version-specific flags (`-set:WithWin32=...`).
  - Handling multi-value flags (`-set:GameConfigurations=...`).
  - Correctly quoting paths with spaces.
  - Adding the correct `-target` and `-script` parameters.

### Step 3: Argument Validation
- Add validation to the builder to ensure incompatible flags aren't used together (e.g., `-HostPlatformOnly` with `-set:WithMac=true`).

### Step 4: Component Integration
- Use the builder in the refactored `IEngineBuildService` and `IPluginBuildService`.
- Replace the `PrepareCommandline` method in `MainWindowViewModel`.

## Validation
- Create unit tests that compare the generated command-line string against known good baseline strings.
- Verify that a build correctly fails early if invalid flag combinations are provided.
