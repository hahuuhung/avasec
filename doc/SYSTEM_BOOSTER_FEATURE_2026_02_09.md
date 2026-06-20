# System Booster Feature Documentation

**Date:** 2026-02-09
**Version:** 1.0

## Overview

The System Booster is a new advanced tool added to the SysAnti Professional dashboard. It provides a centralized interface for users to apply various system optimizations with a single click.

## Features

1. **Network Optimization**: Adjustment of TCP/IP settings (autotuning, heuristics) and DNS flushing.
2. **Visual Effects Optimization**: Disabling unnecessary visual effects for better performance.
3. **Telemetry Disabling**: Disabling Windows telemetry and tracking services.
4. **Disk Cleanup**: Cleaning of temporary files and system cache.
5. **Sleep & Hibernation Optimization**: Disabling system sleep timers and hibernation to maintain system availability.
6. **High Performance Power Plan**: Enabling the Windows "High Performance" power scheme for maximum speed.

## Implementation Details

### UI

- **Entry Point**: Main Dashboard -> Advanced Tools -> System Booster (Rocket Icon).
- **Window**: `SystemBoosterWindow.xaml` - Redesigned with a **950x650** professional layout, featuring a dark navigation sidebar, category-based optimization panels, and a real-time status log.

### Backend Logic

- **Service**: `SysAnti.Optimization.Services.SystemTweaksService` was extended.
- **New Methods**:
  - `OptimizeNetwork()`: Runs `netsh` and `ipconfig` commands.
  - `DisableVisualEffects()`: Modifies Registry keys for visual settings.
  - `DisableTelemetry()`: Disables DiagTrack service and Registry keys.
  - `DisableSleep()`: Disables all sleep/standby timeouts via `powercfg`.
  - `DisableHibernation()`: Disables hibernation via `powercfg`.
  - `SetHighPerformancePlan()`: Sets the active power plan to "High Performance".

## Localization

- Supported Languages: English (`en-US`), Vietnamese (`vi-VN`), Hybrid (`vi-EN`).
- Key: `Lang.Tools.SystemBooster`

## Usage

1. Open SysAnti Dashboard.
2. Expand "Advanced Tools".
3. Click on "System Booster".
4. Select desired optimizations.
5. Click "BOOST NOW".
