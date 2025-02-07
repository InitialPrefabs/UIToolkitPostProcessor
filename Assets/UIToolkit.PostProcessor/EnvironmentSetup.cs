using System.IO;
using UnityEditor;
using UnityEngine;

namespace InitialPrefabs.UIToolkit.PostProcessor {
    [InitializeOnLoad]
    internal class EnvironmentSetup {
        static EnvironmentSetup() {
            Initialize();
        }

        public static void Initialize() {
            var guids = AssetDatabase.FindAssets("t: GeneratorSettings");
            if (guids.Length == 0) {
                var path = Path.Combine(Application.dataPath, "Settings");
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                    AssetDatabase.Refresh();
                }
                var settings = ScriptableObject.CreateInstance<GeneratorSettings>();
                AssetDatabase.CreateAsset(settings, "Assets/Settings/GeneratorSettings.asset");
            }
        }

        public static Option<GeneratorSettings> TryGetGeneratorSettings() {
            var guids = AssetDatabase.FindAssets("t: GeneratorSettings GeneratorSettings");
            if (guids.Length > 0) {
                return Option<GeneratorSettings>.Some(AssetDatabase.LoadAssetAtPath<GeneratorSettings>(AssetDatabase.GUIDToAssetPath(guids[0])));
            }
            return default;
        }
    }
}
