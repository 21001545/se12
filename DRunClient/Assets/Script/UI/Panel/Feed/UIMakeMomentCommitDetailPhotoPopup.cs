using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMakeMomentCommitDetailPhotoPopup : UISingletonPanel<UIMakeMomentCommitDetailPhotoPopup>
{
    [SerializeField]
    private UIMessengerPhotoDetailCell _photoCellPrefab;

    [SerializeField]
    private RectTransform rt_photoContent;

    [SerializeField]
    private UIPhotoThumbnail _currentPhoto;

    [SerializeField]
    private GameObject txt_title;

    [SerializeField]
    private GameObject txt_desc;

    [SerializeField]
    private GameObject btn_confirm;

    [SerializeField]
    private TMP_Text txt_index;


    [SerializeField]
    private GameObject go_inputLocation;

    [SerializeField]
    private GameObject go_location;

    [SerializeField]
    private TMP_Text txt_location;

    private int _totalPhotoCount;
    private int _currentPhotoIndex;

    public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
    {
        base.open(param, transitionType, closeType);

        var viewModel = ClientMain.instance.getViewModel();
        MakeMomentViewModel vm = viewModel.MakeMoment;

        txt_title.SetActive(vm.PhotoList.Count > 1);
        txt_desc.SetActive(vm.PhotoList.Count > 1);
        btn_confirm.SetActive(vm.PhotoList.Count > 1);
        txt_index.gameObject.SetActive(vm.PhotoList.Count > 1);
        rt_photoContent.gameObject.SetActive(vm.PhotoList.Count > 1);

        _totalPhotoCount = vm.PhotoList.Count;
        _currentPhotoIndex = 0;
        if (vm.PhotoList.Count > 1 )
        {
            for (int i = rt_photoContent.childCount; i < _totalPhotoCount; ++i)
            {
                Instantiate(_photoCellPrefab, rt_photoContent);
            }

            for (int i = 0; i < rt_photoContent.childCount; ++i)
            {
                if (i < _totalPhotoCount)
                {
                    var cell = rt_photoContent.GetChild(i).GetComponent<UIMessengerPhotoDetailCell>();
                    cell.gameObject.SetActive(true);

                    int cachedIndex = i;

                    if (vm.PhotoList[i].photoContext != null)
                    {
                        cell.setup(vm.PhotoList[i].photoContext, () =>
                        {
                            selectPhoto(cachedIndex);
                        });
                    }
                    else
                    {
                        cell.setup(ClientMoment.makePhotoURL(GlobalConfig.fileserver_url, vm.PhotoList[i].photoURL), () =>
                        {
                            selectPhoto(cachedIndex);
                        });
                    }
                }
                else
                {
                    rt_photoContent.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
        _currentPhotoIndex = 0;
        selectPhoto(_currentPhotoIndex);
    }

    private void selectPhoto(int index)
    {
        _currentPhotoIndex = index;

        var viewModel = ClientMain.instance.getViewModel();
        MakeMomentViewModel vm = viewModel.MakeMoment;
        if ( index == 0 )
        {
            go_inputLocation.SetActive(false);
            go_location.SetActive(false);
        }
        else
        {
            // 위치 정보가 있니?
            var placeData = vm.PhotoList[_currentPhotoIndex].placeData;
            if (placeData == null )
            {
                go_inputLocation.SetActive(true);
                go_location.SetActive(false);
            }
            else
            {
                go_inputLocation.SetActive(false);
                go_location.SetActive(true);
                var textInfo = txt_location.GetTextInfo(placeData.getAddress());

                var layoutElement = txt_location.GetComponent<LayoutElement>();
                float width = 270.0f;
                if (textInfo.lineCount > 0 )
                {
                    width = textInfo.lineInfo[0].maxAdvance > 270 ? 270 : textInfo.lineInfo[0].maxAdvance;
                }
                layoutElement.preferredWidth = width;
            }
        }
        
        txt_index.text = $"{_currentPhotoIndex + 1}/{_totalPhotoCount}";



        if (vm.PhotoList[_currentPhotoIndex].photoContext != null)
        {
            _currentPhoto.setImageFromFile(vm.PhotoList[_currentPhotoIndex].photoContext);
        }
        else
        {
            _currentPhoto.setImageFromCDN(ClientMoment.makePhotoURL(GlobalConfig.fileserver_url, vm.PhotoList[_currentPhotoIndex].photoURL));
        }

        if (rt_photoContent.gameObject.activeSelf)
        {
            for (int i = 0; i < _totalPhotoCount && i < rt_photoContent.childCount; ++i)
            {
                var cell = rt_photoContent.GetChild(i).GetComponent<UIMessengerPhotoDetailCell>();
                cell.select(i == index);
            }
        }
    }

    public void onClickRemoveLocation()
    {
        go_inputLocation.SetActive(true);
        go_location.SetActive(false);
    }

    public void onClickInputLocation()
    {
        UIMakeMomentLocation.spawn((PlaceData placeData) => {
            if (placeData == null)
                return;

            var viewModel = ClientMain.instance.getViewModel();
            MakeMomentViewModel vm = viewModel.MakeMoment;

            vm.PhotoList[_currentPhotoIndex].placeData = placeData;

            go_inputLocation.SetActive(false);
            go_location.SetActive(true);

            var textInfo = txt_location.GetTextInfo(placeData.getAddress());

            var layoutElement = txt_location.GetComponent<LayoutElement>();
            float width = 270.0f;
            if (textInfo.lineCount > 0)
            {
                width = textInfo.lineInfo[0].maxAdvance > 270 ? 270 : textInfo.lineInfo[0].maxAdvance;
            }
            layoutElement.preferredWidth = width;
        });
    }
}
