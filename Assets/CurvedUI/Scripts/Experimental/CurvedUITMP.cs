using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

#if CURVEDUI_TMP 
using TMPro;
#endif 

//To use this class you have to add CURVEDUI_TMP to your define symbols. You can do it in project settings.
//To learn how to do it visit http://docs.unity3d.com/Manual/PlatformDependentCompilation.html and search for "Platform Custom Defines"

[ExecuteInEditMode]
public class CurvedUITMP : MonoBehaviour {

	#if CURVEDUI_TMP

	//internal
	CurvedUIVertexEffect crvdVE;
	TextMeshProUGUI tmp;
	CurvedUISettings mySettings;
	Mesh savedMesh;
	VertexHelper vh;

	Vector2 savedSize;
	Vector3 savedUp;
	Vector3 savedPos;


	public bool Dirty = false; // set this to true to force mesh update.

    bool curvingRequired = false;
    bool tesselationRequired = false;

    void DiscoverTMP()
    {
       if( this.GetComponent<TextMeshProUGUI>() != null)
        {
            tmp = this.gameObject.GetComponent<TextMeshProUGUI>();
            crvdVE = this.gameObject.GetComponent<CurvedUIVertexEffect>();
            mySettings = GetComponentInParent<CurvedUISettings>();
            transform.hasChanged = false;
        }

    }

	void OnEnable(){

        DiscoverTMP();

        if (tmp != null)
            tmp.RegisterDirtyMaterialCallback(TesselationRequiredCallback);
    }

    void OnDisable()
    {
        if (tmp != null)
            tmp.UnregisterDirtyMaterialCallback(TesselationRequiredCallback);
    }


    void TesselationRequiredCallback()
    {
        tesselationRequired = true;
        curvingRequired = true;
    }


    void LateUpdate(){

		//Edit Mesh on TextMeshPro component
		if(tmp != null){

			if(tmp.havePropertiesChanged){
                tesselationRequired = true;
               // Debug.Log("prop changed");
			}
            else if (savedSize != (transform as RectTransform).rect.size)   {
                tesselationRequired = true;
                //Debug.Log("size changed");

            }
            else if(!savedPos.AlmostEqual(mySettings.transform.worldToLocalMatrix.MultiplyPoint3x4(transform.position))){
                curvingRequired = true;
               // Debug.Log("pos changed");

            }
            else if(!savedUp.AlmostEqual(mySettings.transform.worldToLocalMatrix.MultiplyVector(transform.up))){
                curvingRequired = true;
               // Debug.Log("up changed");

            }


            if (Dirty || tesselationRequired || savedMesh == null || vh == null || (curvingRequired && !Application.isPlaying))
			{

                //Debug.Log("meshing TMP");
                tmp.renderMode = TMPro.TextRenderFlags.Render;
                tmp.ForceMeshUpdate();
                vh = new VertexHelper(tmp.mesh);
                crvdVE.TesselationRequired = true;

                #if UNITY_5_1
				crvdVE.ModifyMesh(vh.GetUIVertexStream);
                #else
                crvdVE.ModifyMesh(vh);
                #endif

                savedMesh = new Mesh();
                vh.FillMesh(savedMesh);

                tmp.renderMode = TMPro.TextRenderFlags.DontRender;

                tesselationRequired = false;
                Dirty = false;
                savedSize = (transform as RectTransform).rect.size;
                savedUp = mySettings.transform.worldToLocalMatrix.MultiplyVector(transform.up);
                savedPos = mySettings.transform.worldToLocalMatrix.MultiplyPoint3x4(transform.position);
            }


            if (curvingRequired)
            {
               // Debug.Log("curving TMP");
                crvdVE.TesselationRequired = false;
                crvdVE.CurvingRequired = true;

#if UNITY_5_1
                crvdVE.ModifyMesh(vh.GetUIVertexStream);
#else
                crvdVE.ModifyMesh(vh);
#endif

                vh.FillMesh(savedMesh);

                curvingRequired = false;
                savedSize = (transform as RectTransform).rect.size;
                savedUp = mySettings.transform.worldToLocalMatrix.MultiplyVector(transform.up);
                savedPos = mySettings.transform.worldToLocalMatrix.MultiplyPoint3x4(transform.position);
            }

			tmp.canvasRenderer.SetMesh(savedMesh);

		} else {
            DiscoverTMP();
        }
	}

	#endif
}



