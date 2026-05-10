# Changelog

## [0.3.0] - 2026-05-10

### Added

- Added camera synchronization with crouching.
- Added `SelectionBase` attribute to the player object.

### Changed

- Refactored the core architecture.

---

## [0.2.0] - 2026-05-09

### Added

- Added crouch functionality with configurable settings.

### Changed

- Movement-related properties and events are now invoked only when the corresponding action is actually performed.
  - Previously, states such as sprinting or moving could still be triggered even when the character was blocked by a wall.
  - Events and properties now update strictly based on real movement/state changes.
- Refactored the core architecture and related submodules for cleaner internal structure and maintainability.

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

- Initial project structure for `SAWController`.
- Base provider-based architecture for cross-platform input.
- Unity 2023.3+ compatibility.
- Basic documentation structure.
- Dual-input support:
  - Legacy Input Manager
  - New Input System

### Changed

- Refactored core modules to reduce coupling.