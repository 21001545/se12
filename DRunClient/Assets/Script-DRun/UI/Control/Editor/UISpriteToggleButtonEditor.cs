using UnityEngine;
using UnityEditor;

namespace DRun.Client
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(UISpriteToggleButton), true)]
	public class UISpriteToggleButtonEditor : UnityEditor.UI.ButtonEditor
	{
		SerializedProperty spriteOnProperty;
		SerializedProperty spriteOffProperty;
		SerializedProperty statusProperty;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUILayout.Space();
			serializedObject.Update();
			EditorGUILayout.PropertyField(spriteOnProperty);
			EditorGUILayout.PropertyField(spriteOffProperty);
			EditorGUILayout.PropertyField(statusProperty);
			serializedObject.ApplyModifiedProperties();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			spriteOnProperty = serializedObject.FindProperty("spriteOn");
			spriteOffProperty = serializedObject.FindProperty("spriteOff");
			statusProperty = serializedObject.FindProperty("status");
		}
	}

}

