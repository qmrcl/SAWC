# Changelog

---

## [0.1.1] - 2026-05-08

### Added

- Added `DisallowMultipleComponent` to `SAWController`.

### Changed

- Refactored input module class naming to align with technical roles and responsibilities:
    - Renamed `TouchZone` to `LookPad` to better represent the UI interaction surface.
    - Renamed `LookPad` (logic) to `TouchInputReceiver` to reflect its role as a data buffer for Cinemachine.

---

## [0.1.0] - 2026-04-29

### Added

- Initial project structure for SAWController.
- Base provider-based architecture for cross-platform input.
- Unity 2023.3+ compatibility.
- Basic documentation structure.
- Dual-input support:
    - Legacy Input Manager
    - New Input System

### Changed

- Refactored core modules to reduce coupling.