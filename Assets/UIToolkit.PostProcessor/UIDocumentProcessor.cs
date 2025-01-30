using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InitialPrefabs.UIToolkit.PostProcessor {

    class TaskWorker {

    }

    public class UIDocumentProcessor : AssetPostprocessor {
        static readonly StringBuilder Builder = new StringBuilder(512);
        static readonly List<XmlDocument> Documents = new List<XmlDocument>(5);
        private static async void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload) {
            Log("IMPORTED: ", importedAssets);
            Log("DELETED: ", deletedAssets);
            Log("MOVED: ", movedAssets);
            Log("MOVED FROM: ", movedFromAssetPaths);

            Documents.Clear();
            LoadXmlDocs(importedAssets, Documents);

        }

        private static void GetValidFileIndices(string[] importAssets, ref Span<int> validIndices, ref int count) {
            for (var i = 0; i < importAssets.Length; i++) {
                var file = importAssets[i];
                if (file.SimpleEndsWith(".uxml")) {
                    validIndices[count++] = i;
                }
            }
        }

        private static void LoadXmlDocs(string[] importedAssets, List<XmlDocument> documents) {
            documents.Clear();
            foreach (var path in importedAssets) {
                if (path.SimpleEndsWith(".uxml")) {
                    var doc = new XmlDocument();
                    doc.Load(FileUtils.AbsolutePath(path));
                    documents.Add(doc);
                    // TODO: Stash some metadata so we know what names will need to be added to a document.
                }
            }
        }

        private static async void PrepareThreadWorkloads(IReadOnlyList<XmlDocument> documents, int threadCount) {
            var tasks = new Task[threadCount];
            var sliceSize = documents.Count / threadCount;
            var span = new (int startIndex, int length)[threadCount];

            for (var i = 0; i < threadCount; i++) {
                span[i] = (i * sliceSize, i == threadCount - 1 ? documents.Count - i * sliceSize : sliceSize);
            }

            for (var i = 0; i < threadCount; i++) {
                tasks[i] = Task.Factory.StartNew(() => {
                    (var startIndex, var length) = span[i];

                    for (var j = 0; j < length; j++) {
                        var offset = startIndex + j;
                        var xml = documents[offset];
                        Recurse(xml, node => {
                            foreach (XmlNode attribute in node.Attributes) {
                                if (attribute.Name == "name") {
                                    // TODO: Store the value into a map, maybe add a hierarchial output, because I want to generate a source file per XML doc
                                }
                            }
                        });
                    }
                });
            }

            await Task.WhenAll(tasks);
        }

        private static void ProcessUXMLFiles(string[] importedAssets, Span<int> validIndices, int count) {
            for (var i = 0; i < count; i++) {
                var assetPath = importedAssets[validIndices[i]];
                Option<VisualTreeAsset>.Some(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath)).Ok(document => {
                    var xml = new XmlDocument();
                    xml.Load(FileUtils.AbsolutePath(assetPath));

                    // TODO: Launch multiple threads on the # of valid files
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
            foreach (XmlNode c in node.ChildNodes) {
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
