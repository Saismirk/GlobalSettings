
using System;
using GlobalSettings;
using UnityEngine;

[GlobalSettings("GlobalSettingsContainer.asset")]
public static partial class Settings {
#if UNITY_EDITOR
    private static GlobalSettingContainer _container;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize() => _container = Resources.Load<GlobalSettingContainer>("GlobalSettings/GlobalSettingsContainer");

    private static T GetSetting<T>(string settingKey) {
        if (_container == null) {
            Debug.LogError("GlobalSettingsContainer not found");
            return default;
        }

        try {
            var setting = _container.GetSetting<T>(settingKey);
            if (setting != null) return setting;
        } catch (Exception e) {
            Debug.LogError($"{e.Message}");
            return default;
        }

        return default;
    }
#endif
}
