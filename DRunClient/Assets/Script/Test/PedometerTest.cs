//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Android;
//using PedometerU;

//public class PedometerTest : MonoBehaviour
//{
//    public TMPro.TMP_Text textInfo;


//    private static string ACITIVTY_RECOGNITION = "android.permission.ACTIVITY_RECOGNITION";
//    private Pedometer stepCounter;

//    // Start is called before the first frame update
//    void Start()
//    {
//        if( Permission.HasUserAuthorizedPermission(ACITIVTY_RECOGNITION) == false)
//        {
//            Debug.Log("doesn't have permission");
//            Permission.RequestUserPermission(ACITIVTY_RECOGNITION);
//        }
//        else
//        {
//            Debug.Log("have permission");
//        }

//        stepCounter = new Pedometer(OnStep);
//    }

//    void OnStep(int steps, double distance)
//	{
//        Debug.Log(string.Format("OnStep:steps[{0}] distance[{1}]", steps, distance));

//        textInfo.text = string.Format("steps {0}\ndistance {1}", steps, distance);
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
