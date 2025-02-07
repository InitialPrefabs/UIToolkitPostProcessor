using InitialPrefabs.TaskExtensions;
using InitialPrefabs.UIToolkit.Constants;
using InitialPrefabs.UIToolkit.PostProcessor.Tasks;
using Scriban;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InitialPrefabs.UIToolkit.PostProcessor {

    public class GeneratorSettingsWindow : EditorWindow {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private SerializedObject serializedObject;

        [MenuItem("Tools/InitialPrefabs/GeneratorSettingsWindow")]
        public static void ShowWindow() {
            var wnd = GetWindow<GeneratorSettingsWindow>();
            wnd.minSize = new Vector2(400, 275);
            wnd.titleContent = new GUIContent("Code Generator Settings");
        }

        public void CreateGUI() {
            EnvironmentSetup.Initialize();
            var root = rootVisualElement;
            var tree = m_VisualTreeAsset.Instantiate();
            EnvironmentSetup.TryGetGeneratorSettings().Ok(settings => {
                ReassignSerializedObject(settings);
                tree.Q<TextField>(GeneratorSettingsWindowNames.PATTERN).BindProperty(serializedObject.FindProperty(nameof(GeneratorSettings.SearchPattern)));
                tree.Q<TextField>(GeneratorSettingsWindowNames.REPLACEMENT).BindProperty(serializedObject.FindProperty(nameof(GeneratorSettings.ReplacementPattern)));

                var group = tree.Q<VisualElement>(GeneratorSettingsWindowNames.GROUP);
                var toggle = tree.Q<Toggle>(GeneratorSettingsWindowNames.AUTO_GENERATE);
                var autoGenerateProp = serializedObject.FindProperty(nameof(GeneratorSettings.AutoGenerate));
                toggle.BindProperty(autoGenerateProp);
                group.SetEnabled(!autoGenerateProp.boolValue);

                toggle.RegisterValueChangedCallback(changeEvt => {
                    if (changeEvt.newValue != changeEvt.previousValue) {
                        group.SetEnabled(changeEvt.newValue);
                    }
                });

                var scriptPathField = tree.Q<TextField>(GeneratorSettingsWindowNames.SCRIPT_PATH);
                scriptPathField.BindProperty(serializedObject.FindProperty(nameof(GeneratorSettings.ScriptGenerationPath)));
                tree.Q<Button>(GeneratorSettingsWindowNames.SELECT_PATH).RegisterCallback<MouseUpEvent>(mouseUp => {
                    var generationProp = serializedObject.FindProperty(nameof(GeneratorSettings.ScriptGenerationPath));
                    var path = EditorUtility.OpenFolderPanel("Select Script Generation Path", generationProp.stringValue, string.Empty).Replace($"{Application.dataPath}/", string.Empty);
                    if (path.Length > 0) {
                        serializedObject.Update();
                        generationProp.stringValue = path;
                        serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        scriptPathField.value = path;
                    }
                });
                var listView = tree.Q<ListView>(GeneratorSettingsWindowNames.QUEUE);
                listView.BindProperty(serializedObject.FindProperty(nameof(GeneratorSettings.Queue)));
                tree.Q<Button>(GeneratorSettingsWindowNames.GENERATE).RegisterCallback<MouseUpEvent>(async _ => {
                    var treeAssets = settings.Queue;
                    var count = treeAssets.Count;
                    if (count > 0 && UIDocumentProcessor.TryFindScribanEnumTemplate(out var scribanTemplate)) {
                        var documents = new List<XmlDocument>(count);
                        var fileNames = new List<string>(count);
                        var keywords = new List<string>[count];

                        foreach (var asset in treeAssets) {
                            var xml = new XmlDocument();
                            xml.Load(FileUtils.AbsolutePath(AssetDatabase.GetAssetPath(asset)));
                            documents.Add(xml);
                            fileNames.Add($"{asset.name}Names");
                        }

                        await new XmlParserTask {
                            Documents = documents,
                            Keywords = keywords
                        }.Schedule(treeAssets.Count, 4);

                        var template = Template.Parse(scribanTemplate.text);
                        await new GenerateConstantsTask {
                            Keywords = keywords,
                            FileNames = fileNames,
                            Settings = settings,
                            ScribanTemplate = template
                        }.Schedule();

                        // Clear the queue, rebind the property to avoid issues when redrawing the list
                        settings.Queue.Clear();
                        serializedObject.Update();
                        serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        listView.BindProperty(serializedObject.FindProperty(nameof(GeneratorSettings.Queue)));
                        listView.Rebuild();

                        AssetDatabase.Refresh();
                    }
                });
            });

            root.Add(tree);
        }

        private void ReassignSerializedObject(GeneratorSettings settings) {
            serializedObject?.Dispose();
            serializedObject = new SerializedObject(settings);
        }
    }
}