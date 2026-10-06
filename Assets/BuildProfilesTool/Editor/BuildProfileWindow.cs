#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BuildProfilesTool
{
    /// <summary>
    /// Tools > Build Profiles > Manager
    ///
    /// Lists every BuildProfile asset in the project and lets you, per profile:
    ///  - Apply its identity/scene settings to the current platform
    ///  - Switch the active build target to match the profile, then apply
    ///  - Open its starting scene
    ///  - Kick off a build straight to disk
    /// </summary>
    public class BuildProfileWindow : EditorWindow
    {
        private Vector2 scroll;
        private BuildProfile[] profiles = new BuildProfile[0];
        private string search = "";
        const string folderPath = "Assets/BuildProfilesTool/BuildProfiles";
        readonly private static Vector2 windowMinSize = new (440, 420);
        readonly private static Color buildButtonBackgroundColor = new (0.6f, 0.85f, 1f);

        [MenuItem("Tools/Build Profiles/Manager", priority = 0)]
        public static void Open()
        {
            BuildProfileWindow window = GetWindow<BuildProfileWindow>("Build Profiles");
            window.minSize = windowMinSize;
            window.RefreshProfiles();
        }

        [MenuItem("Tools/Build Profiles/New Profile", priority = 1)]
        public static void NewProfileMenuItem()
        {
           NewProfile();
        }

        private void OnEnable()
        {
           RefreshProfiles();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawContent();
        }

        private void RefreshProfiles()
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildProfile");
            profiles = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<BuildProfile>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(profile => profile != null)
                .OrderBy(profile => profile.profileName)
                .ToArray();
        }

        private static void NewProfile()
        {
            BuildProfile asset = CreateInstance<BuildProfile>();

            EnsureFolder(folderPath);

            string path = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/New Build Profile.asset");

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private void Duplicate(BuildProfile profile)
        {
            string sourcePath = AssetDatabase.GetAssetPath(profile);
            if (string.IsNullOrEmpty(sourcePath)) return;

            string copyPath = AssetDatabase.GenerateUniqueAssetPath(sourcePath);

            if (!AssetDatabase.CopyAsset(sourcePath, copyPath))
            {
                Debug.LogError($"[BuildProfiles] Failed to duplicate '{profile.profileName}'.");
                return;
            }

            BuildProfile copy = AssetDatabase.LoadAssetAtPath<BuildProfile>(copyPath);
            copy.profileName = profile.profileName + " (Copy)";
            EditorUtility.SetDirty(copy);
            AssetDatabase.SaveAssets();

            Selection.activeObject = copy;
            EditorGUIUtility.PingObject(copy);

            RefreshProfiles();
            GUIUtility.ExitGUI(); 
        }

        /// <summary>Applies a profile's identity, define symbols and scene list; optionally switches the active build target first.</summary>
        public static void ApplyProfileSettings(BuildProfile profile, bool switchTarget)
        {
            if (profile == null) return;

            BuildTarget target = profile.ResolvedBuildTarget;
            BuildTargetGroup targetGroup = profile.ResolvedBuildTargetGroup;

            if (switchTarget && EditorUserBuildSettings.activeBuildTarget != target)
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "Switch Build Target",
                    $"Switching the active build target to {target} ({profile.platform}) can trigger asset re-import and take a while. Continue?",
                    "Switch", "Cancel");

                if (!proceed) return;

                bool success = EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, target);

                if (!success)
                {
                    Debug.LogError($"[BuildProfiles] Failed to switch to {target}. Is that platform module installed?");
                    return;
                }
            }

            SetPlayerSettings(profile, targetGroup);
            SetSceneList(profile);

            Debug.Log($"[BuildProfiles] Applied '{profile.profileName}' ({target}, {profile.platform}).");
        }

        private static void SetPlayerSettings(BuildProfile profile, BuildTargetGroup targetGroup)
        {
            NamedBuildTarget namedGroup = NamedBuildTarget.FromBuildTargetGroup(targetGroup);

            PlayerSettings.productName = profile.productName;
            if (!string.IsNullOrEmpty(profile.companyName))
            {
                PlayerSettings.companyName = profile.companyName;
            }

            PlayerSettings.bundleVersion = profile.bundleVersion;

            if (!string.IsNullOrEmpty(profile.packageName))
            {
                PlayerSettings.SetApplicationIdentifier(namedGroup, profile.packageName);
            }

            if (targetGroup == BuildTargetGroup.Android)
            {
                PlayerSettings.Android.bundleVersionCode = profile.androidVersionCode;
            }

            if (targetGroup == BuildTargetGroup.iOS)
            {
                PlayerSettings.iOS.buildNumber = profile.iosBuildNumber;
            }

            if (profile.extraDefines != null && profile.extraDefines.Length > 0)
            {
                string existing = PlayerSettings.GetScriptingDefineSymbols(namedGroup);
                var defines = existing.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToList();
                foreach (string extraDefine in profile.extraDefines)
                {
                    if (defines.Contains(extraDefine)) continue;

                    defines.Add(extraDefine);
                }

                PlayerSettings.SetScriptingDefineSymbols(namedGroup, string.Join(";", defines));
            }

            if (profile.IsMetaQuest && profile.questApplyAstcCompression)
            {
                EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
                Debug.Log("[BuildProfiles] Meta Quest profile: set Android texture compression to ASTC. " +
                          "Remember XR Plug-in Management (Oculus/OpenXR provider) is configured separately in Project Settings, not by this tool.");
            }
        }

        private static void SetSceneList(BuildProfile profile)
        {
            string[] scenePaths = profile.GetScenePaths();
            if (scenePaths.Length > 0)
            {
                EditorBuildSettings.scenes = scenePaths
                    .Select(path => new EditorBuildSettingsScene(path, true))
                    .ToArray();
            }
            else
            {
                Debug.LogWarning($"[BuildProfiles] Profile '{profile.profileName}' has no starting scene assigned; Build Settings scene list was not changed.");
            }
        }

        private static void OpenStartingScene(BuildProfile profile)
        {
            if (profile.startingScene == null)
            {
                Debug.LogWarning("[BuildProfiles] No starting scene assigned on this profile.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string path = AssetDatabase.GetAssetPath(profile.startingScene);
                EditorSceneManager.OpenScene(path);
            }
        }

        /// <summary>Applies the profile (switching target if needed) and builds it straight to disk.</summary>
        public static void Build(BuildProfile profile)
        {
            if (profile == null) return;

            string[] scenePaths = profile.GetScenePaths();

            if (scenePaths.Length == 0)
            {
                EditorUtility.DisplayDialog("Build Profiles", "This profile has no scenes assigned. Add a starting scene first.", "OK");
                return;
            }

            ApplyProfileSettings(profile, switchTarget: true);

            BuildTarget target = profile.ResolvedBuildTarget;
            BuildTargetGroup targetGroup = profile.ResolvedBuildTargetGroup;

            string buildDirectory = Path.Combine(profile.buildFolderName, profile.platform.ToString());
            Directory.CreateDirectory(buildDirectory);
            string fileName = GetOutputFileName(profile);
            string locationPath = string.IsNullOrEmpty(fileName) ? buildDirectory : Path.Combine(buildDirectory, fileName);

            BuildOptions options = profile.buildOptions;

            if (profile.developmentBuild)
            {
                options |= BuildOptions.Development;
            }

            bool proceed = EditorUtility.DisplayDialog(
                "Build",
                $"Build '{profile.profileName}' for {profile.platform} ({target})\nto: {locationPath}\n\nProceed?",
                "Build", "Cancel");

            if (!proceed) return;

            BuildPlayerOptions buildPlayerOptions = new ()
            {
                scenes = scenePaths,
                locationPathName = locationPath,
                target = target,
                targetGroup = targetGroup,
                options = options
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildProfiles] Build succeeded: {summary.totalSize / 1024} KB -> {locationPath}");
            }
            else
            {
                Debug.LogError($"[BuildProfiles] Build {summary.result} ({summary.totalErrors} error(s)). See Console for details.");
            }
        }

        private static string GetOutputFileName(BuildProfile profile)
        {
            string name = string.IsNullOrEmpty(profile.executableName) ? profile.productName : profile.executableName;

            return profile.ResolvedBuildTarget switch
            {
                BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64 => name + ".exe",
                BuildTarget.StandaloneOSX => name + ".app",
                BuildTarget.StandaloneLinux64 => name,
                BuildTarget.Android => name + ".apk",// covers both AndroidGeneric and AndroidMetaQuest
                BuildTarget.iOS => name,// Xcode project folder
                BuildTarget.WebGL => "",// WebGL builds into the folder itself
                _ => name,
            };
        }

        #region GUI Draw Methods
        private void DrawHelpBox(string message, MessageType messageType = MessageType.Info)
        {
            EditorGUILayout.HelpBox(message, messageType);
        }

        private void DrawSimpleButton(string message, System.Action onPressed)
        {
            if (!GUILayout.Button(message)) return;

            onPressed?.Invoke();
            GUIUtility.ExitGUI();
        }

        private void DrawProfiles()
        {
            foreach (BuildProfile profile in profiles)
            {
                if (profile == null) continue;

                if (!string.IsNullOrEmpty(search) && profile.profileName.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                Draw(profile);
            }
        }

        private void Draw(BuildProfile profile)
        {
            bool isActiveTarget = EditorUserBuildSettings.activeBuildTarget == profile.ResolvedBuildTarget;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUIStyle labelStyle = isActiveTarget ? EditorStyles.boldLabel : EditorStyles.label;
                    GUILayout.Label(profile.profileName + "   [" + profile.platform + "]", labelStyle);
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Ping", GUILayout.Width(60)))
                    {
                        Selection.activeObject = profile;
                        EditorGUIUtility.PingObject(profile);
                    }
                }

                if (!string.IsNullOrEmpty(profile.description))
                {
                    EditorGUILayout.LabelField(profile.description, EditorStyles.wordWrappedMiniLabel);
                }

                EditorGUILayout.LabelField("Starting Scene: " + (profile.startingScene != null ? profile.startingScene.name : "(none assigned)"));
                EditorGUILayout.LabelField($"Product: {profile.productName}    Version: {profile.bundleVersion}");

                if (!string.IsNullOrEmpty(profile.packageName))
                {
                    EditorGUILayout.LabelField("Package: " + profile.packageName);
                }

                EditorGUILayout.Space(2);

                //Footer
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawSimpleButton("Open Starting Scene", () => OpenStartingScene(profile));

                    GUILayout.FlexibleSpace();

                    DrawSimpleButton("Duplicate", () => Duplicate(profile));
                    DrawSimpleButton("Apply", () => EditorApplication.delayCall += () => ApplyProfileSettings(profile, true));

                    Color prevColor = GUI.backgroundColor;
                    GUI.backgroundColor = buildButtonBackgroundColor;

                    DrawSimpleButton("Build", () => EditorApplication.delayCall += () => Build(profile));

                    GUI.backgroundColor = prevColor;
                }
            }
        }

        private void DrawContent()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            if (profiles.Length == 0)
            {
                DrawHelpBox("No Build Profiles found.\nClick 'New' above, or use Assets > Create > Build Tools > Build Profile.");
            }
            else
            {
                DrawProfiles();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Active Target: " + EditorUserBuildSettings.activeBuildTarget, EditorStyles.toolbarButton);
                GUILayout.FlexibleSpace();
                search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(180));

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    RefreshProfiles();
                }

                if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    NewProfile();
                    RefreshProfiles();
                    GUIUtility.ExitGUI();
                }
            }
        }

        #endregion
    }
}
#endif