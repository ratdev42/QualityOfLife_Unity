# EnumForge

Turns designer-friendly enum definitions into real C# enums.

The tool is editor-only, so it doesn't add anything to your runtime builds (except the generated enum files, which are plain C#).

---

### The problem

Enums are usually typed by hand. Keeping them in sync with data, renaming entries, or reordering them causes trouble:

- Renaming or reordering entries can silently change the integer values stored in scenes, prefabs or ScriptableObjects, corrupting saved data.
- Flags enums require manual bit math (`1 << 3`) that is easy to get wrong.
- Designers who need to add a value have to edit C# code.

### The solution

An `EnumDefinition` asset is the single source of truth. You edit it in the Inspector and click **Generate** to write a `.cs` file.

- **Stable values.** Each entry keeps a fixed integer value. Reordering only changes the dropdown order. Values of removed entries are retired and never reused, so old data keeps its meaning.
- **Flags support.** Tick *Behaves as Mask* and the generator adds `[System.Flags]`, a `None = 0` entry and `1 << n` values automatically (bits 0-31).
- **Validation before writing.** Invalid C# identifiers, duplicate names, C# keywords, and bad output folders are reported in the Inspector and block generation. Names are auto-sanitized (e.g. `fire ball-2` becomes `FireBall2`, `3d` becomes `_3d`).
- **Safe output.** EnumForge never overwrites a file it did not generate (it checks for a marker comment). If you rename or move an enum, the previously generated file is removed.
- **Status at a glance.** The Inspector shows whether the generated code is up to date.
- **Bulk regeneration.** `Tools > EnumForge > Regenerate All` regenerates every definition in the project in one batch, triggering a single recompile.

### Usage

1. Create a definition: right-click in the Project window > **Create > EnumForge > Enum Definition**.
2. Set the **Name** (the generated enum name) and the **Output Folder Path** (must be inside `Assets/`, default `Assets/Scripts/Enums`).
3. Tick **Behaves as Mask** if the enum is a flags enum.
4. Add entries with the `+` button in the list and rename them.
5. Click **Generate**. The file `<OutputFolder>/<Name>.cs` is created and Unity recompiles.
6. Use the enum in your code like any other enum:

```csharp
public enum SpeciesType { /* generated */ }

[SerializeField] private SpeciesType speciesType;
```

The demo in `Assets/EnumForge/Definitions/Demo` shows both kinds: `SpeciesTypeDefinition.asset` (regular enum) and `Colors.asset` (flags enum), with the generated files in `Definitions/Demo/Enums/`.

> Do not edit generated files by hand. Change the `EnumDefinition` asset and regenerate.

## Known limitations & reminders

- **Flags enums are limited to 32 entries** (bits 0-31).
- **Enum class output is relative to the Assets folder** , not to the Unity working directory.

## Project layout

```
Assets/
└── EnumForge/
    ├── Editor/
    │    └── EnumDefinitionEditor.cs  # Custom Inspector (entries list, Generate button)
    ├── EnumDefinition.cs            # ScriptableObject: the source of truth
    └── EnumCodeGenerator.cs         # Validation, sanitizing and file generation
```

## Credits

Created by ratdev42, your friendly virtual neighborhood rodent. 
