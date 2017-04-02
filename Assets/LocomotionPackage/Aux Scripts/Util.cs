using UnityEngine;
using System.Collections;

namespace Mixamo {
	public class Util {
	
		public static float CrossFadeUp( float weight , float fade_time ) {
			return( Mathf.Clamp01( weight + (Time.deltaTime / fade_time ) ) );
		}
		
		public static float CrossFadeDown( float weight , float fade_time ) {
			return( Mathf.Clamp01( weight - (Time.deltaTime / fade_time ) ) );
		}
		
		public static void CrossFade( ref float weight_in , ref float weight_out , float fade_time ) {
			float f = (Time.deltaTime / fade_time );
			weight_in = Mathf.Clamp01( weight_in + f );
			weight_out = Mathf.Clamp01( weight_out - f );
		}
	}
	
}