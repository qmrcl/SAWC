# Changelog

## [0.7.1]

### Added
- Added an `UpdatePriority(T item)` method to `PrioritizedList<T>` to support dynamic runtime re-evaluation of modifier execution order.
- Added explicit public management methods (`AddContextModifier`, `RemoveContextModifier`, `UpdateContextModifierPriority`, and their velocity counterparts) to `CharacterModifiers` to provide a controlled API for external registration.

### Changed
- **Refactored `PrioritizedList<T>` Architecture:** Replaced the custom `ReadOnlyListWrapper` struct with a direct implementation of the native `IReadOnlyList<T>` interface to eliminate unnecessary boilerplate and struct allocations.
- **Overhauled Modifier Sorting Logic:** Replaced `BinarySearch` and `PriorityComparer` with a stable linear insertion loop in `PrioritizedList<T>`. This guarantees strict FIFO (First-In, First-Out) execution order for modifiers sharing the exact same priority level, preventing unpredictable pipeline evaluation.
- **Encapsulated `CharacterModifiers` State:** Transitioned internal `PrioritizedList` fields to private access, exposing them strictly as `IReadOnlyList` to external systems. This enforces strict encapsulation and prevents unauthorized state mutations (e.g., direct clearing or index manipulation) from breaking the modifier pipeline.
- **Overhauled `CharacterModifierBase` Lifecycle:** Reimplemented the base modifier class to automatically handle pipeline registration and deregistration via Unity's `OnEnable` and `OnDisable` callbacks. Introduced a protected `SetPriority(int)` method to safely mutate priority values while automatically triggering list re-sorting.
- **Decoupled Core Architecture from New Input System:** Abstracted core input tracking to ensure complete operational stability and compilation fallback even when the New Input System package is completely missing or disabled in Player Settings.
- **Implemented Dual Input System Support across Modules:** Refactored utility and detection modules to dynamically switch between Legacy Input Manager and New Input System APIs, utilizing preprocessor directives to eliminate cross-dependency errors in 'Both' or system-exclusive modes.
- **Finalized Localization Pipeline:** Expanded and completed full inspector translations (EN, RU, ZH) using professional technical terminology, entirely replacing previous machine-generated artifacts and filling all missing property keys.
- **Standardized Native Dual Input Support:** Hardened all peripheral modules (Audio, Camera, UI, Touch) to natively support and seamlessly fallback between Legacy Input Manager and New Input System without cross-dependency compilation errors.

## [0.7.0]

### Added
- Added `CeilingBounceVelocity` and `JumpCooldownDuration` parameters to the `JumpSettings` structure inside `CharacterSettings.cs` to expose internal gravity constants directly to the Unity Inspector.
- Added `VelocityThreshold` and `VelocityThresholdSq` parameters to the `ThresholdSettings` structure inside `CharacterSettings.cs` to eliminate hardcoded magic numbers during horizontal velocity evaluations.
- Added an independent `_actionCooldown` configuration factor to `PlayerAudioController` to mitigate sound clipping and overlapping audio artifacts during rapid posture swaps.
- Added an automated localization pipeline for inspector properties and tooltips.
- Added the **SAWC Hub** central editor window featuring live interface language switching, installed localization asset tracking, and a direct documentation pipeline link.

### Changed
- Refactored `CharacterLocomotion.EvaluateAcceleration` to utilize the new `VelocityThresholdSq` configuration data instead of an unconfigurable constant.
- Translated all core framework debug logs, warning messages, and error diagnostics from Russian to English.

### Fixed
- **Fixed Diagonal Sprint Cutoff ("Diagonal Sprint Funeral"):** Overhauled `CharacterLocomotion.IsSprintDirectionAllowed` to run a dominant-axis evaluation using absolute values (`Mathf.Abs(dir.y) >= Mathf.Abs(dir.x)`), allowing seamless diagonal movement without accidental sprint dropouts when masking flags.
- **Fixed Half-Dead Input on Spawn:** Fixed an initialization bug in `MasterInputProvider.Awake` by explicitly seeding `_activeReader` with the first available device, allowing micro-movements and immediate button polling on spawn without requiring an initial stick threshold breach.
- **Fixed CharacterController Bounds Desync:** Added an explicit `_controller.Move(Vector3.zero)` command to the end of `CharacterPosture.SetHeight` to force the physics engine to immediately compute the modified collider height and center context.
- **Fixed Negative SphereCast Crash:** Fixed a critical physical exploit in `CharacterPosture.HasCeilingObstacle` where a negative casting distance caused by sub-clearance inputs triggered internal physical engine exceptions.
- **Fixed Double Jump via Buffer Exploitation:** Added an explicit buffer wipe (`_jumpBufferTimer = 0f`) in `CharacterGravity.TryExecuteJump` immediately following a successful jump sequence to prevent accidental multi-jumps.
- **Fixed Input Data Frame Loss:** Moved the accumulation delta flush from `LookPad.Update` to `LateUpdate` to prevent raw drag telemetry from being wiped out before target tracking controllers can read it.
- **Fixed Zero Vector Acceleration Collapse:** Added a strict guard clause in `CharacterLocomotion.EvaluateAcceleration` to prevent vector normalization operations on zero magnitude vectors, eliminating potential NaN/division-by-zero crashes.
- **Fixed Zero Vector Rotation Glitch:** Added a square magnitude check in `CharacterRotation.Tick` to shield `Mathf.Atan2` calculations from unstable zero-vector updates.
- **Fixed Infinite Joystick Stick Glitch:** Fixed a logical bug in `UniversalJoystick.OnDrag` where an early return on a zero container width left `_currentDirection` frozen at its last valid frame state; implemented an explicit fallback reset to `Vector2.zero`.
- **Fixed Audio System Crash:** Added defensive null checks inside `PlayerAudioController` routines to prevent fatal `NullReferenceException` crashes if specific audio containers are left unassigned in the inspector.
- **Fixed Cross-Session Audio State Leaks:** Added explicit `ResetState()` calls to `PlayerAudioController.Awake` to guarantee that historical `HashSet` and shuffle buffers are cleared down during component initialization.
- Replaced silent error absorption with descriptive `Debug.LogError` diagnostics inside `CharacterModifierBase.OnEnable` when core controller tracking resolves to null.

## [0.6.1] - 2026-06-01

### Added
- **Advanced Audio Architecture:** Introduced `AudioContainerSettings` configuration class, completely decoupling sound clip logic from primitive Unity `AudioSource` playback.
- Added intelligent playback modes for clips: `Sequential`, `Shuffle`, and `Random` with active history buffering (`AvoidRepeatingLast`) to guarantee no immediate sound repetitions.
- Added procedural audio modulation: dynamic volume and pitch randomization ranges per container.
- Added adaptive action audio support for crouching states (`OnCrouchStarted` / `OnCrouchCanceled`).
- Added a generic time-based `_antiSpamCooldown` factor to protect action audio channels from audio clipping and machine-gun event spam.
- Added `AirStateDebounceTime` parameter to `ThresholdSettings` structure inside `CharacterSettings.cs` to filter out micro-grounding anomalies on jagged meshes.
- Added `GravityVerticalVelocity` tracking to `FrameContext` to feed isolated gravity calculations into the state tracking engine.

### Changed
- **Overhauled PlayerAudioController:** Wiped out the naive and broken `AudioSource.loop` step implementation. Footsteps are now driven by a dedicated state-evaluator (`GetCurrentStepSettings`) that dynamically switches intervals and clip pools between Walking, Sprinting, and Crouching in real-time.
- **Overhauled Camera-Relative Movement:** Completely wiped out the legacy trigonometric loop (`Mathf.Atan2`, `eulerAngles.y`) in `BaseInputProvider.CalculateWorldDirection()`. The system now operates purely on vector math via `Vector3.Cross(camRight, Vector3.up)`, achieving flat execution time and zero allocations.
- Refactored `CharacterStateTracker.CalculateAirFlags`: decoupled physical movement from internal gravity intent by processing `realVerticalVelocity` and `gravityVerticalVelocity` as independent parameters.

### Fixed
- **Fixed Infinity Coyote Jump Exploit:** Restricted `EnableAutoJump` (bunny-hopping) strictly to grounded states (`ctx.IsGrounded`). Holding down the jump button while sliding down slopes or dropping from ledges no longer automatically consumes `CoyoteTime` in mid-air.
- **Fixed Camera Snapping & Jitter (Gimbal Lock):** Eliminated vector collapse and sudden 180-degree teleports when looking strictly vertically (zenith/nadir). The calculation now anchors to a rock-solid horizontal `camRight` that never loses precision.
- **Fixed Sprint Direction Circumvention:** Fixed a logical math flaw in `CharacterLocomotion.IsSprintDirectionAllowed` where diagonal stick input could bypass directional restrictions due to mismatched circular and linear thresholds. Input is now properly normalized before evaluation.
- Fixed an issue where the character state tracker falsely triggered jumping animations when simply running up steep hills or riding elevators.

### Removed
- Wiped the dead `_lastCameraYRotation` tracking variable out of `BaseInputProvider.cs` and all related initialization blocks.

## [0.6.0] - 2026-06-01

### Added
- Added gravity and inertia toggle checkboxes to character settings configuration.
- Added directional configuration restrictions for sprinting (forward, backward, left, and right).
- Exposed `IInputProvider` and `BaseSettings` directly via `SAWController` API, and added `EffectiveSettings` to `ICharacterState`.
- Introduced `CharacterRotation` module to handle character orientation in a separate, dedicated class.
- Introduced a unified `CharacterModifierBase` abstract class, enabling multi-interface registration (`IContextModifier`, `IVelocityModifier`) on a single component.

### Changed
- Refactored `CharacterSettings`: overhauled configuration parameters into nested data structures, and the settings asset is now passed into `FrameContext` as a copy to ensure safe, isolated frame-by-frame calculations.
- Refactored `CharacterPosture` to be stateless: removed the local `CharacterSettings` dependency, modified `CheckCrouchState` to accept `CharacterSettingsData` by reference along with explicit state arguments (`isCurrentlyCrouching`, `canStandUp`), and adjusted height snapping/clearance thresholds.
- Refactored `LookPad` to be completely independent, exposing only look delta values.
- Unified input handling through `MasterInputProvider` abstractions to work simultaneously across all input devices.
- Refactored `SAWController` to explicitly drive execution order via sequential pipeline calls (`_locomotion.Tick()`, `_gravity.Tick()`, `_rotation.Tick()`).
- Refactored `CharacterLocomotion`: removed orientation logic, removed the unused `_transform` reference, eliminated duplicate ground/air ternary checks, and optimized sprint direction checks via early returns.
- Optimized `MasterInputProvider` by adding an early guard clause to `EvaluateActiveDeviceByMovement` to bypass redundant device loops when an active device is processing input.
- Optimized `CharacterGravity`: consolidated all time-dependent mechanics (coyote time, jump buffer, cooldowns) into a synchronous `UpdateTimersAndBuffers` method and cached the `ctx.Settings.Physics` structure locally to reduce deep memory lookups.
- Overhauled the Modifiers registration pipeline to utilize C# pattern matching (`this is T`) inside `OnEnable` and `OnDisable` for automatic list routing.

### Fixed
- Fixed namespace mismatches and domain architecture issues across core systems.

### Removed
- Deprecated and removed the legacy `VelocityModifierBase` class.

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

## [0.4.0] - 2026-05-15
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