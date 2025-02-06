using InitialPrefabs.TaskExtensions;
using Scriban;
using System.Collections.Generic;
using System.IO;

namespace InitialPrefabs.UIToolkit.PostProcessor {

    internal struct GenerateEnumTask : ITask {

        internal struct Pair {
            public string Key;
            public string Value;
        }

        public List<string>[] Keywords;
        public IReadOnlyList<string> FileNames;
        public Template ScribanTemplate;
        public string OutputDirectory;

        public void Execute() {
            var pairs = new List<Pair>();
            for (var i = 0; i < FileNames.Count; i++) {
                var fileName = FileNames[i];
                var keywords = Keywords[i];
                ProcessPairs(ref pairs, keywords);
                var text = ScribanTemplate.Render(new { Name = fileName, Pairs = pairs });
                File.WriteAllText(Path.Combine(OutputDirectory, $"{fileName}.g.cs"), text);
            }
        }

        private static void ProcessPairs(ref List<Pair> pairs, IReadOnlyList<string> keywords) {
            pairs.Clear();
            foreach (var keyword in keywords) {
                // TODO: Replace with Regex
                var key = keyword.Trim().Replace(' ', '_').Replace('-', '_').ToUpper();
                pairs.Add(new Pair {
                    Key = key,
                    Value = keyword
                });
            }
        }
    }
}
