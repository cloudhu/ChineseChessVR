Shader "KriptoFX/RFX4/Decal/BlendedMobile" {
	Properties {
	[HDR]_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,1)
	_MainTex ("Main Texture", 2D) = "gray" {}
	}
	Subshader {
		Tags {"Queue"="Transparent"}
		Pass {
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Offset -1, -1
 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			half4 _TintColor;
			sampler2D _MainTex;

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD1;
			};
			
			float4 _MainTex_ST;


			v2f vert (appdata_t v)
			{
				v2f o;
#if UNITY_VERSION >= 550
				o.vertex = UnityObjectToClipPos(v.vertex);
#else 
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				half4 tex =  tex2D(_MainTex, i.texcoord);
				half4 res = tex * tex * _TintColor;
				return res;
			}
			ENDCG
		}
	}
}
