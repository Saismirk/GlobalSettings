using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
#endif

namespace GlobalSettings {
    [CreateAssetMenu(menuName = "Global Settings/Create Global Settings", fileName = "GlobalSettings", order = 0)]
    public class GlobalSettingContainer : ScriptableObject {
#if UNITY_EDITOR
        [SerializeField] GlobalSetting[] settings;
        [SerializeField] string className = "GlobalSettings";
        [SerializeField] string namespaceName = "";
        [SerializeField] bool generateAssemblyDefinition = true;
        public Dictionary<string, GlobalSetting> Settings { get; private set; } = new();

        public T GetSetting<T>(string settingKey) {
            if (Settings.TryGetValue(settingKey, out var setting) == false) {
                Debug.LogError($"Setting not found: {settingKey}");
                throw new Exception($"Setting not found: {settingKey}");
            }

            var settingValue = setting.GetValue();
            if (settingValue is T value) return value;
            throw new Exception($"Setting type mismatch: {settingKey}");
        }

        private void OnValidate() {
            Settings = settings.ToDictionary(setting => setting.SettingName);
        }

        [CustomEditor(typeof(GlobalSettingContainer))]
        public class GlobalSettingContainerEditor : Editor {
            private const string CLASS_TEMPLATE_NAMESPACE = @"
using System;
using GlobalSettings;
using UnityEngine;

namespace {0} {{
    [GlobalSettings({2})]
    public static class {3} {{
    #if UNITY_EDITOR
        private static GlobalSettingContainer _container;
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize() => _container = Resources.Load<GlobalSettingContainer>(""{1}"");
    
        private static T GetSetting<T>(string settingKey) {{
            if (_container == null) {{
                Debug.LogError(""GlobalSettingsContainer not found"");
                return default;
            }}

            try {{
                var setting = _container.GetSetting<T>(settingKey);
                if (setting != null) return setting;
            }} catch (Exception e) {{
                Debug.LogError($""{{e.Message}}"");
                return default;
            }}

            return default;
        }}
    #endif
    }}
}}";

            private const string CLASS_TEMPLATE = @"
using System;
using GlobalSettings;
using UnityEngine;

[GlobalSettings(""{0}"")]
public static partial class {2} {{
#if UNITY_EDITOR
    private static GlobalSettingContainer _container;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize() => _container = Resources.Load<GlobalSettingContainer>(""{1}"");

    private static T GetSetting<T>(string settingKey) {{
        if (_container == null) {{
            Debug.LogError(""GlobalSettingsContainer not found"");
            return default;
        }}

        try {{
            var setting = _container.GetSetting<T>(settingKey);
            if (setting != null) return setting;
        }} catch (Exception e) {{
            Debug.LogError($""{{e.Message}}"");
            return default;
        }}

        return default;
    }}
#endif
}}
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
                
                var directory = System.IO.Path.GetDirectoryName(containerPath);
                var resourceDirectory = directory?.Split(new[] { "Resources" }, StringSplitOptions.RemoveEmptyEntries)[1];
                var path = System.IO.Path.Combine(directory, $"{className}.cs");
                var resourceLoadPath = !string.IsNullOrEmpty(resourceDirectory) 
                    ? System.IO.Path
                            .Combine(resourceDirectory, container.name)
                            .Replace("\\", "/")[1..]
                    : container.name;
                
                var code = string.IsNullOrEmpty(container.namespaceName)
                    ? string.Format(CLASS_TEMPLATE, $"{container.name}.asset", resourceLoadPath, className)
                    : string.Format(CLASS_TEMPLATE_NAMESPACE, container.namespaceName, $"{container.name}.asset", resourceLoadPath, className);
                if (string.IsNullOrEmpty(directory)) {
                    Debug.LogError("Container must be saved to generate class");
                    return;
                }
                
                System.IO.File.WriteAllText(path, code);
                if (container.generateAssemblyDefinition) {
                    path = path.Replace(".cs", ".asmdef");
                    var assemblyName = string.IsNullOrEmpty(container.namespaceName) 
                        ? className
                        : $"{container.namespaceName}.{className}";
                    System.IO.File.WriteAllText(path, string.Format(ASSEMBLY_TEMPLATE, assemblyName));
                }

                AssetDatabase.Refresh();
                CompilationPipeline.RequestScriptCompilation();
            }
        }
#endif
    }
}