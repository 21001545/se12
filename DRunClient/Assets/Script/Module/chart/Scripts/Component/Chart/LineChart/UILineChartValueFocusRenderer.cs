using AwesomeCharts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 라인차트에서 Focus 라인을 그려보자
[RequireComponent(typeof(CanvasRenderer))]
public class UILineChartValueFocusRenderer : LineSegmentsRenderer
{
    [SerializeField]
    private int dot_segment = 8;        // 짝수로 맞춰줍시당~!

    [SerializeField]
    private float dot_spacing = 1.0f;

    [SerializeField]
    private float dot_radius = 0.5f;

    [SerializeField]
    private float dot_length = 2.0f;

    private Color _dotColor = ColorChart.gray_500;

    private int focusValue;

    private float originX;
    List<GameObject> focusObjects = new List<GameObject>();

    public void Initialize(List<LineDataSet> data, int focusValue, AxisBounds axisBounds)
    {
        foreach(var go in focusObjects)
        {
            Destroy(go);
        }
        focusObjects.Clear();

        this.focusValue = focusValue;

        float positionDelta = axisBounds.XMax - axisBounds.XMin;
        float valueDelta = axisBounds.YMax - axisBounds.YMin;

        originX = ((focusValue - axisBounds.XMin) / positionDelta) * GetSize().x;
        foreach(var lineData in data)
        {
            if ( lineData.valueFocusPrefab != null && lineData.Entries.Count > focusValue )
            {
                var go = Instantiate(lineData.valueFocusPrefab, transform, false);
                float y = ((lineData.Entries[focusValue].Value - axisBounds.YMin) / valueDelta) * GetSize().y;
                focusObjects.Add(go);
                go.GetComponent<RectTransform>().localPosition = new Vector2(originX, y);
            }
        }

        SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Vector2 size = GetSize();

        // size height만큼, vertex를 만들어 보자.

        // 몇개를 만들어야하니?
        var vertex = UIVertex.simpleVert;

        float angle = 360.0f / dot_segment;

        int heightCount = (int)(size.y / (dot_radius * 2.0f + dot_spacing + dot_length));

        for (int h = 0; h < heightCount; ++h)
        {
            float originY = (dot_radius * 2.0f + dot_spacing + dot_length) * (h + 1) - dot_radius;

            vertex.color = _dotColor;
            vertex.position = new Vector3(originX, originY, 0.0f);

            vh.AddVert(vertex);
            int originVertexIndex = vh.currentVertCount - 1;

            // 위에 반원을 먼저 그리구
            for (int i = 0; i < dot_segment / 2; ++i)
            {
                float a = angle * i - 90f;
                float x = Mathf.Sin(a * Mathf.Deg2Rad);
                float y = Mathf.Cos(a * Mathf.Deg2Rad);
                vertex.position = new Vector3(originX + x * dot_radius, originY + y * dot_radius, 0.0f);
                vh.AddVert(vertex);

                a = angle * (i + 1) - 90f;
                x = Mathf.Sin(a * Mathf.Deg2Rad);
                y = Mathf.Cos(a * Mathf.Deg2Rad);
                vertex.position = new Vector3(originX + x * dot_radius, originY + y * dot_radius, 0.0f);
                vh.AddVert(vertex);

                vh.AddTriangle(originVertexIndex, originVertexIndex + (i * 2) + 1, originVertexIndex + (i * 2) + 2);
            }

            // 가운데 네모
            vertex.position = new Vector3(originX - dot_radius, originY, 0.0f);
            vh.AddVert(vertex);
            vertex.position = new Vector3(originX + dot_radius, originY, 0.0f);
            vh.AddVert(vertex);
            vertex.position = new Vector3(originX - dot_radius, originY - dot_length, 0.0f);
            vh.AddVert(vertex);
            vertex.position = new Vector3(originX + dot_radius, originY - dot_length, 0.0f);
            vh.AddVert(vertex);

            vh.AddTriangle(vh.currentVertCount - 4, vh.currentVertCount - 3, vh.currentVertCount - 2);
            vh.AddTriangle(vh.currentVertCount - 3, vh.currentVertCount - 2, vh.currentVertCount - 1);

            originY -= dot_length;

            // 밑에 반원을 그린당!
            for (int i = dot_segment / 2; i < dot_segment; ++i)
            {
                float a = angle * i - 90f;
                float x = Mathf.Sin(a * Mathf.Deg2Rad);
                float y = Mathf.Cos(a * Mathf.Deg2Rad);
                vertex.position = new Vector3(originX + x * dot_radius, originY + y * dot_radius, 0.0f);
                vh.AddVert(vertex);

                a = angle * (i + 1) - 90f;
                x = Mathf.Sin(a * Mathf.Deg2Rad);
                y = Mathf.Cos(a * Mathf.Deg2Rad);
                vertex.position = new Vector3(originX + x * dot_radius, originY + y * dot_radius, 0.0f);
                vh.AddVert(vertex);

                vh.AddTriangle(originVertexIndex, 4 + originVertexIndex + (i * 2) + 1, 4 + originVertexIndex + (i * 2) + 2);
            }
        }
    }
}
