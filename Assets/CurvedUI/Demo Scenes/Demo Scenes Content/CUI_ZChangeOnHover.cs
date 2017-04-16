using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CUI_ZChangeOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public float restZ = 0;
	public float OnHoverZ = -50;

	bool Zoomed = false;

	// Update is called once per frame
	void Update () {
	
		(transform as RectTransform).anchoredPosition3D = (transform as RectTransform).anchoredPosition3D.ModifyZ(Mathf.Clamp((Zoomed ? 
			(transform as RectTransform).anchoredPosition3D.z + Time.deltaTime * (OnHoverZ - restZ) * 6 :
			(transform as RectTransform).anchoredPosition3D.z - Time.deltaTime * (OnHoverZ - restZ) * 6), OnHoverZ, restZ));

	}

	public void  OnPointerEnter (PointerEventData eventData){
		Zoomed = true;
	
	}

	public void  OnPointerExit (PointerEventData eventData){

		Zoomed = false;
	}
}
