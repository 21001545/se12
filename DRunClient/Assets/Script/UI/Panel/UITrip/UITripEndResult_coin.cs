using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using Festa.Client;
using UnityEngine;
using TMPro;

public class UITripEndResult_coin : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_normalCoin;
    [SerializeField]
    private TMP_Text txt_bonusCoin;
    [SerializeField]
    private TMP_Text txt_commitCoin;

    public void setup(int normal, int bonus)
    {
        txt_normalCoin.text = normal.ToString("N0");
        txt_bonusCoin.text = bonus.ToString("N0");
        txt_commitCoin.text = GlobalRefDataContainer.getStringCollection().getFormat("triproute.result.coin.getCoin", 0, normal + bonus);
    }
}
