#pragma strict



var prefabs:GameObject[];
var bgrs:Material[];
var lights:Light[];

var nextButton:GameObject;
var prevButton:GameObject;
var bgrButton:GameObject;
var lightButton:GameObject;

var texturePreview:GameObject;

private var activeObj:GameObject;
private var counter:int;
private var bCounter:int;
private var lCounter:int;
var txt:TextMesh;
var debug:TextMesh;

function Start () {
	Swap ();
	if(!txt)
	txt = transform.Find("txt").GetComponent(TextMesh);
	if(!nextButton)
	nextButton = transform.Find("nextButton").GetComponent(GameObject);
	if(!prevButton)
	prevButton = transform.Find("prevButton").GetComponent(GameObject);
	if(!bgrButton)
	bgrButton = transform.Find("bgrButton").GetComponent(GameObject);
	if(!lightButton)
	lightButton = transform.Find("lightButton").gameObject;
	
	
	if(!texturePreview)
	texturePreview = transform.Find("texturePreview").GetComponent(GameObject);
	if(!debug)
	debug = transform.Find("debug").GetComponent(TextMesh);
	
}

function Update () {
	if(Input.GetMouseButtonUp(0)){		
		ButtonUp();
	}
	if(Input.GetKeyUp("right"))
	Next();
	if(Input.GetKeyUp("left"))
	Prev();
	if(Input.GetKeyUp("space")){
	nextButton.SetActive(!nextButton.activeInHierarchy);
	prevButton.SetActive(nextButton.activeInHierarchy);
	bgrButton.SetActive(nextButton.activeInHierarchy);
	texturePreview.SetActive(nextButton.activeInHierarchy);
	txt.gameObject.SetActive(nextButton.activeInHierarchy);
	}
}

function ButtonUp() {
	var ray:Ray	= Camera.main.ScreenPointToRay(Input.mousePosition);
	var hit:RaycastHit;
	if (Physics.Raycast (ray, hit)) {
		if(hit.transform.gameObject == nextButton)
			Next();	
		else if(hit.transform.gameObject == prevButton)
			Prev();	
		else if(hit.transform.gameObject == bgrButton)
			NextBgr();
		else if(hit.transform.gameObject == lightButton)
			LightChange();	
	}
}
function LightChange () {
	if(lights.Length > 0){
		lights[lCounter].enabled = false;
		lCounter++;
		if(lCounter >= lights.Length)
		lCounter = 0;	
		lights[lCounter].enabled = true;		
	}
}
function NextBgr () {
	if(bgrs.Length > 0){
		bCounter++;
		if(bCounter >= bgrs.Length)
		bCounter = 0;
		RenderSettings.skybox = bgrs[bCounter];
	}
}

function Next () {	
	counter++;
	if(counter > prefabs.Length -1)
		counter = 0;
	Swap ();
}

function Prev () {
	counter--;
	if(counter < 0)
		counter = prefabs.Length -1;
	Swap ();	
}

function Swap () {
	if(prefabs.Length > 0){
		Destroy(activeObj);
		var o:GameObject = Instantiate(prefabs[counter]);
		activeObj = o;
		if(txt){
		txt.text = activeObj.name;
		txt.text = txt.text.Replace("(Clone)", "");		
		txt.text = txt.text + " " + activeObj.GetComponent(UnluckAnimatedMesh).meshContainerFBX.name;
		txt.text = txt.text.Replace("_", " ");
		txt.text = txt.text.Replace("Flag ", "");
		}
		if(texturePreview)
		texturePreview.GetComponent(Renderer).sharedMaterial = activeObj.GetComponent(Renderer).sharedMaterial;
	}
}