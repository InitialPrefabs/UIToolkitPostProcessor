using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InitialPrefabs.UIToolkit.PostProcessor {
    internal class GeneratorSettings : ScriptableObject {
        public string SearchPattern = @"[-\. ]";
        public string ReplacementPattern = "_";
        public string ScriptGenerationPath;
        public bool AutoGenerate;
        public List<VisualTreeAsset> Queue = new List<VisualTreeAsset>();
    }
}
