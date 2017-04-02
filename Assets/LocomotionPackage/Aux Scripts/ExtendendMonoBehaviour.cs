using UnityEngine;
using System.Collections;

public class ExtendedMonoBehaviour : MonoBehaviour {
	
	
	public static GameObject AddVisualGuide( Transform t ) {
		return AddVisualGuide( t , Color.white );
	}
	
	public static GameObject AddVisualGuide( Transform t , Color c ) {
		GameObject gos = GameObject.CreatePrimitive( PrimitiveType.Sphere );
		gos.transform.parent = t;
		gos.transform.localPosition = Vector3.zero;
		gos.transform.localEulerAngles = Vector3.zero;
		gos.transform.localScale = Vector3.one * 0.125f;
		Material m = new Material( Shader.Find( "Transparent/Diffuse" ) );
		m.color = new Color( c.r , c.g , c.b , 0.5f );
		gos.GetComponent<MeshRenderer>().material = m;
		return gos;
	}
	
	public static string FormatVector( Vector3 v) {
		return( string.Format( "{0:0.0000},{1:0.0000},{2:0.0000}", v.x , v.y , v.z ) );
	}
	
	public static string FormatFloat( float f ) {
		return string.Format( "{000:0.00}" , f );
	}
}
