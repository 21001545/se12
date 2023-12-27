using Festa.Client;
using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIMakeMomentLocationCell : MonoBehaviour, ICell
{
    [SerializeField]
    private TMP_Text txt_location;

    private PlaceData _placeData;

    private UnityAction<PlaceData> _callback;

    public void onClicked()
    {

        _callback?.Invoke(_placeData);
    }
    
    public void setLocation(PlaceData placeData, UnityAction<PlaceData> callback )
    {
        _callback = callback;
        _placeData = placeData;
        txt_location.text = placeData.getAddress();
    }
}
