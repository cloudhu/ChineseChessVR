using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {

	public enum HoldType
    {
        Hilt, Pinch, Sphere, Gun, Misc
    }


    public HoldType Size;
    public Vector3 heldPosition;
	public Vector3 leftHeldPosition;
	public Vector3 heldRotation;
	public Vector3 leftHeldRotation;
	public float grabRange = .2f;
	public bool hoverOverGrabPoint = true;
	public bool isBeingHeld;

    public Rigidbody rby;

    public Transform holdPoint;
    //degree of squeezing
    public float squeeze;
}
