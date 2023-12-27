using Festa.Client.Module.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICustomize : UISingletonPanel<UICustomize>
{
    [SerializeField]
    private AvatarMeshCustomize _avatar = null;

    [SerializeField]
    private GameObject _faceMesh = null;

    [SerializeField]
    private GameObject _eyebrowMesh = null;

    [SerializeField]
    private GameObject _eyelashMesh = null;

    [SerializeField]
    private GameObject _eyeLeftMesh = null;

    [SerializeField]
    private GameObject _eyeRightMesh = null;

    //// 음... 별로군! Tab Component를 만들어야지!
    //[SerializeField]
    //private UnityEngine.UI.Button _faceTab = null;

    [SerializeField]
    private GameObject _faceContent = null;

    //[SerializeField]
    //private UnityEngine.UI.Button _ripTab = null;

    [SerializeField]
    private GameObject _ripContent = null;

    //[SerializeField]
    //private UnityEngine.UI.Button _eyebrowTab = null;

    [SerializeField]
    private GameObject _eyebrowContent = null;

    [SerializeField]
    private GameObject _meshContent = null;    

    // 조절해야할거
    [SerializeField]
    private UIColorPicker _faceColorPicker = null;
    
    [SerializeField]
    private UIColorPicker _blushColorPicker = null;

    [SerializeField]
    private UIColorPicker _eyebrowSkinColorPicker = null;

    [SerializeField]
    private UIColorPicker _eyebrowColorPicker = null;
    [SerializeField]
    private UIColorPicker _eyelashColorPicker = null;

    [SerializeField]
    private UIColorPicker _eyeColorPicker = null;
    
    [SerializeField]
    private UIColorPicker _ripColorPicker = null;
    
    [SerializeField]
    private Slider _ripSpecularPowerSlider = null;

    // materials...
    List<Material> _faceMaterial = new List<Material>();

    // 아직 통합 머테리얼이 없어서.. 각자 머테리얼을 사용한다... 나중에 합쳐졌으면..
    Material _eyebrowMaterial = null;
    Material _eyelashMaterial = null;
    Material _eyeLeftMaterial = null;
    Material _eyeRightMaterial = null;

    void Start()
    {
        var faceMeshRenderer = _faceMesh.GetComponent<SkinnedMeshRenderer>();
        if (faceMeshRenderer != null)
        {
            faceMeshRenderer.GetMaterials(_faceMaterial);
        }

        var eyebrowMeshRenderer = _eyebrowMesh.GetComponent<SkinnedMeshRenderer>();
        if (eyebrowMeshRenderer != null)
        {
            // 머테리얼은 일단 하나로 설계 한다.
            List<Material> materials = new List<Material>();
            eyebrowMeshRenderer.GetMaterials(materials);

            if (materials.Count > 0)
            {
                _eyebrowMaterial = materials[0];
            }
        }

        var eyelashMeshRenderer = _eyelashMesh.GetComponent<SkinnedMeshRenderer>();
        if (eyelashMeshRenderer != null)
        {
            List<Material> materials = new List<Material>();
            eyelashMeshRenderer.GetMaterials(materials);

            if (materials.Count == 2)
            {
                _eyelashMaterial = materials[1];
            }
        }

        var eyeLeftMeshRenderer = _eyeLeftMesh.GetComponent<SkinnedMeshRenderer>();
        if (eyeLeftMeshRenderer != null)
        {
            // 머테리얼은 일단 하나로 설계 한다.
            List<Material> materials = new List<Material>();
            eyeLeftMeshRenderer.GetMaterials(materials);

            if (materials.Count > 0)
            {
                _eyeLeftMaterial = materials[0];
            }
        }

        var eyeRightMeshRenderer = _eyeRightMesh.GetComponent<SkinnedMeshRenderer>();
        if (eyeRightMeshRenderer != null)
        {
            // 머테리얼은 일단 하나로 설계 한다.
            List<Material> materials = new List<Material>();
            eyeRightMeshRenderer.GetMaterials(materials);

            if (materials.Count > 0)
            {
                _eyeRightMaterial = materials[0];
            }
        }

        _faceColorPicker?.OnChangedColor.AddListener(onChangedFaceColor);
        _blushColorPicker?.OnChangedColor.AddListener(onChangedBlushColor);

        _eyebrowSkinColorPicker?.OnChangedColor.AddListener(onChangedEyebrowSkinColor);
        _eyebrowColorPicker?.OnChangedColor.AddListener(onChangedEyebrowColor);
        _eyelashColorPicker?.OnChangedColor.AddListener(onChangedEyelashColor);
        _eyeColorPicker?.OnChangedColor.AddListener(onChangedEyeColor);

        _ripColorPicker?.OnChangedColor.AddListener(onChangedRipColor);

        _ripSpecularPowerSlider?.onValueChanged.AddListener(onChangedRipSpecularPower);


        // 장비 커마 버튼 추가 해보자.
        for(int i = 0; i < _meshPrefabs.Length;++i)
        {
            GameObject mesh = _meshPrefabs[i];
            var newCell = Instantiate(_meshPrefab, _sliderList.transform);

            var text = newCell.GetComponentInChildren<TMP_Text>();
            text.text = mesh.name;

            var button = newCell.GetComponentInChildren<Button>();
            button.onClick.AddListener(() => {
                if ( mesh.name.StartsWith("top") )
                {
                    _avatar.equipMesh(Instantiate(mesh), "top");
                }
                else if (mesh.name.StartsWith("bottom"))
                {
                    _avatar.equipMesh(Instantiate(mesh), "bottom");
                }
                else if (mesh.name.StartsWith("onepiece"))
                {
                    _avatar.equipMesh(Instantiate(mesh), "onepiece");
                }
                else if (mesh.name.StartsWith("shoes"))
                {
                    _avatar.equipMesh(Instantiate(mesh), "shoes");
                }
                else if (mesh.name.StartsWith("cap"))
                {
                    _avatar.equipMesh(Instantiate(mesh), "cap");
                }
            });

        }   
        
        var count = _avatar.getBlendShapeCount();
        for (int i = 49; i < count; ++i)
        {
            var blendShapeName = _avatar.getBlendShapeName(i);

            var newCell = Instantiate(_sliderPrefab, _sliderList.transform);
            var text = newCell.transform.Find("Title").GetComponent<TMP_Text>();
            text.text = blendShapeName;
            var slider = newCell.GetComponent<Slider>();
            slider.onValueChanged.AddListener((float value) => {
                _avatar.setBlendShapeWeight(blendShapeName, value * 100.0f);
            });

            slider.value = 0.0f;

            _avatar.setBlendShapeWeight(blendShapeName, 0.0f);
        }

        foreach ( var range in _avatar.BoneScaleValueRange )
        {
            var name = range.Key;
            var valueRange = range.Value;

            var newCell = Instantiate(_sliderPrefab, _sliderList.transform);
            var text = newCell.transform.Find("Title").GetComponent<TMP_Text>();
            text.text = name + "_scale";
            var slider = newCell.GetComponent<Slider>();
            slider.onValueChanged.AddListener((float value) =>
            {
                var newValue = Vector3.Lerp(valueRange.Min, valueRange.Max, value);
                _avatar.setFaceBoneScale(name, newValue);
            });

            slider.value = (_avatar.getFaceBoneScale(name) - valueRange.Min).magnitude / (valueRange.Max - valueRange.Min).magnitude;
        }

        foreach (var range in _avatar.BoneTranslateValueRange)
        {
            var name = range.Key;
            var valueRange = range.Value;
            Slider sliderX = null;
            Slider sliderY = null;
            Slider sliderZ = null;
            {
                var newCell = Instantiate(_sliderPrefab, _sliderList.transform);
                var text = newCell.transform.Find("Title").GetComponent<TMP_Text>();
                text.text = name + "_tx";
                 sliderX = newCell.GetComponent<Slider>();
            }
            {
                var newCell = Instantiate(_sliderPrefab, _sliderList.transform);
                var text = newCell.transform.Find("Title").GetComponent<TMP_Text>();
                text.text = name + "_ty";
                 sliderY = newCell.GetComponent<Slider>();
            }
            {
                var newCell = Instantiate(_sliderPrefab, _sliderList.transform);
                var text = newCell.transform.Find("Title").GetComponent<TMP_Text>();
                text.text = name + "_tz";
                sliderZ = newCell.GetComponent<Slider>();
            }
            sliderX.value = (_avatar.getFaceBoneTranslation(name).x - valueRange.Min.x) / (valueRange.Max.x - valueRange.Min.x);
            sliderY.value = (_avatar.getFaceBoneTranslation(name).y - valueRange.Min.y) / (valueRange.Max.y - valueRange.Min.y);
            sliderZ.value = (_avatar.getFaceBoneTranslation(name).z - valueRange.Min.z) / (valueRange.Max.z - valueRange.Min.z);

            sliderX.onValueChanged.AddListener((float value) =>
            {
                var vec = _avatar.getFaceBoneTranslation(name);
                vec.x = Mathf.Lerp(valueRange.Min.x, valueRange.Max.x, sliderX.value);
                vec.y = Mathf.Lerp(valueRange.Min.y, valueRange.Max.y, sliderY.value);
                vec.z = Mathf.Lerp(valueRange.Min.z, valueRange.Max.z, sliderZ.value);
                _avatar.setFaceBoneTranslation(name, vec);
            });
            sliderY.onValueChanged.AddListener((float value) =>
            {
                var vec = _avatar.getFaceBoneTranslation(name);
                vec.x = Mathf.Lerp(valueRange.Min.x, valueRange.Max.x, sliderX.value);
                vec.y = Mathf.Lerp(valueRange.Min.y, valueRange.Max.y, sliderY.value);
                vec.z = Mathf.Lerp(valueRange.Min.z, valueRange.Max.z, sliderZ.value);
                _avatar.setFaceBoneTranslation(name, vec);
            });
            sliderZ.onValueChanged.AddListener((float value) =>
            {
                var vec = _avatar.getFaceBoneTranslation(name);
                vec.x = Mathf.Lerp(valueRange.Min.x, valueRange.Max.x, sliderX.value);
                vec.y = Mathf.Lerp(valueRange.Min.y, valueRange.Max.y, sliderY.value);
                vec.z = Mathf.Lerp(valueRange.Min.z, valueRange.Max.z, sliderZ.value);
                _avatar.setFaceBoneTranslation(name, vec);
            });
        }

        _avatar.playAnimation();
    }

    [Header("Mesh customize..")]
    [SerializeField]
    private GameObject _sliderPrefab;
    [SerializeField]
    private GameObject _meshPrefab;
    [SerializeField]
    private GameObject _sliderList;


    void onChangedFaceColor(Color color)
    {
        foreach(var material in _faceMaterial)
        {
            material.SetColor("FaceColorTone", color);
        }
    }

    void onChangedBlushColor(Color color)
    {
        foreach (var material in _faceMaterial)
        {
            material.SetColor("BlushColor", color);
        }
    }

    void onChangedEyebrowSkinColor(Color color)
    {
        foreach (var material in _faceMaterial)
        {
            material.SetColor("EyeBrowColorTone", color);
        }
    }

    void onChangedEyebrowColor(Color color)
    {
        foreach (var material in _faceMaterial)
        {
            material.SetColor("BrowColor", color);
        }
    }

    void onChangedEyelashColor(Color color)
    {
        _eyelashMaterial?.SetColor("ColorTone", color);
    }

    void onChangedEyeColor(Color color)
    {
        _eyeLeftMaterial?.SetColor("EyeColor", color);
        _eyeRightMaterial?.SetColor("EyeColor", color);
    }

    void onChangedRipColor(Color color)
    {
        foreach (var material in _faceMaterial)
        {
            material.SetColor("RipColorTone", color);
        }
    }

    void onChangedRipSpecularPower(float power)
    {
        foreach (var material in _faceMaterial)
        {
            material.SetFloat("RipSpecularPower", power);
        }
    }

    public void onClickTab(string tabName)
    {
        _faceContent.SetActive(tabName == "face");
        _ripContent.SetActive(tabName == "rip");
        _eyebrowContent.SetActive(tabName == "eyebrow");
        _meshContent.SetActive(tabName == "mesh");
    }

    #region MeshCustom
    [SerializeField]
    GameObject[] _hairPrefabs = null;

    [SerializeField]
    GameObject[] _meshPrefabs = null;
    public void onClickHair(int index)
    {
        _avatar.equipHair(Instantiate(_hairPrefabs[index]));
    }
    public void onClickAnim(string animName)
    {
        _avatar.changeAnimation(animName);
    }


    public void onClickSave()
    {
        // 일단 더미용 json 만들자.

        var path = Path.Combine(Application.temporaryCachePath, "customize.json");
        var avatar = new Festa.Client.Module.JsonObject();

        //avatar.put("Face.CheekIn", _avatar.GetBlendShapeWeight("Face.CheekIn"));
        //avatar.put("Face.CheekOut", _avatar.GetBlendShapeWeight("Face.CheekOut"));
        //avatar.put("Face.ChinThin", _avatar.GetBlendShapeWeight("Face.ChinThin"));
        //avatar.put("Face.ChinThick", _avatar.GetBlendShapeWeight("Face.ChinThick"));
        //avatar.put("Face.EarBig", _avatar.GetBlendShapeWeight("Face.EarBig"));
        //avatar.put("Face.EyeInUp", _avatar.GetBlendShapeWeight("Face.EyeInUp"));
        //avatar.put("Face.EyeInDown", _avatar.GetBlendShapeWeight("Face.EyeInDown"));
        //avatar.put("Face.EyeOutUp", _avatar.GetBlendShapeWeight("Face.EyeOutUp"));
        //avatar.put("Face.EyeOutDown", _avatar.GetBlendShapeWeight("Face.EyeOutDown"));
        //avatar.put("Face.JawBig", _avatar.GetBlendShapeWeight("Face.JawBig"));
        //avatar.put("Face.JawSmall", _avatar.GetBlendShapeWeight("Face.JawSmall"));
        //avatar.put("Face.HeadLength_short", _avatar.GetBlendShapeWeight("Face.HeadLength_short"));
        //avatar.put("Face.HeadLength_long", _avatar.GetBlendShapeWeight("Face.HeadLength_long"));
        //avatar.put("Face.LipsUpperThick", _avatar.GetBlendShapeWeight("Face.LipsUpperThick"));
        //avatar.put("Face.LipsLowerThick", _avatar.GetBlendShapeWeight("Face.LipsLowerThick"));
        //avatar.put("Face.NoseSharp", _avatar.GetBlendShapeWeight("Face.NoseSharp"));
        //avatar.put("Face.MouthOut", _avatar.GetBlendShapeWeight("Face.MouthOut"));
        //avatar.put("Face.MouthIn", _avatar.GetBlendShapeWeight("Face.MouthIn"));
        //Vector3 t = _avatar.GetFaceBoneTranslation("mouth");
        //avatar.put("mouthTranslateX", t.x);
        //avatar.put("mouthTranslateY", t.y);
        //avatar.put("mouthTranslateZ", t.z);
        //t = _avatar.GetFaceBoneScale("mouth");
        //avatar.put("mouthScaleX", t.x);
        //avatar.put("mouthScaleY", t.y);
        //avatar.put("mouthScaleZ", t.z);
        //t = _avatar.GetFaceBoneTranslation("nose");
        //avatar.put("noseTranslateX", t.x);
        //avatar.put("noseTranslateY", t.y);
        //avatar.put("noseTranslateZ", t.z);
        //t = _avatar.GetFaceBoneScale("nose");
        //avatar.put("noseScaleX", t.x);
        //avatar.put("noseScaleY", t.y);
        //avatar.put("noseScaleZ", t.z);
        //avatar.put("eye", _avatar.GetFaceBoneScale("iScale_R").x);

        Debug.Log(path);
        File.WriteAllText(path, avatar.encode());
    }

    public void onClickLoad()
    {
        // 일단 더미용 json 만들자.
        var path = Path.Combine(Application.temporaryCachePath, "customize.json");
        _avatar.loadCustomize(path);
    }
    #endregion
}
