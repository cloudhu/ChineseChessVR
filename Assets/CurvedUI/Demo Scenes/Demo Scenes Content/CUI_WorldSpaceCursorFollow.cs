using UnityEngine;
using System.Collections;

public class CUI_WorldSpaceCursorFollow : MonoBehaviour {

    CurvedUISettings mySettings;

	// Use this for initialization
	void Start () {
        mySettings = GetComponentInParent<CurvedUISettings>();
        mySettings.WorldSpaceMouseInCanvasSpace -= (mySettings.transform as RectTransform).rect.size / 2.0f;
    }
	
	// Update is called once per frame
	void Update () {
        transform.localPosition = mySettings.WorldSpaceMouseInCanvasSpace;
	}
}
