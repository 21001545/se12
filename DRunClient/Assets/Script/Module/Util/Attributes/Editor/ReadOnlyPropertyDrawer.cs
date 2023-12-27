#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Festa.Client.Module
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prevGUIState = GUI.enabled;
            GUI.enabled = false;
            
            EditorGUI.PropertyField(position, property, label);

            GUI.enabled = prevGUIState;
        }
    }
}
#endif