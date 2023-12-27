using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.ViewModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMomentPostingCell : EnhancedScrollerCellView
{
    [SerializeField]
    private UIPhotoThumbnail photo;

    [SerializeField]
    private Slider slider_progress;
    private JobProgressItemViewModel _model;
    public void setup(JobProgressItemViewModel model)
    {
        _model = model;

        if( model.Photo != null)
		{
            photo.setImageFromFile(model.Photo.path);
		}
    }

    public void Update()
    {
        if (_model != null )
            slider_progress.value = _model.Progress * 0.01f;
    }
}
