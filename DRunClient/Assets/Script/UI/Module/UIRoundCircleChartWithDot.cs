
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UIRoundCircleChartWithDot : MonoBehaviour
{
    public UILineRenderer background = null;
    public UILineRenderer data = null;

    public int circle_segment = 80;
    public int dot_count = 12;

    [SerializeField]
    private float fillSpeed = 30;

    public float size = 100.0f;
    public float data_percentage = 0.0f;
    private float current_data_percentage = 0.0f;
    //private bool is_dirty = true;

    public bool RunAnimation = true;

    public GameObject dot_prefab = null;
    public Image[] dots = null;
    private Coroutine cachedDrawValueCoroutine; 
    void Start()
    {
        var dotParent = Instantiate(new GameObject("dots"), transform);

        dots = new Image[dot_count];
        for( int i = 0; i < dot_count; ++i )
        {
            float angle = (360.0f / dot_count) * i;
            float x = Mathf.Sin(angle * Mathf.Deg2Rad);
            float y = Mathf.Cos(angle * Mathf.Deg2Rad);

            GameObject dot = Instantiate(dot_prefab, dotParent.transform);
            dot.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;

            var rect = dot.GetComponent<RectTransform>();
            if ( rect != null )
            {
                rect.anchoredPosition = new Vector2(x * size * 0.9f, y * size * 0.9f);
            }

            dots[i] = dot.GetComponent<Image>();

        }

        DrawBackground();
    }

    public void SetValue(float value)
    {
        data_percentage = Mathf.Min(value / 100.0f, 1.0f);

        Color bar_color;
        if (data_percentage <= 0.3f)
        {
            ColorUtility.TryParseHtmlString("#F78080", out bar_color);
        }
        else if (data_percentage <= 0.7f)
        {
            ColorUtility.TryParseHtmlString("#FFC146", out bar_color);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#646CEB", out bar_color);
        }
        if ( current_data_percentage > data_percentage )
        {
            current_data_percentage = Mathf.Max(0.0f, data_percentage - 0.02f);
        }
        data.color = bar_color;

        if (cachedDrawValueCoroutine != null)
        {
            StopCoroutine(cachedDrawValueCoroutine);
            cachedDrawValueCoroutine = null;
        }

        if (RunAnimation)
        {
            // 2021.11.10 이강희
            if( gameObject.activeInHierarchy)
			{
                StartCoroutine(DrawValueCoroutine());
            }
        }
        else
        {
            current_data_percentage = data_percentage;
            DrawValue(current_data_percentage);
        }
    }

    private void DrawBackground()
    {
        Vector2[] points = new Vector2[circle_segment + 1];
        for (int i = 0; i <= circle_segment; ++i)
        {
            float angle = (360.0f / circle_segment) * i;

            float x = Mathf.Sin(angle * Mathf.Deg2Rad);
            float y = Mathf.Cos(angle * Mathf.Deg2Rad);
            points[i] = new Vector2(x * size, y * size);
        }

        background.Points = points;
    }

    IEnumerator DrawValueCoroutine()
    {
        var cachedWaitForEndOfFrame = new WaitForEndOfFrame();
        while (current_data_percentage < data_percentage)
        {
            current_data_percentage += (fillSpeed / 100.0f) * Time.deltaTime;
            current_data_percentage = Mathf.Min(current_data_percentage, data_percentage);
            DrawValue(current_data_percentage);

            yield return cachedWaitForEndOfFrame;
        }
    }

    private void DrawValue(float percentage)
    {
        Vector2[] points;
        int count = (int)((int)(360.0f * percentage) / (360.0f / circle_segment));
        if (count > 0)
        {
            points = new Vector2[count + 1];
            for (int i = 0; i < count; ++i)
            {
                float angle = (360.0f / circle_segment) * i;

                float x = Mathf.Sin(angle * Mathf.Deg2Rad);
                float y = Mathf.Cos(angle * Mathf.Deg2Rad);
                points[i] = new Vector2(x * size, y * size);
            }
            points[count] = new Vector3(Mathf.Sin(360.0f * percentage * Mathf.Deg2Rad) * size, Mathf.Cos(360.0f * percentage * Mathf.Deg2Rad) * size, 0.0f);
        }
        else
        {
            points = new Vector2[2];
            float x = Mathf.Sin(0);
            float y = Mathf.Cos(0);
            points[0] = points[1] = new Vector3(x * size, y * size);
        }

        Color bar_color;
        if (data_percentage <= 0.3f)
        {
            ColorUtility.TryParseHtmlString("#F78080", out bar_color);
        }
        else if (data_percentage <= 0.7f)
        {
            ColorUtility.TryParseHtmlString("#FFC146", out bar_color);
        }
        else
        {
            ColorUtility.TryParseHtmlString("#646CEB", out bar_color);
        }

        Color color;
        // dot에 색깔 변경
        for (int i = 0; i < dots.Length; ++i)
        {
            float angle = (360.0f / dots.Length) * i;
            if (angle <= 360.0f * percentage)
            {
                color = bar_color;
            }
            else
            {
                ColorUtility.TryParseHtmlString("#C6C6CE", out color);
            }
            dots[i].color = color;
        }
        data.Points = points;
    }
}
