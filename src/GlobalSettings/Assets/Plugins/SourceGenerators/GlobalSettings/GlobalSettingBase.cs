using System;
using UnityEditor;
using UnityEngine;

namespace GlobalSettings {
    public enum SettingType {
        Bool,
        Int,
        Float,
        String
    }

    [Serializable]
    internal record GlobalSetting(string SettingName) {
        [field: SerializeField] public SettingType SettingType { get; private set; }
        [field: SerializeField] public string      SettingName { get; private set; } = SettingName;
        [field: SerializeField] public float       FloatValue  { get; private set; }
        [field: SerializeField] public int         IntValue    { get; private set; }
        [field: SerializeField] public bool        BoolValue   { get; private set; }
        [field: SerializeField] public string      StringValue { get; private set; } = string.Empty;
        [field: SerializeField] public string      Description { get; private set; } = string.Empty;

        public object GetValue() => SettingType switch {
            SettingType.Bool   => BoolValue,
            SettingType.Int    => IntValue,
            SettingType.Float  => FloatValue,
            SettingType.String => StringValue,
            var _              => throw new ArgumentOutOfRangeException()
        };

        [CustomPropertyDrawer(typeof(GlobalSetting))]
        public class GlobalSettingDrawer : PropertyDrawer {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
                var settingName = property.FindPropertyRelative("<SettingName>k__BackingField");
                var settingType = property.FindPropertyRelative("<SettingType>k__BackingField");
                var description = property.FindPropertyRelative("<Description>k__BackingField");

                var rect  = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                var width = rect.width;
                rect.width = width * 0.4f;
                settingName.stringValue = EditorGUI.TextField(rect, settingName?.stringValue);
                rect.x = rect.width + 20;
                rect.width = width * 0.4f - 20;

                switch ((SettingType)settingType.enumValueIndex) {
                    case SettingType.Bool:
                        var boolValue = property.FindPropertyRelative("<BoolValue>k__BackingField");
                        boolValue.boolValue = EditorGUI.Toggle(rect, boolValue.boolValue);
                        break;
                    case SettingType.Int:
                        var intValue = property.FindPropertyRelative("<IntValue>k__BackingField");
                        intValue.intValue = EditorGUI.IntField(rect, intValue.intValue);
                        break;
                    case SettingType.Float:
                        var floatValue = property.FindPropertyRelative("<FloatValue>k__BackingField");
                        floatValue.floatValue = EditorGUI.FloatField(rect, floatValue.floatValue);
                        break;
                    case SettingType.String:
                        var stringValue = property.FindPropertyRelative("<StringValue>k__BackingField");
                        stringValue.stringValue = EditorGUI.TextField(rect, stringValue.stringValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                rect.x += rect.width + 20;
                rect.width = width * 0.2f - 20;
                settingType.enumValueIndex = (int)(SettingType)EditorGUI.EnumPopup(rect, (SettingType)settingType.enumValueIndex);
                var descriptionRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 5, position.width, EditorGUIUtility.singleLineHeight * 2);
                description.stringValue = EditorGUI.TextField(descriptionRect, description.stringValue);
                var settings = new GlobalSettings();
            }
            
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight * 3 + 5;
        }
    }

    [GlobalSettings("../../../Resources/GlobalSettingContainer.asset")]
    public partial class GlobalSettings { 

    }
}