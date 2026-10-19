using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnumForge
{
    /// <summary>
    /// Designer-editable source of truth for a generated C# enum.
    /// Edit the entries in the Inspector, then click "Generate".
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnumDefinition", menuName = "EnumForge/Enum Definition")]
    public class EnumDefinition : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string name;
            [HideInInspector] public int value; // Using a fixed value means that renaming or reordering entries never corrupts existing data
        }

        public new string name = "MyEnum";

        [Tooltip("Project-relative folder for the generated .cs file")]
        public string outputFolderPath = "Assets/Scripts/Enums";

        [Tooltip("Behaves as Mask, multiple entries can be selected at once")]
        public bool isMask;

        public List<Entry> entries = new();

        // Monotonic counter: values of removed entries are retired, never reused
        [HideInInspector] public int nextValue;

        // Lets the generator clean up the old file when the enum is renamed or moved
        [HideInInspector] public string lastGeneratedPath;

        public string OutputPath => $"{outputFolderPath.Replace('\\', '/').TrimEnd('/')}/{name}.cs";
    }
}