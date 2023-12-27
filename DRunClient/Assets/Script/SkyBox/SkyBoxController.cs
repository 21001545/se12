using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class SkyBoxController : MonoBehaviour
{
    Material _skyboxMaterial = null;

    [SerializeField]
    private bool _enableUpdate = true;

    [Header("Sky Color")]
    [SerializeField]
    Color _skyColor;
    [SerializeField]
    Color _equatorColor;
    [SerializeField]
    Color _groundColor;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _equatorHeight;
    [SerializeField]
    [Range(0.01f, 1.0f)]
    float _equatorSmoothness;

    [Header("Star")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    float _starsIntensity;
    [SerializeField]
    [Range(0.0f, 0.99f)]
    float _starsSize;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _starsSunMask;
    [SerializeField]
    [Range(0.0f, 2.0f)]
    float _twinklingContrast;
    [SerializeField]
    float _twinklingSpeed;
    [SerializeField]
    float _starsRotationSpeed;

    [Header("Cloud")]
    [SerializeField]
    Texture _cloudsCubemap;
    [SerializeField]
    [Range(0.0f, 360.0f)]
    float _cloudsRotationSpeed;
    [SerializeField]
    [Range(-0.5f, 0.5f)]
    float _cloudsHeight;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _cloudsIntensity;
    [SerializeField]
    Color _cloudsLightColor;
    [SerializeField]
    Color _cloudsShadowColor;

    [Header("Sun")]
    [SerializeField]
    bool _enableSun = true;

    [SerializeField]
    Texture _sunTexture;
    [SerializeField]
    Color _sunColor;
    [SerializeField]
    [Range(0.1f, 1.0f)]
    float _sunSize;
    [SerializeField]
    [Range(1.0f, 10.0f)]
    float _sunIntensity;

    [Header("Fog")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _fogHeight;
    [SerializeField]
    [Range(0.01f, 1.0f)]
    float _fogSmoothness;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _fogFill;

    [Header("Skybox")]
    [SerializeField]
    [Range(-1.0f, 1.0f)]
    float _skyboxOffset;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float _skyboxRotation;

    [Header("Environment")]
    [SerializeField]
    Color _environmentAmbientColor;
    [SerializeField]
    Color _fogColor;

    private void Awake()
    {
        var defaultMaterial = RenderSettings.skybox;

        //_skyColor = defaultMaterial.GetColor("_SkyColor");
        //_equatorColor = defaultMaterial.GetColor("_EquatorColor");
        //_groundColor = defaultMaterial.GetColor("_GroundColor");
        //_equatorHeight = defaultMaterial.GetFloat("_EquatorHeight");
        //_equatorSmoothness = defaultMaterial.GetFloat("_EquatorSmoothness");

        //_starsIntensity = defaultMaterial.GetFloat("_StarsIntensity");
        //_starsSize = defaultMaterial.GetFloat("_StarsSize");
        //_starsSunMask = defaultMaterial.GetFloat("_StarsSunMask");
        //_twinklingContrast = defaultMaterial.GetFloat("_TwinklingContrast");
        //_twinklingSpeed = defaultMaterial.GetFloat("_TwinklingSpeed");
        //_starsRotationSpeed = defaultMaterial.GetFloat("_StarsRotationSpeed");

        //_cloudsCubemap = defaultMaterial.GetTexture("_CloudsCubemap");
        //_cloudsRotationSpeed = defaultMaterial.GetFloat("_CloudsRotationSpeed");
        //_cloudsHeight = defaultMaterial.GetFloat("_CloudsHeight");
        //_cloudsIntensity = defaultMaterial.GetFloat("_CloudsIntensity");
        //_cloudsLightColor = defaultMaterial.GetColor("_CloudsLightColor");
        //_cloudsShadowColor = defaultMaterial.GetColor("_CloudsShadowColor");
        //_enableSun = defaultMaterial.GetInt("_EnableSun") == 1;
        //_sunTexture = defaultMaterial.GetTexture("_SunTexture");
        //_sunColor = defaultMaterial.GetColor("_SunColor");
        //_sunSize = defaultMaterial.GetFloat("_SunSize");
        //_sunIntensity = defaultMaterial.GetFloat("_SunIntensity");

        //_fogHeight = defaultMaterial.GetFloat("_FogHeight");
        //_fogSmoothness = defaultMaterial.GetFloat("_FogSmoothness");
        //_fogFill = defaultMaterial.GetFloat("_FogFill");

        //_skyboxOffset = defaultMaterial.GetFloat("_SkyboxOffset");
        //_skyboxRotation = defaultMaterial.GetFloat("_SkyboxRotation");

        //_environmentAmbientColor = RenderSettings.ambientLight;
        //_fogColor = RenderSettings.fogColor;

        if (RenderSettings.skybox != _skyboxMaterial)
        {
            _skyboxMaterial = new Material(RenderSettings.skybox);
            _skyboxMaterial.name += "_clone";
            RenderSettings.skybox = _skyboxMaterial;
        }

        UpdateMaterial();
    }

    private void Update()
    {
        if (_enableUpdate)
        {
            UpdateMaterial();
        }
    }

    private void OnValidate()
    {
        //if (RenderSettings.skybox != _skyboxMaterial)
        //{
        //    _skyboxMaterial = new Material(RenderSettings.skybox);
        //    _skyboxMaterial.name += "_clone";
        //    RenderSettings.skybox = _skyboxMaterial;
        //}

        UpdateMaterial();
    }

    public void UpdateMaterial()
    {
        if (_skyboxMaterial == null )
        {
            return;
        }

        _skyboxMaterial.SetColor("_SkyColor", _skyColor);
        _skyboxMaterial.SetColor("_EquatorColor", _equatorColor);
        _skyboxMaterial.SetColor("_GroundColor", _groundColor);
        _skyboxMaterial.SetFloat("_EquatorHeight", _equatorHeight);
        _skyboxMaterial.SetFloat("_EquatorSmoothness", _equatorSmoothness);

        _skyboxMaterial.SetFloat("_StarsIntensity", _starsIntensity);
        _skyboxMaterial.SetFloat("_StarsSize", _starsSize);
        _skyboxMaterial.SetFloat("_StarsSunMask", _starsSunMask);
        _skyboxMaterial.SetFloat("_TwinklingContrast", _twinklingContrast);
        _skyboxMaterial.SetFloat("_TwinklingSpeed", _twinklingSpeed);
        _skyboxMaterial.SetFloat("_StarsRotationSpeed", _starsRotationSpeed);

        _skyboxMaterial.SetTexture("_CloudsCubemap", _cloudsCubemap);
        _skyboxMaterial.SetFloat("_CloudsRotationSpeed", _cloudsRotationSpeed);
        _skyboxMaterial.SetFloat("_CloudsHeight", _cloudsHeight);
        _skyboxMaterial.SetFloat("_CloudsIntensity", _cloudsIntensity);
        _skyboxMaterial.SetColor("_CloudsLightColor", _cloudsLightColor);
        _skyboxMaterial.SetColor("_CloudsShadowColor", _cloudsShadowColor);

        _skyboxMaterial.SetFloat("_EnableSun", _enableSun ? 1 : 0);
        _skyboxMaterial.SetTexture("_SunTexture", _sunTexture);
        _skyboxMaterial.SetColor("_SunColor", _sunColor);
        _skyboxMaterial.SetFloat("_SunSize", _sunSize);
        _skyboxMaterial.SetFloat("_SunIntensity", _sunIntensity);

        _skyboxMaterial.SetFloat("_FogHeight", _fogHeight);
        _skyboxMaterial.SetFloat("_FogSmoothness", _fogSmoothness);
        _skyboxMaterial.SetFloat("_FogFill", _fogFill);

        _skyboxMaterial.SetFloat("_SkyboxOffset", _skyboxOffset);
        _skyboxMaterial.SetFloat("_SkyboxRotation", _skyboxRotation);

        RenderSettings.ambientLight = _environmentAmbientColor;
        RenderSettings.fogColor = _fogColor;        
    }
}
