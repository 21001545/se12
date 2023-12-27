using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;

public class GoogleMapTest : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public RawImage map_image;
    public double Longitude = 37.50634312319362;
    public double Latitude = 127.05441805010621;

    private RectTransform _rt;

    private static string API_KEY = "AIzaSyC_S2FvrIIFhIT1-JE1C0-nhrAfYtr2VH8";
    private static string QUERY_URL = "https://maps.googleapis.com/maps/api/staticmap";

    private bool needUpdate;

    void Awake()
	{
        _rt = transform as RectTransform;
	}

    void Start()
    {
        StartCoroutine(updateImage(Longitude, Latitude));

        
    }

    void Update()
    {
    }


    IEnumerator updateImage(double lg,double lt)
	{
        Vector3[] corners = new Vector3[4];
        map_image.rectTransform.GetLocalCorners(corners);

        float controlWidth = Mathf.Abs(corners[2].x - corners[0].x);
        float controlHeight = Mathf.Abs(corners[2].y - corners[0].y);
        float aspect = controlHeight / controlWidth;
        int queryMapSizeX = 500;
        int queryMapSizeY = (int)(queryMapSizeX * aspect);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(QUERY_URL);
        sb.AppendFormat("?center={0},{1}", lg, lt);
        sb.AppendFormat("&size={0}x{1}", queryMapSizeX, queryMapSizeY);
        sb.Append("&maptype=roadmap");
        sb.Append("&format=jpg");
        sb.Append("&zoom=17&style=feature:poi|visibility:off");
        sb.AppendFormat("&key={0}", API_KEY);

        string url = sb.ToString();

        Debug.Log(url);

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        yield return request.SendWebRequest();
        if( request.result != UnityWebRequest.Result.Success)
		{
            Debug.Log(request.error);
		}
        else
		{
            Texture myTexture = DownloadHandlerTexture.GetContent(request);
            map_image.texture = myTexture;
            map_image.rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    Vector2 lastDragPos;

	public void OnBeginDrag(PointerEventData eventData)
	{
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, eventData.position, eventData.pressEventCamera, out lastDragPos);
    }

	public void OnEndDrag(PointerEventData eventData)
	{
        Vector2 dragPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, eventData.position, eventData.pressEventCamera, out dragPos);

        //Longitude
       // StartCoroutine(updateImage(Longitude, Latitude));
    }

    public void OnDrag(PointerEventData eventData)
	{
        Vector2 dragPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, eventData.position, eventData.pressEventCamera, out dragPos);

        Vector2 delta = dragPos - lastDragPos;

        map_image.rectTransform.anchoredPosition += delta;

        lastDragPos = dragPos;
    }
}
