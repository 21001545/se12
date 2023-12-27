using UnityEngine;
using UnityEngine.UI;
using Festa.Client;
using TMPro;

public class Bar : MonoBehaviour {

	public Image image;
	public Button button;
	public Outline outline;
	public GameObject go_profile;
	public UIPhotoThumbnail profile_tumbnail;
	public GameObject go_checkMark;

	void Start () { }

	public void SetColor (Color color) {
		image.color = color;

		if (outline != null )
        {
			outline.effectColor = new Color(color.r, color.g, color.b, 0.3f);
		}
	}
}