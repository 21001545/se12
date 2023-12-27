using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalleryTest : MonoBehaviour, PolyAndCode.UI.IRecyclableScrollRectDataSource
{
    [SerializeField]
    private RecyclableScrollRect _recycleScrollRect = null;

    [SerializeField]
    private Text _memoryText = null;

    [SerializeField]
    private Text _exifText = null;

    // ī�޶�� ���� �̹��� Ȯ�ο�..
    [SerializeField]
    private GalleryTestCell _cameraImage = null;

    private List<NativeGallery.NativePhotoContext> _source = new List<NativeGallery.NativePhotoContext>();

    private void Awake()
    {
        _recycleScrollRect.DataSource = this;

        var paths = NativeGallery.GetRecentImagePaths(0, 1000);
        if (paths == null)
        {
#if UNITY_EDITOR
            for(var i = 0; i < 500; ++i)
            { 
            
                _source.Add(new NativeGallery.NativePhotoContext("C:/work/LifeFesta/festa/FestaClient/FestaUnityClient/Assets/Avatar/Textures/eye_brow.png"));
            }
#endif
            return;
        }

        foreach (var path in paths)
        {
            _source.Add(path);
        }
    }

    private void Start()
    {
        if (_cameraImage != null)
        {
            _cameraImage.ExifCallback = onDisplayExif;
        }
    }

    float _deltaTime = 0.0f;
    private void Update()
    {

        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        if ( _memoryText != null )
        {
            _memoryText.text = $"Mono HeapSize : {UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong()}\n"
                + $"Mono UsedSize : {UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong()}\n"
            + $"Mono TotalAllocateMemory : {UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()}\n";
        }
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        float msec = _deltaTime * 1000.0f;
        float fps = 1.0f / _deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }

    public void onTakePickture()
    {
        var permission = NativeCamera.TakePicture((string path) =>
        {
            Debug.Log($"Take! : {path}");
            _cameraImage?.setThumbnail(new NativeGallery.NativePhotoContext(path));
            onDisplayExif(path);
        }, 1024);

    }

    void onDisplayExif(string path) {
        ExifLib.JpegInfo info = ExifLib.ExifReader.ReadJpeg(path);

        if (info == null)
        {
            _exifText.text = "���� ����";
        }
        else
        {
            var longitude = info.GpsLongitude;
            var latitude = info.GpsLatitude;
            _exifText.text = $"name : {info.FileName}\n"
                + $"path : {path}\n"
            + $"date : {info.DateTime}\n"
            + $"longitude :{(longitude != null ? longitude[0] : -1)}, {(longitude != null ? longitude[1] : -1)}\n"
            + $"latitude :{(latitude != null ? latitude[0] : -1)}, {(latitude != null ? latitude[1] : -1)}, {(latitude != null ? latitude[2] : -1)}\n";
        }
    }

    public void onGalleryPickerClick()
    {
        NativeGallery.GetImageFromGallery((string path)=>{
            Debug.Log($"Pick : {path}");
        });
    }

    #region Recycle Scroll Rect
    public int GetItemCount()
    {
        return _source.Count;
    }

    public void SetCell(ICell cell, int index)
    {
        UnityEngine.Profiling.Profiler.BeginSample("SetCell");
        var testCell = cell as GalleryTestCell;

        testCell.ExifCallback = onDisplayExif;
        testCell.setThumbnail(_source[index]);
        UnityEngine.Profiling.Profiler.EndSample();

    }
    #endregion
}
