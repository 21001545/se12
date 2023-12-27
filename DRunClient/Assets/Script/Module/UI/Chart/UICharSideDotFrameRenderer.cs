using AwesomeCharts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// 차트의 양엽으로 Dot Frame을 그리자.
[RequireComponent(typeof(CanvasRenderer))]
public class UICharSideDotFrameRenderer : LineSegmentsRenderer
{
    [SerializeField]
    private float dot_radius = 0.5f;

    // dot의 반지름
    public float DotRadius
    {
        get
        {
            return dot_radius;
        }
        set
        {
            dot_radius = value;
            SetAllDirty();
        }
    }

    [SerializeField]
    private float dot_spacing = 2.0f;

    [SerializeField]
    private int dot_segment = 8;


    [SerializeField]
    private Color dot_color = new Color(198.0f / 255.0f, 198.0f / 255.0f, 206.0f / 255.0f);


    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Vector2 size = GetSize();

        // size height만큼, vertex를 만들어 보자.

        // 몇개를 만들어야하니?
        var vertex = UIVertex.simpleVert;

        float angle = 360.0f / dot_segment;

        int heightCount = (int)(size.y / (dot_radius * 2.0f + dot_spacing));
        for (int h = 0; h < heightCount; ++h)
        {
            float originY = -size.y * 0.5f + ((dot_radius * 2.0f + dot_spacing) * (h+1));
            float originX = -size.x * 0.5f;
            vertex.color = dot_color;
            vertex.position = new Vector3(originX, originY, 0.0f);
            vh.AddVert(vertex);
            int originVertexIndex = vh.currentVertCount - 1;
            for (int i = 0; i < dot_segment; ++i)
            {
                float a = angle * i;
                float x = Mathf.Sin(a * Mathf.Deg2Rad);
                float y = Mathf.Cos(a * Mathf.Deg2Rad);
                vertex.position = new Vector3(originX + x * dot_radius, originY + y * dot_radius, 0.0f);
                vh.AddVert(vertex);

                a = angle * (i + 1);
                x = Mathf.Sin(a * Mathf.Deg2Rad);
                y = Mathf.Cos(a * Mathf.Deg2Rad);
                vertex.position = new Vector3(originX + x * dot_radius, originY + y * dot_radius, 0.0f);
                vh.AddVert(vertex);

                vh.AddTriangle(originVertexIndex, originVertexIndex + (i * 2) + 1, originVertexIndex + (i * 2) + 2);
            }

            originX = size.x * 0.5f;
            vertex.position = new Vector3(originX, originY, 0.0f);
            vh.AddVert(vertex);
            originVertexIndex = vh.currentVertCount - 1;
            for (int i = 0; i < dot_segment; ++i)
            {
                float a = angle * i * Mathf.Deg2Rad;
                float x = Mathf.Sin(a);
                float y = Mathf.Cos(a);
                vertex.position = new Vector3(originX + x * dot_radius, originY + y * dot_radius, 0.0f);
                vh.AddVert(vertex);

                a = angle * (i + 1) * Mathf.Deg2Rad;
                x = Mathf.Sin(a);
                y = Mathf.Cos(a);
                vertex.position = new Vector3(originX + x * dot_radius, originY + y * dot_radius, 0.0f);
                vh.AddVert(vertex);

                vh.AddTriangle(originVertexIndex, originVertexIndex + (i * 2) + 1, originVertexIndex + (i * 2) + 2);
            }
        }
    }
}
