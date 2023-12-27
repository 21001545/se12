using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class UIToggle : MonoBehaviour
{
    [SerializeField]
    private bool _isOn;

    [SerializeField]
    private Animator _animator;

    public UnityEvent OnChangeValue;


    private void OnEnable()
    {
        if(_isOn)
            _animator.Play("toRight", -1, 1f);
        else
            _animator.Play("toLeft", -1, 1f);
    }

    public void set(bool on,bool noAnimation = false,bool invokeCallback = true)
    {
      //  Debug.Log($"set:on[{on}] noAnimation[{noAnimation}] invokeCallback[{invokeCallback}]");

        if( _isOn != on)
		{
            if (on)
                _animator.Play("toRight", -1, noAnimation ? 1.0f : 0f);
            else
                _animator.Play("toLeft", -1, noAnimation ? 1.0f : 0f);
        }

        _isOn = on;

        if ( invokeCallback)
		{
            OnChangeValue?.Invoke();
        }
    }

    public bool isOn()
    {
        return _isOn;
    }

    public void onClick()
    {
       // Debug.Log($"onClick:{_isOn}");
        set(!_isOn, false, true);
    }
}
