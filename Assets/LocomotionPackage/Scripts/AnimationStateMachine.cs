using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class AnimationStateMachine : ExtendedMonoBehaviour {
	
	public GameObject Target;
	private string GraphText = null;
	public bool SetGraphText( string s ) {
		if( GraphText != null ) {
			Debug.LogError( "You can not set the graph text once it has been set!" );
			return false;
		} else {
			GraphText = s;
			return true;
		}
	}
	public TextAsset GraphTextAsset;
	
	private Mixamo.RootMotionSetup rmSetup {
		get {
			return this.graph.rmSetup;
		}
	}
	
	private Mixamo.AnimationGraph graph;
	
	public bool IsLoaded {
		get {
			return( graph != null && graph.IsLoaded );
		}
	}
	
	Mixamo.TransitionHandler _transitionHandler = null;
	public void SetTransitionHandler( Mixamo.TransitionHandler t ) {
		_transitionHandler = t;
		if( this.graph != null ) {
			
			this.graph.TransitionHandler = _transitionHandler;
		}
	}
	
	void Start () {
		try {
			
			if( GraphText == null ) {
				GraphText = GraphTextAsset.text;
			}
			
			if( Target == null ) {
				throw new System.Exception( "Target character was not specified!" );
			}
			
			if( Target.GetComponent<Animation>() == null ) {
				throw new System.Exception( "Target.animation component not found. Please make sure the Target has animation on it.");
			}
			graph = Mixamo.AnimationGraph.FromJson( (Hashtable)Mixamo.JSON.JsonDecode( GraphText ) , this);
			DebugLog( graph.ToString() );
			
			GameObject go = new GameObject( "RootMotionBase" );
			go.transform.parent = rmSetup.pelvis.parent; go.transform.localPosition = Vector3.zero; go.transform.localEulerAngles = Vector3.zero; go.transform.localScale = Vector3.one;
			if( _debugMode ) {
				AddVisualGuide( go.transform , Color.red );
			}
			
			go = new GameObject("RootMotionLocalPosition");
			go.transform.parent = rmSetup.pelvis.parent; go.transform.localPosition = Vector3.zero; go.transform.localEulerAngles = Vector3.zero; go.transform.localScale = Vector3.one;
			RootMotionLocalPosition = go.transform;
			if( _debugMode ) {
				AddVisualGuide( go.transform , Color.green );
				AddVisualGuide( go.transform , Color.green ).transform.localPosition = Vector3.forward;
				AddVisualGuide( graph.rmSetup.pelvis , Color.blue).transform.localScale = new Vector3( 0.25f,0.25f,0.25f);
				Transform t = AddVisualGuide( graph.rmSetup.pelvis , Color.blue).transform;
				t.localPosition = Vector3.forward;
				t.localScale = new Vector3( 0.25f , 0.25f , 0.25f );
			}
			
			foreach( AnimationState s in Target.GetComponent<Animation>() ) {
				DebugLog( "State: " + s.name + " - " + s.length + " - " + s.layer );
			}
			
			graph.TransitionHandler = _transitionHandler;
			
			graph.clips[0].anim_state.weight = 1f;
		} catch( System.Exception e ) {
			Debug.LogError( "Could not load the Animation State Machine: " + e.Message + "\n" +  e.StackTrace );
		}
		
	}
	
	Transform RootMotionLocalPosition;
	
	class GUIStuff {
		public string normTime = "";
		public string speed = "";
		public Vector2 scroll = Vector2.zero;
		public string goToState = "";
		public int selectedState = 0;
	}
	
	private GUIStuff inputs = new GUIStuff();
	
	private bool UsePerWeightSelection = false;
	private bool _debugMode = false;
	public void ToggleDebugMode() {
		_debugMode = !_debugMode;
	}
	
	void DebugLog( string str ) {
		if( _debugMode ) {
			Debug.Log( str );
		}
	}
	
	void OnGUI() {
		if( _debugMode && IsLoaded ) {
			GUIStyle sty = new GUIStyle();
			sty.padding = new RectOffset( 5,5,5,5 );
			inputs.scroll = GUILayout.BeginScrollView( inputs.scroll , GUILayout.Width( 400) , GUILayout.ExpandHeight( true ) );
			GUILayout.BeginVertical( sty , GUILayout.ExpandWidth( true ) , GUILayout.ExpandHeight( true ) );
			
			if( UsePerWeightSelection ) {
				OnGUI_PerWeight();
			} else {
				OnGUI_PerControl();
			}
			
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		}
		
	}
	
	void OnGUI_PerWeight() {
		GUI.Label( new Rect( 0 , Screen.height - 20 , 100 , 20 ) , "Motion Pack Ready" );
		
		GUIStyle sty = new GUIStyle();
		sty.padding = new RectOffset( 5,5,5,5 );
		
		GUILayout.Label( "Name - IsPlaying - Length - Weight - Layer - NSpeed" );
		foreach( Mixamo.Clip clip in graph.clips ) {
			AnimationState s = clip.anim_state;
			GUILayout.Label( s.name.ToString() + " - " + Target.GetComponent<Animation>().IsPlaying( s.name ) + " - " + FormatFloat( s.length ) + " - " + string.Format( "{0:0.00}" , s.weight ) + " - " + s.layer.ToString() + " - " + FormatFloat( s.normalizedSpeed ) , GUILayout.ExpandWidth( true ));
			
			GUILayout.BeginVertical( sty, GUILayout.ExpandWidth(true) , GUILayout.ExpandHeight(true ) );
			
				s.weight = GUILayout.HorizontalSlider( s.weight , 0 , 1 , GUILayout.Width( 200 ) );
				
				s.enabled = GUILayout.Toggle( s.enabled , " Enabled" , GUILayout.Width( 200 ) );
				
				GUILayout.BeginHorizontal( GUILayout.ExpandWidth( false ) );
				GUILayout.Label( "Time" );
				inputs.normTime = GUILayout.TextField( inputs.normTime );
				GUILayout.Label( string.Format( "{0:0.00} by {0:0.00}" , s.normalizedTime , s.time ) );
				if( GUILayout.Button( "Set" ) ) {
					try {
					s.normalizedTime = int.Parse( inputs.normTime );
					} catch {}
				}
				GUILayout.EndHorizontal();
			
				GUILayout.BeginHorizontal( GUILayout.ExpandWidth( false ) );
				GUILayout.Label( "Speed" );
				inputs.speed = GUILayout.TextField( inputs.speed );
				GUILayout.Label( string.Format( "{0:0.00}" , s.speed ) );
				if( GUILayout.Button( "Set" ) ) {
					try {
					s.speed = float.Parse( inputs.speed );
					} catch {}
				}
				GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
		
		
	}
	
	void OnGUI_PerControl() {
		bool b = GUILayout.Toggle( (RootMotionMode == AnimationStateMachine.RootMotionModeType.Automatic) , new GUIContent( "Extract Root Automatically" ) );
		RootMotionMode  = (b ? AnimationStateMachine.RootMotionModeType.Automatic : AnimationStateMachine.RootMotionModeType.Manual );
		GUILayout.Label( "Extracted Root Motion: " + FormatVector( extractedRootMotion ) );
		GUILayout.Label( "Current State: " + graph.layers[0].GetCurrentState().name );
		GUILayout.BeginHorizontal();
		GUILayout.Label( "Go To State: " );
		GUIContent[] guiContents = new GUIContent[ graph.layers[0].states.Length ];
		for(int i=0; i<graph.layers[0].states.Length;i++ ) {
			guiContents[i] = new GUIContent( graph.layers[0].states[i].name );
		}
		int currState = inputs.selectedState;
		inputs.selectedState = GUILayout.SelectionGrid( inputs.selectedState , guiContents , 3 , GUILayout.Width( 200 ), GUILayout.ExpandHeight(true) );
		
		if( currState != inputs.selectedState ) {
			Mixamo.State s = graph.layers[0].states[inputs.selectedState];
			DebugLog( "Go To State: " + s.name.ToString() );
			DebugLog( "Success: " + graph.layers[0].ChangeState( s.name.Clone().ToString() ).ToString() );
		}
		GUILayout.EndHorizontal();
		foreach( string name in graph.ControlParams.Names ) {
			GUILayout.Label( name );
			graph.ControlParams[name] = GUILayout.HorizontalSlider( graph.ControlParams[name] , 0f , (name.EndsWith( "_speed" ) ? 2f : 1f) , GUILayout.Width( 200 ));
		}
		GUIStyle sty = new GUIStyle();
		sty.padding = new RectOffset( 0,0,0,0 );
		sty.margin = new RectOffset( 0,0,0,0 );
		sty.normal.textColor = Color.white;
		GUILayout.BeginVertical( sty );
		foreach( Mixamo.Clip c in this.graph.clips ) {
			AnimationState s = c.anim_state;
			GUILayout.Label( s.name + ": " + FormatFloat( s.weight ) + " : " + FormatFloat(s.speed ) + " : " + FormatFloat( s.length ) +  " (" + c.name + ")"  , sty );
		}
		GUILayout.EndVertical();
	}
	
	void Update () {
		if( IsLoaded ) {
			lastRootMotionResult = null;
		
			this.graph.UpdateGraph();
		}
	}
	
	void LateUpdate() {
		if( IsLoaded ) {
			_CalculateRootMotion();
		}
	}
	
	
	
	public class DescIntComparer : IComparer<int> {
		public int Compare (int x, int y)
		{
			return( x.CompareTo( y ) * -1);
		}
	}
	
	
	private Vector3 extractedRootMotion = Vector3.zero;
		
	public class RootMotionResult {
		
		/// <summary>
		/// The global translation of the root node in global space.
		/// </summary>
		public Vector3 GlobalTranslation;
		
		/// <summary>
		/// The translation of the pelvis in local space.
		/// </summary>
		public Vector3 LocalTranslation;
		
		/// <summary>
		/// The local position of the pelvis based on animations that have their root motion extracted. This position is offset from the starting position of the root.
		/// This value is subtracted from the pelvis when the root motion calculation is run.
		/// </summary>
		public Vector3 PelvisLocalPosition;
		
		public Vector3 LocalDeltaRotation;
		public Vector3 PelvisLocalRotation;
		public Vector3 GlobalRotation;
	}
	
	private RootMotionResult lastRootMotionResult = null;
	private RootMotionResult _CalculateRootMotion() {
		if( lastRootMotionResult == null ) {
			RootMotionResult result = new RootMotionResult();
			
			// ---
			// 1. Reverse Engineer the Unity Weights based on the amount and the layer
			// ---
			
			// 1a) temporarily accumulate all current AnimationStates and layers
			foreach( AnimationState s in Target.GetComponent<Animation>() ) {
				Mixamo.Clip c = graph.GetClipByAnimName( s.name );
				c.Weight = s.weight;
			}
			
			SortedList< int , List<AnimationState> > lst = new SortedList<int, List<AnimationState>>( new DescIntComparer() );
			
			foreach (AnimationState aState in Target.GetComponent<Animation>())
			{
				if( aState.layer == 0 ) {
					if( !lst.ContainsKey( aState.layer ) ) {
						lst.Add( aState.layer , new List<AnimationState>() );
					}
					lst[aState.layer].Add( aState );
				}
			}

			
			// 1b) iterate thru each layer in descending order and calculate the "real" normalized weight of each AnimationState in that layer
			float remainingWeight = 1f;
			List<AnimationState> lastWeightedList = null;
			float lastRemainingWeight = 0f;
			float lastSumWeight = 0f;
			foreach( KeyValuePair<int , List<AnimationState>> kvp in lst ) {
				if( remainingWeight > 0f ) {
					float sumWeight = 0f;
					foreach( AnimationState s in kvp.Value ) {
						if( s.enabled ) {
							sumWeight += s.weight;
						}
					}
					
					if( sumWeight > 0f ) {
						lastWeightedList = kvp.Value;
						lastRemainingWeight = remainingWeight;
						lastSumWeight = sumWeight;
					}
					
					float normalizeBy = 1f;
					if( sumWeight > 1f ) {
						normalizeBy = 1f / sumWeight;
					}
					
					float lostWeight = 0f;
					foreach( AnimationState s in kvp.Value ) {
						Mixamo.Clip c = graph.GetClipByAnimName( s.name );
						if( s.enabled ) {
							c.NormalizedRootWeight = s.weight * normalizeBy * remainingWeight;
							lostWeight += c.NormalizedRootWeight;
						} else {
							c.NormalizedRootWeight = 0f;
						}
					}
					
					remainingWeight -= lostWeight;
					
				} else {
					foreach( AnimationState s in kvp.Value ) {
						Mixamo.Clip c = graph.GetClipByAnimName( s.name );
						c.NormalizedRootWeight = 0f;
					}
				}
			}
			
			if( remainingWeight > 0f && lastWeightedList != null && lastRemainingWeight > 0f && lastSumWeight > 0f) {
				foreach( AnimationState s in lastWeightedList ) {
					Mixamo.Clip c = graph.GetClipByAnimName( s.name );
					if( s.enabled ) {
						c.NormalizedRootWeight = (s.weight / lastSumWeight) * lastRemainingWeight;
						remainingWeight -= c.NormalizedRootWeight;
					}
				}
			}
			
			
			// ---
			// 2. Calculate Root Motion
			// ---
			
			// 2a) turn off all animations, so we can look at one by one
			foreach( AnimationState s in Target.GetComponent<Animation>() ) {
				s.weight = 0f;
			}
			
			result.LocalTranslation = Vector3.zero;
			result.PelvisLocalPosition = Vector3.zero;
			result.GlobalTranslation = Vector3.zero;
			result.LocalDeltaRotation = Vector3.zero;
			result.PelvisLocalRotation = Vector3.zero;
			result.GlobalRotation = Vector3.zero;
			
			// 2b) based on the json graph, iterate thru each clip that is set up to have its root motion extracted, sample that clip, and save the root motion change
			foreach( Mixamo.Clip clip in graph.clips ) {
				if(clip.RootMotion.ExtractionType != Mixamo.RootMotionExtractionType.None || clip.RootMotion.RotExtractionType != Mixamo.RootMotionRotExtractionType.None ) {
					// TODO: doesn't need to be computed every time (should optimize this by storing precalculated root motion)
					
					// sample animation
					clip.anim_state.weight = 1f;
					Target.GetComponent<Animation>().Sample();
					
					// compute root motion
					Vector3 currPos = rmSetup.GetPelvisPosition();
					Vector3 currRot = rmSetup.GetPelvisRotation();
					clip.RootMotion.ComputeRootMotion( currPos , currRot , clip.anim_state.normalizedTime , clip.anim_state.wrapMode );
					
					clip.anim_state.weight = 0f;
					
					if(clip.RootMotion.ExtractionType != Mixamo.RootMotionExtractionType.None) {
						// calculate root motion translation
						Vector3 axis = Vector3.one;
						
						switch( clip.RootMotion.ExtractionType ) {
						case Mixamo.RootMotionExtractionType.Z:
							axis = Vector3.forward;
							break;
						case Mixamo.RootMotionExtractionType.X:
							axis = Vector3.right;
							break;
						case Mixamo.RootMotionExtractionType.Y:
							axis = Vector3.up;
							break;
						case Mixamo.RootMotionExtractionType.XY:
							axis = new Vector3( 1f ,1f , 0f);
							break;
						case Mixamo.RootMotionExtractionType.YZ:
							axis = new Vector3( 0f , 1f , 1f);
							break;
						case Mixamo.RootMotionExtractionType.XZ:
							axis = new Vector3( 1f ,0f , 1f);
							break;
						case Mixamo.RootMotionExtractionType.XYZ:
							axis = Vector3.one;
							break;
						}
						
						Matrix4x4 axis_m = Matrix4x4.Scale( axis );
						result.LocalTranslation += axis_m.MultiplyVector( clip.RootMotion.DeltaPosition * clip.NormalizedRootWeight );
						result.PelvisLocalPosition += axis_m.MultiplyVector( (currPos - clip.RootMotion.StartPosition) * clip.NormalizedRootWeight );
					}
					
					if( clip.RootMotion.RotExtractionType != Mixamo.RootMotionRotExtractionType.None ) {
						// calculate root motion rotation
						Vector3 rotAxis = Vector3.one;
						if( clip.RootMotion.RotExtractionType == Mixamo.RootMotionRotExtractionType.Y ) {
							rotAxis = Vector3.up;
						} else if (  clip.RootMotion.RotExtractionType == Mixamo.RootMotionRotExtractionType.X ) {
							rotAxis = Vector3.right;
						} else if(  clip.RootMotion.RotExtractionType == Mixamo.RootMotionRotExtractionType.Z ) {
							rotAxis = Vector3.forward;
						}
						
						Matrix4x4 rotAxis_m = Matrix4x4.Scale( rotAxis );
						result.LocalDeltaRotation += rotAxis_m.MultiplyVector( clip.RootMotion.DeltaRotation * clip.NormalizedRootWeight  );
						result.PelvisLocalRotation += rotAxis_m.MultiplyVector(  currRot * clip.NormalizedRootWeight  );
					}
					
				}
			}
			
			// 2c) return to normal position based on animation
			foreach( AnimationState s in Target.GetComponent<Animation>() ) {
				Mixamo.Clip c = graph.GetClipByAnimName( s.name );
				s.weight = c.Weight;
			}
			Target.GetComponent<Animation>().Sample();
			
			
			// 2d) remove the calculated root translation from the root node and optionally apply it to the current transform attached to this script
			
			// save root position to separate node
			RootMotionLocalPosition.localPosition = result.PelvisLocalPosition;
			// remove root position from pelvis
			rmSetup.pelvis.localPosition -= result.PelvisLocalPosition;
			
			// calculate global root motion translation
			Vector3 startPosition = rmSetup.pelvis.position;
			rmSetup.pelvis.localPosition += result.LocalTranslation;
			Vector3 endPosition = rmSetup.pelvis.position;
			result.GlobalTranslation = endPosition - startPosition;
			rmSetup.pelvis.localPosition -= result.LocalTranslation;
			
			// 2e) remove the calculated root rotation and optionally apply it to the current transform
			// TODO: uncomment out this code and make it work. Basically I've calculated the root rotation the same way I've calculated translation, the problem is
			// that this calculation below (removing the rotation and translation from the pelvis node and applying it to the root node) is wrong.
			// I know I need to apply the rotation displaced to the translation or something like that, but I just can't figure it out.
			/*
			// remove root motion rotation from pelvis
			RootMotionLocalPosition.localEulerAngles = result.PelvisLocalRotation;
			
			rmSetup.pelvis.localEulerAngles -= result.PelvisLocalRotation;
			
			// calculate global root motion rotation
			Vector3 startRot = rmSetup.pelvis.eulerAngles;
			rmSetup.pelvis.localEulerAngles += result.LocalDeltaRotation;
			Vector3 endRot = rmSetup.pelvis.eulerAngles;
			result.GlobalRotation = (endRot - startRot);
			rmSetup.pelvis.localEulerAngles -= result.LocalDeltaRotation;
			*/
			
			if( RootMotionMode == AnimationStateMachine.RootMotionModeType.Automatic ) {
				// TODO: uncomment out this code and get it to work, see above step 2e
				//	rmSetup.root.Rotate( result.GlobalRotation , Space.World );
				CharacterController cc = this.GetComponent<CharacterController>();
				if( cc == null ) {
					rmSetup.root.Translate(  result.GlobalTranslation , Space.World );
				} else {
					cc.Move( result.GlobalTranslation  );
				}
			}
			
			
			
			lastRootMotionResult = result;
		}
		
		return lastRootMotionResult;
	}
	
	/*
	 PUBLIC API FOR ACCESSING ANIMATION STATE MACHINE --------------------------------
 	*/
	
	/// <summary>
	/// Whether the root motion is applied automatically to the attached transform or whether it is used manually.
	/// </summary>
	public enum RootMotionModeType {
		Manual,
		Automatic
	}
	public RootMotionModeType RootMotionMode = AnimationStateMachine.RootMotionModeType.Automatic;
	
	/// <summary>
	/// Returns the root motion that should be applied to the character's root mode. Always call this in LateUpdate. 
	/// If RootMotionMode is set to Automatic, this is done automatically and applied to this GameObject or character controller attached to it.
	/// </summary>
	/// <returns>
	/// A <see cref="RootMotionResult"/> - resulting root motion that was extracted.
	/// </returns>
	public RootMotionResult GetRootMotion() {
		if( IsLoaded ) {
			return( _CalculateRootMotion() );
		} else {
			return null;
		}
	}
	
	
	/// <summary>
	/// Change to another state in layer zero;
	/// </summary>
	/// <param name="next_state">
	/// A <see cref="System.String"/> of the state to transition to.
	/// </param>
	/// <returns>
	/// A <see cref="System.Boolean"/> whether or not the state was actually changed.
	/// </returns>
	public bool ChangeState( string next_state ) {
		return( ChangeState( 0 , next_state ) );
	}
	
	/// <summary>
	/// Change to another state in the given layer
	/// </summary>
	/// <param name="layer">
	/// A <see cref="System.Int32"/> of the layer
	/// </param>
	/// <param name="next_state">
	/// A <see cref="System.String"/> of the state to transition to
	/// </param>
	/// <returns>
	/// A <see cref="System.Boolean"/> whether or not the state was actually changed.
	/// </returns>
	public bool ChangeState( int layer , string next_state ) {
		if( IsLoaded ) {
			return( graph.layers[layer].ChangeState( next_state ) );
		} else {
			return false;
		}
	}
	
	/// <summary>
	/// Returns the current state in layer 0
	/// </summary>
	/// <returns>
	/// A <see cref="System.String"/>
	/// </returns>
	public string GetCurrentState( ) {
		return GetCurrentState( 0 );
	}
	
	/// <summary>
	/// Returns the current state transitioning to in layer 0
	/// </summary>
	/// <returns>
	/// A <see cref="System.String"/>
	/// </returns>
	public string GetCurrentDestinationState() {
		return( GetCurrentDestinationState(0) );
	}
	
	/// <summary>
	/// Returns the state transitioning to in the specified layer
	/// </summary>
	/// <param name="layer">
	/// A <see cref="System.Int32"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.String"/>
	/// </returns>
	public string GetCurrentDestinationState(int layer) {
		if( IsLoaded ) {
			Mixamo.State s = this.graph.layers[layer].GetCurrentDestinationState();
			return( (s == null) ? "" : s.name );
		} else {
			return "";
		}
	}
	
	/// <summary>
	/// Returns the current state in the specified layer
	/// </summary>
	/// <param name="layer">
	/// A <see cref="System.Int32"/>
	/// </param>
	/// <returns>
	/// A <see cref="System.String"/>
	/// </returns>
	public string GetCurrentState( int layer ) {
		if( IsLoaded ) {
			Mixamo.State s = this.graph.layers[layer].GetCurrentState();
			return( ( s == null ? "" : s.name ) );
		} else {
			return "";
		}
	}
	
	/// <summary>
	/// A hash of string -> float, which are inputs into the different blend weights and state speeds.
	/// Use "<statename>_speed" to access the speed of a given state.
	/// E.G. : asm.ControlWeights["run_speed"] = 0.5f;
	/// </summary>
	public Mixamo.ControlParameters ControlWeights {
		get {
			if( IsLoaded ) {
				return( graph.ControlParams );
			} else {
				return( new Mixamo.ControlParameters(new string[] {} , new float[] {} ) );
			}
		}
	}
	
	/*
	 END OF PUBLIC API
	 */
}
