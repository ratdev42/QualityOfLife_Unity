#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BuildProfilesTool
{
    /// <summary>
    /// Platform choices exposed in the profile inspector. Unity itself has no
    /// distinct BuildTarget for Meta Quest -- Quest builds use BuildTarget.Android
    /// under the hood, same as a regular Android build. This enum keeps them as
    /// separate, clearly-labelled options in the tool while both resolve to the
    /// same underlying BuildTarget/BuildTargetGroup.
    /// </summary>
    public enum TargetPlatform
    {
        WindowsStandalone,
        MacStandalone,
        LinuxStandalone,
        AndroidGeneric,
        AndroidMetaQuest,
        iOS,
        WebGL
    }

    /// <summary>
    /// Data-only description of a build: which scene to start from, which extra
    /// scenes to include, which platform to target, and the identity info
    /// (product name / version / build code) to apply before building.
    ///
    /// Create instances via: Assets > Create > Build Tools > Build Profile
    /// (or use the "New" button in the Build Profiles window).
    /// </summary>
    [CreateAssetMenu(fileName = "New Build Profile", menuName = "Build Tools/Build Profile", order = 1)]
    public class BuildProfile : ScriptableObject
    {
        [Header("Profile")]
        public string profileName = "New Profile";
        [TextArea(2, 4)] public string description;

        [Header("Scenes")]
        [Tooltip("Scene the build boots into. Always placed first in Build Settings.")]
        public SceneAsset startingScene;
        [Tooltip("Any other scenes this build needs, in order.")]
        public SceneAsset[] additionalScenes = new SceneAsset[0];

        [Header("Target Platform")]
        public TargetPlatform platform = TargetPlatform.WindowsStandalone;

        [Tooltip("Meta Quest only: set the standard ASTC mobile texture compression format on apply/build.")]
        public bool questApplyAstcCompression = true;

        [Header("Build Identity")]
        public string productName = "MyGame";
        [Tooltip("Leave blank to keep the project's current Company Name.")]
        public string companyName = "";
        [Tooltip("Package name / bundle identifier for this platform, e.g. com.yourcompany.yourgame. Leave blank to keep whatever is currently set.")]
        public string packageName = "";
        public string bundleVersion = "0.1.0";
        [Tooltip("Android version code (int, must increase for store updates).")]
        public int androidVersionCode = 1;
        [Tooltip("iOS build number.")]
        public string iosBuildNumber = "1";

        [Header("Scripting Define Symbols")]
        [Tooltip("Extra defines merged into this platform's existing symbols (nothing is removed).")]
        public string[] extraDefines = new string[0];

        [Header("Output")]
        public string buildFolderName = "Builds";
        public string executableName = "Game";
        public bool developmentBuild = false;
        public BuildOptions buildOptions = BuildOptions.None;

        /// <summary>True when this profile targets Meta Quest specifically (as opposed to generic Android).</summary>
        public bool IsMetaQuest => platform == TargetPlatform.AndroidMetaQuest;

        /// <summary>The real Unity BuildTarget this platform choice maps to.</summary>
        public BuildTarget ResolvedBuildTarget
        {
            get
            {
                return platform switch
                {
                    TargetPlatform.WindowsStandalone => BuildTarget.StandaloneWindows64,
                    TargetPlatform.MacStandalone => BuildTarget.StandaloneOSX,
                    TargetPlatform.LinuxStandalone => BuildTarget.StandaloneLinux64,
                    TargetPlatform.AndroidGeneric => BuildTarget.Android,
                    TargetPlatform.AndroidMetaQuest => BuildTarget.Android,
                    TargetPlatform.iOS => BuildTarget.iOS,
                    TargetPlatform.WebGL => BuildTarget.WebGL,
                    _ => BuildTarget.StandaloneWindows64,
                };
            }
        }

        /// <summary>The real Unity BuildTargetGroup this platform choice maps to.</summary>
        public BuildTargetGroup ResolvedBuildTargetGroup
        {
            get
            {
                return platform switch
                {
                    TargetPlatform.WindowsStandalone or TargetPlatform.MacStandalone or TargetPlatform.LinuxStandalone => BuildTargetGroup.Standalone,
                    TargetPlatform.AndroidGeneric or TargetPlatform.AndroidMetaQuest => BuildTargetGroup.Android,
                    TargetPlatform.iOS => BuildTargetGroup.iOS,
                    TargetPlatform.WebGL => BuildTargetGroup.WebGL,
                    _ => BuildTargetGroup.Standalone,
                };
            }
        }

        /// <summary>Starting scene followed by additional scenes, as build-settings paths, de-duplicated.</summary>
        public string[] GetScenePaths()
        {
            List<string> paths = new ();

            if (startingScene != null)
            {
                paths.Add(AssetDatabase.GetAssetPath(startingScene));
            }

            if (additionalScenes != null)
            {
                foreach (SceneAsset scene in additionalScenes)
                {
                    if (scene == null) continue;

                    string scenePath = AssetDatabase.GetAssetPath(scene);

                    if (paths.Contains(scenePath)) continue;

                    paths.Add(scenePath);
                }
            }

            return paths.ToArray();
        }
    }
}
#endif