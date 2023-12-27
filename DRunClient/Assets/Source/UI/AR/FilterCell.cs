using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterCell : TabCell
{
    Animator _animator;
    protected override void Start()
    {
        base.Start();
    }

    public void select()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
        _animator.SetBool("select", true);
    }
    public void unselect()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
        _animator.SetBool("select", false);
    }
}
