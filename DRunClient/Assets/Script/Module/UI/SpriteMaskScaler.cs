using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(SpriteMask))]
public class SpriteMaskScaler : MonoBehaviour
{
	RectTransform _rtParent;
	RectTransform _rt;
	float _referencePixelsPerUnit;
	Rect _spriteRect;

	private void Awake()
	{
		_rt = transform as RectTransform;
		_rtParent = _rt.parent as RectTransform;

		Canvas c = GetComponentInParent<Canvas>();
		_referencePixelsPerUnit = c.referencePixelsPerUnit;

		SpriteMask sm = GetComponent<SpriteMask>();
		_spriteRect = sm.sprite.rect;

		fitToParent();
	}

	void fitToParent()
	{
		Vector3[] corners = new Vector3[4];
		_rt.GetLocalCorners( corners);

		float sizeX = corners[2].x - corners[0].x;
		float sizeY = corners[2].y - corners[0].y;

		float scaleX = sizeX * _referencePixelsPerUnit / _spriteRect.width;
		float scaleY = sizeY * _referencePixelsPerUnit / _spriteRect.height;

		_rt.localScale = new Vector3(scaleX, scaleY, 1.0f);
	}
}
