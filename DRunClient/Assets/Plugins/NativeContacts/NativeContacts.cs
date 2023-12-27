using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Threading;
using System.Threading.Tasks;
using AOT;

namespace NativeContacts
{
    public class NativeContactContext
    {
        public string phoneNumber;
        public string name;

        public NativeContactContext(string phoneNumber, string name)
        {
            this.phoneNumber = phoneNumber;
            this.name = name;
        }
    }

    public static class NativeContacts
    {
#if !UNITY_EDITOR && UNITY_IOS
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern int _NativeContacts_CheckPermission();
    
	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern string _NativeContacts_GetContacts();

	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern int _NativeContacts_OpenSMS(string phoneNumber, string message);
#endif


        public static List<NativeContactContext> GetContacts()
        {
            List<NativeContactContext> resultList = new List<NativeContactContext>();
#if UNITY_EDITOR
            resultList.Add(new NativeContactContext("+8201044387414", "박신우"));
#elif UNITY_IOS
        string contacts = _NativeContacts_GetContacts();
        if (contacts != null)
        {
            string[] result = contacts.Split('>');
            resultList.Capacity = result.Length / 2;
            for (int i = 0; i < result.Length; i += 2)
            {
                resultList.Add(new NativeContactContext(result[i], result[i + 1]));
            }
        }
#endif
            return resultList;
        }

        // 따로 뭐 다른곳에 만들지 않고.. 여기서 해보자.
        public static bool OpenSMS(string phoneNumber, string message)
        {
            int result = 0;

#if UNITY_EDITOR
            // 할 수 있는게 없군.
#elif UNITY_IOS
            result = _NativeContacts_OpenSMS(phoneNumber, message);
#endif
            return result == 1;
        }
    }
}