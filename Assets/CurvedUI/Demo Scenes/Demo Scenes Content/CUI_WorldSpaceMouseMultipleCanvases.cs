using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CUI_WorldSpaceMouseMultipleCanvases : MonoBehaviour {


    [SerializeField] List<CurvedUISettings> ControlledCanvases;
    [SerializeField] Transform WorldSpaceMouse;
    [SerializeField] CurvedUISettings MouseCanvas;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 worldSpaceMousePosInWorldSpace = MouseCanvas.CanvasToCurvedCanvas(WorldSpaceMouse.localPosition);
        Ray ControllerRay = new Ray(Camera.main.transform.position, worldSpaceMousePosInWorldSpace - Camera.main.transform.position);

        foreach (CurvedUISettings set in ControlledCanvases)
        {
            set.CustomControllerRay = ControllerRay;
        }

        if (Input.GetButton("Fire2"))
        {
            Vector2 newPos = Vector2.zero;
            MouseCanvas.RaycastToCanvasSpace(new Ray(Camera.main.transform.position, Camera.main.transform.forward), out newPos);
            MouseCanvas.WorldSpaceMouseInCanvasSpace = newPos;

        }

        Debug.DrawRay(ControllerRay.GetPoint(0), ControllerRay.direction * 1000, Color.cyan);
    }
}
