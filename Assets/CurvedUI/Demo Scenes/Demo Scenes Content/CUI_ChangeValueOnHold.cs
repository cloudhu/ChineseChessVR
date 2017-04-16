using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CUI_ChangeValueOnHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler   {

	bool pressed = false;

	[SerializeField] Image bg;
	[SerializeField] Color SelectedColor;
	[SerializeField] Color NormalColor ;

	[SerializeField] CanvasGroup IntroCG;
	[SerializeField] CanvasGroup MenuCG;

	// Update is called once per frame
	void Update () {
		ChangeVal();

		if(Input.GetButtonDown("Jump")){
			pressed = true;
		}

		if(Input.GetButtonUp("Jump")){
			pressed = false;
		}
	}

	void ChangeVal(){

		if(this.GetComponent<Slider>().normalizedValue == 1){
			IntroCG.alpha -=  Time.deltaTime;
			MenuCG.alpha +=  Time.deltaTime;
		} else {
			this.GetComponent<Slider>().normalizedValue += pressed ?  Time.deltaTime :  -Time.deltaTime;
		}

		if (IntroCG.alpha > 0) {

			IntroCG.blocksRaycasts = true;
		} else {
			IntroCG.blocksRaycasts = false;
		}
	}

	public void OnPointerDown(PointerEventData data){
		pressed = true;
	}

	public void OnPointerUp(PointerEventData data){
		pressed = false;
	}

	public void OnPointerEnter(PointerEventData data){
        pressed = true;
		bg.color = SelectedColor;
		bg.GetComponent<CurvedUIVertexEffect>().TesselationRequired = true;
	}

	public void OnPointerExit(PointerEventData data){
        pressed = false;
		bg.color = NormalColor;
		bg.GetComponent<CurvedUIVertexEffect>().TesselationRequired = true;
	}

//	public void OnSubmit(BaseEventData data){
//		pressed = true;
//	}
}
