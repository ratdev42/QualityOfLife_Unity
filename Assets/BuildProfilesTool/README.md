# Build Profiles Tool

Saves each build configuration (platform, scenes, identity, defines) as an asset you can apply or build with one click.

The tool is editor-only, so it doesn't add anything to your runtime builds.

---

### The problem

Shipping a project to several targets (for example Windows, Android and Meta Quest) means repeating the same manual steps every time also, keeping multiple builds with varying references can quickly become messy:

- Switching the active build target in Build Settings.
- Re-typing product name, version, bundle identifier, Android version code, etc.
- Re-adding the right scenes, starting scene first.
- Editing scripting define symbols for each platform.
- Remembering the output file name and folder.

These steps are easy to forget, and forgetting one ships a build with the wrong identity or scenes.

### The solution

A **Build Profile** asset stores a complete build configuration. The Build Profiles window lists every profile in the project and applies or builds it in one click.

A profile contains:

- **Scenes:** a starting scene (always first) and any additional scenes.
- **Platform:** Windows, macOS, Linux, Android, Meta Quest, iOS or WebGL. Meta Quest is a distinct choice in the tool, but it builds with Unity's Android target.
- **Identity:** product name, company name, package name, bundle version, Android version code, iOS build number.
- **Scripting define symbols:** extra defines merged into the platform's existing ones (nothing is removed).
- **Output:** build folder, executable name, development build flag, extra `BuildOptions`.
- **Quest option:** optionally set ASTC texture compression on apply.

### Usage

1. Open **Tools > Build Profiles > Manager**.
2. Click **New** (or **Tools > Build Profiles > New Profile**) to create a profile. Profiles are stored in `Assets/BuildProfilesTool/BuildProfiles`. You can also use **Create > Build Tools > Build Profile**.
3. Fill in the profile in the Inspector: scenes, platform, identity and output settings.
4. Use the buttons on each profile in the window:
   - **Open Starting Scene:** opens the profile's starting scene (asks to save modified scenes first).
   - **Duplicate:** copies the profile, e.g. to make a "Quest Beta" variant of a "Quest" profile.
   - **Apply:** applies identity, defines and the scene list to the project. If the active target differs, it offers to switch to the profile's platform first.
   - **Build:** applies the profile (switching target if needed), asks for confirmation, and builds to disk.
5. Builds are written to `<project folder>/<Build Folder>/<Platform>/`, for example `Builds/AndroidMetaQuest/Game.apk`.

The Build button shows the result in the Console, with the total size on success or the error count on failure.

---

## Requirements

- Unity 6 (the project is on 6000.3.11f1). The Build Profiles tool uses the `NamedBuildTarget` API, so older versions may not compile it as-is.

## Known limitations & reminders

- **Build Profiles does not configure XR.** For Meta Quest, configure XR Plug-in Management (Oculus or OpenXR) in Project Settings yourself. The profile only sets the texture compression.
- **Switching targets can be slow.** Changing the active build target triggers asset re-import, which the tool warns about before it starts.
- **Build output is relative to the project folder** (the Unity working directory), not to the Assets folder.
- **The Build Profiles folder is fixed** to `Assets/BuildProfilesTool/BuildProfiles` for new profiles. Existing profiles can be moved freely.

## Project layout

```
Assets/
└── BuildProfilesTool/
    └── Editor/
        ├── BuildProfile.cs          # ScriptableObject: one build configuration
        └── BuildProfileWindow.cs    # Tools > Build Profiles > Manager window
```

## Credits

Created by ratdev42, your friendly virtual neighborhood rodent. 