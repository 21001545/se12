using AwesomeCharts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 차트의 양엽으로 Dot Frame을 그리자.
[RequireComponent(typeof(CanvasRenderer))]
public class UIChartMaxValueLineRenderer : LineSegmentsRenderer
{
    private float dot_radius = 0f;
    private float dot_spacing = 0f;
    private int dot_segment = 0;
    private Color dot_color;

    private string value;
    private Vector2 leftBottomPosition;

    private AxisLabel label;

    public void Initialize(AxisLabel labelPrefab, Vector2 leftBottomPosition, string value, Color color)
    {
        raycastTarget = false;
        dot_color = color;
        this.value = value;
        this.leftBottomPosition = leftBottomPosition;

        Debug.Log(leftBottomPosition);
        if (label == null && labelPrefab != null)
        {
            label = Instantiate(labelPrefab, transform, false);
            label.gameObject.hideFlags = HideFlags.DontSave;
            label.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            label.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
            label.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
            //label.SetLabelTextAlignment(TextAnchor.LowerLeft);
        }

        if (label != null)
        {
            label.SetLabelText(value);
            label.SetLabelColor(color);
            this.leftBottomPosition.x -= label.GetTextPreferredWidth() * 0.5f;

            label.GetComponent<RectTransform>().anchoredPosition = new Vector2(this.leftBottomPosition.x, this.leftBottomPosition.y);

        }
        SetAllDirty();
    }

    public void Initialize(AxisLabel labelPrefab, Vector2 leftBottomPosition, string value, float radius, float spacing, int segment, Color color)
    {
        raycastTarget = false;

/*        dot_radius = radius;
        dot_spacing = spacing;
        dot_segment = segment;*/
        dot_color = color;
        this.value = value;
        this.leftBottomPosition = leftBottomPosition;

        Debug.Log(leftBottomPosition);
        if (label == null && labelPrefab != null)
        {
            label = Instantiate(labelPrefab, transform, false);
            //label.gameObject.hideFlags = HideFlags.DontSave |
            //    HideFlags.HideInHierarchy |
            //    HideFlags.HideInInspector;
            label.gameObject.hideFlags = HideFlags.DontSave;
            label.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            label.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
            label.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
            label.SetLabelTextAlignment(TextAnchor.LowerLeft);
        }

        if ( label != null )
        {
            label.GetComponent<RectTransform>().anchoredPosition = new Vector2(leftBottomPosition.x, leftBottomPosition.y + 7.0f) ;
            label.SetLabelText(value);
            label.SetLabelColor(color);
        }
        SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (string.IsNullOrEmpty(value) || dot_segment == 0 || dot_radius <= 0.0f)
            return;

        Vector2 size = GetSize();

        // 몇개를 만들어야하니?
        var vertex = UIVertex.simpleVert;

        float angle = 360.0f / dot_segment;
        int widthCount = (int)((size.x - leftBottomPosition.x) / (dot_radius * 2.0f + dot_spacing));
        for (int w = 0; w < widthCount; ++w)
        {
            float originY = leftBottomPosition.y + 5.0f; ;
            float originX = leftBottomPosition.x + ((dot_radius * 2.0f + dot_spacing) * w);
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
