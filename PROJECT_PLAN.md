# Project Setup: Agent Documentation Infrastructure

## Core Documentation
- [x] Create `GEMINI.md` (links to AGENTS.md)
- [x] Create `AGENTS.md` (central information hub, links to `agent/`)

## Agent Directory Structure
- [x] Create directory `agent/`
  - [x] Create `agent/summary.md`
- [x] Create directory `agent/agent_policies/`
  - [x] Create `agent/agent_policies/summary.md`
- [x] Create directory `agent/plans_done/`
  - [x] Create `agent/plans_done/summary.md`
- [x] Create directory `agent/plans_wip/`
  - [x] Create `agent/plans_wip/summary.md`
- [x] Create directory `agent/project_state/`
  - [x] Create `agent/project_state/summary.md`

# Phase 2: UI Framework Migration Evaluation

## Research & Evaluation
- [x] Evaluate WinUI 3 (Modern Windows-only)
- [x] Evaluate Avalonia UI (Cross-platform WPF-like)
- [x] Evaluate .NET MAUI / Uno Platform
- [x] Assess dependency compatibility (HandyControl, AvalonEdit, CefSharp)

## Decision & Prototyping
- [x] Select target framework (Selected: Avalonia UI)
- [x] Create proof-of-concept for core UI components (Built on .NET 10.0)
- [x] Draft full migration plan

# Phase 3: Full Migration Implementation (See MIGRATION_PLAN.md for details)

## 1. ViewModels & UI Porting
- [x] Port `MainWindow` logic to Avalonia ViewModels
- [x] Convert WPF UserControls (PluginCard, etc.) to Avalonia
- [x] Replace `HandyControl` with `FluentAvalonia` equivalents

## 2. Logic & System Integration
- [x] Integrate existing `Classes/` logic with new UI
- [x] Implement cross-platform file picking and shell execution
- [x] Resolve `AvaloniaEdit` assembly loading for the final build

## 3. Infrastructure & Deployment
- [x] Port `UnrealBinaryBuilderUpdater` to the new stack
- [x] Migrate Appcast/Sparkle integration
- [x] Setup cross-platform deployment pipeline

# Phase 4: Architectural Refactoring

## 1. Clean Code & Decoupling
- [x] Implement Strong-Typed Build Configurations (Builder pattern)
- [ ] Refactor God ViewModel (`MainWindowViewModel`) into services
- [x] Implement Unified Logging Strategy
- [x] Centralize platform-specific shell execution logic
