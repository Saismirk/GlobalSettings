using System;

namespace GlobalSettings {
    [Serializable]
    public sealed class GlobalSetting<T> : IGlobalSetting {
        public string SettingName { get; }
        public T      Value       { get; }

        public GlobalSetting(string settingName) {
            SettingName = settingName;
            Value = GlobalSettingContainer.TryGetSetting(settingName, out T value) ? value : default;
        }
    }
}