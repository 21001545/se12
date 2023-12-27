using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal static class AzureShaderUniforms
{
    internal static readonly int Layer_0_StartPosition = Shader.PropertyToID("_Layer0_StartPosition");
    internal static readonly int Layer_0_EndPosition = Shader.PropertyToID("_Layer0_EndPosition");
    internal static readonly int Layer_0_Alpha = Shader.PropertyToID("_Layer0_Alpha");
    internal static readonly int Layer_0_StartColor = Shader.PropertyToID("_Layer0_StartColor");
    internal static readonly int Layer_0_EndColor = Shader.PropertyToID("_Layer0_EndColor");

    internal static readonly int Layer_1_StartPosition = Shader.PropertyToID("_Layer1_StartPosition");
    internal static readonly int Layer_1_EndPosition = Shader.PropertyToID("_Layer1_EndPosition");
    internal static readonly int Layer_1_Alpha = Shader.PropertyToID("_Layer1_Alpha");
    internal static readonly int Layer_1_StartColor = Shader.PropertyToID("_Layer1_StartColor");
    internal static readonly int Layer_1_EndColor = Shader.PropertyToID("_Layer1_EndColor");

    internal static readonly int Layer_2_StartPosition = Shader.PropertyToID("_Layer2_StartPosition");
    internal static readonly int Layer_2_EndPosition = Shader.PropertyToID("_Layer2_EndPosition");
    internal static readonly int Layer_2_Alpha = Shader.PropertyToID("_Layer2_Alpha");
    internal static readonly int Layer_2_StartColor = Shader.PropertyToID("_Layer2_StartColor");
    internal static readonly int Layer_2_EndColor = Shader.PropertyToID("_Layer2_EndColor");

    internal static readonly int Layer_3_StartPosition = Shader.PropertyToID("_Layer3_StartPosition");
    internal static readonly int Layer_3_EndPosition = Shader.PropertyToID("_Layer3_EndPosition");
    internal static readonly int Layer_3_Alpha = Shader.PropertyToID("_Layer3_Alpha");
    internal static readonly int Layer_3_StartColor = Shader.PropertyToID("_Layer3_StartColor");
    internal static readonly int Layer_3_EndColor = Shader.PropertyToID("_Layer3_EndColor");

    internal static readonly int Layer_4_StartPosition = Shader.PropertyToID("_Layer4_StartPosition");
    internal static readonly int Layer_4_EndPosition = Shader.PropertyToID("_Layer4_EndPosition");
    internal static readonly int Layer_4_Alpha = Shader.PropertyToID("_Layer4_Alpha");
    internal static readonly int Layer_4_StartColor = Shader.PropertyToID("_Layer4_StartColor");
    internal static readonly int Layer_4_EndColor = Shader.PropertyToID("_Layer4_EndColor");

    internal static readonly int Layer_5_StartPosition = Shader.PropertyToID("_Layer5_StartPosition");
    internal static readonly int Layer_5_EndPosition = Shader.PropertyToID("_Layer5_EndPosition");
    internal static readonly int Layer_5_Alpha = Shader.PropertyToID("_Layer5_Alpha");
    internal static readonly int Layer_5_StartColor = Shader.PropertyToID("_Layer5_StartColor");
    internal static readonly int Layer_5_EndColor = Shader.PropertyToID("_Layer5_EndColor");

    internal static readonly int Layer_6_StartPosition = Shader.PropertyToID("_Layer6_StartPosition");
    internal static readonly int Layer_6_EndPosition = Shader.PropertyToID("_Layer6_EndPosition");
    internal static readonly int Layer_6_Alpha = Shader.PropertyToID("_Layer6_Alpha");
    internal static readonly int Layer_6_StartColor = Shader.PropertyToID("_Layer6_StartColor");
    internal static readonly int Layer_6_EndColor = Shader.PropertyToID("_Layer6_EndColor");

    internal static readonly int Layer_7_StartPosition = Shader.PropertyToID("_Layer7_StartPosition");
    internal static readonly int Layer_7_EndPosition = Shader.PropertyToID("_Layer7_EndPosition");
    internal static readonly int Layer_7_Alpha = Shader.PropertyToID("_Layer7_Alpha");
    internal static readonly int Layer_7_StartColor = Shader.PropertyToID("_Layer7_StartColor");
    internal static readonly int Layer_7_EndColor = Shader.PropertyToID("_Layer7_EndColor");

    internal static readonly int Layer_8_StartPosition = Shader.PropertyToID("_Layer8_StartPosition");
    internal static readonly int Layer_8_EndPosition = Shader.PropertyToID("_Layer8_EndPosition");
    internal static readonly int Layer_8_Alpha = Shader.PropertyToID("_Layer8_Alpha");
    internal static readonly int Layer_8_StartColor = Shader.PropertyToID("_Layer8_StartColor");
    internal static readonly int Layer_8_EndColor = Shader.PropertyToID("_Layer8_EndColor");

    internal static readonly int Layer_9_StartPosition = Shader.PropertyToID("_Layer9_StartPosition");
    internal static readonly int Layer_9_EndPosition = Shader.PropertyToID("_Layer9_EndPosition");
    internal static readonly int Layer_9_Alpha = Shader.PropertyToID("_Layer9_Alpha");
    internal static readonly int Layer_9_StartColor = Shader.PropertyToID("_Layer9_StartColor");
    internal static readonly int Layer_9_EndColor = Shader.PropertyToID("_Layer9_EndColor");

    internal static readonly int Layer_10_StartPosition = Shader.PropertyToID("_Layer10_StartPosition");
    internal static readonly int Layer_10_EndPosition = Shader.PropertyToID("_Layer10_EndPosition");
    internal static readonly int Layer_10_Alpha = Shader.PropertyToID("_Layer10_Alpha");
    internal static readonly int Layer_10_StartColor = Shader.PropertyToID("_Layer10_StartColor");
    internal static readonly int Layer_10_EndColor = Shader.PropertyToID("_Layer10_EndColor");
}

[ExecuteInEditMode]
public class FestaSkyBoxController : MonoBehaviour
{
    public enum Gradient
    {
        Linear,
        Radial,
    };
    public enum Blend
    {
        Normal,
        Lighten,
        Darken,
        SoftLight,
        LinearLight,
    };

    [SerializeField] private Material _skyMaterial = null;

    [Header("Layer 0")]
    [SerializeField] private Color Layer_0_StartColor = Color.white;
    [SerializeField] private Color Layer_0_EndColor = Color.white;
    [SerializeField] [Range(0, 1)] private float Layer_0_Alpha = 1.0f;
    [SerializeField] private Gradient Layer_0_Gradient;
    [SerializeField] private Blend Layer_0_Blend;
    [SerializeField] private Vector2 Layer_0_StartPosition;
    [SerializeField] private Vector2 Layer_0_EndPosition;

    [Header("Layer 1")]
    [SerializeField] private Color Layer_1_StartColor = Color.white;
    [SerializeField] private Color Layer_1_EndColor = Color.white;
    [SerializeField] [Range(0, 1)] private float Layer_1_Alpha = 1.0f;
    [SerializeField] private Gradient Layer_1_Gradient;
    [SerializeField] private Blend Layer_1_Blend;
    [SerializeField] private Vector2 Layer_1_StartPosition;
    [SerializeField] private Vector2 Layer_1_EndPosition;

    [Header("Layer 2")]
    [SerializeField] private Color Layer_2_StartColor = Color.white;
    [SerializeField] private Color Layer_2_EndColor = Color.white;
    [SerializeField] [Range(0, 1)] private float Layer_2_Alpha = 1.0f;
    [SerializeField] private Gradient Layer_2_Gradient;
    [SerializeField] private Blend Layer_2_Blend;
    [SerializeField] private Vector2 Layer_2_StartPosition;
    [SerializeField] private Vector2 Layer_2_EndPosition;

    [Header("Layer 3")]
    [SerializeField] private Color Layer_3_StartColor = Color.white;
    [SerializeField] private Color Layer_3_EndColor = Color.white;
    [SerializeField] [Range(0, 1)] private float Layer_3_Alpha = 1.0f;
    [SerializeField] private Gradient Layer_3_Gradient;
    [SerializeField] private Blend Layer_3_Blend;
    [SerializeField] private Vector2 Layer_3_StartPosition;
    [SerializeField] private Vector2 Layer_3_EndPosition;

    [Header("Layer 4")]
    [SerializeField] private Color Layer_4_StartColor = Color.white;
    [SerializeField] private Color Layer_4_EndColor = Color.white;
    [SerializeField] [Range(0, 1)] private float Layer_4_Alpha;
    [SerializeField] private Gradient Layer_4_Gradient;
    [SerializeField] private Blend Layer_4_Blend;
    [SerializeField] private Vector2 Layer_4_StartPosition;
    [SerializeField] private Vector2 Layer_4_EndPosition;

    [Header("Layer 5")]
    [SerializeField] private Color Layer_5_StartColor = Color.white;
    [SerializeField] private Color Layer_5_EndColor = Color.white;
    [SerializeField] [Range(0, 1)] private float Layer_5_Alpha = 1.0f;
    [SerializeField] private Gradient Layer_5_Gradient;
    [SerializeField] private Blend Layer_5_Blend;
    [SerializeField] private Vector2 Layer_5_StartPosition;
    [SerializeField] private Vector2 Layer_5_EndPosition;

    [Header("Layer 6")]
    [SerializeField] private Color Layer_6_StartColor = Color.white;
    [SerializeField] private Color Layer_6_EndColor = Color.white;
    [SerializeField] [Range(0, 1)] private float Layer_6_Alpha;
    [SerializeField] private Gradient Layer_6_Gradient;
    [SerializeField] private Blend Layer_6_Blend;
    [SerializeField] private Vector2 Layer_6_StartPosition;
    [SerializeField] private Vector2 Layer_6_EndPosition;

    [Header("Layer 7")]
    [SerializeField] private Color Layer_7_StartColor = Color.white;
    [SerializeField] private Color Layer_7_EndColor = Color.white;
    [SerializeField] [Range(0, 1)] private float Layer_7_Alpha = 1.0f;
    [SerializeField] private Gradient Layer_7_Gradient;
    [SerializeField] private Blend Layer_7_Blend;
    [SerializeField] private Vector2 Layer_7_StartPosition;
    [SerializeField] private Vector2 Layer_7_EndPosition;

    [Header("Layer 8")]
    [SerializeField] private Color Layer_8_StartColor = Color.white;
    [SerializeField] private Color Layer_8_EndColor = Color.white;
    [SerializeField] [Range(0, 1)] private float Layer_8_Alpha = 1.0f;
    [SerializeField] private Gradient Layer_8_Gradient;
    [SerializeField] private Blend Layer_8_Blend;
    [SerializeField] private Vector2 Layer_8_StartPosition;
    [SerializeField] private Vector2 Layer_8_EndPosition;

    [Header("Layer 9")]
    [SerializeField] private Color Layer_9_StartColor = Color.white;
    [SerializeField] private Color Layer_9_EndColor = Color.white;
    [SerializeField] [Range(0, 1)] private float Layer_9_Alpha = 1.0f;
    [SerializeField] private Gradient Layer_9_Gradient;
    [SerializeField] private Blend Layer_9_Blend;
    [SerializeField] private Vector2 Layer_9_StartPosition;
    [SerializeField] private Vector2 Layer_9_EndPosition;

    [Header("Layer 10")]
    [SerializeField] private Color Layer_10_StartColor = Color.white;
    [SerializeField] private Color Layer_10_EndColor = Color.white;
    [SerializeField] [Range(0, 1)] private float Layer_10_Alpha = 1.0f;
    [SerializeField] private Gradient Layer_10_Gradient;
    [SerializeField] private Blend Layer_10_Blend;
    [SerializeField] private Vector2 Layer_10_StartPosition;
    [SerializeField] private Vector2 Layer_10_EndPosition;

    private void Awake()
    {
        UpdateShaderUniforms();
    }


    private void OnEnable()
    {
        if (_skyMaterial)
            RenderSettings.skybox = _skyMaterial;
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UpdateShaderUniforms();

            if (_skyMaterial)
                RenderSettings.skybox = _skyMaterial;
        }
#endif
        UpdateShaderUniforms();
    }

    private void UpdateShaderUniforms()
    {
        _skyMaterial.SetColor(AzureShaderUniforms.Layer_0_StartColor, Layer_0_StartColor);
        _skyMaterial.SetColor(AzureShaderUniforms.Layer_0_EndColor, Layer_0_EndColor);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_0_StartPosition, Layer_0_StartPosition);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_0_EndPosition, Layer_0_EndPosition);
        _skyMaterial.SetFloat(AzureShaderUniforms.Layer_0_Alpha, Layer_0_Alpha);

        _skyMaterial.SetColor(AzureShaderUniforms.Layer_1_StartColor, Layer_1_StartColor);
        _skyMaterial.SetColor(AzureShaderUniforms.Layer_1_EndColor, Layer_1_EndColor);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_1_StartPosition, Layer_1_StartPosition);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_1_EndPosition, Layer_1_EndPosition);
        _skyMaterial.SetFloat(AzureShaderUniforms.Layer_1_Alpha, Layer_1_Alpha);

        _skyMaterial.SetColor(AzureShaderUniforms.Layer_2_StartColor, Layer_2_StartColor);
        _skyMaterial.SetColor(AzureShaderUniforms.Layer_2_EndColor, Layer_2_EndColor);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_2_StartPosition, Layer_2_StartPosition);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_2_EndPosition, Layer_2_EndPosition);
        _skyMaterial.SetFloat(AzureShaderUniforms.Layer_2_Alpha, Layer_2_Alpha);

        _skyMaterial.SetColor(AzureShaderUniforms.Layer_3_StartColor, Layer_3_StartColor);
        _skyMaterial.SetColor(AzureShaderUniforms.Layer_3_EndColor, Layer_3_EndColor);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_3_StartPosition, Layer_3_StartPosition);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_3_EndPosition, Layer_3_EndPosition);
        _skyMaterial.SetFloat(AzureShaderUniforms.Layer_3_Alpha, Layer_3_Alpha);

        _skyMaterial.SetColor(AzureShaderUniforms.Layer_4_StartColor, Layer_4_StartColor);
        _skyMaterial.SetColor(AzureShaderUniforms.Layer_4_EndColor, Layer_4_EndColor);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_4_StartPosition, Layer_4_StartPosition);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_4_EndPosition, Layer_4_EndPosition);
        _skyMaterial.SetFloat(AzureShaderUniforms.Layer_4_Alpha, Layer_4_Alpha);

        _skyMaterial.SetColor(AzureShaderUniforms.Layer_5_StartColor, Layer_5_StartColor);
        _skyMaterial.SetColor(AzureShaderUniforms.Layer_5_EndColor, Layer_5_EndColor);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_5_StartPosition, Layer_5_StartPosition);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_5_EndPosition, Layer_5_EndPosition);
        _skyMaterial.SetFloat(AzureShaderUniforms.Layer_5_Alpha, Layer_5_Alpha);

        _skyMaterial.SetColor(AzureShaderUniforms.Layer_6_StartColor, Layer_6_StartColor);
        _skyMaterial.SetColor(AzureShaderUniforms.Layer_6_EndColor, Layer_6_EndColor);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_6_StartPosition, Layer_6_StartPosition);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_6_EndPosition, Layer_6_EndPosition);
        _skyMaterial.SetFloat(AzureShaderUniforms.Layer_6_Alpha, Layer_6_Alpha);

        _skyMaterial.SetColor(AzureShaderUniforms.Layer_7_StartColor, Layer_7_StartColor);
        _skyMaterial.SetColor(AzureShaderUniforms.Layer_7_EndColor, Layer_7_EndColor);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_7_StartPosition, Layer_7_StartPosition);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_7_EndPosition, Layer_7_EndPosition);
        _skyMaterial.SetFloat(AzureShaderUniforms.Layer_7_Alpha, Layer_7_Alpha);

        _skyMaterial.SetColor(AzureShaderUniforms.Layer_8_StartColor, Layer_8_StartColor);
        _skyMaterial.SetColor(AzureShaderUniforms.Layer_8_EndColor, Layer_8_EndColor);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_8_StartPosition, Layer_8_StartPosition);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_8_EndPosition, Layer_8_EndPosition);
        _skyMaterial.SetFloat(AzureShaderUniforms.Layer_8_Alpha, Layer_8_Alpha);

        _skyMaterial.SetColor(AzureShaderUniforms.Layer_9_StartColor, Layer_9_StartColor);
        _skyMaterial.SetColor(AzureShaderUniforms.Layer_9_EndColor, Layer_9_EndColor);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_9_StartPosition, Layer_9_StartPosition);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_9_EndPosition, Layer_9_EndPosition);
        _skyMaterial.SetFloat(AzureShaderUniforms.Layer_9_Alpha, Layer_9_Alpha);

        _skyMaterial.SetColor(AzureShaderUniforms.Layer_10_StartColor, Layer_10_StartColor);
        _skyMaterial.SetColor(AzureShaderUniforms.Layer_10_EndColor, Layer_10_EndColor);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_10_StartPosition, Layer_10_StartPosition);
        _skyMaterial.SetVector(AzureShaderUniforms.Layer_10_EndPosition, Layer_10_EndPosition);
        _skyMaterial.SetFloat(AzureShaderUniforms.Layer_10_Alpha, Layer_10_Alpha);

        UpdateBlend(0, Layer_0_Blend);
        UpdateBlend(1, Layer_1_Blend);
        UpdateBlend(2, Layer_2_Blend);
        UpdateBlend(3, Layer_3_Blend);
        UpdateBlend(4, Layer_4_Blend);
        UpdateBlend(5, Layer_5_Blend);
        UpdateBlend(6, Layer_6_Blend);
        UpdateBlend(7, Layer_7_Blend);
        UpdateBlend(8, Layer_8_Blend);
        UpdateBlend(9, Layer_9_Blend);
        UpdateBlend(10, Layer_10_Blend);
        

        UpdateGradient(0, Layer_0_Gradient);
        UpdateGradient(1, Layer_1_Gradient);
        UpdateGradient(2, Layer_2_Gradient);
        UpdateGradient(3, Layer_3_Gradient);
        UpdateGradient(4, Layer_4_Gradient);
        UpdateGradient(5, Layer_5_Gradient);
        UpdateGradient(6, Layer_6_Gradient);
        UpdateGradient(7, Layer_7_Gradient);
        UpdateGradient(8, Layer_8_Gradient);
        UpdateGradient(9, Layer_9_Gradient);
        UpdateGradient(10, Layer_10_Gradient);
    }

    private void UpdateGradient(int layer, Gradient mode)
    {
        if (mode == Gradient.Linear)
        {
            _skyMaterial.EnableKeyword($"LAYER_{layer}_USE_LINEAR_GRADIENT");
        }
        else 
        {
            _skyMaterial.DisableKeyword($"LAYER_{layer}_USE_LINEAR_GRADIENT");
        }
    }

    private void UpdateBlend(int layer, Blend mode)
    {
        if (mode == Blend.Normal)
        {
            _skyMaterial.DisableKeyword($"LAYER_{layer}_DARKEN_BLEND");
            _skyMaterial.DisableKeyword($"LAYER_{layer}_LIGHTEN_BLEND");
            _skyMaterial.DisableKeyword($"LAYER_{layer}_LINEAR_LIGHT_BLEND");
            _skyMaterial.DisableKeyword($"LAYER_{layer}_SOFT_LIGHT_BLEND");
        }
        else if (mode == Blend.Lighten)
        {
            _skyMaterial.DisableKeyword($"LAYER_{layer}_DARKEN_BLEND");
            _skyMaterial.EnableKeyword($"LAYER_{layer}_LIGHTEN_BLEND");
            _skyMaterial.DisableKeyword($"LAYER_{layer}_LINEAR_LIGHT_BLEND");
            _skyMaterial.DisableKeyword($"LAYER_{layer}_SOFT_LIGHT_BLEND");
        }
        else if (mode == Blend.Darken)
        {
            _skyMaterial.EnableKeyword($"LAYER_{layer}_DARKEN_BLEND");
            _skyMaterial.DisableKeyword($"LAYER_{layer}_LIGHTEN_BLEND");
            _skyMaterial.DisableKeyword($"LAYER_{layer}_LINEAR_LIGHT_BLEND");
            _skyMaterial.DisableKeyword($"LAYER_{layer}_SOFT_LIGHT_BLEND");
        }
        else if (mode == Blend.SoftLight)
        {
            _skyMaterial.DisableKeyword($"LAYER_{layer}_DARKEN_BLEND");
            _skyMaterial.DisableKeyword($"LAYER_{layer}_LIGHTEN_BLEND");
            _skyMaterial.DisableKeyword($"LAYER_{layer}_LINEAR_LIGHT_BLEND");
            _skyMaterial.EnableKeyword($"LAYER_{layer}_SOFT_LIGHT_BLEND");
        }
        else if (mode == Blend.LinearLight)
        {
            _skyMaterial.DisableKeyword($"LAYER_{layer}_DARKEN_BLEND");
            _skyMaterial.DisableKeyword($"LAYER_{layer}_LIGHTEN_BLEND");
            _skyMaterial.EnableKeyword($"LAYER_{layer}_LINEAR_LIGHT_BLEND");
            _skyMaterial.DisableKeyword($"LAYER_{layer}_SOFT_LIGHT_BLEND");
        }
    }

}
