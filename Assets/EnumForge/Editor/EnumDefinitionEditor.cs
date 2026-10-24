using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EnumForge.Editor
{
    [CustomEditor(typeof(EnumDefinition))]
    public class EnumDefinitionEditor : UnityEditor.Editor
    {
        private new SerializedProperty name;
        private SerializedProperty isMask;
        private SerializedProperty outputFolderPath;
        private SerializedProperty entries;
        private SerializedProperty nextValue;
        private ReorderableList list;
        private List<string> errors = new();
        private bool isUpToDate;
        private EnumDefinition Definition => (EnumDefinition)target;
        private const float valueWidth = 64f;
        private static readonly float lineHeight = EditorGUIUtility.singleLineHeight;

        private void OnEnable()
        {
            name = serializedObject.FindProperty("name");
            isMask = serializedObject.FindProperty("isMask");
            outputFolderPath = serializedObject.FindProperty("outputFolderPath");
            entries = serializedObject.FindProperty("entries");
            nextValue = serializedObject.FindProperty("nextValue");

            list = new ReorderableList(serializedObject, entries, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, $"Entries ({entries.arraySize})"),
                elementHeightCallback = _ => lineHeight,
                drawElementCallback = DrawEntry,
                onAddCallback = AddEntry,
            };

#pragma warning disable UDR0004 // unsubscribed in OnDisable
            Undo.undoRedoPerformed += OnUndoRedo;
#pragma warning restore UDR0004
            RefreshStatus();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            RefreshStatus();
            Repaint();
        }

        private void RefreshStatus()
        {
            errors = EnumCodeGenerator.Validate(Definition);
            isUpToDate = errors.Count == 0 && EnumCodeGenerator.IsUpToDate(Definition);
        }

        private void AddEntry(ReorderableList list)
        {
            int entryIndex = entries.arraySize;
            entries.arraySize++;

            SerializedProperty entry = entries.GetArrayElementAtIndex(entryIndex);
            int value = nextValue.intValue++;

            entry.FindPropertyRelative("name").stringValue = UniqueName("NewEntry");
            entry.FindPropertyRelative("value").intValue = value;

            list.index = entryIndex;
        }

        private string UniqueName(string baseName)
        {
            string candidate = baseName;

            for (int n = 1; DoesNameExist(candidate, -1); n++)
            {
                candidate = baseName + n;
            }

            return candidate;
        }

        private bool DoesNameExist(string name, int ignoreIndex)
        {
            for (int i = 0; i < entries.arraySize; i++)
            {
                if (i == ignoreIndex) continue;

                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == name)
                {
                    return true;
                }
            }

            return false;
        }

        #region GUI Draw Methods
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Type", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            string newName = EditorGUILayout.DelayedTextField("Name", name.stringValue);

            if (EditorGUI.EndChangeCheck()) name.stringValue = EnumCodeGenerator.Sanitize(newName);

            EditorGUILayout.PropertyField(outputFolderPath);

            EditorGUILayout.PropertyField(isMask, new GUIContent("Behaves as Mask", "Multiple entries can be selected at once"));

            EditorGUILayout.Space();
            list.DoLayoutList();
            EditorGUILayout.LabelField("Reordering only changes dropdown order. Removed entries' values are retired, never reused.", EditorStyles.wordWrappedMiniLabel);

            if (serializedObject.ApplyModifiedProperties())
            {
                RefreshStatus();
            }

            EditorGUILayout.Space();

            foreach (string error in errors)
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
            }

            if (errors.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    isUpToDate ? "Generated code is up to date." : "There are changes that haven't been generated yet.",
                    isUpToDate ? MessageType.Info : MessageType.Warning);
            }

            using (new EditorGUI.DisabledScope(errors.Count > 0 || EditorApplication.isCompiling))
            {
                string label = EditorApplication.isCompiling ? "Compiling..." : isUpToDate ? "Regenerate" : "Generate";

                if (GUILayout.Button(label, GUILayout.Height(28)))
                {
                    EnumCodeGenerator.Generate(Definition);
                    serializedObject.Update();
                    RefreshStatus();
                    GUIUtility.ExitGUI();
                }
            }

            string path = Definition.OutputPath;

            if (File.Exists(path) && GUILayout.Button("Ping"))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<MonoScript>(path));
            }
        }
        

        private void DrawEntry(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(index);
            SerializedProperty name = entry.FindPropertyRelative("name");

            rect.y += 2;
            
            Rect nameRect = new (rect.x, rect.y, rect.width - valueWidth, lineHeight);

            // Name, tinted red when invalid or duplicated. Sanitized on Enter / focus loss.
            bool valid = EnumCodeGenerator.IsValidIdentifier(name.stringValue) && !DoesNameExist(name.stringValue, index);

            Color prevColor = GUI.color;

            if (!valid)
            {
                GUI.color = new Color(1f, 0.55f, 0.55f);
            }

            EditorGUI.BeginChangeCheck();
            string newName = EditorGUI.DelayedTextField(nameRect, name.stringValue);

            if (EditorGUI.EndChangeCheck())
            {
                name.stringValue = EnumCodeGenerator.Sanitize(newName);
            }

            GUI.color = prevColor;
        }
        #endregion
    }
}