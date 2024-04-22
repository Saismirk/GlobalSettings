using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GlobalSettings {
    [CreateAssetMenu(menuName = "Settings/Create GlobalSettingContainer", fileName = "GlobalSettingContainer", order = 0)]
    public class GlobalSettingContainer : ScriptableObject {
        public static GlobalSettingContainer Instance {
            get {
                if (_instance != null) return _instance;
                _instance = Resources.Load<GlobalSettingContainer>("GlobalSettings");
                if (_instance != null) return _instance;
                AssetDatabase.CreateAsset(CreateInstance<GlobalSettingContainer>(), "Assets/Resources/GlobalSettings.asset");
                AssetDatabase.Refresh();
                _instance = Resources.Load<GlobalSettingContainer>("GlobalSettings");
                _instance.settings = Array.Empty<GlobalSetting>();
                return _instance;
            }
        }

        static GlobalSettingContainer _instance;

        [SerializeField] GlobalSetting[] settings;

        bool TryGetSettingInternal<T>(string settingName, out T value) {
            var values = settings.Where(setting => setting.SettingName == settingName)
                                 .Select(setting => setting.GetValue())
                                 .OfType<T>()
                                 .ToArray();
             value = values.FirstOrDefault();
            return values.Length > 0;
        }

        public static bool TryGetSetting<T>(string settingName, out T value) => Instance.TryGetSettingInternal(settingName, out value);
    }
}