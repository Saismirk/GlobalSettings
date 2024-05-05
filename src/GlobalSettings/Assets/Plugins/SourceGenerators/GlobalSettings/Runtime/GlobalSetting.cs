using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GlobalSettings {
#if UNITY_EDITOR
    public enum SettingType {
        Bool,
        Int,
        Float,
        String
    }

    [Serializable]
    public record GlobalSetting(string SettingName) {
        [field: SerializeField] public SettingType SettingType { get; set; }
        [field: SerializeField] public string      SettingName { get; set; } = SettingName;
        [field: SerializeField] public float       FloatValue  { get; set; }
        [field: SerializeField] public int         IntValue    { get; set; }
        [field: SerializeField] public bool        BoolValue   { get; set; }
        [field: SerializeField] public string      StringValue { get; set; } = string.Empty;
        [field: SerializeField] public string      Description { get; set; } = string.Empty;

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

                var dropDownRect = new Rect(position.x, position.y, 6, 15);
                property.isExpanded = EditorGUI.Foldout(dropDownRect, property.isExpanded, GUIContent.none);

                var rect  = new Rect(position.x + 6, position.y, position.width, EditorGUIUtility.singleLineHeight);
                if (!property.isExpanded) {
                    var valueString = (SettingType)settingType.enumValueIndex switch {
                        SettingType.Bool   => property.FindPropertyRelative("<BoolValue>k__BackingField").boolValue.ToString() + " (bool)",
                        SettingType.Int    => property.FindPropertyRelative("<IntValue>k__BackingField").intValue.ToString() + " (int)",
                        SettingType.Float  => property.FindPropertyRelative("<FloatValue>k__BackingField").floatValue.ToString() + " (float)",
                        SettingType.String => property.FindPropertyRelative("<StringValue>k__BackingField").stringValue + " (string)",
                        _                  => string.Empty
                    };
                    EditorGUI.LabelField(rect, settingName.stringValue + " = " + valueString);
                    return;    
                }
                
                var width = rect.width;
                rect.width = width * 0.4f;
                settingName.stringValue = EditorGUI.TextField(rect, settingName.stringValue);
                rect.x += rect.width + 20;
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

                var description = property.FindPropertyRelative("<Description>k__BackingField");
                rect.x += rect.width + 20;
                rect.width = width * 0.2f - 20;
                settingType.enumValueIndex = (int)(SettingType)EditorGUI.EnumPopup(rect, (SettingType)settingType.enumValueIndex);
                var descriptionRect = new Rect(position.x + 6, position.y + EditorGUIUtility.singleLineHeight + 5, position.width, EditorGUIUtility
                    .singleLineHeight * 2);
                description.stringValue = EditorGUI.TextField(descriptionRect, description.stringValue);
                EditorGUI.DrawRect(new Rect(descriptionRect.x, descriptionRect.y + descriptionRect.height + 5, position.width, 1), Color.grey);
            }
            
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => property.isExpanded 
                ? EditorGUIUtility.singleLineHeight * 3 + 15 
                : EditorGUIUtility.singleLineHeight;
        }
    }
#endif
}