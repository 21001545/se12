using UnityEditor;

namespace DRun.Client
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(UIColorToggleButton), true)]
	public class UIColorToggleButtonEditor : UnityEditor.UI.ButtonEditor
	{
		SerializedProperty colorOnProperty;
		SerializedProperty colorOffProperty;
		SerializedProperty statusProperty;
		SerializedProperty targetRootProperty;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();
			serializedObject.Update();
			EditorGUILayout.PropertyField(colorOnProperty);
			EditorGUILayout.PropertyField(colorOffProperty);
			EditorGUILayout.PropertyField(statusProperty);
			EditorGUILayout.PropertyField(targetRootProperty);
			serializedObject.ApplyModifiedProperties();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			colorOnProperty = serializedObject.FindProperty("colorOn");
			colorOffProperty = serializedObject.FindProperty("colorOff");
			statusProperty = serializedObject.FindProperty("status");
			targetRootProperty = serializedObject.FindProperty("targetRoot");
		}
	}
}
