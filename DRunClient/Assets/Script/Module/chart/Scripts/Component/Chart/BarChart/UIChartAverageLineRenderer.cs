using AwesomeCharts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UIChartAverageLineRenderer : LineSegmentsRenderer
{
    private float dot_radius = 2f;
    private float dot_spacing = 2.0f;
    private int dot_segment = 2;
    private Color dot_color = ColorChart.gray_700;
    float originY;


    public void Initialize(float y)
    {
        raycastTarget = false;
        originY = y;
        SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Vector2 size = GetSize();

        // 몇개를 만들어야하니?
        var vertex = UIVertex.simpleVert;

        float angle = 360.0f / dot_segment;
        Debug.Log(size);

        int widthCount = (int)((size.x) / (dot_radius * 2.0f + dot_spacing));
        for (int w = 0; w < widthCount; ++w)
        {
            float originX = dot_spacing + ((dot_radius * 2.0f + dot_spacing) * w);
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
        }
    }
}
