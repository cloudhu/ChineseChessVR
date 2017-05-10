// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with '_Object2World'

Shader "KriptoFX/RFX4/Portal/PortalSky" {
	Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,1)
	_TurbulenceMask ("Turbulence Mask", 2D) = "white" {}
	_Cube ("Environment Map", Cube) = "" {}
	_NoiseScale("Noize Scale (XYZ) Height (W)", Vector) = (1, 1, 1, 0.2)
}
Category {
	
	Tags { "Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent" }
				Blend SrcAlpha OneMinusSrcAlpha
				Cull Off 
				Lighting Off 
				ZWrite Off

	SubShader {
	
	Stencil {
		Ref 2
		Comp Equal
		Pass Keep
		Fail Keep
	}
		Pass {
				

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
#pragma target 3.0
			#include "UnityCG.cginc"

			half4 _TintColor;
			samplerCUBE _Cube;   
			float4 _NoiseScale;
			sampler2D _TurbulenceMask;

			struct appdata_t {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float3 viewDir : TEXCOORD1;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;

				float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float4 coordNoise = float4(wpos * _NoiseScale.xyz, 0);
				float4 tex1 = tex2Dlod (_TurbulenceMask, coordNoise + float4(_Time.x*3, _Time.x * 5, _Time.x * 2.5, 0));
				v.vertex.xyz += v.normal* 0.005 + tex1.rgb * _NoiseScale.w - _NoiseScale.w/2;

#if UNITY_VERSION >= 550
				o.vertex = UnityObjectToClipPos(v.vertex);
#else 
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
				o.viewDir = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
				return o;
			}

			half4 frag (v2f i) : SV_Target
			{
				float4 cubeTex = texCUBE(_Cube, i.viewDir)*_TintColor;
				return cubeTex;
			}
			ENDCG 
		}
	}	
}
}
