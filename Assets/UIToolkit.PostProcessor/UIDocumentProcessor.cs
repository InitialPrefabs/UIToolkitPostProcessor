using InitialPrefabs.TaskExtensions;
using InitialPrefabs.UIToolkit.PostProcessor.Tasks;
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

        public static bool TryFindScribanEnumTemplate(out TextAsset textAsset) {
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
            var settings = EnvironmentSetup.TryGetGeneratorSettings();
            if (!settings.IsValid) {
                return;
            }
            TryLoadXmlDocs(importedAssets, Documents, FileNames, settings.Value);

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
                await new GenerateConstantsTask {
                    Keywords = keywords,
                    FileNames = FileNames,
                    Settings = settings.Value,
                    ScribanTemplate = template
                }.Schedule();
                AssetDatabase.Refresh();
            }
        }

        private static void TryLoadXmlDocs(string[] importedAssets, List<XmlDocument> documents, List<string> fileNames, GeneratorSettings settings) {
            if (settings.AutoGenerate) {
                documents.Clear();
                fileNames.Clear();
                foreach (var path in importedAssets) {
                    if (path.SimpleEndsWith(".uxml")) {
                        new Option<VisualTreeAsset>(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path)).Ok(treeAsset => {
                            var doc = new XmlDocument();
                            doc.Load(FileUtils.AbsolutePath(path));
                            documents.Add(doc);
                            fileNames.Add($"{treeAsset.name}Names");
                        });
                    }
                }
            } else {
                foreach (var path in importedAssets) {
                    if (path.SimpleEndsWith(".uxml")) {
                        new Option<VisualTreeAsset>(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path)).Ok(treeAsset => {
                            if (!settings.Queue.Contains(treeAsset)) {
                                settings.Queue.Add(treeAsset);
                            }
                        });
                    }
                }
                using var so = new SerializedObject(settings);
                so.Update();
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
