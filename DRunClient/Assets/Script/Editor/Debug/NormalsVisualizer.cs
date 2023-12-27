using UnityEditor;
using UnityEngine;

//if false
[CustomEditor(typeof(MeshRenderer))]
public class NormalsVisualizer : Editor
{
    private static float normalScale = 0.1f;
    private Mesh mesh;
    private bool isDraw = false;

    void OnEnable()
    {
        mesh = null;
        if ( target is SkinnedMeshRenderer)
		{
            mesh = ((SkinnedMeshRenderer)target).sharedMesh;
		}
        else if( target is MeshRenderer)
		{
            MeshFilter mf = ((MeshRenderer)target).GetComponent<MeshFilter>();
            if( mf != null)
			{
                mesh = mf.sharedMesh;
			}
		}
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        isDraw = EditorGUILayout.Toggle("Visualize Normal!", isDraw);

        normalScale = EditorGUILayout.FloatField("Normal Scale", normalScale);
    }

    void OnSceneGUI()
    {
        if (mesh == null)
        {
            return;
        }

        if ( isDraw == false )
        {
            return;
        }

        Handles.matrix = (target as Renderer).transform.localToWorldMatrix;
        
        Vector3[] verts = mesh.vertices;
        Vector3[] normals = mesh.normals;
        int len = mesh.vertexCount;

        for (int i = 0; i < len; i++)
        {
            Handles.color = new Color((normals[i].x + 1.0f ) *0.5f, (normals[i].y + 1.0f) * 0.5f,( normals[i].z + 1.0f ) *0.5f);
            Handles.DrawLine(verts[i], verts[i] + normals[i] * normalScale);
        }
    }
}
//#endif