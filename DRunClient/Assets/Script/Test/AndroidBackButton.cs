using UnityEngine;

public class AndroidBackButton : MonoBehaviour
{
    /// 필요한 것 같아서 일단 넣어온 것
    /// 인풋모듈에 넣어서 만들면 될 것 같은데,,

    void Update ()
    {
        #if UNITY_ANDROID 
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit(); 
        #endif
    }

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
