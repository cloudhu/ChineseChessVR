// --------------------------------------------------------------------------------------------------------------------
// <copyright file=AnimatedFlag.cs company=League of HTC Vive Dev>
/*
11111111111111111111111111111111111111001111111111111111111111111
11111111111111111111111111111111111100011111111111111111111111111
11111111111111111111111111111111100001111111111111111111111111111
11111111111111111111111111111110000111111111111111111111111111111
11111111111111111111111111111000000111111111111111111111111111111
11111111111111111111111111100000011110001100000000000000011111111
11111111111111111100000000000000000000000000000000011111111111111
11111111111111110111000000000000000000000000000011111111111111111
11111111111111111111111000000000000000000000000000000000111111111
11111111111111111110000000000000000000000000000000111111111111111
11111111111111111100011100000000000000000000000000000111111111111
11111111111111100000110000000000011000000000000000000011111111111
11111111111111000000000000000100111100000000000001100000111111111
11111111110000000000000000001110111110000000000000111000011111111
11111111000000000000000000011111111100000000000000011110001111111
11111110000000011111111111111111111100000000000000001111100111111
11111111000001111111111111111111110000000000000000001111111111111
11111111110111111111111111111100000000000000000000000111111111111
11111111111111110000000000000000000000000000000000000111111111111
11111111111111111100000000000000000000000000001100000111111111111
11111111111111000000000000000000000000000000111100000111111111111
11111111111000000000000000000000000000000001111110000111111111111
11111111100000000000000000000000000000001111111110000111111111111
11111110000000000000000000000000000000111111111110000111111111111
11111100000000000000000001110000001111111111111110001111111111111
11111000000000000000011111111111111111111111111110011111111111111
11110000000000000001111111111111111100111111111111111111111111111
11100000000000000011111111111111111111100001111111111111111111111
11100000000001000111111111111111111111111000001111111111111111111
11000000000001100111111111111111111111111110000000111111111111111
11000000000000111011111111111100011111000011100000001111111111111
11000000000000011111111111111111000111110000000000000011111111111
11000000000000000011111111111111000000000000000000000000111111111
11001000000000000000001111111110000000000000000000000000001111111
11100110000000000001111111110000000000000000111000000000000111111
11110110000000000000000000000000000000000111111111110000000011111
11111110000000000000000000000000000000001111111111111100000001111
11111110000010000000000000000001100000000111011111111110000001111
11111111000111110000000000000111110000000000111111111110110000111
11111110001111111100010000000001111100000111111111111111110000111
11111110001111111111111110000000111111100000000111111111111000111
11111111001111111111111111111000000111111111111111111111111100011
11111111101111111111111111111110000111111111111111111111111001111
11111111111111111111111111111110001111111111111111111111100111111
11111111111111111111111111111111001111111111111111111111001111111
11111111111111111111111111111111100111111111111111111111111111111
11111111111111111111111111111111110111111111111111111111111111111
*/
//   
// </copyright>
// <summary>
//  ChineseChessVR
// </summary>
// <author>胡良云（CloudHu）</author>
//中文注释：胡良云（CloudHu） 4/18/2017
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FileName: AnimatedFlag.cs
/// Author: 胡良云（CloudHu）
/// Corporation: 
/// Description: 飘动的旗帜
/// DateTime: 4/18/2017
/// </summary>
public class AnimatedFlag : MonoBehaviour {
	
	#region Public Variables  //公共变量区域
	public MeshFilter[] meshCache;
	public Transform meshContainerFBX;
	public float playSpeed=24f;
	public float playSpeedRandom=0f;
	public bool randomSpeedLoop;
	public float updateInterval=0.05f;
	public bool randomRotateX, randomRotateY, randomRotateZ;
	public bool randomStartFrame=true;
	public bool randomRotateLoop,pingPong;
	public bool playOnAwake=true;
	public bool loop=true;
	public Vector2 randomStartDelay=Vector2.zero;

	public float delta;
	public static float updateSeed;

	#endregion


	#region Private Variables   //私有变量区域
	private MeshFilter meshFilter;
	private MeshRenderer rendererComponent;
	private Transform meshCached;
	private float currentSpeed;
	private float currentFrame;
	private int meshCacheCount;
	private float startDelay, startDelayCounter;
	private bool pingPongToggle;
	private Transform transformCache;
	#endregion
	
	
	#region MonoBehaviour CallBacks //回调函数区域
	// Use this for initialization
	void Start () {
		transformCache = transform;
		if(rendererComponent==null) rendererComponent=GetComponent<MeshRenderer>();
		if(meshFilter==null) meshFilter = GetComponent<MeshFilter>();
		if (meshFilter==null) {
			Debug.LogError (meshFilter+"is null,这个组件不能为空");
		}
		CheckIfMeshHasChanged();
		startDelay = Random.Range(randomStartDelay.x, randomStartDelay.y);
		updateSeed+=0.0005f;
		if(playOnAwake)
			Invoke("Play", updateInterval+updateSeed);
		if(updateSeed >= updateInterval)
			updateSeed = 0;

	}
	
	// Update is called once per frame
	void Update () {
		
	}
	#endregion
	
	#region Public Methods	//公共方法区域
	
	
	#endregion
	
	#region Private Methods	//私有方法区域
	void CheckIfMeshHasChanged(){
		if(meshCached != meshContainerFBX){  
			if(meshContainerFBX!=null)
			FillCacheArray();
		}
	}

	void FillCacheArray () {
		
		meshCacheCount = meshContainerFBX.childCount;
		meshCached = meshContainerFBX;
		meshCache = new MeshFilter[meshCacheCount];
		for(int i = 0; i < meshCacheCount; i++){
			meshCache[i] = meshContainerFBX.GetChild(i).GetComponent<MeshFilter>();
		}
		currentFrame = meshCacheCount*Random.value;	
		meshFilter.sharedMesh = meshCache[(int)currentFrame].sharedMesh;
	}

	void Play () {
		CancelInvoke();
		if(randomStartFrame)
			currentFrame = meshCacheCount*Random.value;
		else
			currentFrame = 0;

		meshFilter.sharedMesh = meshCache[(int)currentFrame].sharedMesh;		

		this.enabled = true;	
		RandomizePlaySpeed();	
		Invoke("AnimatedMesh", updateInterval);
		RandomRotate();

		//	if(transformCache.childCount > 0){
		//		for (var i:int; i < transformCache.childCount; i++){
		//			
		//			transformCache.GetChild(i).GetComponent(UnluckAnimatedMesh).Play();
		//			
		//		}
		//	}
	}

	void RandomizePlaySpeed (){
		if(playSpeedRandom > 0)
			currentSpeed = Random.Range(playSpeed-playSpeedRandom, playSpeed+playSpeedRandom);
		else
			currentSpeed = playSpeed;
	}

	void RandomRotate (){
		float x = transformCache.localRotation.eulerAngles.x;
		float y = transformCache.localRotation.eulerAngles.y;
		float z = transformCache.localRotation.eulerAngles.z;
		if (randomRotateX)
			x = (float)Random.Range (0, 360);
		if(randomRotateY)
			y = (float)Random.Range (0, 360);
		if(randomRotateZ)
			z = (float)Random.Range (0, 360);
		
		transformCache.localRotation.eulerAngles.Set (x,y,z);
	}

	void AnimatedMesh () {
		delta = updateInterval;
		startDelayCounter+=updateInterval;		
		if(startDelayCounter > startDelay) {
			rendererComponent.enabled = true;
			Animate();	
		}
		if(this.enabled){
			Invoke("AnimatedMesh", updateInterval);
			return;
		}
		rendererComponent.enabled = false;
	}

	void  Animate () {
		if(rendererComponent.isVisible){
			if(pingPong && PingPongFrame()){
				RandomizePropertiesAfterLoop();
			}else if(!pingPong && NextFrame()){
				RandomizePropertiesAfterLoop();
			}
			meshFilter.sharedMesh = meshCache[(int)currentFrame].sharedMesh;		
		}
	}

	bool PingPongFrame(){	
		if(pingPongToggle)
			currentFrame+= currentSpeed*delta;
		else
			currentFrame-= currentSpeed*delta;	
		if(currentFrame <= 0){			
			currentFrame = 0;
			pingPongToggle = true;
			return true;
		}	
		if(currentFrame >= meshCacheCount){
			pingPongToggle = false;
			currentFrame = meshCacheCount-1;
			return true;
		}
		return false;
	}

	bool NextFrame(){
		currentFrame+= currentSpeed*delta;
		if(currentFrame > meshCacheCount+1){
			currentFrame = 0;
			if(!loop) this.enabled = false;
			return true;
		}
		if(currentFrame >= meshCacheCount){	
			currentFrame = meshCacheCount - currentFrame;
			if(!loop) this.enabled = false;
			return true;
		}
		return false;
	}

	void RandomizePropertiesAfterLoop () {
		if(randomSpeedLoop) 
			RandomizePlaySpeed();
		if(randomRotateLoop) RandomRotate();
	}
	#endregion
}
