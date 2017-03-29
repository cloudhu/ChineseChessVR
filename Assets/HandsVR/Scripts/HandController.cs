using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/***Places to set prefferencial bounds and setting adjustments (ctr+F these terms)

		*	(VRInput) This is where the input is set, Change these for Occulus support
		*	(CustomMapping) These are the buttons used for controller input, change for custom controlls
		*	(HoverToSpeed) This is how fast the hand will hover to the target location, the higher the faster.
		*	(SnapFollowDistance) This is how far the hand will have to be from the controller before it will stop Lerping to it and just snap-follow it, The higher the more range it will snap from.
		*	(StickyObjecs) This is used for sticking the hover animation to the object slightly longer then the normal grabrange would allow, used to stop hands from chopping to and from objects when on the border.
				larger numbers will make the hand stick for longer.
		*	(HoverResetSpeed) This is how fast the object has to be moving to correct its hover too location to account for its movement. Raising it will increase optimization when hovering to moving objects.
		*	(ThrowableObject) This is the logic that throws objects, remove for optimizing
		*	(GrabRangeChecker) Used on an object to find a good grab range for it
		*	(AngleHiltRange) Used to set how forgiving a hit is with the hands orientation for pickup, closer to 0 the less range it will accept
		*	(AngleCheckerSphere) Used for any non Hilt item to find a good angle range for it
			*	Will show a (1) after the number, the closer the used number is to 1 the harder an object will be to grab, and closer to 0 the easier	
			*	These check the diffrence in Normals between a pretend Sphere around the object and the hand. 
		*	(ForceSnapAngle) This is a function used to Snap the hand to the Perfect angle wile grabbing a hilt object
			*	Can be commented out if you want to allow a variance in the way objects are actualy held
					**NOTE** does not affect ability to actualy pick up objects

	**Console logs usefull when setting up, can be commented in (ctr+F (SetUpLogs))**
	**Public variables and functions used for ease of setting up objecs, comment in for setup (ctr+F (SetUpHelpers))
	
*/
public class HandController : MonoBehaviour {



	public GameObject ViveControllerRoot;		//(VRInput)										

    //Reference to Input events for controllers
	private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;				//(VRInput)
	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;//(VRInput)

    //Public variables we can watch that show us the final result of input
	private bool gripButtonDown;
    private bool triggerButtonDown;
    private bool triggerButtonUp;


    //Use this to get consistent reference to This joystick controller thing
	private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }	//(VRInput)
	private SteamVR_TrackedObject trackedObj;																			//(VRInput)

    public Animator animHand;

	public HandController otherHand;
	public Transform emptyPrefab;
	public Transform controllerGrabPoint;
    public Transform modelGrabPoint;
	public Transform handOffset;

	List<Pickup> nearby = new List<Pickup>();
   
	private Pickup currentClosest;		
	private Pickup hoverObj;				
	private Pickup heldObj;	
   
	public bool isLeftHand;

//	public float angleOfX;			//(SetUpHelpers)
//	public float angleOfY;			//(SetUpHelpers)
//	public float angleOfZ;			//(SetUpHelpers)
//	public float grabDistance;		//(SetUpHelpers)
//	public float normalOfSphere;	//(SetUpHelpers)

	private bool needReset;
	private bool isFollowingController;
	private bool isLerping;
	private bool holdingObj;
	private bool useTrigger;

    private float grabToHoldDistance;

	private Vector3 handVelocity;		//(ThrowableObject)
	private Vector3 handLastFrame;		//(ThrowableObject)
	private float lastFrameTime;		//(ThrowableObject)
	private float thisFrameTime;		//(ThrowableObject)


	void Start() {
        if(ViveControllerRoot != null){
			trackedObj = ViveControllerRoot.GetComponent<SteamVR_TrackedObject>();		//(VRInput)
        }
		useTrigger = false;
		needReset = true;
		isLerping = false;
        handOffset.parent = null;
		isFollowingController = true;
	}

    void Update(){
//		GetClick();							//(SetUpHelpers)
//        if (ViveControllerRoot != null){
            GetVrInput();
//        }
		if (isFollowingController){
			FollowController ();
		}
//		if (holdingObj == true){						//(SetUpHelpers)
//			animHand.SetFloat("Squeeze", heldObj.squeeze);
//        }
//        
    }
    

	void FixedUpdate(){
		if (useTrigger == true){
			CoreTriggerLogic ();
		}

		if (holdingObj){						//(ThrowableObject)
			lastFrameTime = thisFrameTime;
			thisFrameTime += Time.deltaTime;
			handVelocity = (transform.position - handLastFrame) / (thisFrameTime - lastFrameTime);
			handLastFrame = transform.position;
		}
    }

	void OnTriggerEnter(Collider obj){						// Whenever a new object hits the trigger, it adds it to a nearby list
		if(obj.gameObject.GetComponent<Pickup>() != null){
			nearby.Add(obj.gameObject.GetComponent<Pickup>());
		}
		if (currentClosest == null) {							// if no current closest innitializes some variables
			useTrigger = true;
			currentClosest = obj.GetComponent<Pickup>();
			animHand.SetBool (currentClosest.Size.ToString(), true);
			animHand.SetBool ("Prepped", true);
		}
	}
    void OnTriggerExit(Collider obj){						// Removes the object from nearby list as it exits trigger
		if(obj.gameObject.GetComponent<Pickup>() != null){
			nearby.RemoveAt (nearby.IndexOf (obj.gameObject.GetComponent<Pickup> ()));
		}
		if(nearby.Count == 0){
			ResetDefault();
			ResetAnimator ();
		}
	}


//	void GetClick(){															//(SetUpHelpers)
//		if(Input.GetAxis("Fire1") == 1 && !holdingObj && (nearby.Count != 0)){
//			if (!isFollowingController) {
//				Debug.Log ("Picking up");
//				PickItUp ();
//			}
//		}
//		if (Input.GetAxis("Fire2") == 1 && holdingObj){
//			DropIt();
//		}
//	}

    void GetVrInput(){
		gripButtonDown = controller.GetPressDown(gripButton);		//(VRInput)
		triggerButtonDown = controller.GetPressDown(triggerButton);	//(VRInput)
		triggerButtonUp = controller.GetPressUp(triggerButton);		//(VRInput)

		if (triggerButtonUp/*CustomMapping*/){					//used to unclench fist if nothing in hand
            animHand.SetBool("FailGrab", false);
        }

		if (triggerButtonDown && !holdingObj && (hoverObj != null)){	//button used to pick up objects
            if (!isFollowingController || hoverObj.isBeingHeld){
                Debug.Log("Picking up");
                PickItUp();
            }
            else {
                animHand.SetTrigger("FailReach");
            }
        }
        else if(nearby.Count != 0){
            animHand.SetBool("FailGrab", false);
        }
		else if(triggerButtonDown){					//button used to pick up objects, if none near will make clenched fist
            animHand.SetBool("FailGrab", true);
        }

		if (gripButtonDown && holdingObj){			//button used to drop objects
            DropIt();
        }
    }

	void CoreTriggerLogic(){				// Core logic used for setting a prep position and hovering a hand to the holdpoint
		GetClosest();							// Itterates through the nearby object list to find the closest
		if (currentClosest.tag == "PickUp"){	// If the closest is a Pickupable, then the program will procide
			grabToHoldDistance = (currentClosest.holdPoint.position - controllerGrabPoint.position).magnitude;								//used for finding range away from the grabpoint an object is
//			grabDistance =  grabToHoldDistance;		//(SetUpHelpers)
			animHand.SetFloat("Closeness", NormalizeDistance((currentClosest.holdPoint.position - modelGrabPoint.position).magnitude));		//used for setting the prepped squeez value
			if (CanGrab ()) {		//Check to see if an object is in range and if it has the correct angle to grab
				needReset = false;				
				isFollowingController = false;
				if (currentClosest.isBeingHeld){
					hoverObj = currentClosest;
					isFollowingController = true;
				}
				else if (!isLerping && currentClosest != hoverObj) {		//isLerping is used in the Corutine HoverHand as to not prematurely interupt  the hand hovering to the holdpoint
					hoverObj = currentClosest;
					SnapHoverRotation (hoverObj.Size.ToString());		// (ForceSnapAngle)
					StartCoroutine (HoverHand (handOffset.position,hoverObj.holdPoint.position - GetOffSet(), 6/*HoverToSpeed*/)); //the call that hovers the hand to the hoverpoint
				}
				else if(isLerping && currentClosest != hoverObj){	// used if a new closest is found that doesn't break the CanGrab check
					needReset = true;				
					isFollowingController = true;
				}
			}
			else if (grabToHoldDistance > (currentClosest.grabRange * 1.15/*StickyObjecs*/) && currentClosest == hoverObj) {	//Remove this else if statement to remove sticky objects
				needReset = true;				
				isFollowingController = true;
			}
			else{
				if (!isFollowingController){
					needReset = true;				
				}
				isFollowingController = true;
			}
		}
	}  

	//This routine picks up the object and parents it to the handmesh grabpoint
    void PickItUp() {
		if (hoverObj.isBeingHeld){
			otherHand.DropIt ();
		}
		SetPickup ();
		heldObj.isBeingHeld = true;
        animHand.SetBool("Grab", true);
        animHand.SetFloat("Squeeze", currentClosest.squeeze);

        currentClosest.rby.useGravity = false;
        currentClosest.rby.isKinematic = true;
		this.GetComponent<Collider>().enabled = false;

		holdingObj = true;
        ResetDefault();
    }
    
	//Drops the object and resumes the hands colider to sence for more objects
    public void DropIt(){
		ResetAnimator ();
        animHand.SetBool("Grab", false);
		heldObj.transform.parent = null;
		
		heldObj.rby.useGravity = true;
		heldObj.rby.isKinematic = false;
		this.GetComponent<Collider>().enabled = true;
		
		heldObj.rby.velocity = handVelocity;			//(ThrowableObject)
		heldObj.isBeingHeld = false;

		holdingObj = false;
		needReset = true;
		isFollowingController = true;
    }

	void FollowController(){
		if (needReset){		// Used to re-innitialize some variables, also lerps to the controller untill a satisfactory distance is met to hard set the location
			isLerping = false;
			hoverObj = null;
			handOffset.position = Vector3.Lerp (handOffset.position, this.transform.position, Time.deltaTime * 15);
            handOffset.rotation = this.transform.rotation;
			if((handOffset.position - this.transform.position).magnitude < .02f/*SnapFollowDistance*/){
				needReset = false;
			}
		}
		else{				//standard routine for having the hand follow the controller
            handOffset.position = transform.position;
			handOffset.rotation = transform.rotation;
        }
	}
	
	IEnumerator HoverHand(Vector3 start, Vector3 target, float overTime){		// function used to hover the hand to the holdpoint over multible frames
		isLerping = true;
		float startTime = Time.time;
		while(Time.time < startTime + overTime){	
			if (needReset || isFollowingController) {
				break;
			}
            handOffset.position = Vector3.Lerp(start, target, (Time.time - startTime) * overTime);
			if (hoverObj.rby.velocity.magnitude> .01f/*HoverResetSpeed*/){		// this line is for if an object is moving it will find the new location to hover to
				target = hoverObj.holdPoint.position - GetOffSet();
			}
			yield return null;
		}
		isLerping = false;
	}

	void SetPickup(){		// used in picking up objects to set its position and rotation
		heldObj = hoverObj;
		handOffset.rotation = transform.rotation;
		handOffset.position = heldObj.holdPoint.position - GetOffSet();
		if (heldObj.heldRotation != new Vector3(0, 0, 0)){
			if (isLeftHand) {
				Transform tempTranny = Instantiate (emptyPrefab, heldObj.holdPoint.position, heldObj.transform.rotation) as Transform;
				heldObj.transform.parent = tempTranny;
				tempTranny.parent = modelGrabPoint;
				tempTranny.localEulerAngles = heldObj.leftHeldRotation;
				heldObj.transform.parent = modelGrabPoint;
				Destroy (tempTranny.gameObject);
			}
			else{
				Transform tempTranny = Instantiate(emptyPrefab, heldObj.holdPoint.position, heldObj.transform.rotation) as Transform;
				heldObj.transform.parent = tempTranny;
				tempTranny.parent = modelGrabPoint;
				tempTranny.localEulerAngles = heldObj.heldRotation;
				heldObj.transform.parent = modelGrabPoint;
				Destroy (tempTranny.gameObject);
			}

		}
		else{
			heldObj.transform.parent = modelGrabPoint;
		}
	}

	float NormalizeDistance(float dist){    //set  0 - 1 scale to control default to prep animation
                                            //Also contrain squeeze value here!
		dist =(Mathf.Clamp((.0009f / (Mathf.Pow(dist / 2f, 2.4f))) + .15f, 0f, 1f)) ;
		return dist;
	}

	void ResetDefault(){				//used to hard reset most variables
		thisFrameTime = 0;					//(ThrowableObject)
		currentClosest = null;
		isFollowingController = true;
		needReset = true;
		useTrigger = false;
		hoverObj = null;
		nearby.Clear ();
	} 

	void ResetAnimator(){			//used to hard reset most animations
		animHand.SetBool("Prepped", false);
        animHand.SetBool("Hilt", false);
        animHand.SetBool("Gun", false);
        animHand.SetBool("Sphere", false);
        animHand.SetBool("Pinch", false);
        animHand.SetBool("Misc", false);

    }

    void GetClosest(){		//used to find the nearest grab point
		foreach (var nearbyObj in nearby) {
			float tempDistance = (nearbyObj.holdPoint.position - controllerGrabPoint.position).magnitude;
			if(tempDistance < grabToHoldDistance){
				animHand.SetBool(currentClosest.Size.ToString(), false);
				currentClosest = nearbyObj;
				animHand.SetBool(currentClosest.Size.ToString(), true);
			}
		}
    }

	//This is used to asses weather the object is in grabrange
	bool CanGrab(){
		if (grabToHoldDistance < currentClosest.grabRange){		//(GrabRangeChecker)
			if(AnglesMatch(currentClosest.Size.ToString())){
				return true;
			}
//			else{							//(SetUpLogs)
//				Debug.Log ("Wrong angle");
//			}
        }
//		else{								//(SetUpLogs)
//			Debug.Log ("Too far away");	
//        }
		return false;
	}

	// used to Check weather the angles match based on type of object based on Pickups Holdtype enum on the object
	bool AnglesMatch(string holdType){
		if (holdType == "Hilt" || holdType == "Gun") {
			int angleRange = 20/*(AngleHiltRange)*/;
			float angleZ = Mathf.Abs ((currentClosest.holdPoint.eulerAngles.z - transform.eulerAngles.z));
			float angleY = Mathf.Abs ((currentClosest.holdPoint.eulerAngles.y - transform.eulerAngles.y));
//			angleOfY = angleY;		//(SetUpHelpers)
//			angleOfZ = angleZ;		//(SetUpHelpers)
			if (((180 - angleRange) < angleY && angleY < (180 + angleRange)) || ((360 - angleRange) < angleY || angleY < (0 + angleRange))) {		//	Rotates around an axis going through the palm
				if (((90 - angleRange) < angleZ && angleZ < (90 + angleRange)) || ((270 - angleRange) < angleZ && angleZ < (270 + angleRange))) {	//	Rotates around an axis going through the Middle finger twards the wrist
					return true;
				} 
//				else {														//(SetUpLogs)
//					Debug.Log ("angle Z needs to be close to 270 or 90");		
//				}
			} 
//			else {															//(SetUpLogs)
//				Debug.Log ("angle Y needs to be between 180 or 360/0");		
//			}
			return false;

		}
        else if (holdType == "Sphere") {
			Vector3 centerOfSphere = currentClosest.holdPoint.position;
			Vector3 placementPosition = controllerGrabPoint.position;
			float normal = Vector3.Dot ((placementPosition - centerOfSphere).normalized, transform.up);
//			normalOfSphere = normal;		//(SetUpHelpers)
			if (normal> 0.9f/*(1)*/){		// (AngleCheckerSphere)
				return true;
			}
		} 
		else {		// "misc" and "pinch" can be setup just like sphere, but are mostly used for animation purposes, therefor not warrenting their own type of pickup
			Vector3 centerOfSphere = currentClosest.holdPoint.position;
			Vector3 placementPosition = controllerGrabPoint.position;
			float normal = Vector3.Dot ((placementPosition - centerOfSphere).normalized, transform.up);
//			normalOfSphere = normal;		//(SetUpHelpers)
			if (normal> 0.5f/*(1)*/) {	// (AngleCheckerSphere)		**Wide angle can be used here as its more of a check to see if the hand is facing the object**
				return true;
			}
		}
		return false;
	}

	//This function will correct any offsets wile hovering to and picking up a hilt object
	void SnapHoverRotation(string holdType){				//(ForceSnapAngle)
		if (holdType == "Hilt"){
			float angleZ = Mathf.Abs((currentClosest.holdPoint.eulerAngles.z - transform.eulerAngles.z));
			float angleY = Mathf.Abs((currentClosest.holdPoint.eulerAngles.y - transform.eulerAngles.y));
			Vector3 targetRotation = handOffset.eulerAngles;
			if (160 < angleY && angleY < 200){
				angleY -= 180;
				angleY /= 2;
				targetRotation.y += angleY;
			}
			else if(340 < angleY){
				angleY -= 360;
				angleY /= 2;
				targetRotation.y += angleY;
			}
			else if (angleY < 20){
				angleY /= 2;
				targetRotation.y += angleY;
			}
			if (250 < angleZ && angleZ < 290) {
				angleZ -= 270;
				angleZ /= 2;
				targetRotation.z += angleZ;
			}
			else if(70 < angleZ && angleZ < 110) {
				angleZ -= 90;
				angleZ /= 2;
				targetRotation.z += angleZ;
			} 
			handOffset.eulerAngles = targetRotation;
		}
	}

	Vector3 GetOffSet(){		//used to find any offset between the controller, grab point, and holdpoint
		Vector3 modelGrabPointOffSet;
		Vector3 holdPointOffSet;
		modelGrabPointOffSet = modelGrabPoint.position - handOffset.position;
		holdPointOffSet = hoverObj.leftHeldPosition;
		if (hoverObj.hoverOverGrabPoint && heldObj == null){			
			holdPointOffSet = new Vector3 (0, 0, 0);
		}
		else if (isLeftHand) {
            holdPointOffSet = hoverObj.leftHeldPosition;
            holdPointOffSet = transform.rotation * holdPointOffSet;
			holdPointOffSet = Vector3.Scale (handOffset.localScale, holdPointOffSet);
		} 
		else{
			holdPointOffSet = hoverObj.heldPosition;
			holdPointOffSet = transform.rotation * holdPointOffSet;
			holdPointOffSet = Vector3.Scale (handOffset.localScale, holdPointOffSet);
		}
		return holdPointOffSet + modelGrabPointOffSet;
	}
}
