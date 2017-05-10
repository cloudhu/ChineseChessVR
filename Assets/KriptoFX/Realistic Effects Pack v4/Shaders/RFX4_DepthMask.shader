Shader "KriptoFX/RFX4/Decal/DepthMask" {
	Properties{
		_Cutoff("Cutoff", Range(0,1)) = 0
		_Mask("Mask", 2D) = "white" {}
	}
	SubShader{
			Tags{ "Queue" = "Geometry-1" }
			//LOD 200
				ColorMask 0
				ZWrite On
			CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard

	sampler2D _Mask;
	float _Cutoff;

		struct Input {
			float2 uv_Mask;
		};

		half _Glossiness;
		half _Metallic;
		half4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			float4 tex = tex2D(_Mask, IN.uv_Mask);
			clip(tex.r - _Cutoff);
		}
		ENDCG
	}
	SubShader{
		// Render the mask after regular geometry, but before masked geometry and
		// transparent things.

		Tags{ "Queue" = "Geometry-1" }

		// Don't draw in the RGBA channels; just the depth buffer

		ZWrite On
			ColorMask 0

		// Do nothing specific in the pass:

			Pass{

			CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"
			
		sampler2D _Mask;
		float _Cutoff;

		struct appdata_t {
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			float2 texcoord : TEXCOORD0;
		};

		float4 _Mask_ST;

		v2f vert(appdata_t v)
		{
			v2f o;
#if UNITY_VERSION >= 550
			o.vertex = UnityObjectToClipPos(v.vertex);
			
#else 
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
			o.texcoord = TRANSFORM_TEX(v.texcoord, _Mask);
			UNITY_TRANSFER_FOG(o,o.vertex);
			return o;
		}

		half4 frag(v2f i) : SV_Target
		{
			float4 tex = tex2D(_Mask, i.texcoord);
			clip(tex.r - _Cutoff);
			return 0;
		}
			ENDCG
		}

	}
}
