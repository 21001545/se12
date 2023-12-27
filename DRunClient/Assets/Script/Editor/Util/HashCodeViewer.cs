using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Festa.Client.Module;

namespace Festa.Client.EditorTool
{
    public class HashCodeViewer : EditorWindow
    {
        [MenuItem("Window/Festa/HashCodeViewer")]
        static void init()
		{
            HashCodeViewer window = EditorWindow.GetWindow<HashCodeViewer>();
            window.Show();
		}

        private string _code;

        void OnGUI()
		{
            _code = EditorGUILayout.TextField("text", _code);
            if( string.IsNullOrEmpty(_code) == false)
			{
                EditorGUILayout.LabelField("hashcode", EncryptUtil.makeHashCode(_code).ToString());
            }
        }
    }

}

