using TMPro.EditorUtilities;
using UnityEditor;

namespace DRun.Client
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(UIInputField), true)]
	public class UIInputFieldEditor : TMP_InputFieldEditor
	{
		SerializedProperty btnClearProperty;
		SerializedProperty caretGradientProperty;
		SerializedProperty asteriskChar;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			EditorGUILayout.Space();
			serializedObject.Update();
			EditorGUILayout.PropertyField(btnClearProperty);
			EditorGUILayout.PropertyField(caretGradientProperty);
			EditorGUILayout.PropertyField(asteriskChar);
			serializedObject.ApplyModifiedProperties();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			btnClearProperty = serializedObject.FindProperty("btn_clear");
			caretGradientProperty = serializedObject.FindProperty("caret_gradient");
			asteriskChar = serializedObject.FindProperty("m_AsteriskChar");
		}
	}
}
