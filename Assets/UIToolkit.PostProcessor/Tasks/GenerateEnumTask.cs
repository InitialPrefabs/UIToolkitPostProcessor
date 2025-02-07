using InitialPrefabs.TaskExtensions;
using Scriban;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace InitialPrefabs.UIToolkit.PostProcessor.Tasks {

    internal struct GenerateEnumTask : ITask {

        internal struct Pair {
            public string Key;
            public string Value;
        }

        public List<string>[] Keywords;
        public IReadOnlyList<string> FileNames;
        public Template ScribanTemplate;
        public GeneratorSettings Settings;

        public void Execute() {
            var regex = new Regex(Settings.SearchPattern);
            var pairs = new List<Pair>();
            for (var i = 0; i < FileNames.Count; i++) {
                var fileName = FileNames[i];
                var keywords = Keywords[i];
                ProcessPairs(ref pairs, keywords, regex, Settings.ReplacementPattern);
                var text = ScribanTemplate.Render(new { Name = fileName, Pairs = pairs });
                File.WriteAllText(Path.Combine(Settings.ScriptGenerationPath, $"{fileName}.g.cs"), text);
            }
        }

        private static void ProcessPairs(ref List<Pair> pairs, IReadOnlyList<string> keywords, Regex searchRegex, string replacement) {
            pairs.Clear();
            foreach (var keyword in keywords) {
                var key = searchRegex.Replace(keyword, replacement).Trim().ToUpper();
                pairs.Add(new Pair {
                    Key = key,
                    Value = keyword
                });
            }
        }
    }
}
