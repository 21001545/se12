using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SkyBox : MonoBehaviour
{
    public class TimeType
    {
        public static int day = 0;
        public static int sunset = 1;
        public static int night = 2;
    };

    private Animator _animator;
    private int _time;
    private bool _rainning = false;

    public int GetTime() => _time;


    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetTime(int time, bool rainning)
    {
        // 타이밍 이슈가 있는듯
        if( _animator == null)
		{
            return;
		}

        _animator.SetBool("changeTime", _time != time);
        _animator.SetBool("changeRain", _rainning != rainning);

        _animator.SetInteger("prevIdle", _time);

        _time = time;
        _rainning = rainning;


        _animator.SetBool("rain", _rainning);
        _animator.SetTrigger("change");
        _animator.SetFloat("idle", _time);
    }
}
