var target : Transform;
var distance = 10.0;

var xSpeed = 250.0;
var ySpeed = 120.0;

var yMinLimit = -20;
var yMaxLimit = 80;

private var x = 0.0;
private var y = 0.0;

@script AddComponentMenu("Camera-Control/Mouse Orbit")

function Start () {
    var angles = transform.eulerAngles;
    x = angles.y;
    y = angles.x;

	// Make the rigid body not change rotation
   	if (GetComponent.<Rigidbody>())
		GetComponent.<Rigidbody>().freezeRotation = true;
}

var prevDistance;
function LateUpdate () {
    if (distance < 2) distance = 2;
    distance -= Input.GetAxis("Mouse ScrollWheel") * 2;
    if (target && (Input.GetMouseButton(0) || Input.GetMouseButton(1))) {
        var pos = Input.mousePosition;
        var dpiScale = 1;
        if (Screen.dpi < 1) dpiScale = 1;
        if (Screen.dpi < 200) dpiScale = 1;
        else dpiScale = Screen.dpi / 200f;

        if (pos.x < 380 * dpiScale && Screen.height - pos.y < 250 * dpiScale ) return;
		
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        x += Input.GetAxis("Mouse X") * xSpeed * 0.02;
        y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02;

        y = ClampAngle(y, yMinLimit, yMaxLimit);
        var rotation = Quaternion.Euler(y, x, 0);
        var position = rotation * Vector3(0.0, 0.0, -distance) + target.position;
        transform.rotation = rotation;
        transform.position = position;
       
    } else {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
        //Screen.lockCursor = false;
    }
   
    if (prevDistance != distance) {
        prevDistance = distance;
        var rot = Quaternion.Euler(y, x, 0);
        var po = rot * Vector3(0.0, 0.0, -distance) + target.position;
        transform.rotation = rot;
        transform.position = po;
    }
}

static function ClampAngle (angle : float, min : float, max : float) {
	if (angle < -360)
		angle += 360;
	if (angle > 360)
		angle -= 360;
	return Mathf.Clamp (angle, min, max);
}