using UnityEditor;

namespace DRun.Client
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(UIMultiColorToggleButton), true)]
	public class UIMultiColorToggleButtonEditor : UnityEditor.UI.ButtonEditor
	{
		SerializedProperty stateListProperty;
		SerializedProperty statusProperty;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();
			serializedObject.Update();
			EditorGUILayout.PropertyField(stateListProperty);
			EditorGUILayout.PropertyField(statusProperty);
			serializedObject.ApplyModifiedProperties();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			stateListProperty = serializedObject.FindProperty("dataList");
			statusProperty = serializedObject.FindProperty("status");
		}
	}
}
