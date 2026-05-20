# Changelog

## [0.5.1] - 2026-05-20

### Fixed
- Fixed issues related to joystick behavior.

### Changed
- Refactored core internal code structure.
- Slightly refined and adjusted settings configuration.

## [0.5.0] - 2026-05-19
### Added
- Introduced a modular extension architecture (`SAWC.Pipeline`), featuring `IFrameMiddleware` and `IVelocityModifier` interfaces, enabling non-destructive injection of custom gameplay mechanics into the character's physics loop.
- Added `IntendedMoveDirection` and `LookDirection` properties to `ICharacterState`, exposing finalized world-space vectors to external systems (e.g., IK, animators, combat modules) without compromising encapsulation.
- Established a dedicated directory structure for baseline pipeline extensions to separate core physics logic from modular mechanics.

### Changed
- Refactored camera dependency: completely decoupled the camera transform resolution from the core locomotion logic, delegating world-space view calculations entirely to the `IInputProvider` layer.
- Overhauled namespace architecture, segregating the codebase into strict, independent domains (`SAWC.Core`, `SAWC.Input`, `SAWC.Pipeline`) to resolve cross-dependencies and ensure proper UPM package compliance.

## [0.4.0]
### Added
- Added new threshold parameters to `CharacterSettings` (`InputThreshold`, `VerticalVelocityThreshold`, `IdleTransitionMultiplier`) with corresponding safe rules in `OnValidate` to prevent physics anomalies and state machine bugs.
- Added dynamic FOV adjustment functionality.
- Added `CameraMotionDynamics` module (Cinemachine Extension) to handle procedural camera sway, tilt, and pitch based on character velocity (Strafe, Forward, Vertical).
- Added Game Feel mechanics: Coyote Time and Jump Buffer, making jumping highly responsive and forgiving.
- Added `Velocity` property to the core state to expose the actual movement direction and magnitude.
- Added `EnvironmentMask` to `CharacterSettings` to prevent the character from getting stuck under triggers or non-physical layers when standing up from a crouch.

### Changed
- Refactored `CharacterLocomotion` to utilize adjustable squared thresholds from `CharacterSettings` instead of hardcoded magic numbers (`0.01f`, `0.001f`).
- Refactored `CharacterStateTracker` to accept the `CharacterSettings` context in its `Tick` method, enabling custom air-state apex detection and variable hysteresis for grounding transitions.
- Consolidated all camera-related modules into a single `Camera` directory (removed the legacy `Camera Sync` and `Camera Shake` folders).
- Refactored `[AddComponentMenu]` paths across all scripts for a cleaner and more logical component hierarchy.

### Fixed
- Fixed crouching pivot calculation; child objects now no longer clip into the floor when changing character height.

## [0.3.0] - 2026-05-10
### Added
- Added camera synchronization with crouching.
- Added `SelectionBase` attribute to the player object.
### Changed
- Refactored the core architecture.

## [0.2.0] - 2026-05-09
### Added
- Added crouch functionality with configurable settings.
### Changed
- Movement-related properties and events are now invoked only when the corresponding action is actually performed. Previously, states such as sprinting or moving could still be triggered even when the character was blocked by a wall.
- Events and properties now update strictly based on real movement/state changes.
- Refactored the core architecture and related submodules for cleaner internal structure and maintainability.

## [0.1.1] - 2026-05-08
### Added
- Added `DisallowMultipleComponent` to `SAWController`.
### Changed
- Refactored input module class naming to align with technical roles and responsibilities:
  - Renamed `TouchZone` to `LookPad` to better represent the UI interaction surface.
  - Renamed `LookPad` (logic) to `TouchInputReceiver` to reflect its role as a data buffer for Cinemachine.

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