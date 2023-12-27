using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventCallback : MonoBehaviour
{
    public Action<int> AnimationFinishIntCallback;
    public Action<string> AnimationFinishStringCallback;
    public Action AnimationFinishVoidCallback;

    public void onAnimationFinishInt(int value)
    {
        AnimationFinishIntCallback?.Invoke(value);
    }

    public void onAnimationFinishString(string value)
    {
        AnimationFinishStringCallback?.Invoke(value);
    }


    public void onAnimationFinish()
    {
        AnimationFinishVoidCallback?.Invoke();
    }
}
