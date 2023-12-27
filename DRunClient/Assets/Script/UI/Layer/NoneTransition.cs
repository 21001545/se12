using Festa.Client.Module.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoneTransition : AbstractPanelTransition
{
    private ITransitionEventHandler _eventHandler;
    private bool _isClosing;
    private float _closeWaitTime;

    public override void init(ITransitionEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public override float getDuration()
    {
        return 0f;
    }
    public override bool isActive()
    {
        return false;
    }

    public override float startClose()
    {
        transform.SetAsFirstSibling();
        gameObject.SetActive(false);
        _eventHandler.onTransitionEvent(TransitionEventType.start_close);
        _eventHandler.onTransitionEvent(TransitionEventType.end_close);
        return 0.0f;
    }

    public override float startOpen()
    {
        transform.SetAsLastSibling();
        _isClosing = false;
        gameObject.SetActive(true);
        _eventHandler.onTransitionEvent(TransitionEventType.start_open);
        _eventHandler.onTransitionEvent(TransitionEventType.end_open);
        return 0.0f;
    }

    public override float openImmediately()
    {
        transform.SetAsFirstSibling();
        _isClosing = false;
        gameObject.SetActive(true);
        _eventHandler.onTransitionEvent(TransitionEventType.start_open);
        _eventHandler.onTransitionEvent(TransitionEventType.end_open);
        return 0.0f;
    }

    public override float closeImmediately(float duration)
    {
        transform.SetAsFirstSibling();
        _closeWaitTime = duration;
        _isClosing = true;
        return 0f;
    }

    public override void update()
    {
        if(_isClosing)
        {
            if(_closeWaitTime <= 0f)
            {
                startClose();
                _isClosing = false;
            }
            else
            {
                _closeWaitTime -= Time.deltaTime;
            }
        }
    }

}
