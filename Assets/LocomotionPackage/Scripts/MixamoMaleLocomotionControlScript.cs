using UnityEngine;
using System.Collections;

public class MixamoMaleLocomotionControlScript : MonoBehaviour, Mixamo.TransitionHandler {
	
	
	public bool CanTransitionTo (string guard, string source, string destination)
	{
		if( guard == "block_on_player_control" ) {
			return true;
		} else {
			return true;
		}
	}
	
	public string[] KeyControls() {
		return keys;
	}
	
	
	AnimationStateMachine GetASM() {
		return this.GetComponent<AnimationStateMachine>();
	}
	
	// Use this for initialization


	void Start () {
		GetASM().SetTransitionHandler( this );
		controller = GetComponent<CharacterController>();
		asm = GetASM();
	}
	
	public bool ShowGUIKey = true;
	public float turnDegrees = 115f;
	private float gravity = 9.81f;
	private AnimationStateMachine asm;
	private AnimationStateMachine.RootMotionResult result;
	private CharacterController controller;
	private Vector3 moveDirection = Vector3.zero;
	private string[] keys ={
		"W", "Forward",
		"S", "Backwards",
		"A", "Strafe Left",
		"D", "Strafe Right",
		"Shift", "run",
		"Q", "Turn Left",
		"E", "Turn Right",
	};
	
	void OnGUI() {
		if( ShowGUIKey ) {
			GUILayout.BeginVertical( GUILayout.ExpandWidth( true ) );
			GUILayout.Label( "Key Options:" );
			
			// loop keys
			
			for (int i = 0; i < keys.Length; i += 2) {
				GUILayout.Label(keys[i] + " - " + keys[i+1]);
			}

			GUILayout.EndVertical();
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	//Apply gravity  
	moveDirection.y = controller.velocity.y - gravity * Time.deltaTime;
		
	AnimationStateMachine asm = GetASM();
		int turnDirection = 0;
			if( Input.GetKey( KeyCode.W ) ) {
				if( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) ) {
						asm.ControlWeights["ctrl_move"] = Mixamo.Util.CrossFadeDown( asm.ControlWeights["ctrl_move"] , 0.3f );
					} else {
						asm.ControlWeights["ctrl_move"] = Mixamo.Util.CrossFadeUp( asm.ControlWeights["ctrl_move"] , 0.3f );
					}
				asm.ChangeState( "move" );
					
			} else if( Input.GetKey( KeyCode.S ) ) {
				if( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) ) 
					asm.ChangeState( "run_backwards" );
				
				else 
					asm.ChangeState( "walk_backwards" );
			} else if( Input.GetKey( KeyCode.A ) ) {
				if( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) ) 
					asm.ChangeState( "strafe_left" );
				
				else 
					asm.ChangeState( "walk_strafe_left" );
			} 
			
			else if( Input.GetKey( KeyCode.D ) ) {
				if( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) ) 
					asm.ChangeState( "strafe_right" );
				
				else 
					asm.ChangeState( "walk_strafe_right" );
			} 
			
			else if( Input.GetKey( KeyCode.Q ) ) {
				asm.ChangeState("turn_left");
			} else if( Input.GetKey( KeyCode.E ) ) {
				asm.ChangeState("turn_right");
			}
			 
			
			else {
					asm.ChangeState( 0, "idle" );
			}
			
			if( Input.GetKey( KeyCode.Space ) ) {
					asm.ChangeState( "jump" );	
			}
			
		
		if( Input.GetKey( KeyCode.Q ) ) {
			turnDirection = -1;
		} else if( Input.GetKey( KeyCode.E ) ) {
			turnDirection = 1;
		}
		
		if( turnDirection != 0f ){
			Vector3 forward = this.transform.forward;
			forward.y = 0;
			forward = forward.normalized;
			Vector3 right = new Vector3(forward.z, 0, -forward.x);
			transform.rotation = Quaternion.LookRotation( Vector3.RotateTowards( forward , right * turnDirection , turnDegrees * Mathf.Deg2Rad * Time.deltaTime , 1000f ) );
		}
	}
	
		void LateUpdate() {
		if (controller != null){
			result = asm.GetRootMotion();
			if( result != null ) {
				controller.Move((moveDirection * Time.deltaTime) + result.GlobalTranslation);
			}
		}
	}
}
