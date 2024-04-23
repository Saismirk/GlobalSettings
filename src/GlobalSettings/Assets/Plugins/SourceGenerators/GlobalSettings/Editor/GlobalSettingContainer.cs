using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace GlobalSettings {
    [CreateAssetMenu(menuName = "Global Settings/Create Global Settings", fileName = "GlobalSettings", order = 0)]
    public class GlobalSettingContainer : ScriptableObject {
        [SerializeField] GlobalSetting[] settings;
        [SerializeField] string className = "GlobalSettings";
        [SerializeField] string namespaceName = "";
        [SerializeField] bool generateAssemblyDefinition = true;

        [CustomEditor(typeof(GlobalSettingContainer))]
        public class GlobalSettingContainerEditor : Editor {
            private const string CLASS_TEMPLATE_NAMESPACE = @"
using System;
using UnityEngine;

namespace {0} {{
    [GlobalSettings({1})]
    public static class {2} {{}}
}}";

            private const string CLASS_TEMPLATE = @"
using System;
using UnityEngine;

[GlobalSettings(""{0}"")]
public static partial class {1} {{}}
";

            private const string ASSEMBLY_TEMPLATE = @"
{{
    ""name"": ""{0}"",
    ""rootNamespace"": """",
    ""references"": [
        ""GlobalSettings""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": true,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";

            public override void OnInspectorGUI() {
                base.OnInspectorGUI();
                var container = (GlobalSettingContainer)target;

                if (GUILayout.Button("Generate Class")) {
                    GenerateClass(container);
                }
            }

            private static void GenerateClass(GlobalSettingContainer container) {
                var className = container.className;
                if (string.IsNullOrEmpty(className)) {
                    className = container.name;
                    return;
                }

                var containerPath = AssetDatabase.GetAssetPath(container);
                if (string.IsNullOrEmpty(containerPath) || System.IO.File.Exists(containerPath) == false) {
                    Debug.LogError("Container must be saved to generate class");
                    return;
                }

                var code = string.IsNullOrEmpty(container.namespaceName)
                    ? string.Format(CLASS_TEMPLATE, $"{container.name}.asset", className)
                    : string.Format(CLASS_TEMPLATE_NAMESPACE, container.namespaceName, $"{container.name}.asset", className);
                var directory = System.IO.Path.GetDirectoryName(containerPath);
                if (string.IsNullOrEmpty(directory)) {
                    Debug.LogError("Container must be saved to generate class");
                    return;
                }
                
                var path = System.IO.Path.Combine(directory, $"{className}.cs");
                System.IO.File.WriteAllText(path, code);
                if (container.generateAssemblyDefinition) {
                    path = path.Replace(".cs", ".asmdef");
                    System.IO.File.WriteAllText(path, string.Format(ASSEMBLY_TEMPLATE, $"{container.namespaceName}.{className}"));
                }

                AssetDatabase.Refresh();
                CompilationPipeline.RequestScriptCompilation();
            }
        }
    }
}