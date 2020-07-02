using System;
using UnityEngine;
using UnityEditor;

[Serializable]
public struct MinMax
{
    public float Min;
    public float Max;

    public MinMax(float min, float max)
    {
        Min = min;
        Max = max;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MinMax))]
public class MinMaxDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var minRect = new Rect(position.x, position.y, 50, position.height);
        var maxRect = new Rect(position.x + 55, position.y, 50, position.height);

        EditorGUI.PropertyField(minRect, property.FindPropertyRelative("Min"), GUIContent.none);
        EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("Max"), GUIContent.none);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
#endif

public class MinMaxRangeAttribute : PropertyAttribute
{
    private readonly float min;
    private readonly float max;
    private readonly string minField;
    private readonly string maxField;
    private readonly bool relative;

    public MinMaxRangeAttribute(string minField, float min, string maxField, float max, bool relative = true)
    {
        this.min = min;
        this.max = max;
        this.minField = minField;
        this.maxField = maxField;
        this.relative = relative;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    public class MinMaxRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var cast = (MinMaxRangeAttribute) attribute;

            SerializedProperty minField;
            SerializedProperty maxField;

            if (cast.relative)
            {
                minField = property.FindPropertyRelative(cast.minField);
                maxField = property.FindPropertyRelative(cast.maxField);
            }
            else
            {
                minField = property.serializedObject.FindProperty(cast.minField);
                maxField = property.serializedObject.FindProperty(cast.maxField);
            }

            float minValue = minField.floatValue;
            float maxValue = maxField.floatValue;

            EditorGUI.LabelField(position, label);

            int indent = EditorGUI.indentLevel;
            float width = EditorGUIUtility.currentViewWidth;
            EditorGUI.indentLevel = 0;
            if (EditorGUIUtility.wideMode)
            {
                width -= EditorGUIUtility.labelWidth;
                position.x += EditorGUIUtility.labelWidth;
            }
            else
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;


            var rect = new Rect(position.x, position.y, width - 130, EditorGUIUtility.singleLineHeight);
            var minRect = new Rect(position.x + width - 125, position.y, 50, EditorGUIUtility.singleLineHeight);
            var maxRect = new Rect(position.x + width - 70, position.y, 50, EditorGUIUtility.singleLineHeight);

            EditorGUI.MinMaxSlider(
                rect,
                GUIContent.none,
                ref minValue,
                ref maxValue,
                cast.min,
                cast.max);

            minField.floatValue = minValue;
            maxField.floatValue = maxValue;

            EditorGUI.PropertyField(minRect, minField, GUIContent.none);
            EditorGUI.PropertyField(maxRect, maxField, GUIContent.none);

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label);
            if (!EditorGUIUtility.wideMode)
                height += EditorGUIUtility.singleLineHeight + 2f * EditorGUIUtility.standardVerticalSpacing;

            return height;
        }
    }
#endif
}