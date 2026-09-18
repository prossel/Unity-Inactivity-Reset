# Unity-Inactivity-Reset

A simple inactivity reset system for Unity.

[https://github.com/prossel/Unity-Inactivity-Reset](https://github.com/prossel/Unity-Inactivity-Reset)

![screenshot](Screenshots/Inactivity-reset-countdown.jpg)

## Installation

### With package manager

* Unity, menu Window > Package Manager
* Click the [ + ] button at top left
* Add Package from git URL...
* Enter this URL: `https://github.com/prossel/Unity-Inactivity-Reset.git`
* When you want to update the package, just use the [ Update ] button from the package in the package manager.

## Setup

1. Add the package to your project.
2. In an install scene, add the `InactivityResetManager` component to a GameObject and keep it alive across scene loads.
3. Set the `Reset Scene Override` field if you want to reset to a specific scene instead of the install scene.
4. If you want the package to use iOS system settings on Apple builds, run `Tools > Inactivity Reset > Create iOS Settings Bundle` in the Unity Editor. This creates `Assets/Plugins/iOS/Settings.bundle` with the default timeout of `60` seconds and countdown of `5` seconds. After installing the app, open the iOS Settings app and select the app to find and change these values.

The built-in post-build step will register the bundle with the generated Xcode project when building for iOS.

## Runtime behavior

* Idle timeout defaults to 60 seconds and countdown to 5 seconds.
* Any input while monitoring resets the idle timer.
* A four-corner touch gesture immediately starts the countdown without waiting for the idle timeout.
* While counting down, the overlay dims the screen and lets the user tap the countdown or the cancel icon to either reset immediately or cancel the reset.
* If the overlay is canceled, the package returns to monitoring and treats that interaction as activity.

## Custom actions on reset

`InactivityResetManager` exposes two `UnityEvent`s in the Inspector, under the "Events" header:

* `On Before Reset` — invoked right before the reset scene starts loading (e.g. to stop in-flight audio, save state, or fade out UI).
* `On After Reset` — invoked once the reset scene has finished loading and monitoring has resumed.

## Notes

This package is designed to survive scene loads via `DontDestroyOnLoad`, so it can live in any install scene and keep monitoring across the rest of the experience.

### Limitations of `DontDestroyOnLoad` with `On Before Reset` / `On After Reset`

Because the manager (and its GameObject) persist across the scene reload while everything else in the reset scene is destroyed and recreated, Inspector-wired event listeners have some restrictions:

* **Inspector-assigned listeners are baked to specific object instances.** If you drag a scene object into `On Before Reset` or `On After Reset` in the Inspector, that reference points to the object instance that existed when the scene was serialized/loaded. `On Before Reset` still fires safely against that instance (the scene hasn't reloaded yet), but `On After Reset` fires *after* the scene has reloaded, so the original instance no longer exists — the listener call will silently no-op or throw `MissingReferenceException` depending on Unity version.
* **Only persistent objects are safe Inspector targets for `On After Reset`.** Objects that also live under `DontDestroyOnLoad` (including the manager itself and its children) remain valid targets. Anything living in the reloadable scene is not.
* **Recommended workaround:** for logic that needs to reference freshly-loaded scene objects after a reset, subscribe from code at runtime instead of wiring it in the Inspector, e.g. in that object's own `Awake`/`Start`:

  ```csharp
  private void OnEnable()
  {
      InactivityResetManager.Instance.OnAfterReset.AddListener(HandlePostReset);
  }

  private void OnDisable()
  {
      InactivityResetManager.Instance.OnAfterReset.RemoveListener(HandlePostReset);
  }
  ```

  Runtime listeners added this way are naturally re-registered each time the scene reloads and never point at a stale, destroyed object.
* **Singleton enforcement.** `Awake` destroys any duplicate `InactivityResetManager` found in a newly loaded scene, so only the first-loaded instance's events are ever used — don't rely on a second instance's Inspector-configured events elsewhere.

## History

See [CHANGELOG.md](CHANGELOG.md)
