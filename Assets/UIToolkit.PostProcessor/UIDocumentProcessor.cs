using InitialPrefabs.TaskExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InitialPrefabs.UIToolkit.PostProcessor {

    struct GenerateJsonTask : ITask {
        public List<string>[] Keywords;
        public List<string> FileNames;
        public StringBuilder StringBuilder;
        public string OutputPath;

        public void Execute() {
            using (var json = new JsonScope(StringBuilder)) {
                for (var i = 0; i < FileNames.Count; i++) {
                    var fileName = FileNames[i];
                    var keywords = Keywords[i];
                    json
                        .Variable(fileName)
                        .Array(keywords);
                }
                json.Pop();
            }
            File.WriteAllText(OutputPath, StringBuilder.ToString(), Encoding.ASCII);
        }

        private readonly ref struct JsonScope {
            private readonly StringBuilder sb;

            public JsonScope(StringBuilder sb) {
                this.sb = sb;
                sb.Append('{');
            }

            public readonly JsonScope Variable(string name) {
                sb.Append('"').Append(name).Append('\"').Append(':');
                return this;
            }

            public readonly JsonScope Array(IReadOnlyList<string> collection) {
                sb.Append('[');
                foreach (var element in collection) {
                    sb.Append('"').Append(element).Append('"').Append(',');
                }
                Pop();
                sb.Append(']').Append(',');
                return this;
            }

            public readonly JsonScope Pop() {
                sb.Remove(sb.Length - 1, 1);
                return this;
            }

            public void Dispose() {
                sb.Append('}');
            }
        }
    }

    public class UIDocumentProcessor : AssetPostprocessor {
        static readonly StringBuilder Builder = new StringBuilder(512);
        static readonly List<XmlDocument> Documents = new List<XmlDocument>(5);
        static readonly List<string> FileNames = new List<string>(5);
        // private static readonly string AdditionalFilesTag = $"<AdditionalFiles Include=\"{Application.dataPath}/Generated/TextData.json\" />";

        // private static bool DidImportCsFiles(ReadOnlySpan<string> importedAssets) {
        //     foreach (ref readonly var asset in importedAssets) {
        //         if (asset.SimpleEndsWith(".cs")) {
        //             return true;
        //         }
        //     }
        //     return false;
        // }

        // private static Task ModifyCsProjFiles() {
        //     var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        //     var csProjs = Directory.GetFiles(projectRoot, "*.csproj");
        //     var tasks = new List<Task>(csProjs.Length);

        //     foreach (var csProj in csProjs) {
        //         string content = File.ReadAllText(csProj);
        //         if (!content.Contains(AdditionalFilesTag)) {
        //             content = content.Replace("</ItemGroup>", $"{AdditionalFilesTag}\n</ItemGroup>");
        //             tasks.Add(File.WriteAllTextAsync(csProj, content));
        //         }
        //     }
        //     Debug.Log($"Modifying {tasks.Count} cs project files!");
        //     return Task.WhenAll(tasks);
        // }

        [MenuItem("Tools/Print")]
        private static void Print() {
            Debug.Log("Test");
            // Debug.Log(ExampleSourceGenerated.ExampleSourceGenerated.GetTestText());
        }

        private static async void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload) {
            Log("IMPORTED: ", importedAssets);
            Log("DELETED: ", deletedAssets);
            Log("MOVED: ", movedAssets);
            Log("MOVED FROM: ", movedFromAssetPaths);

            // if (DidImportCsFiles(new ReadOnlySpan<string>(importedAssets))) {
            //     await ModifyCsProjFiles();
            // }

            TaskHelper.Flush();
            LoadXmlDocs(importedAssets, Documents, FileNames);
            if (Documents.Count > 0) {
                var keywords = new List<string>[Documents.Count];

                await new XmlParser {
                    Documents = Documents,
                    Keywords = keywords
                }.Schedule(Documents.Count, 16);

                var sb = new StringBuilder(1024);
                var path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Assets", "Generated", "TextData.UIToolkitSourceGenerators.additionalfile");
                if (!AssetDatabase.IsValidFolder("Assets/Generated")) {
                    AssetDatabase.CreateFolder("Assets", "Generated");
                }

                await new GenerateJsonTask {
                    StringBuilder = sb,
                    Keywords = keywords,
                    FileNames = FileNames,
                    OutputPath = path
                }.Schedule();

                AssetDatabase.Refresh();
            }
        }

        private static void LoadXmlDocs(string[] importedAssets, List<XmlDocument> documents, List<string> fileNames) {
            documents.Clear();
            fileNames.Clear();
            foreach (var path in importedAssets) {
                if (path.SimpleEndsWith(".uxml")) {
                    new Option<VisualTreeAsset>(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path)).Ok(treeAsset => {
                        var doc = new XmlDocument();
                        doc.Load(FileUtils.AbsolutePath(path));
                        documents.Add(doc);
                        fileNames.Add(treeAsset.name);
                    });
                }
            }
        }

        private static void Log(string header, string[] contents) {
            if (contents.Length == 0) {
                return;
            }
            Builder.Clear();
            Builder.Append(header);
            foreach (var content in contents) {
                Builder.Append(content).Append(", ");
            }

            Debug.Log(Builder.ToString());
        }
    }
}
