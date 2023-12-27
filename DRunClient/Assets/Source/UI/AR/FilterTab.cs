//using DG.Tweening;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//[System.Serializable]
//public struct FilterParameter
//{
//    [SerializeField]
//    public string _name;

//    [SerializeField]
//    public float _defaultValue;

//    [SerializeField]
//    public float _minValue;

//    [SerializeField]
//    public float _maxValue;
//}

//[System.Serializable]
//public struct FilterParameters
//{
//    [SerializeField]
//    public List<FilterParameter> _parameters;
//}

//public class FilterTab : Tab
//{
//    [SerializeField]a
//    protected List<FilterParameters> _filterParameters = new List<FilterParameters>();

//    [SerializeField]
//    private CameraBackground _cameraBackground;

//    [SerializeField]
//    private RectTransform _content;

//    [SerializeField]
//    private RectTransform rt_parameterContent;

//    [SerializeField]
//    private GameObject go_sliderParameterPrefab;

//    public void setCameraBackground(CameraBackground cameraBackground)
//    {
//        _cameraBackground = cameraBackground;
//    }

//    protected override void onTabClicked(TabCell cell, int index)
//    {
//        //base.onTabClicked(cell, index);
//        _cameraBackground?.setFilterMode(index);

//        var filterCell = cell as FilterCell;

//        for (var i = 0; i < _contents.Count; ++i)
//        {
//            if ( _contents[i]._cell != cell)
//            {
//                (_contents[i]._cell as FilterCell)?.unselect();
//            }
//        }

//        // 파라미터를 구성하자.
//        if (_filterParameters.Count > index && _filterParameters[index]._parameters.Count > 0)
//        {
//            rt_parameterContent.gameObject.SetActive(true);
//            foreach ( Transform child in rt_parameterContent.transform )
//            {
//                Destroy(child.gameObject);
//            }

//            foreach (FilterParameter parameter in _filterParameters[index]._parameters)
//            {
//                var slider = Instantiate(go_sliderParameterPrefab, rt_parameterContent).GetComponent<Slider>();
//                if ( slider != null )
//                {
//                    slider.minValue = parameter._minValue;
//                    slider.maxValue = parameter._maxValue;
//                    slider.value = parameter._defaultValue;
//                    string name = parameter._name;
//                    slider.onValueChanged.AddListener((value) => {
//                        _cameraBackground.setFilterParameter(name, value);
//                    });
//                }

//            }
//        }
//        else
//        {
//            // 파라미터 가려
//            rt_parameterContent.gameObject.SetActive(false);
//        }

//        filterCell?.select();

//        // 정중앙으로 Tab을 옮기자.
//        if (_content != null )
//            DOTween.To(() => _content.localPosition, x => _content.localPosition = x, new Vector3(-index * 68, _content.localPosition.y, _content.localPosition.z), 0.5f);
//    }
//}
