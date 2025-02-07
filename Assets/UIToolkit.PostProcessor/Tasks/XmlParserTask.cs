using InitialPrefabs.TaskExtensions;
using System;
using System.Collections.Generic;
using System.Xml;

namespace InitialPrefabs.UIToolkit.PostProcessor.Tasks {
    internal struct XmlParserTask : ITaskParallelFor {

        public IReadOnlyList<XmlDocument> Documents;
        public List<string>[] Keywords;

        public void Execute(int index) {
            var document = Documents[index];
            var keywords = new List<string>();

            Recurse(document, node => {
                foreach (XmlAttribute xmlAttribute in node.Attributes) {
                    if (xmlAttribute.Name == "name" && xmlAttribute.Value.Length > 0) {
                        keywords.Add(xmlAttribute.Value);
                    }
                }
            });
            Keywords[index] = keywords;
        }

        private static void Recurse(XmlNode node, Action<XmlNode> onNode) {
            foreach (XmlNode c in node.ChildNodes) {
                onNode?.Invoke(c);
                Recurse(c, onNode);
            }
        }
    }
}
