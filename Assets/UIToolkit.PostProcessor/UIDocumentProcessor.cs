using System;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InitialPrefabs.UIToolkit.PostProcessor {

    public class UIDocumentProcessor : AssetPostprocessor {
        static readonly StringBuilder Builder = new StringBuilder(512);
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload) {
            Log("IMPORTED: ", importedAssets);
            Log("DELETED: ", deletedAssets);
            Log("MOVED: ", movedAssets);
            Log("MOVED FROM: ", movedFromAssetPaths);

            Span<int> validFiles = stackalloc int[importedAssets.Length];
            var count = 0;

            GetValidFileIndices(importedAssets, ref validFiles, ref count);
            ProcessUXMLFiles(importedAssets, validFiles, count);
        }

        private static void GetValidFileIndices(string[] importAssets, ref Span<int> validIndices, ref int count) {
            for (var i = 0; i < importAssets.Length; i++) {
                var file = importAssets[i];
                if (file.SimpleEndsWith(".uxml")) {
                    validIndices[count++] = i;
                }
            }
        }

        private static void ProcessUXMLFiles(string[] importedAssets, Span<int> validIndices, int count) {
            for (var i = 0; i < count; i++) {
                var assetPath = importedAssets[validIndices[i]];
                Debug.Log(assetPath);
                Debug.Log(Option<VisualTreeAsset>.Some(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath)).IsValid);
                Option<VisualTreeAsset>.Some(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath)).Ok(document => {
                    var xml = new XmlDocument();
                    FileUtils.AbsolutePath(assetPath);
                    Debug.Log(FileUtils.AbsolutePath(assetPath));
                    xml.Load(FileUtils.AbsolutePath(assetPath));

                    Recurse(xml, node => {
                        Debug.Log($"Attribute for: {node.Name}");
                        foreach (XmlNode attribute in node.Attributes) {
                            Debug.Log($"Key: {attribute.Name}, Value: {attribute.Value}");
                        }
                    });
                });
            }
        }

        private static void Recurse(XmlNode node, Action<XmlNode> onNode) {
            foreach (XmlNode c in node.ChildNodes)
            {
                onNode?.Invoke(c);
                Recurse(c, onNode);
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
