using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.UI;

#if CURVEDUI_TMP
using TMPro;
#endif 

[ExecuteInEditMode]
[CustomEditor(typeof(CurvedUISettings))]
public class CurvedUISettingsEditor : Editor {

    ////GUI
    //GUISkin editorSkin;
    //GUIStyle GUIPhoto;

    ////Editor options
    //[SerializeField] bool openFeedback = false;
    //[SerializeField] bool openAdvanced = false;

    ////internal values
    //string message = "";
  

    void Start(){
		AddCurvedUIComponents();
	}

    public override void OnInspectorGUI() {
        CurvedUISettings myTarget = (CurvedUISettings)target;


        ////load up editor skin
        //if (editorSkin == null) {
        //    editorSkin = (GUISkin)(AssetDatabase.LoadAssetAtPath("Assets/CurvedUI/Scripts/Editor/CurvedUIGUISkin.guiskin", typeof(GUISkin)));
        //    GUIPhoto = editorSkin.FindStyle("DanielPhoto");
        //}
        //GUI.skin = editorSkin;

        //initial settings
        GUI.changed = false;
        EditorGUIUtility.labelWidth = 150;

        //shape settings
        GUILayout.Label("Shape", EditorStyles.boldLabel);
        myTarget.Shape = (CurvedUISettings.CurvedUIShape)EditorGUILayout.EnumPopup("Canvas Shape", myTarget.Shape);
        switch (myTarget.Shape) {
            case CurvedUISettings.CurvedUIShape.CYLINDER: {
                myTarget.Angle = EditorGUILayout.IntSlider("Angle", myTarget.Angle, -360, 360);
                myTarget.PreserveAspect = EditorGUILayout.Toggle("Preserve Aspect", myTarget.PreserveAspect);

                break;
            }
            case CurvedUISettings.CurvedUIShape.RING: {
                myTarget.RingExternalDiameter = Mathf.Clamp(EditorGUILayout.IntField("External Diameter", myTarget.RingExternalDiameter), 1, 100000);
                myTarget.Angle = EditorGUILayout.IntSlider("Angle", myTarget.Angle, 0, 360);
                myTarget.RingFill = EditorGUILayout.Slider("Fill", myTarget.RingFill, 0.0f, 1.0f);
                myTarget.RingFlipVertical = EditorGUILayout.Toggle("Flip Canvas Vertically", myTarget.RingFlipVertical);
                break;
            }
            case CurvedUISettings.CurvedUIShape.SPHERE: {
                GUILayout.BeginHorizontal();
                GUILayout.Space(150);
                GUILayout.Label("Sphere shape is more expensive than a Cyllinder shape. Keep this in mind when working on mobile VR.", EditorStyles.helpBox);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                if (myTarget.PreserveAspect) {
                    myTarget.Angle = EditorGUILayout.IntSlider("Angle", myTarget.Angle, -360, 360);
                } else {
                    myTarget.Angle = EditorGUILayout.IntSlider("Horizontal Angle", myTarget.Angle, 0, 360);
                    myTarget.VerticalAngle = EditorGUILayout.IntSlider("Vertical Angle", myTarget.VerticalAngle, 0, 180);
                }
                myTarget.PreserveAspect = EditorGUILayout.Toggle("Preserve Aspect", myTarget.PreserveAspect);

                break;
            }
        }

       


        //advanced settings
        GUILayout.Space(10);
        GUILayout.Label("Advanced Settings", EditorStyles.boldLabel);


        //controller
        myTarget.Controller = (CurvedUISettings.CurvedUIController)EditorGUILayout.EnumPopup("Control Method", myTarget.Controller);
        GUILayout.BeginHorizontal();
        GUILayout.Space(150);
        switch (myTarget.Controller)
        {
            case CurvedUISettings.CurvedUIController.MOUSE:
            {

                GUILayout.Label("Basic Controller. Mouse in screen space.", EditorStyles.helpBox);
                break;
            }
            case CurvedUISettings.CurvedUIController.GAZE:
            {
                GUILayout.Label("Center of Canvas's World Camera acts as a pointer.", EditorStyles.helpBox);
                break;
            }
            case CurvedUISettings.CurvedUIController.WORLD_MOUSE:
            {
                GUILayout.Label("Mouse controller that is independent of the camera view. Use WorldSpaceMouseOnCanvas function to get its position.", EditorStyles.helpBox);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(150);
                myTarget.WorldSpaceMouseSensitivity = EditorGUILayout.FloatField("Mouse Sensitivity", myTarget.WorldSpaceMouseSensitivity);
                break;
            }
            case CurvedUISettings.CurvedUIController.CUSTOM_RAY:
            {
                GUILayout.Label("You can set a custom ray as a controller with CustomControllerRay function. Raycaster will use that ray to find selected objects.", EditorStyles.helpBox);
                break;
            }
        }
        GUILayout.EndHorizontal();


        myTarget.Interactable = EditorGUILayout.Toggle("Interactable", myTarget.Interactable);
        myTarget.RaycastMyLayerOnly = EditorGUILayout.Toggle("Raycast My Layer Only", myTarget.RaycastMyLayerOnly);
        myTarget.Quality = EditorGUILayout.Slider("Quality", myTarget.Quality, 0.1f, 3.0f);
        GUILayout.BeginHorizontal();
        GUILayout.Space(150);
        GUILayout.Label("Smoothness of the curve. Bigger values mean more subdivisions. Decrease for better performance. Default 1", EditorStyles.helpBox);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Components", GUILayout.Width(146));
        if (GUILayout.Button("Add Effect To Children"))
        {
            AddCurvedUIComponents();
        }
        GUILayout.EndHorizontal();

        /*
        //Feedback
        GUILayout.Space(20);
        if (GUILayout.Button(openFeedback ? "Cancel" : "Feedback & Questions")) {
            openFeedback = !openFeedback;
        if


        } (openFeedback) {
            GUILayout.BeginHorizontal();
            GUILayout.Box("", GUIPhoto, new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) });
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            message = GUILayout.TextArea(message, EditorStyles.helpBox, new GUILayoutOption[] { GUILayout.MinHeight(100) });
            //if (GUILayout.Button("Email Me", GUILayout.Height(25)));
            //{
            //    SendEmail();
            //}
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

        }*/

        //final settings
        if (GUI.changed)
			EditorUtility.SetDirty(myTarget);

	}


	void OnEnable()
	{
		EditorApplication.hierarchyWindowChanged += AddCurvedUIComponents;
	}

	void OnDisable() 
	{
		EditorApplication.hierarchyWindowChanged -= AddCurvedUIComponents;
	}

	//Travel the hierarchy and add CurvedUIVertexEffect to every gameobject that can be bent.
	private void AddCurvedUIComponents()
	{
		if(target == null)return;
		
		foreach(UnityEngine.UI.Graphic graph in ((CurvedUISettings)target).GetComponentsInChildren<UnityEngine.UI.Graphic>(true)){
			if(graph.GetComponent<CurvedUIVertexEffect>() == null){
				graph.gameObject.AddComponent<CurvedUIVertexEffect>();
				graph.SetAllDirty();
			}
		}

		//TextMeshPro experimental support. Go to CurvedUITMP.cs to learn how to enable it.
		#if CURVEDUI_TMP 
		foreach(TextMeshProUGUI tmp in ((CurvedUISettings)target).GetComponentsInChildren<TextMeshProUGUI>(true)){
			if(tmp.GetComponent<CurvedUITMP>() == null){
				tmp.gameObject.AddComponent<CurvedUITMP>();
				tmp.SetAllDirty();
			}
		}
		#endif

	}

    //yes, im going to implement contact via email straight from the editor. Im going to have best customer support you have ever seen in unity asset.
    void SendEmail()
    {
        //string email = "curvedui@chisely.com";
        //string subject = MyEscapeURL("My Subject");
        //string body = MyEscapeURL("My Body\r\nFull of non-escaped chars");
        //Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
        //Application.OpenURL("mailto:" + email);

       Application.OpenURL("https://unity3diy.blogspot.com");
    }
    string MyEscapeURL(string url)
    {
        return WWW.EscapeURL(url).Replace("+", "%20");
    }

}
