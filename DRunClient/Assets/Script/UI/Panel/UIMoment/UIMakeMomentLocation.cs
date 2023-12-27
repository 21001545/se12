using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Festa.Client
{
    public class UIMakeMomentLocation : UIPanel, IRecyclableScrollRectDataSource
    {
        [SerializeField]
        private TMP_InputField input_location;

        [SerializeField]
        private GameObject btn_cancel;

        [SerializeField]
        private RecyclableScrollRect scroll_rect;

        private string prev_search_location = "";

        // abort 시키기 위하여, 
        private UnityWebRequest query_request = null;
        private Coroutine cachedSearchCorotine = null;

        //private List<string> search_location_result = null;
        private List<PlaceData> search_location_result;

        // 위치 선택 여부
        private UnityAction<PlaceData> _closeCallback = null;

        public void setup(UnityAction<PlaceData> closeCallback = null)
        {
            input_location.text = "";
            prev_search_location = "";
            scroll_rect.DataSource = this;
            search_location_result = new List<PlaceData>();
            scroll_rect.ReloadData();

            input_location.onSubmit.AddListener(onClickSearch);
            _closeCallback = closeCallback;
        }

        public static UIMakeMomentLocation spawn(UnityAction<PlaceData> closeCallback)
        {
            UIMakeMomentLocation popup = UIManager.getInstance().spawnInstantPanel<UIMakeMomentLocation>();
            popup.setup(closeCallback);
            return popup;
        }

        public override void onTransitionEvent(int type)
        {
            base.onTransitionEvent(type);
            if ( type == TransitionEventType.end_open)
            {
                input_location.ActivateInputField();
                input_location.onDeselect.AddListener((string v) => {
                    Debug.Log($"Location deselect : {v}");
                    });
            }
        }

        public void onValueChanged(string value)
        {
            btn_cancel.SetActive(value.Length > 0);
        }

        public void onClickCancelInput()
        {
            btn_cancel.SetActive(false);
            input_location.text = "";
        }

        public override void close(int transitionType = 0)
        {
            _closeCallback?.Invoke(null);
            base.close(transitionType);
        }

        public int GetItemCount()
        {
            return search_location_result.Count;
        }

        public void SetCell(ICell cell, int index)
        {
            var locationCell = cell as UIMakeMomentLocationCell;
            locationCell.setLocation(search_location_result[index], (PlaceData placeData) => {
                _closeCallback?.Invoke(placeData);
                base.close(0);
            });
        }

        public void onClickSearch(string value)
        {
            if (string.IsNullOrEmpty(input_location.text) || prev_search_location == input_location.text )
            {
                // 검색하지 맙시다.
                return;
            }

            prev_search_location = input_location.text;

            if ( cachedSearchCorotine != null )
            {
                query_request?.Abort();
                StopCoroutine(cachedSearchCorotine);
                cachedSearchCorotine = null;
            }

            StartCoroutine(SearchCoroutine());
        }

        // LocationManager에서도 요 API들을 사용하는 것같은데.. 
        // 내 위치 정보 조회만을 담당하는 것 같으므로, 그냥 여기서 별도로 구현하자.
        private IEnumerator SearchCoroutine()
        {
            //string query = $"https://nominatim.openstreetmap.org/search.php?q={prev_search_location}&format=jsonv2";

            string lang = "en";
            int type = GlobalRefDataContainer.getStringCollection().getCurrentLangType();
            
            // swpark 이거 LanguageType에 함수 만들면 안될까?
            if (type == LanguageType.ko)
                lang = "ko";

            string query = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={prev_search_location}&key={"AIzaSyC_S2FvrIIFhIT1-JE1C0-nhrAfYtr2VH8"}&language={lang}";
            Debug.Log($"위치 검색 시작 {prev_search_location}");
            using (query_request = UnityWebRequest.Get(query))
            {
                yield return query_request.SendWebRequest();

                search_location_result.Clear();

                if (query_request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(query_request.error);
                }
                else
                {
                    try
                    {
                        JsonObject json = new JsonObject(query_request.downloadHandler.text);

                        Debug.Log(json.encode());

                        var list = json.getJsonArray("results");                        
                        for( int i = 0; i < list.size(); ++i )
                        {
                            var data = list.getJsonObject(i);

                            if( string.IsNullOrEmpty(data.getString("name")))
							{
                                continue;
							}

                            PlaceData placeData = PlaceData.fromGooglePlace(data, type);
                            search_location_result.Add(placeData);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                scroll_rect.ReloadData();
            }
            query_request = null;
            cachedSearchCorotine = null;
        }
    }
}