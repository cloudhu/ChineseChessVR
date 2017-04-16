//CurvedUI version 1.4.000 - release

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof(Canvas))]
public class CurvedUISettings : MonoBehaviour {

	public enum CurvedUIShape{
		CYLINDER = 0,
		RING = 1,
		SPHERE = 2
	}

    public enum CurvedUIController
    {
        MOUSE = 0,
        GAZE = 1,
        WORLD_MOUSE = 2,
        CUSTOM_RAY = 3,
    }
			
	//Global settings
	[SerializeField] CurvedUIShape shape;
    [SerializeField] CurvedUIController controller;
    [SerializeField] float quality = 1f;
	[SerializeField] bool interactable = true;
    [SerializeField] bool raycastMyLayerOnly = false;

    //Cyllinder
    [SerializeField] int angle = 90;
	[SerializeField] bool preserveAspect = true;

	//Sphere settings
	[SerializeField] int vertAngle = 90;

	//ring settings
	[SerializeField] float ringFill = 0.5f;
	[SerializeField] int ringExternalDiamater = 1000;
	[SerializeField] bool ringFlipVertical = false;

	//internal system settings
	int baseCircleSegments = 24;

    Vector2 savedRectSize;
    float savedRadius;
    Canvas myCanvas;

    Ray customControllerRay;
    Vector3 lastMouseOnScreenPos = Vector2.zero;
    Vector2 worldSpaceMouseInCanvasSpace = Vector2.zero;
    Vector2 lastWorldSpaceMouseOnCanvas = Vector2.zero;
    Vector2 worldSpaceMouseOnCanvasDelta = Vector2.zero;
    float worldSpaceMouseSensitivity = 1;

    #region LIFECYCLE
    void Start() {

        if (Application.isPlaying)
        {     //lets get rid of any raycasters and add our custom one
            GraphicRaycaster castie = GetComponent<GraphicRaycaster>();

            if (castie != null)
            {
                if (!(castie is CurvedUIRaycaster))
                {
                    Destroy(castie);
                    this.gameObject.AddComponent<CurvedUIRaycaster>();
                }
            }
            else {
                this.gameObject.AddComponent<CurvedUIRaycaster>();
            }
        }

        if (myCanvas == null)
            myCanvas = GetComponent<Canvas>();

        savedRadius = GetCyllinderRadiusInCanvasSpace();
    }

	void OnEnable(){
		foreach(UnityEngine.UI.Graphic graph in (this).GetComponentsInChildren<UnityEngine.UI.Graphic>()){
			graph.SetAllDirty();
		}
	}

	void OnDisable(){
		foreach(UnityEngine.UI.Graphic graph in (this).GetComponentsInChildren<UnityEngine.UI.Graphic>()){
			graph.SetAllDirty();
		}
	}

    void Update() {

        //recreate the geometry if entire canvas has been changed
        if((transform as RectTransform).rect.size != savedRectSize)
        {
            savedRectSize = (transform as RectTransform).rect.size;
            SetUIAngle(angle);
        }

        //moving the world space mouse
        if(Controller == CurvedUIController.WORLD_MOUSE)
        {
            //touch can also be used to control a world space mouse, although its probably not the best experience
            //Use standard mouse controller with touch.
            if(Input.touchCount > 0)
            {
                worldSpaceMouseOnCanvasDelta = Input.GetTouch(0).deltaPosition * worldSpaceMouseSensitivity;
            }  else  {
                worldSpaceMouseOnCanvasDelta = new Vector2((Input.mousePosition - lastMouseOnScreenPos).x, (Input.mousePosition - lastMouseOnScreenPos).y) * worldSpaceMouseSensitivity;
                lastMouseOnScreenPos = Input.mousePosition;
            }
            lastWorldSpaceMouseOnCanvas = worldSpaceMouseInCanvasSpace;
            worldSpaceMouseInCanvasSpace += worldSpaceMouseOnCanvasDelta;

           // Debug.Log("mouse canvas pos: " + worldSpaceMouseOnCanvas);
        }

    }
	
	#endregion 


	#region PRIVATE

	void SetUIAngle(int newAngle){

        if (myCanvas == null)
            myCanvas = GetComponent<Canvas>();

		angle =  newAngle;

        savedRadius = GetCyllinderRadiusInCanvasSpace();

        foreach (CurvedUIVertexEffect ve in GetComponentsInChildren<CurvedUIVertexEffect>())
			ve.TesselationRequired = true;

		foreach(Graphic graph in GetComponentsInChildren<Graphic>())
			graph.SetVerticesDirty();

		if(Application.isPlaying && GetComponent<CurvedUIRaycaster>() != null)
			//tell raycaster to update its collider now that angle has changed.
			GetComponent<CurvedUIRaycaster>().RebuildCollider();
	}

    Vector3 CanvasToCyllinder(Vector3 pos)
    {
        float theta = (pos.x / savedRectSize.x) * Angle * Mathf.Deg2Rad;
        pos.x = Mathf.Sin(theta) * (SavedRadius + pos.z);
        pos.z += Mathf.Cos(theta) * (SavedRadius + pos.z) - (SavedRadius + pos.z);

        return pos;
    }

    Vector3 CanvasToRing(Vector3 pos)
    {
        float r = pos.y.Remap(savedRectSize.y * 0.5f * (RingFlipVertical ? 1 : -1), -savedRectSize.y * 0.5f * (RingFlipVertical ? 1 : -1), RingExternalDiameter * (1 - RingFill) * 0.5f, RingExternalDiameter * 0.5f);
        float theta = (pos.x / savedRectSize.x).Remap(-0.5f, 0.5f, Mathf.PI / 2.0f, angle * Mathf.Deg2Rad + Mathf.PI / 2.0f);
        pos.x = r * Mathf.Cos(theta);
        pos.y = r * Mathf.Sin(theta);

        return pos;
    }

    Vector3 CanvasToSphere(Vector3 pos)
    {
        float radius = SavedRadius;
        float vAngle = VerticalAngle;

        if (PreserveAspect)
        {
            vAngle = angle * (savedRectSize.y / savedRectSize.x);
            radius += Angle > 0 ? -pos.z : pos.z;
        }  else {
            radius = savedRectSize.x / 2.0f + pos.z;
            if (vAngle == 0) return Vector3.zero;
        }

        //convert planar coordinates to spherical coordinates
        float theta = (pos.x / savedRectSize.x).Remap(-0.5f, 0.5f, (180 - angle) / 2.0f - 90, 180 - (180 - angle) / 2.0f - 90);
        theta *= Mathf.Deg2Rad;
        float gamma = (pos.y / savedRectSize.y).Remap(-0.5f, 0.5f, (180 - vAngle) / 2.0f, 180 - (180 - vAngle) / 2.0f);
        gamma *= Mathf.Deg2Rad;

        pos.z = Mathf.Sin(gamma) * Mathf.Cos(theta) * radius;
        pos.y = -radius * Mathf.Cos(gamma);
        pos.x = Mathf.Sin(gamma) * Mathf.Sin(theta) * radius;

        if (PreserveAspect)
            pos.z -= radius;

        return pos;
    }
    #endregion

 

    #region PUBLIC

    /// <summary>
    /// When in CUSTOM_RAY controller mode, RayCaster will use this worldspace Ray to determine which Canvas objects are being selected.
    /// </summary>
    public Ray CustomControllerRay {
        get{ return customControllerRay; }
        set{ customControllerRay = value;
            if (Controller != CurvedUIController.CUSTOM_RAY)
                Debug.LogWarning("A custom ray has been supplied, but CurvedUI canvas is not set to Custom Ray mode.");
        }
    }

    public Vector2 WorldSpaceMouseInCanvasSpace {
        get { return worldSpaceMouseInCanvasSpace; }
        set { worldSpaceMouseInCanvasSpace = value;
            lastWorldSpaceMouseOnCanvas = value;
        }
    }

    public Vector2 WorldSpaceMouseInCanvasSpaceDelta {
        get { return worldSpaceMouseInCanvasSpace - lastWorldSpaceMouseOnCanvas; }
    }

    /// <summary>
    /// How many units in Canvas space equals one unit in screen space.
    /// </summary>
    public float WorldSpaceMouseSensitivity {
        get  { return worldSpaceMouseSensitivity;  }
        set  {   worldSpaceMouseSensitivity = value;  }
    }



    /// <summary>
    /// Maps a world space vector to a curved canvas. 
    /// Operates in Canvas's local space.
    /// </summary>
    /// <param name="pos">World space vector</param>
    /// <returns>
    /// A vector on curved canvas in canvas's local space
    /// </returns>
    public Vector3 VertexPositionToCurvedCanvas(Vector3 pos)
    {
        switch (Shape)
        {
            case CurvedUISettings.CurvedUIShape.CYLINDER:
            {
                return CanvasToCyllinder(pos);
            }
            case CurvedUISettings.CurvedUIShape.RING:
            {
                return CanvasToRing(pos);
            }
            case CurvedUISettings.CurvedUIShape.SPHERE:
            {
                return CanvasToSphere(pos);
            }
            default:
            {
                return Vector3.zero;
            }
        }
        
    }

    /// <summary>
    /// Converts a point in Canvas space to a point on Curved surface in world space. \n
    /// </summary>
    /// <param name="pos">Position on canvas in canvas space</param>
    /// <returns>
    /// Position on curved canvas in world space.
    /// </returns>
    public Vector3 CanvasToCurvedCanvas(Vector3 pos) 
    {
        pos = VertexPositionToCurvedCanvas(pos);
        if (float.IsNaN(pos.x) || float.IsInfinity(pos.x)) return Vector3.zero;
        else return transform.localToWorldMatrix.MultiplyPoint3x4(pos);
    }

    public Vector3 CanvasToCurvedCanvasNormal(Vector3 pos)
    {
        //find the position in canvas space
        pos = VertexPositionToCurvedCanvas(pos);

        switch (Shape)
        {
            case CurvedUISettings.CurvedUIShape.CYLINDER:
            {
                // find the direction to the center of cyllinder on flat XZ plane
                return transform.localToWorldMatrix.MultiplyVector((pos - new Vector3(0, 0, -GetCyllinderRadiusInCanvasSpace())).ModifyY(0)).normalized;
            }
            case CurvedUISettings.CurvedUIShape.RING:
            {
                // just return the back direction of the canvas
                return -transform.forward;
            }
            case CurvedUISettings.CurvedUIShape.SPHERE:
            {
                //return the direction towards the sphere's center
                Vector3 center = (PreserveAspect ? new Vector3(0, 0, -GetCyllinderRadiusInCanvasSpace()) : Vector3.zero);
                return transform.localToWorldMatrix.MultiplyVector((pos - center)).normalized;
            }
            default:
            {
                return Vector3.zero;
            }
        }
    }

    /// <summary>
    /// Raycasts along the given ray and returns the intersection with the flat canvas. 
    /// Use to convert from world space to flat canvas space. 
    /// </summary>
    /// <param name="ray"></param>
    /// <returns>
    /// Returns the true if ray hits the CurvedCanvas.
    /// Outputs intersection point in flat canvas space. 
    /// </returns>
    public bool RaycastToCanvasSpace(Ray ray, out Vector2 o_positionOnCanvas)
    {
        CurvedUIRaycaster caster = this.GetComponent<CurvedUIRaycaster>();
        o_positionOnCanvas = Vector2.zero;

        switch (Shape)
        {
            case CurvedUISettings.CurvedUIShape.CYLINDER:
            {
                return caster.RaycastToCyllinderCanvas(ray, out o_positionOnCanvas, true);
            }
            case CurvedUISettings.CurvedUIShape.RING:
            {
                return caster.RaycastToRingCanvas(ray, out o_positionOnCanvas, true);
            }
            case CurvedUISettings.CurvedUIShape.SPHERE:
            {
                return caster.RaycastToSphereCanvas(ray, out o_positionOnCanvas, true);
            }
            default:
            {
                return false;
            }
        }
    }

	/// <summary>
	/// Returns the radius of curved canvas cyllinder, expressed in Cavas's local space units.
	/// </summary>
	public float GetCyllinderRadiusInCanvasSpace(){
		float ret;
		if(PreserveAspect){
			ret = ((transform as RectTransform).rect.size.x / ((2 * Mathf.PI) * (angle / 360.0f)));
		} else 
			ret = ((transform as RectTransform).rect.size.x * 0.5f) / Mathf.Sin( Mathf.Clamp(angle,-180.0f, 180.0f) * 0.5f * Mathf.Deg2Rad);
		
		return angle == 0? 0 : ret;
	}


	/// <summary>
	/// Tells you how big UI quads can get before they should be tesselate to look good on current canvas settings.
	/// Used by CurvedUIVertexEffect
	/// </summary>
	/// <returns>The tesslation size.</returns>
	/// 
	/// TODO: Make tesselation size increase in logarythmic fashion
	/// 
	public Vector2 GetTesslationSize(bool UnmodifiedByQuality = false){

		float ret, ret2;

		Vector2 canvasSize = GetComponent<RectTransform>().rect.size;

		ret = canvasSize.x;
		ret2 = canvasSize.y;

		if(Angle != 0 || (!PreserveAspect && vertAngle != 0)){

			switch(shape){

				case CurvedUIShape.CYLINDER:{
					
					ret = Mathf.Min(canvasSize.x / 4,  canvasSize.x / (Mathf.Abs(angle).Remap(0.0f, 360.0f) * baseCircleSegments ));
					ret2 = Mathf.Min(canvasSize.y / 4, canvasSize.y / (Mathf.Abs(angle).Remap(0.0f, 360.0f) * baseCircleSegments ));
					break;

				} case CurvedUIShape.RING:{

					goto case CurvedUIShape.CYLINDER;
						
				} case CurvedUIShape.SPHERE: {
					
					ret = Mathf.Min(canvasSize.x / 4, canvasSize.x / (Mathf.Abs(angle).Remap(0.0f, 360.0f) * baseCircleSegments * 0.5f));

					if(PreserveAspect){
						ret2 = ret * canvasSize.y / canvasSize.x;
					} else {
						ret2 = VerticalAngle == 0 ? 10000 : canvasSize.y / (Mathf.Abs(VerticalAngle).Remap(0.0f, 180.0f) * baseCircleSegments * 0.5f);
					}
					
					break;
				}	

			}
		}

		return new Vector2(ret, ret2) / (UnmodifiedByQuality ? 1 : Mathf.Clamp(Quality, 0.01f, 10.0f));

	}


	/// <summary>
	/// Tells you how many segmetens should the entire 360 deg. cyllinder consist of.
	/// Used by CurvedUIVertexEffect
	/// </summary>
	/// <value>The base circle segments.</value>
	public int BaseCircleSegments {
		get { return baseCircleSegments;
		}
	}

	/// <summary>
	/// The measure of the arc of the Canvas.
	/// </summary>
	/// <value>The angle.</value>
	public int Angle{
		get {return angle;}
		set { if(angle != value)
				SetUIAngle(value);}

	}

	public float Quality{
		get {return quality;}
		set {
            if (quality != value) {
                quality = value;
                SetUIAngle(angle);
            }   
        }
	}

	public CurvedUIShape Shape {
		get { return shape; }
		set { if(shape != value){
				shape = value; 
				SetUIAngle(angle);
			}
		}
	}


	public int VerticalAngle {
		get { return vertAngle; }
		set { 
			if(vertAngle != value){
				vertAngle = value;
				SetUIAngle(angle);
			}
		}
	}

	public float RingFill {
		get { return ringFill; }
		set { 
			if(ringFill != value){
				ringFill = value;
				SetUIAngle(angle);
			}
		}
	}

    float SavedRadius {
        get {
            if(savedRadius == 0)
                savedRadius = GetCyllinderRadiusInCanvasSpace();

                return savedRadius;
        }
       
    }

    public int RingExternalDiameter {
		get { return ringExternalDiamater; }
		set {if(ringExternalDiamater != value){ 
				ringExternalDiamater = value;
				SetUIAngle(angle);
			}
		}
	}

	public bool RingFlipVertical {
		get { return ringFlipVertical; }
		set { if(ringFlipVertical != value){ 
				ringFlipVertical = value;
				SetUIAngle(angle);
			}
		}
	}

	public bool PreserveAspect {
		get { return preserveAspect; }
		set { if(preserveAspect != value){ 
				preserveAspect = value;
				SetUIAngle(angle);
			}
		}
	}

	public bool Interactable {
		get { return interactable; }
		set { if(interactable != value){ 
				interactable = value;

				if(Application.isPlaying && GetComponent<CurvedUIRaycaster>() != null)
					//tell raycaster to update its collider now that angle has changed.
					GetComponent<CurvedUIRaycaster>().RebuildCollider();
			}
		}
	}


    public CurvedUIController Controller {
        get { return controller; }
        set
        {
            if (controller != value)
            {
                controller = value;
                SetUIAngle(angle);
            }
        }
    }

    public bool RaycastMyLayerOnly {
        get  { return raycastMyLayerOnly;  }
        set  {  raycastMyLayerOnly = value; }
    }


    #endregion
}
