#pragma strict
//@HideInInspector
var meshCache:MeshFilter[];
@HideInInspector
var meshCached:Transform;
var meshContainerFBX:Transform;
var playSpeed:float = 1f;
var playSpeedRandom:float = 0f;
var randomSpeedLoop:boolean;
private var currentSpeed:float;
@HideInInspector
var currentFrame:float;
@HideInInspector
var meshCacheCount:int;
@HideInInspector
var meshFilter:MeshFilter;
@HideInInspector
var rendererComponent:Renderer;
var updateInterval:float = 0.05f;

var randomRotateX:boolean;
var randomRotateY:boolean;
var randomRotateZ:boolean;

var randomStartFrame:boolean = true;

var randomRotateLoop:boolean;

var loop:boolean = true;
var pingPong:boolean;

var playOnAwake:boolean = true;
var randomStartDelay:Vector2 = Vector2(0,0);
private var startDelay:float;
private var startDelayCounter:float;

static var updateSeed:float;

private var pingPongToggle:boolean;

var transformCache:Transform;
var delta:float;

function Start () {
	transformCache = transform;
	
	CheckIfMeshHasChanged();
	startDelay = Random.Range(randomStartDelay.x, randomStartDelay.y);
	updateSeed+=0.0005;
	if(playOnAwake)
	Invoke("Play", updateInterval+updateSeed);
	if(updateSeed >= updateInterval)
	updateSeed = 0;
	if(!rendererComponent)GetRequiredComponents();
	//rendererComponent.enabled = false;
}

function Play () {
	CancelInvoke();
	if(randomStartFrame)
		currentFrame = meshCacheCount*Random.value;
	else
		currentFrame = 0;
	
	meshFilter.sharedMesh = meshCache[currentFrame].sharedMesh;		
	
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

function RandomRotate (){
	if(randomRotateX)
		transformCache.localRotation.eulerAngles.x = Random.Range(0, 360);
	if(randomRotateY)
		transformCache.localRotation.eulerAngles.y = Random.Range(0, 360);
	if(randomRotateZ)
		transformCache.localRotation.eulerAngles.z = Random.Range(0, 360);
}

function GetRequiredComponents () {
	rendererComponent = GetComponent(Renderer);
}

function RandomizePlaySpeed (){
	if(playSpeedRandom > 0)
	currentSpeed = Random.Range(playSpeed-playSpeedRandom, playSpeed+playSpeedRandom);
	else
	currentSpeed = playSpeed;
}

function FillCacheArray () {
	GetRequiredComponents();
	meshFilter = transformCache.GetComponent(MeshFilter);
	meshCacheCount = meshContainerFBX.childCount;
	meshCached = meshContainerFBX;
	meshCache = new MeshFilter[meshCacheCount];
	for(var i:int = 0; i < meshCacheCount; i++){
		meshCache[i] = meshContainerFBX.GetChild(i).GetComponent(MeshFilter);
	}
	currentFrame = meshCacheCount*Random.value;	
	meshFilter.sharedMesh = meshCache[currentFrame].sharedMesh;
}

function CheckIfMeshHasChanged(){
	if(meshCached != meshContainerFBX){  
	    if(meshContainerFBX!=null);
			FillCacheArray();
	}
}

//function Update () {
//	delta =Time.deltaTime;	
//	startDelayCounter+= delta;		
//	if(startDelayCounter > startDelay) {
//		rendererComponent.enabled = true;
//		Animate();	
//	}
//	if(this.enabled){
//		return;
//	}
//	rendererComponent.enabled = false;
//}

function AnimatedMesh () {
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

function PingPongFrame():boolean{	
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

function NextFrame():boolean{
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

function RandomizePropertiesAfterLoop () {
	if(randomSpeedLoop) 
		RandomizePlaySpeed();
	if(randomRotateLoop) RandomRotate();
}

function Animate () {
	if(rendererComponent.isVisible){
		if(pingPong && PingPongFrame()){
			RandomizePropertiesAfterLoop();
		}else if(!pingPong && NextFrame()){
			RandomizePropertiesAfterLoop();
		}
		meshFilter.sharedMesh = meshCache[currentFrame].sharedMesh;		
	}
}