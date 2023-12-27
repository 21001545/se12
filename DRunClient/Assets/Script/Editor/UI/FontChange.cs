using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class FontChange : MonoBehaviour
{
    [MenuItem("Tools/Font Change")]
    public static void FindAndChangeFont()
    {
        var bold = Resources.Load<TMP_FontAsset>("Fonts & Materials/SpoqaHanSansNeo-Bold SDF");
        var light = Resources.Load<TMP_FontAsset>("Fonts & Materials/SpoqaHanSansNeo-Light SDF");
        var regular = Resources.Load<TMP_FontAsset>("Fonts & Materials/SpoqaHanSansNeo-Regular SDF");
        var Medium = Resources.Load<TMP_FontAsset>("Fonts & Materials/SpoqaHanSansNeo-Medium SDF");

        foreach (var s in Selection.gameObjects)
        {
            
            var assetPath = AssetDatabase.GetAssetPath(s);
            Debug.Log(assetPath);
            var go = PrefabUtility.LoadPrefabContents(assetPath);
            var texts = go.transform.GetComponentsInChildren<TMP_Text>(true);

            foreach( var text in texts )
            {
                if ( text == null )
                {
                    continue;
                }

                var fontName = text.font != null ? text.font.ToString() : "";
                if ( fontName.Contains("SemiBold") )
                {
                    text.font = bold;
                }
                else if (fontName.Contains("Bold"))
                {
                    text.font = bold;
                }
                else if (fontName.Contains("ExtraBold"))
                {
                    text.font = bold;
                }
                else if (fontName.Contains("ExtraLight"))
                {
                    text.font = light;
                }
                else if (fontName.Contains("Light"))
                {
                    text.font = light;
                }
                else if (fontName.Contains("Medium"))
                {
                    text.font = Medium;
                }
                else if (fontName.Contains("Regular"))
                {
                    text.font = regular;
                }
                else
                {
                    Debug.Log($"{fontName} -> Regular�� ��ü");
                    text.font = regular;
                }
            }

            var inputFields = go.transform.GetComponentsInChildren<TMP_InputField>(true);

            foreach (var text in inputFields)
            {
                if (text == null)
                {
                    continue;
                }

                var fontName = text.fontAsset != null ? text.fontAsset.ToString() : "";
                if (fontName.Contains("SemiBold"))
                {
                    text.fontAsset = bold;
                }
                else if (fontName.Contains("Bold"))
                {
                    text.fontAsset = bold;
                }
                else if (fontName.Contains("ExtraBold"))
                {
                    text.fontAsset = bold;
                }
                else if (fontName.Contains("ExtraLight"))
                {
                    text.fontAsset = light;
                }
                else if (fontName.Contains("Light"))
                {
                    text.fontAsset = light;
                }
                else if (fontName.Contains("Medium"))
                {
                    text.fontAsset = Medium;
                }
                else if (fontName.Contains("Regular"))
                {
                    text.fontAsset = regular;
                }
                else
                {
                    Debug.Log($"{fontName} -> Regular�� ��ü");
                    text.fontAsset = regular;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(go, assetPath);
            PrefabUtility.UnloadPrefabContents(go);
        }
    }

    [MenuItem("Tools/Find InputFIeld(hideSoftKeyboard)")]
    public static void FindInputField_hideSoftKeyboard()
    {
        foreach (var s in Selection.gameObjects)
        {
            try
            {
                var assetPath = AssetDatabase.GetAssetPath(s);
                var go = PrefabUtility.LoadPrefabContents(assetPath);
                var inputFields = go.transform.GetComponentsInChildren<TMP_InputField>(true);

                foreach (var text in inputFields)
                {
                    if (text == null)
                    {
                        continue;
                    }

                    if (text.shouldHideSoftKeyboard)
                    {
                        Debug.Log($"{s.name} - {text.name} should hide soft keyboard");
                        text.shouldHideSoftKeyboard = false;
                        text.shouldHideMobileInput = true;
                        PrefabUtility.SaveAsPrefabAsset(go, assetPath);
                        PrefabUtility.UnloadPrefabContents(go);
                    }
                }

            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
