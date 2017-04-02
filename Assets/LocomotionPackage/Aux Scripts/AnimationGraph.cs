using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Mixamo {
	
	public interface TransitionHandler {
		string[] KeyControls();
		bool CanTransitionTo( string guard , string source , string destination );
	}
	
	/// <summary>
	/// A class for holding all the control parameters (weights that control blend trees) in the graph.
	/// This also contains a paramter "state_speed" to control the playback speed of a state.
	/// </summary>
	public class ControlParameters {
		
		Dictionary< string , float> dict = new Dictionary<string, float>();
		
		public ControlParameters(string[] opts,float[] vals) {
			if( opts != null && vals != null ) {
				for( int i=0 ;i < opts.Length;i++ ) {
					dict.Add( opts[i] , vals[i] );
				}
			}
		}
		
		public float this[string s]
		{
		    get { return dict[s]; }
		    set { 
				dict[s] = value;
			}
		}
		
		/// <summary>
		/// Returns a list of all control paramters
		/// </summary>
		public string[] Names {
			get {
				string[] arr = new string[dict.Keys.Count];
				dict.Keys.CopyTo( arr , 0 );
				return arr;
			}
		}
	}
	
	/// <summary>
	/// Base class that describes a Transition. All other transitions must override this class.
	/// </summary>
	public abstract class Transition  {
		
		public bool WaitTillEnd = false;
		protected State destination;
		private State _start_destination = null;
		protected string[] guards;
		protected State start_destination {
			get {
				return _start_destination;
			}
			set {
				_start_destination = value;
			}
		}
		
		/// <summary>
		/// The state where the transition want's to go to.
		/// </summary>
		public State Destination {
			get {
				return this.destination;
			}
		}
		protected State source;
		protected bool finished = true;
		
		/// <summary>
		/// Initiate the transition. Called when a transition is started from ChangeState().
		/// </summary>
		/// <param name="dest">
		/// A <see cref="State"/>
		/// </param>
		public virtual void Start(State dest) {
			finished = false;
			this.destination = dest;
			if( !this.destination.IsLooping ) {
				this.destination.ResetTime( 0f );
			}
		}
		
		/// <summary>
		/// Called when the state is changed and the transition is finished.
		/// </summary>
		public virtual void Finish(){
			
		}
		
		/// <summary>
		/// Returns whether or the transition can be taken to the destination state.
		/// </summary>
		/// <param name="dest">
		/// A <see cref="State"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public virtual bool CanBeMade( State dest) {
			if( guards != null ) {
				foreach( string g in guards ) {
					if( source.layer.graph.TransitionHandler != null && !source.layer.graph.TransitionHandler.CanTransitionTo( g , source.name , dest.name ) ) {
						return false;
					}
				}
			}
			
			if( start_destination == null ) {
				return true;
			} else {
				return( dest == start_destination );
			}
		}
		
		/// <summary>
		/// Compute the weight of the Source state and the Destination state based on the remaining weight.
		/// </summary>
		/// <param name="remaining_weight">
		/// A <see cref="System.Single"/>
		/// </param>
		public abstract void UpdateGraph( float remaining_weight );
		
		
		/// <summary>
		/// Returns whether or not the transition has completed. Tells the state machine to set the Current State = Destination.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool IsDone() {
			return finished;
		}
		
		public override string ToString ()
		{
			return string.Format ("[Transition: Source={0} Destination={1} Type={2}]", source.name , (start_destination == null ? "*" : start_destination.name) , this.GetType().ToString());
		}
	}
	
	/// <summary>
	/// Transitions between two states by crossfading between them over a duration. This means immeditately playing the next state with weight 0 and fading it in to weight 1, while
	/// fading the current state to weight 0.
	/// </summary>
	public class CrossfadeTransition : Transition {
		
		private float t_weight = 0f;
		
		public CrossfadeTransition( State source , State destination , float duration , string[] guards ) {
			this.source = source;
			this.start_destination = destination;
			this.duration = duration;
			this.guards = guards;
		}
		
		
		public override void Start(State dest) {
			base.Start(dest);
			t_weight = 0f;
			destination.ResetTime(0f);
		}
		
		private float duration = 0f;
		public override void UpdateGraph (float remaining_weight)
		{
			if( WaitTillEnd && (source.MaxTime < (source.MaxTimeLength - duration) ) ) {
				source.UpdateGraph( remaining_weight );
				destination.UpdateGraph( 0f );
				destination.ResetTime(0f);
			} else {
				t_weight = Mixamo.Util.CrossFadeUp( t_weight , this.duration );
				destination.UpdateGraph( t_weight * remaining_weight );
				source.UpdateGraph( (1-t_weight) * remaining_weight );
				
				if( t_weight >= 1f ) {
					finished = true;
				}
			}
		}
		
		public override void Finish ()
		{
		}
		
	}
	
	/// <summary>
	/// Transitions between two states by playing an inbetween clip.
	/// This transitions crossfades the current state with the inbetween clip in duration_in seconds and then crossfades the inbetween clip with the destination state in duration_out seconds.
	/// If duration_in == 0f then we wait till the source state actually finishes playing before transitioning.
	/// </summary>
	public class ClipTransition : Transition {
		public Clip clip;
		public float duration_in = 0f;
		public float duration_out = 0f;
		private float t_weight_start = 0f;
		private float t_weight_end = 0f;
		
		public ClipTransition(  Clip c , State source , State dest , float dur_in , float dur_out , string[] guards ) {
			clip = c;
			clip.anim_state.wrapMode = WrapMode.ClampForever;
			clip.anim_state.enabled = true;
			this.source = source;
			this.start_destination = dest;
			duration_in = dur_in;
			if( duration_in == 0f ) {
				WaitTillEnd = true;
			}
			duration_out = dur_out;
			this.guards = guards;
		}
		public override void Start ( State dest)
		{
			base.Start(dest);
			t_weight_start = 0f;
			t_weight_end = 0f;
			clip.ResetTime(0f);
			dest.ResetTime(0f);
			if( duration_in == 0f ) {
				source.SetCurrentWrapMode( MixamoWrapMode.ClampForever );
			}
		}
		
		public override void UpdateGraph (float remaining_weight)
		{
			
			if( WaitTillEnd && (source.NormalizedTime < 1f ) ) {
				source.UpdateGraph( remaining_weight );
				clip.UpdateGraph(0);
				destination.UpdateGraph( 0f );
				destination.ResetTime(0f);
				clip.ResetTime(0f);
			} else if( clip.anim_state.time < this.duration_in ) {
				// fade in
				t_weight_start = Mixamo.Util.CrossFadeUp( t_weight_start , this.duration_in );
				source.UpdateGraph( (1-t_weight_start) * remaining_weight );
				clip.UpdateGraph( (t_weight_start) * remaining_weight );
				destination.UpdateGraph( 0f );
				destination.ResetTime(0f);
			} else if( clip.anim_state.time > (clip.anim_state.length - this.duration_out ) && clip.anim_state.time < clip.anim_state.length ) {
				// fade out
				t_weight_end = Mixamo.Util.CrossFadeUp( t_weight_end , this.duration_out );
				source.UpdateGraph( 0f );
				clip.UpdateGraph( ( 1-t_weight_end) * remaining_weight );
				destination.UpdateGraph( t_weight_end * remaining_weight );
			} else if( clip.anim_state.time < clip.anim_state.length ) {
				// play normally
				clip.UpdateGraph( remaining_weight );
				source.UpdateGraph( 0f );
				destination.UpdateGraph( 0f );
				destination.ResetTime(0f);
			} else {
				// end
				destination.UpdateGraph( remaining_weight );
				source.UpdateGraph( 0f );
				clip.UpdateGraph( 0f );
				source.ResetWrapMode();
				finished = true;
			}
		}
	}
	
	
	/// <summary>
	/// A layer of the animation graph. Each layer can be in one state at a time. Weighting is computed by calculating a normalized weight (weights summing to 1) for each layer and then using
	/// Unity's weight calculation to average each layer depending on it's layer priority and any MixingTransforms on the individual bone.
	/// </summary>
	public class Layer {
		public string name;
		public State[] states;
		public AnimationGraph graph;
		public int priority;
		public Transition CreateDefaultTransition(  State source  ) {
			// default transition is to crossfade to every state
			Transition t = new CrossfadeTransition( source , null , 0.1f , new string[0] {} );
			return t;
		}
		
		State _current_state;
		Transition _current_transition = null;
		
		public Layer() {
			
		}
		
		public void Init() {
			_current_state = states[0];
			_desired_state = _current_state;
		}
		
		public State GetCurrentState() {
			return _current_state;
		}
		
		public State GetCurrentDestinationState() {
			if( _current_transition != null ) {
				return _current_transition.Destination;
			} else {
				return null;
			}
		}
		
		private State _desired_state;
		public bool ChangeState( string name ) {
			State next = this.GetStateByName( name );
			if( next != null ) {
				// save this state, in case you need to transition to it immediately after a non looping state
				_desired_state = next;
			}
			if( _current_transition != null ) {
				// you can't change state if you're in a transition
				return false;
			} else if( next == null ) {
				Debug.LogError( "Could not find the state: " + name.ToString() );
				return false;
			} else if( next != _current_state ) {
				// find a transition to the next state and make it if possible
				Transition t = _current_state.GetTransitionTo( next );
				if( t != null ) {
					_current_transition = t;
					_current_transition.Start( next );
					return true;
				} else {
					return false;
				}
			} else {
				return true;
			}
		}
		
		public override string ToString ()
		{
			string str = ( "Layer: " + name + "\n" );
			str += "States: \n";
			foreach( State s in states ) {
				str += s.ToString() + "\n";
			}
			return str;
		}
		
		public void UpdateGraph(float remaining_weight) {
			if( _current_transition != null && _current_transition.IsDone() ) {
				// if the current transition is finished, then jump to the destination state
				_desired_state = _current_state;
				_current_state = _current_transition.Destination;
				_current_transition = null;
			}
			if( _current_transition != null ) {
				_current_transition.UpdateGraph( remaining_weight );
			} else {
				_current_state.UpdateGraph( remaining_weight );
				if( !_current_state.IsLooping ) {
					// if the current state is not looping, change back immeditately to the previous state (don't worry the transition will know to wait for the non looping state to finish)
					ChangeState( _desired_state.name );
				}
			}
		}
		
		public State GetStateByName( string name) {
			foreach( State s in this.states ) {
				if( s.name.Equals( name , System.StringComparison.CurrentCultureIgnoreCase ) ) {
					return s;
				}
			}
			return null;
		}
		
	}
	
	public enum MixamoWrapMode {
		ClampForever,
		Loop
	}
	
	/// <summary>
	/// A State represents the current state that the character's animation is in. Each state has a list of transitions that it can make to other states, if no such list exists then
	/// it can transition to anything.
	/// </summary>
	public class State  {
		public string name;
		public TreeNode root;
		public Transition[] transitions;
		public Layer layer;
		private MixamoWrapMode initialWrapMode = MixamoWrapMode.Loop;
		private MixamoWrapMode currentWrapMode = MixamoWrapMode.Loop;
		
		public bool SyncState = true;
		
		public bool IsLooping = true;
		
		public override string ToString ()
		{
			string str = "";
			foreach( Transition t in transitions ) {
				str += t.ToString();
			}
			return string.Format ("State: " + name + "\n" + root.ToString() + "\nTransitions: " + str );
		}
		
		public TreeNode[] GetAllNodes() {
			List< TreeNode> lst = new List<TreeNode>();
			Stack<TreeNode> s = new Stack<TreeNode>();
			s.Push( root );
			TreeNode current = null;
			while( s.Count > 0 ) {
				current = s.Pop();
				lst.Add( current );
				foreach( TreeNode t in current.Children) {
					s.Push( t );
				}
			}
			return lst.ToArray();
		}
		
		private Clip[] _allNormalClips = null;
		public Clip[] GetAllNormalClips() {
			if( _allNormalClips == null ) {
				List<Clip> lst = new List<Clip>();
				foreach( TreeNode t in GetAllNodes() ) {
					if( t is Clip ) {
						Clip c = (Clip ) t;
						if( c.type == "normal" ) {
							lst.Add( c );
						}
					}
				}
				_allNormalClips = lst.ToArray();
			}
			return _allNormalClips;
		}
		
		
		public void UpdateGraph( float remaining_weight ) {
			// update the weights of the state
			root.UpdateGraph( remaining_weight );
			
			if( SyncState ) {
				// change playback speed of state
				Clip[] clps = GetAllNormalClips();
				float nspeed = 0f;
				float nweight = 0f;
				foreach( Clip c in clps ) {
					nspeed += c.anim_state.weight / c.anim_state.length;
					nweight += c.anim_state.weight;
				}
				if( nweight > 0f ) {
					float avg_nspeed = (nspeed / nweight) * SpeedControl;
					foreach( Clip c in clps ) {
						c.anim_state.speed = avg_nspeed * c.anim_state.length;
					}
				}
			} else {
				foreach( Clip c in GetAllNormalClips() ) {
					c.anim_state.speed = SpeedControl; 
				}
			}
		}
		
		
		public Transition GetTransitionTo(State next) {
			foreach( Transition t in transitions ) {
				if( t.CanBeMade( next ) ) {
					return t;
				}
			}
			return null;
		}
		
		public void ResetTime(float t) {
			root.ResetTime(t);
		}
		
		public float NormalizedTime {
			get {
				return root.NormalizedTime();
			}
		}
		
		public void SetInitialWrapMode( MixamoWrapMode s ) {
			initialWrapMode = s;
			root.SetWrapMode( s );
		}
		
		public void SetCurrentWrapMode( MixamoWrapMode s ) {
			currentWrapMode = s;
			root.SetWrapMode( s );
		}
		
		public void ResetWrapMode() {
			currentWrapMode = initialWrapMode;
			root.SetWrapMode( currentWrapMode );
		}
		public static UnityEngine.WrapMode MixamoWrapModeToUnityWrapMode( Mixamo.MixamoWrapMode s ) {
			if( s == MixamoWrapMode.ClampForever ) {
				return UnityEngine.WrapMode.ClampForever;
			} else {
				return UnityEngine.WrapMode.Loop;
			}
		}
		
		public string SpeedControlName {
			get {
				return( this.name + "_speed"  );
			}
		}
		
		public float SpeedControl {
			get {
				return( this.layer.graph.ControlParams[ this.SpeedControlName ] );
			}
		}
		
		public float AverageNormalizedSpeed = 1f;
		/// <summary>
		/// Used for normalizing the speeds of all the clips in the state - matching stride length
		/// </summary>
		/// <param name="norm">
		/// A <see cref="System.Single"/>
		/// </param>
		public void CalculateAverageNormalizedSpeed( float norm) {
			Clip[] clps = GetAllNormalClips();
			AverageNormalizedSpeed = 0f;
			foreach( Clip c in clps) {
				AverageNormalizedSpeed += c.anim_state.normalizedSpeed;
			}
			AverageNormalizedSpeed = (AverageNormalizedSpeed / clps.Length);

		}
		
		public float MaxTime {
			get {
				float MaxLen = 0f;
				foreach( Clip c in GetAllNormalClips() ) {
					MaxLen = Mathf.Max( c.anim_state.time , MaxLen );
				}
				return MaxLen;
			}
		}
		
		public float MaxTimeLength {
			get {
				float MaxLen = 0f;
				foreach( Clip c in GetAllNormalClips() ) {
					MaxLen = Mathf.Max( c.anim_state.length , MaxLen );
				}
				return MaxLen;
			}
		}
	}
	
	
	/// <summary>
	/// Some info about a clip's root motion.
	/// </summary>
	public class ClipRootMotionInfo {
		public float PreviousTime = float.NegativeInfinity;
		public Vector3 PreviousPosition = Vector3.zero;
		public Vector3 PreviousRotation = Vector3.zero;
		public Vector3 DeltaPosition = Vector3.zero;
		public Vector3 DeltaRotation = Vector3.zero;
		public float CurrentNormalizedWeight = 0f;
		
		public RootMotionExtractionType ExtractionType = RootMotionExtractionType.None;
		public RootMotionRotExtractionType RotExtractionType = RootMotionRotExtractionType.Y;
		
		// Rotations
		private Vector3 _StartRotation;
		public Vector3 StartRotation {
			get {
				return( _StartRotation );
			}
			set {
				_StartRotation = value;
				_TotalRotation = _EndRotation - _StartRotation;
			}
		}
		
		private Vector3 _EndRotation;
		public Vector3 EndRotation {
			get {
				return( _EndRotation );
			}
			set {
				_EndRotation = value;
				_TotalRotation = _EndRotation - _StartRotation;
			}
		}
		
		private Vector3 _TotalRotation;
		
		public Vector3 TotalRotation {
			get {
				return( _TotalRotation );
			}
		}
		
		// Positions
		private Vector3 _StartPosition;
		public Vector3 StartPosition {
			get {
				return( _StartPosition );
			}
			set {
				_StartPosition = value;
				_TotalPosition = _EndPosition - _StartPosition;
			}
		}
		private Vector3 _EndPosition;
		public Vector3 EndPosition {
			get {
				return( _EndPosition );
			}
			set {
				_EndPosition = value;
				_TotalPosition = _EndPosition - _StartPosition;
			}
		}
		
		private Vector3 _TotalPosition;
		
		public Vector3 TotalPosition {
			get {
				return( _TotalPosition );
			}
		}
		
		/// <summary>
		/// Computes the difference between this animation's root at the current position and what it was last time this function was called.
		/// The result is stored in DeltaPosition and DeltaRotation.
		/// </summary>
		/// <param name="CurrentPosition">
		/// A <see cref="Vector3"/>
		/// </param>
		/// <param name="CurrentRotation">
		/// A <see cref="Vector3"/>
		/// </param>
		/// <param name="CurrentTime">
		/// A <see cref="System.Single"/>
		/// </param>
		/// <param name="mode">
		/// A <see cref="WrapMode"/>
		/// </param>
		public void ComputeRootMotion( Vector3 CurrentPosition , Vector3 CurrentRotation, float CurrentTime , WrapMode mode ) {
			Vector3 dPos;
			Vector3 dRot;
			if( mode == WrapMode.Loop && ((PreviousTime - (int)PreviousTime) > (CurrentTime-(int)CurrentTime)) )
			{
				dPos = ((EndPosition - PreviousPosition) + (CurrentPosition - StartPosition));
				dRot = ((EndRotation - PreviousRotation) + (CurrentRotation - StartRotation));
			}
			else
			{
				dPos = ((CurrentPosition - PreviousPosition) );
				dRot = (CurrentRotation - PreviousRotation );
			}
			
			PreviousTime = CurrentTime;
			PreviousPosition = CurrentPosition;
			PreviousRotation = CurrentRotation;
			DeltaPosition = dPos;
			DeltaRotation = dRot;
		}
		
	}
	
	public class RootMotionSetup {
		public Transform root;
		public Transform pelvis;
		public GameObject game_obj;
		
		public RootMotionSetup( Transform r , Transform p , GameObject go ) {
			root = r;
			pelvis = p;
			game_obj = go;
			if( pelvis == null  ) {
				throw new System.Exception( "Pelvis not specified!!! " );
			}
			if( root == null  ) {
				throw new System.Exception( "Root not specified!!! " );
			}
			if(  go == null ) {
				throw new System.Exception( "Target Game Object not specified!!! " );
			}
		}
		
		public Vector3 GetPelvisPosition()
		{
			Vector3 p = pelvis.localPosition;
			return p;
		}
		
		public Vector3 GetPelvisRotation()
		{
			Vector3 p = pelvis.localEulerAngles;
			return p;
		}
		
	}
	
	public enum RootMotionExtractionType {
		None,
		Z,
		X,
		Y,
		XY,
		YZ,
		XZ,
		XYZ
	}
	
	public enum RootMotionRotExtractionType {
		None,
		X,
		Y,
		Z
	}
	
	public abstract class TreeNode {
		protected string[] controls = null;
		
		public AnimationGraph graph;
		protected TreeNode[] children;
		public Mixamo.State state;
		
		public TreeNode[] Children {
			get {
				return children;
			}
		}
		public override string ToString ()
		{
			return "Node: " + this.GetType().ToString() + " Controls: " + (controls == null ? "None" : string.Join("," , controls ) );
		}
		
		public abstract void UpdateGraph( float remaining_weight );
		
		public string Control( int i ) {
			return controls[i];
		}
		
		public float ControlValue( int i ) {
			return Mathf.Clamp01( graph.ControlParams[controls[i]] );
		}
		
		public string[] Controls {
			get {
				return controls;
			}
		}
		
		public abstract void ResetTime(float time);
		
		public abstract float NormalizedTime();
		
		public abstract void SetWrapMode( MixamoWrapMode s );
	}
	
	public class BlankClip : TreeNode {
		
		public BlankClip() {
			children = new TreeNode[0];
		}
		public override void UpdateGraph (float remaining_weight)
		{
		}
		
		public override void ResetTime (float time)
		{
		}
		
		public override float NormalizedTime ()
		{
			return 1f;
		}
		
		public override void SetWrapMode (MixamoWrapMode s)
		{
		}
	}
	
	public class Clip : TreeNode {
		
		private bool _inUse = false;
		public void MarkInUse() {
			if( _inUse ) {
			//	throw new System.Exception( string.Format( "The following clip ({0}) is already in use. A clip cannot be set to be used by multiple things." , _inUse ) );
			}
			_inUse = true;
		}
		
		public ClipRootMotionInfo RootMotion = new ClipRootMotionInfo();
		
		public string name;
		public string anim_name;
		public string type;
		public int layer;
		public AnimationState anim_state;
		
		public int? sync_clip_group = null;
		
		public Clip() {
			children = new TreeNode[0];
		}
		
		public Mixamo.AnimationGraph parent;
		
		public float UnityWeight {
			get {
				return anim_state.weight;
			}
		}
		
		public float Weight;
		
		public float NormalizedRootWeight {
			get {
				return( RootMotion.CurrentNormalizedWeight );
			}
			set {
				RootMotion.CurrentNormalizedWeight = value;
			}
		}
		
		public void SetupRootMotionDeltas() {
			
			RootMotionSetup setup = parent.rmSetup;
			Animation anim = setup.game_obj.GetComponent<Animation>();
			
			bool isEnabled = anim_state.enabled;
			WrapMode wrapMode = anim_state.wrapMode;
			
			// activate the animation state
			anim_state.weight = 1f;
			anim_state.enabled = true;
			anim_state.wrapMode = WrapMode.Clamp; // ensures the value at normalizedTime = 1f is not necessarily the same as normalizedTime = 0f
			
			// scrub to the beginning of the animation state and store initial position and rotation values
			anim_state.normalizedTime = 0f;
			anim.Sample();
			RootMotion.StartPosition = setup.GetPelvisPosition();
			RootMotion.StartRotation = setup.GetPelvisRotation();
			RootMotion.PreviousPosition = RootMotion.StartPosition;
			RootMotion.PreviousTime = 0f;
			
			// scrub to the end of the animation state and store final position and rotation values
			anim_state.normalizedTime = 1f;
			anim.Sample();
			RootMotion.EndPosition = setup.GetPelvisPosition();
			RootMotion.EndRotation = setup.GetPelvisRotation();
			// reset the clip
			anim_state.enabled = isEnabled;
			anim_state.wrapMode = wrapMode;
			anim.Sample();
			
			anim_state.weight = 0f;
			
			parent.DebugLog( string.Format( "Root Translation for {0}: {1} " , this.name , ExtendedMonoBehaviour.FormatVector( this.RootMotion.TotalPosition )  ) );
			parent.DebugLog( string.Format( "Root Rotation for {0}: {1}" , this.name , ExtendedMonoBehaviour.FormatVector( this.RootMotion.TotalRotation )  ) );
		}
		
		public override void ResetTime(float time) {
			RootMotion.PreviousPosition = RootMotion.StartPosition;
			RootMotion.PreviousTime = 0f;
			
			anim_state.normalizedTime = time;
		}
		
		public override void UpdateGraph (float remaining_weight)
		{
			anim_state.weight = remaining_weight;
			if( state != null ) {
				anim_state.normalizedSpeed = state.SpeedControl;
			}
		}
		
		public override float NormalizedTime ()
		{
			return( anim_state.normalizedTime );
		}
		
		public override void SetWrapMode (MixamoWrapMode s)
		{
			if( anim_state.normalizedTime > 1f && s == MixamoWrapMode.ClampForever && anim_state.wrapMode == State.MixamoWrapModeToUnityWrapMode(MixamoWrapMode.Loop) ) {
				anim_state.normalizedTime = anim_state.normalizedTime - (int)anim_state.normalizedTime;
			}
			anim_state.wrapMode = State.MixamoWrapModeToUnityWrapMode( s );
		}
		
	}
	
	public abstract class BlendNode: TreeNode {
	}
	
	public class Blend2d : BlendNode {
		public TreeNode blend1 {
			get {
				return children[0];
			}
		}
		public TreeNode blend2 {
			get {
				return children[1];
			}
		}
		
		public Blend2d( TreeNode b1 , TreeNode b2 , string cp) {
			children = new TreeNode[2];
			children[0] = b1;
			children[1] = b2;
			controls = new string[1];
			controls[0] = cp;
		}
		
		public override void UpdateGraph (float remaining_weight)
		{
			float c = ControlValue(0);
			blend1.UpdateGraph( remaining_weight * ( c ) );
			blend2.UpdateGraph( remaining_weight * ( 1-c ) );
		}
		
		public override void ResetTime (float t)
		{
			blend1.ResetTime(t);
			blend2.ResetTime(t);
		}
		
		public override float NormalizedTime ()
		{
			return( (blend1.NormalizedTime() + blend2.NormalizedTime()) / 2.0f );
		}
		
		public override void SetWrapMode (MixamoWrapMode s)
		{
			blend1.SetWrapMode( s );
			blend2.SetWrapMode( s) ;
		}
		
	}
	
	/*public class ListBlend: BlendNode {
		public TreeNode[] blends {
			get {
				return children;
			}
		}
	}*/
	
	public class AdditiveBlend: BlendNode {
		
		public Clip difference_clip {
			get {
				return (Clip)children[0];
			}
		}
		public TreeNode blend {
			get {
				return children[1];
			}
		}
		
		public AdditiveBlend( Clip c , TreeNode b , string blend_control , string add_control ) {
			children = new TreeNode[2];
			children[0] = c;
			children[1] = b;
			controls = new string[2];
			controls[0] = blend_control;
			controls[1] = add_control;
		}
		
		public override void UpdateGraph (float remaining_weight)
		{
			float c = ControlValue(0);
			difference_clip.anim_state.normalizedTime = ControlValue(1);
			difference_clip.UpdateGraph( remaining_weight * c );
			blend.UpdateGraph( remaining_weight *  (1-c) );
		}
		
		public override void ResetTime (float t)
		{
			difference_clip.ResetTime(t);
			blend.ResetTime(t);
		}
		
		public override float NormalizedTime ()
		{
			return( blend.NormalizedTime() );
		}
		
		public override void SetWrapMode (MixamoWrapMode s)
		{
			blend.SetWrapMode( s);
		}
	}
	
	public class AnimationGraph  {
		
		public Mixamo.TransitionHandler TransitionHandler;
		
		public Mixamo.RootMotionSetup rmSetup;
		
		public string name;
		public Layer[] layers;
		public Clip[] clips;
		
		public string rootPath;
		
		ControlParameters control_params;
		
		public void DebugLog( string str ) {
		}
		
		public ControlParameters ControlParams {
			get {
				return control_params;
			}
		}
		
		public Clip GetClipByName( string name ) {
			foreach( Clip c in clips ) {
				if( c.name.Equals( name ) ) {
					return c;
				}
			}
			return null; //throw new System.Exception( "Error: can not find the associated clip: " + name );
		}
		
		public Clip GetClipByNameAndMarkInUse( string name , State parent) {
			Clip c = GetClipByName(name );
			c.MarkInUse();
			c.state = parent;
			return c;
		}
		
		public Clip GetClipByAnimName( string anim_name ) {
			foreach( Clip c in clips ) {
				if( c.anim_name.Equals( anim_name ) ) {
					return c;
				}
			}
			return null; //throw new System.Exception( "Error: can not find the associated animation: " + anim_name );
		}
		
		public void Init() {
			foreach( Layer l in layers ) {
				l.Init();
			}
			
			// gather control params
			List<string> lst = new List<string>();
			List<float> vals = new List<float>();
			foreach( Layer l in layers ) {
				foreach( State s in l.states ) {
					if( s.SpeedControlName != null ) {
						if( lst.Contains( s.SpeedControlName ) ) {
							Debug.LogError( "Multiple control params with the same name exist: " + s.SpeedControlName );
						}
						lst.Add( s.SpeedControlName );
						vals.Add( 1f );
					}
					foreach( TreeNode t in s.GetAllNodes() ) {
						t.graph = this;
						if( t.Controls != null ) {
							foreach( string c in t.Controls ) {
								if( lst.Contains( c ) ) {
									Debug.LogError( "Multiple control params with the same name exist: " + c );
								}
								lst.Add( c );
								vals.Add( 0f );
							}
						}
					}
					s.SetInitialWrapMode( s.IsLooping ? MixamoWrapMode.Loop : MixamoWrapMode.ClampForever  );
				}
			}
			control_params = new ControlParameters( lst.ToArray() , vals.ToArray() );
		}
		
		public override string ToString ()
		{
			string str =  "AnimationGraph: " + name + "\n";
			str += "Layers: \n";
			foreach( Layer l in layers ) {
				str += l.ToString() + "\n";
			}
			
			return str;
		}
		
		public void UpdateGraph() {
			// zero out weights
			foreach( Clip c in clips ) {
				c.anim_state.weight = 0f;
			}
			
			foreach( Layer l in layers ) {
				l.UpdateGraph(1f);
			}
		}
		
		public bool IsLoaded {
			get {
				return( this.layers != null && this.layers.Length > 0 && this.clips.Length > 0 );
			}
		}
		
		public static Mixamo.AnimationGraph FromJson( Hashtable json_graph , AnimationStateMachine asm ) {
			
			Mixamo.AnimationGraph graph = new Mixamo.AnimationGraph();
			graph.name = (string) json_graph["name"];
			graph.rootPath = (string) json_graph["root_path"];
			
			graph.rmSetup = new Mixamo.RootMotionSetup( asm.transform , asm.Target.transform.Find( graph.rootPath ) , asm.Target );
			
			ArrayList json_clips = (ArrayList) json_graph["clips"];
			graph.clips = new Mixamo.Clip[json_clips.Count];
			
			foreach( AnimationState s in asm.Target.GetComponent<Animation>() ) {
				s.weight = 0f;
			}
			
			Dictionary<int , List<Clip>> sync_groups = new Dictionary<int, List<Clip>>();
			
			// Clips
			for( int i=0;i<json_clips.Count;i++ ) {
				Hashtable json_clip = (Hashtable)json_clips[i];
				Mixamo.Clip c = (graph.clips[i] = new Mixamo.Clip() );
				c.name = (string) json_clip["name"];
				c.type = (string) json_clip["type"];
				
				if( json_clip.ContainsKey("sync_clip_group" ) ) {
					c.sync_clip_group = int.Parse( json_clip["sync_clip_group"].ToString() );
				}
				c.anim_name = (string) json_clip["anim_name"];
				//c.layer = 0;
				if( json_clip.ContainsKey( "layer" ) ) {
					c.layer = int.Parse( json_clip["layer"].ToString() );
				} else {
					c.layer = 0;
				}
				c.parent = graph;
			
				if( json_clip.ContainsKey( "root_motion_translation" ) ) {
					switch( json_clip["root_motion_translation"].ToString() ) {
					case "z":
						c.RootMotion.ExtractionType = RootMotionExtractionType.Z;
						break;
					case "x":
						c.RootMotion.ExtractionType = RootMotionExtractionType.X;
						break;
					case "y":
						c.RootMotion.ExtractionType = RootMotionExtractionType.Y;
						break;
					case "xz":
						c.RootMotion.ExtractionType = RootMotionExtractionType.XZ;
						break;
					case "xy":
						c.RootMotion.ExtractionType = RootMotionExtractionType.XY;
						break;
					case "yz":
						c.RootMotion.ExtractionType = RootMotionExtractionType.YZ;
						break;
					case "xyz":
						c.RootMotion.ExtractionType = RootMotionExtractionType.XYZ;
						break;
					case "":
						c.RootMotion.ExtractionType = RootMotionExtractionType.None;
						break;
					default:
						throw new System.Exception( "Invalid Root Motion Translation Type: '" + json_clip["root_motion_translation"].ToString() + "'! Valid types: x,y,z,xy,xz,yz,xyz" );
					}
				}
				
				if( json_clip.ContainsKey( "root_motion_rotation" ) && json_clip["root_motion_rotation"].ToString()  != "" ) {
				
					switch( json_clip["root_motion_rotation"].ToString() ) {
					case "x":
						c.RootMotion.RotExtractionType = RootMotionRotExtractionType.X;
						break;
					case "y":
						c.RootMotion.RotExtractionType = RootMotionRotExtractionType.Y;
						break;
					case "z":
						c.RootMotion.RotExtractionType = RootMotionRotExtractionType.Z;
						break;
					default:
						throw new System.Exception( "Invalid Root Motion Rotation Type: " + json_clip["root_motion_rotation"].ToString() + "! Valid types: xyz" );
					}
				} else {
					c.RootMotion.RotExtractionType = RootMotionRotExtractionType.None;
				}
				
				// setup animation state clip
				AnimationState s = asm.Target.GetComponent<Animation>()[c.anim_name];
				s.wrapMode = WrapMode.Loop;
				if( c.type == "additive" )
					s.blendMode = AnimationBlendMode.Additive;
				else
					s.blendMode = AnimationBlendMode.Blend;
				s.layer = c.layer;
				s.weight = 0f; 
				if( json_clip.ContainsKey("mixing_transform") ) {
					Transform t = asm.Target.transform.Find( json_clip["mixing_transform"].ToString() );
					s.AddMixingTransform( t , true);
				}
				s.enabled = true;
				c.anim_state = s;
				
				if( c.sync_clip_group != null ) {
					int grp = (int)c.sync_clip_group;
					if( !sync_groups.ContainsKey( grp ) ) {
						sync_groups.Add( grp , new List<Clip>() );
					}
					sync_groups[ grp ].Add( c );
				}
			}
			
			// sync any layers that need to be synced together
			/*foreach( List<Clip> lst in sync_groups.Values ) {
				float sum_speed = 0;
				foreach( Clip c in lst ) {
					sum_speed += c.anim_state.normalizedSpeed;
				}
				
				float avg_ns = sum_speed / lst.Count;
				foreach( Clip c in lst ) {
					c.anim_state.normalizedSpeed = avg_ns;
				}
			}*/
			
			
			for(int i=0;i<graph.clips.Length;i++) {
				graph.clips[i].SetupRootMotionDeltas();
			}
			
			List<AnimationClip> clipsToDelete = new List<AnimationClip>();
			foreach( AnimationState s in asm.Target.GetComponent<Animation>() ) {
				if( graph.GetClipByAnimName( s.name )  == null ) {
					Debug.LogWarning( "Did not load the animation clip: " + s.name + ". Removing it!" );
					clipsToDelete.Add( s.clip );
				}
			}
			
			
			foreach( AnimationClip c in clipsToDelete ) {
				asm.Target.GetComponent<Animation>().RemoveClip( c );
			}
			
			
			// Layers
			ArrayList json_layers = (ArrayList)json_graph["layers"];
			//please support more than one with this line removed!
/*			if( json_layers.Count != 1 ) {
				throw new System.Exception( "Only supports 1 layer thus far!" );
			}*/
			graph.layers = new Mixamo.Layer[json_layers.Count];
			for( int i=0;i<json_layers.Count;i++) {
				// Layer
				Hashtable json_layer = (Hashtable)json_layers[i];
				Mixamo.Layer layer = (graph.layers[i] = new Mixamo.Layer());
				layer.name = (string)json_layer["name"];
				layer.priority = int.Parse( json_layer["priority"].ToString() );
				layer.graph = graph;
				
				// States
				ArrayList json_states = (ArrayList)json_layer["states"];
				layer.states = new State[json_states.Count];
				for(int j=0;j<json_states.Count;j++) {
					// State
					Hashtable json_state = (Hashtable)json_states[j];
					State state = (layer.states[j] = new State());
					state.name = (string)json_state["name"];
					state.layer = layer;
					state.IsLooping = json_state.ContainsKey("is_looping") ? bool.Parse( json_state["is_looping"].ToString() ) : true;
					state.root = graph.JsonToTreeNode( (Hashtable)json_state["tree"] , state );
				}
				
				// Transitions
				for(int j=0;j<json_states.Count;j++ ) {
					Hashtable json_state = (Hashtable)json_states[j];
					State s = layer.states[j];
					if( json_state.ContainsKey( "transitions" ) ) {
						ArrayList json_transitions = (ArrayList) json_state["transitions"];
						s.transitions = new Transition[ json_transitions.Count];
						for( int k=0; k < json_transitions.Count;k++) {
							Hashtable json_transition = (Hashtable)json_transitions[k];
							Transition t;
							State dest = ( json_transition["destination"].ToString() == "*" ? null : layer.GetStateByName( json_transition["destination"].ToString() ) );
							string[] guards;
							if( json_transition.ContainsKey( "guards" ) ) {
								ArrayList arr = (ArrayList) json_transition["guards"];
								guards = new string[arr.Count];
								for(int a=0;a<arr.Count;++a) { guards[a] = arr[a].ToString(); }
							} else {
								guards = new string[0] {};
							}
							if( json_transition["type"].ToString() == "crossfade" ) {
								t = new CrossfadeTransition( s , dest , (float)(double)json_transition["duration"] , guards );
							} else if( json_transition["type"].ToString() == "clip" ) {
								t = new ClipTransition( graph.GetClipByNameAndMarkInUse( json_transition["clip"].ToString() , null ) , s , dest , (float)(double) json_transition["duration_in"] , float.Parse(json_transition["duration_out"].ToString() ) , guards );
							} else {
								throw new System.Exception( "Transition type not supported: " + json_transition["type"] );
							}
							if( !s.IsLooping )
								t.WaitTillEnd = true;
							s.transitions[k] = t;
						}
					} else {
						s.transitions = new Transition[1];
						s.transitions[0] = layer.CreateDefaultTransition( s );
						if( !s.IsLooping )
							s.transitions[0].WaitTillEnd = true;
					}
				}
			}
			
			graph.Init();
			return graph;
		}
		
		private TreeNode JsonToTreeNode( Hashtable json_tree  , State parent ) {
			string str = (string) json_tree["type"];
			switch( str ) {
			case "blank":
				return( new BlankClip() );
			case "clip":
				return( this.GetClipByNameAndMarkInUse( (string) json_tree["name"] , parent) );
			case "blend2d":
				Blend2d sb = new Blend2d(JsonToTreeNode( (Hashtable) json_tree["blend1"] , parent ) , JsonToTreeNode( (Hashtable) json_tree["blend2"] , parent ) , (string) json_tree["control"] );
				sb.state = parent;
				return sb;
			/*case "list":
				ListBlend lb = new ListBlend();
				lb.control = (string) json_tree["control"];
				ArrayList json_blends = (ArrayList) json_tree["blends"];
				lb.blends = new TreeNode[json_blends.Count];
				for( int i=0;i<json_blends.Count;i++ ) {
					lb.blends[i] = JsonToTreeNode( (Hashtable) json_blends[i] );
				}
				return lb;
				break;*/
			case "additive":
				AdditiveBlend ab = new AdditiveBlend( 
				                                     (Clip) JsonToTreeNode( (Hashtable) json_tree["difference_clip"] , parent ) , 
				                                     JsonToTreeNode( (Hashtable) json_tree["blend"], parent) , 
				                                     (string) json_tree["control"],
				                                     (string) json_tree["additive_control"]
				                                     );
				ab.state = parent;
				return ab;
			default:
				throw new System.Exception( "Could not create" );
			}
		}
		
	}
}
