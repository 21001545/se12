using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogRenderSettings : MonoBehaviour
{
    [SerializeField]
    private Color _fogColor = Color.white;

    [SerializeField]
    private float _fogDensity = 1.0f;

    [SerializeField]
    private float _ambientIntensity = 1.0f;

    [SerializeField]
    private Color _shadowColor = Color.blue;

    void Update()
    {
        RenderSettings.fogColor = _fogColor;
        RenderSettings.fogDensity = _fogDensity;
        RenderSettings.ambientIntensity = _ambientIntensity;
        RenderSettings.subtractiveShadowColor = _shadowColor;
    }
}
