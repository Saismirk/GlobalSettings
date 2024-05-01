using System.Linq;
using System.Text;

namespace GlobalSettingGenerator;

public static class GlobalSettingProvider {
    private static string GetSettingType(GlobalSettingType type) {
        return type switch {
            GlobalSettingType.Bool => "bool",
            GlobalSettingType.Int => "int",
            GlobalSettingType.Float => "float",
            GlobalSettingType.String => "string",
            _ => "object"
        };
    }

    private static string GetSettingName(string settingName) =>
        settingName.Split('_')
                   .SelectMany(word => word.Split(' '))
                   .SelectMany(word => word.Split('.'))
                   .SelectMany(word => word.PascalToWords())
                   .Select(word => word.ToUpper())
                   .Aggregate((a, b) => $"{a}_{b}");

    public static void Generate(StringBuilder sourceBuilder, GlobalSetting setting) {
        var settingName = GetSettingName(setting.Name);
        var settingType = GetSettingType(setting.Type);
        sourceBuilder.AppendLine($@"
        /// <summary>
        /// [Source-Generated] Global setting. {setting.Description}).
        /// </summary>
    #if !UNITY_EDITOR
        public const {settingType} {settingName} = {setting.Value};
    #else
        public static {settingType} {settingName} => GetSetting<{settingType}>(""{setting.Name}"");
    #endif
");
    }
}