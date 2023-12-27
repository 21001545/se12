using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ABCWalletKit.Android
{
    public class ABCWalletKitHelper
    {
        public static readonly string DEVICE = "abcwallet-sdk-service-android";
        public static AndroidJavaObject DictionaryToHashMap(IDictionary<string, string> parameters)
        {
            AndroidJavaObject javaMap = new AndroidJavaObject("java.util.HashMap");
            IntPtr putMethod = AndroidJNIHelper.GetMethodID(
                javaMap.GetRawClass(), "put",
                    "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");

            object[] args = new object[2];
            foreach (KeyValuePair<string, string> kvp in parameters)
            {

                using (AndroidJavaObject k = new AndroidJavaObject(
                    "java.lang.String", kvp.Key))
                {
                    using (AndroidJavaObject v = new AndroidJavaObject(
                        "java.lang.String", kvp.Value))
                    {
                        args[0] = k;
                        args[1] = v;
                        AndroidJNI.CallObjectMethod(javaMap.GetRawObject(),
                                putMethod, AndroidJNIHelper.CreateJNIArgArray(args));
                    }
                }
            }

            return javaMap;
        }
    }
}
