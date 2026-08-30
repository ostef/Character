using UnityEditor;
using UnityEngine;

/// <summary>
/// Draws any serialized field marked with [SerializeReadOnly] as disabled
/// </summary>
[CustomPropertyDrawer(typeof(SerializeReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label){
        // Preserve correct height for nested/complex properties (arrays, structs, etc.)
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var previousGUIState = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = previousGUIState;
    }
}
