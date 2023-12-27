using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Festa.Client
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(UIToggleButton), true)]
	public class UIToggleButtonEditor : UnityEditor.UI.ButtonEditor
	{
		SerializedProperty stateSpritesProperty1;
		SerializedProperty stateSpritesProperty2;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			EditorGUILayout.Space();

			serializedObject.Update();
			EditorGUILayout.PropertyField(stateSpritesProperty1);
			EditorGUILayout.PropertyField(stateSpritesProperty2);
			serializedObject.ApplyModifiedProperties();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			stateSpritesProperty1 = serializedObject.FindProperty("stateSprites");
			stateSpritesProperty2 = serializedObject.FindProperty("stateTextColors");
		}
	}
}
