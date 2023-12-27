//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.Android;

//public class uGUIMapTest : MonoBehaviour
//{
//    public RectTransform mapControl;
//    public Slider mapSize;

//	public OnlineMaps onlineMaps;
//	public TMPro.TMP_Text txtZoom;
//	public TMPro.TMP_Text txtLocation;

//	public GameObject markerPrefab;
//	public MarkerData[] markers;
//	public RectTransform maker_container;
//	public OnlineMapsControlBase control;

//	private List<MarkerInstance> markerList = new List<MarkerInstance>();

//	[System.Serializable]
//	public class MarkerData
//	{
//		public string title;
//		public double logitude;
//		public double latitude;
//	}

//	public class MarkerInstance
//	{
//		public MarkerData data;
//		public GameObject gameObject;
//		public RectTransform transform;
//	}

//	private void Start()
//	{
//		if( Permission.HasUserAuthorizedPermission(Permission.FineLocation) == false)
//		{
//			Permission.RequestUserPermission(Permission.FineLocation);
//		}
		
//		prepareMarkers();
//		StartCoroutine(_UpdateZoom());

//		if( Application.isEditor == false)
//		{
//			StartCoroutine(_LocationService());
//		}
//	}

//	public void onSliderChanged(float size)
//	{
//        mapControl.sizeDelta = new Vector2(size, size);
//	}

//	IEnumerator _UpdateZoom()
//	{
//		while(true)
//		{
//			txtZoom.text = onlineMaps.floatZoom.ToString();

//			yield return new WaitForSeconds(0.1f);
//		}
//	}

//	void prepareMarkers()
//	{
//		onlineMaps.OnMapUpdated += updateMarkers;
//		//OnlineMapsCameraOrbit.instance.OnCameraControl += updateMarkers;

//		foreach(MarkerData data in markers)
//		{
//			GameObject markerGameObject = Instantiate(markerPrefab, maker_container, false) as GameObject;
//			RectTransform rectTransform = markerGameObject.transform as RectTransform;
//			rectTransform.localScale = Vector3.one;
//			MarkerInstance marker = new MarkerInstance();
//			marker.data = data;
//			marker.gameObject = markerGameObject;
//			marker.transform = rectTransform;

//			markerList.Add(marker);
//		}

//		updateMarkers();
//	}

//	void updateMarkers()
//	{
//		foreach(MarkerInstance marker in markerList)
//		{
//			updateMarker(marker);
//		}
//	}

//	void updateMarker(MarkerInstance marker)
//	{
//		double px = marker.data.logitude;
//		double py = marker.data.latitude;

//		Vector2 screenPosition = control.GetScreenPosition(px, py);
//		if (screenPosition.x < 0 || screenPosition.x > Screen.width ||
//			screenPosition.y < 0 || screenPosition.y > Screen.height)
//		{
//			marker.gameObject.SetActive(false);
//			return;
//		}

//		RectTransform markerRectTransform = marker.transform;

//		if (!marker.gameObject.activeSelf) marker.gameObject.SetActive(true);

//		Vector2 point;
//		RectTransformUtility.ScreenPointToLocalPointInRectangle(markerRectTransform.parent as RectTransform, screenPosition, Camera.main, out point);
//		markerRectTransform.localPosition = point;
//	}

//	IEnumerator _LocationService()
//	{
//		txtLocation.text = "init..";

//		if (!Input.location.isEnabledByUser)
//		{
//			Debug.Log("User has not enabled location");
//			yield break;
//		}

//		Input.location.Start();
//		yield return new WaitWhile(() => { return Input.location.status == LocationServiceStatus.Initializing; });
	
//		if(Input.location.status == LocationServiceStatus.Failed)
//		{
//			Debug.LogError("Unable to determine device location");
//			yield break;
//		}

//		while(true)
//		{
//			double lat = Input.location.lastData.latitude;
//			double lng = Input.location.lastData.longitude;

//			markerList[0].data.logitude = lng;
//			markerList[0].data.latitude = lat;

//			updateMarker(markerList[0]);

//			txtLocation.text = string.Format("{0},{1}", lat, lng);

//			yield return new WaitForSeconds(5.0f);
//		}
//	}
//}
