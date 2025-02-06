using InitialPrefabs.TaskExtensions;
using Scriban;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InitialPrefabs.UIToolkit.PostProcessor {

    public class UIDocumentProcessor : AssetPostprocessor {
        static readonly List<XmlDocument> Documents = new List<XmlDocument>(5);
        static readonly List<string> FileNames = new List<string>(5);

        private static bool TryFindScribanEnumTemplate(out TextAsset textAsset) {
            var guids = AssetDatabase.FindAssets("t: TextAsset PostProcessedEnums");
            foreach (var guid in guids) {
                var txtAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (txtAsset != null) {
                    textAsset = txtAsset;
                    return true;
                }
            }
            textAsset = null;
            return false;
        }

        private static async void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload) {
            TaskHelper.Flush();
            LoadXmlDocs(importedAssets, Documents, FileNames);
            if (Documents.Count > 0) {
                var keywords = new List<string>[Documents.Count];

                await new XmlParserTask {
                    Documents = Documents,
                    Keywords = keywords
                }.Schedule(Documents.Count, 4);

                if (!TryFindScribanEnumTemplate(out var scribanTemplate)) {
                    Debug.LogError("Failed to find PostProcessedEnums.scriban-cs file in the project! The UIDocumentPostProcessor cannot generate named ids!");
                    return;
                }

                var template = Template.Parse(scribanTemplate.text);
                await new GenerateEnumTask {
                    Keywords = keywords,
                    FileNames = FileNames,
                    OutputDirectory = Application.dataPath,
                    ScribanTemplate = template
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
    }
}
