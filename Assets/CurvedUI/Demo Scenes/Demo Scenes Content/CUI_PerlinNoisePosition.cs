using UnityEngine;
using System.Collections;

public class CUI_PerlinNoisePosition : MonoBehaviour {

	public float samplingSpeed = 1;

	RectTransform rectie;
	RectTransform parentie;

	// Use this for initialization
	void Start () {
		rectie = transform as RectTransform;
		parentie = transform.parent as RectTransform;
	}

	// Update is called once per frame
	void Update () {
		rectie.anchoredPosition = new Vector2(Mathf.PerlinNoise(Time.time * samplingSpeed, Time.time * samplingSpeed).Remap(0,1, 0, parentie.rect.width),
			Mathf.PerlinNoise(Time.time * samplingSpeed * 1.333f, Time.time * samplingSpeed * 0.888f).Remap(0,1, 0.3f, parentie.rect.height));
	}
}
