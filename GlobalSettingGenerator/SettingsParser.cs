using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace GlobalSettingGenerator;

public static class SettingsParser {
    public static string ParseSettings(SyntaxNode settingsType,
        string settingsContainerPath,
        ICollection<GlobalSetting> tempSettings) {
        var basePath = Path.GetDirectoryName(settingsType.SyntaxTree.FilePath) ?? string.Empty;
        var combinedPath = Path.Combine(basePath, settingsContainerPath);
        var settingsContainerFullPath = Path.GetFullPath(combinedPath);
        if (!File.Exists(settingsContainerFullPath)) {
            return "File not found at: " + settingsContainerFullPath + "\n";
        }

        string content;
        try {
            using var contentReader = new StreamReader(settingsContainerFullPath, Encoding.UTF8);
            content = contentReader.ReadToEnd();
        }
        catch (Exception e) {
            return "Error reading file: " + settingsContainerFullPath + "\n" + e.Message;
        }

        var rawParameters = content.Split("settings:")[1];
        ParseSettings(rawParameters, tempSettings);

        return settingsContainerFullPath;
    }

    private static void ParseSettings(string content, ICollection<GlobalSetting> tempSettings) {
        var names = GetYamlValue(content, "<SettingName>k__BackingField");
        var types = GetYamlValue(content, "<SettingType>k__BackingField");
        var descriptions = GetYamlValue(content, "<Description>k__BackingField");
        var floats = GetYamlValue(content, "<FloatValue>k__BackingField");
        var ints = GetYamlValue(content, "<IntValue>k__BackingField");
        var bools = GetYamlValue(content, "<BoolValue>k__BackingField");
        var strings = GetYamlValue(content, "<StringValue>k__BackingField");

        if (names.Length == 0) {
            throw new Exception("No parameters found");
        }

        if (names.Length != types.Length) {
            throw new Exception("Names and Types count mismatch");
        }

        for (var i = 0; i < names.Length; i++) {
            var name = names[i];
            var type = i < types.Length ? types[i] : "-1";
            var description = i < descriptions.Length ? descriptions[i] : string.Empty;
            if (!int.TryParse(type, out var result)) {
                continue;
            }

            var value = result switch {
                0 => i < bools.Length ? (bools[i] == "1" ? "true" : "false") : "false",
                1 => i < ints.Length ? ints[i] : "0",
                2 => i < floats.Length ? $"{floats[i]}f" : "0f",
                3 => i < strings.Length ? $"\"{strings[i]}\"" : "\"\"",
                _ => string.Empty
            };
            
            if (result < 0) continue;
            if (tempSettings.Any(s => s.Name == name)) {
                continue;
            }
            
            tempSettings.Add(new GlobalSetting((GlobalSettingType)result, name, description, value));
        }
    }

    private static string[] GetYamlValue(string content, string key) {
        try {
            var regex = new Regex($@"{key}:\s(?<value>.*)\n");
            var matches = regex.Matches(content);
            var result = new string[matches.Count];
            for (var i = 0; i < matches.Count; i++) {
                var match = matches[i];
                result[i] = match.Groups[1].Value;
            }

            return result;
        }
        catch (Exception e) {
            throw new Exception("Error parsing yaml: " + e.Message + "\n" + content + "\n" + key);
        }
    }
}

public enum GlobalSettingType {
    Bool = 0,
    Int = 1,
    Float = 2,
    String = 3,
}

public static class StringExtensions {
    public static string[] Split(this string str, string separator) =>
        str.Split(new[] { separator }, StringSplitOptions.None);
    
    public static string[] PascalToWords(this string str) {
        var words = new List<string>();
        var word = new StringBuilder();
        foreach (var c in str) {
            if (char.IsUpper(c)) {
                if (word.Length > 0) {
                    words.Add(word.ToString());
                    word.Clear();
                }
            }

            word.Append(c);
        }

        if (word.Length > 0) {
            words.Add(word.ToString());
        }

        return words.ToArray();
    }
}