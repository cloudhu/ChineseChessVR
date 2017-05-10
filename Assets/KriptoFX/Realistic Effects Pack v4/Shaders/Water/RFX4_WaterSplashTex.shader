// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with '_Object2World'

// Upgrade NOTE: replaced '_Object2World' with '_Object2World'

Shader "KriptoFX/RFX4/WaterSplashTex" {
Properties {
        [HDR]_TintColor ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Main Tex (RGB) Alpha (A)", 2D) = "black" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
		_BumpAmt ("Distortion Scale", Float) = 10
}
Category {
	
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite Off
				Cull Back

	SubShader {
		GrabPass {							
			"_GrabTexture"
 		}
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
#include "Lighting.cginc"

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _GrabTexture;

			float4 _GrabTexture_TexelSize;
			half4 _TintColor;
			float4 _BumpMap_ST;
			float4 _MainTex_ST;

			float _BumpAmt;
			
			struct appdata_t {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				half4 vertex : SV_POSITION;
				half2 uv_MainTex : TEXCOORD0;
				half2 uv_BumpMap : TEXCOORD1;
				half4 uvgrab : TEXCOORD2;
				half4 color : COLOR;
			};

			v2f vert (appdata_full v)
			{
				v2f o;
#if UNITY_VERSION >= 550
				o.vertex = UnityObjectToClipPos(v.vertex);
#else 
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
#if UNITY_UV_STARTS_AT_TOP
				half scale = -1.0;
#else
				half scale = 1.0;
#endif
				o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
				o.uvgrab.zw = o.vertex.w;
#if UNITY_SINGLE_PASS_STEREO
				o.uvgrab.xy = TransformStereoScreenSpaceTex(o.uvgrab.xy, o.uvgrab.w);
#endif
				o.uvgrab.z /= distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex));

				o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_BumpMap = TRANSFORM_TEX(v.texcoord, _BumpMap);
				o.color = v.color;
				o.color.rgb *= _LightColor0.rgb * _LightColor0.w;
				
				return o;
			}

			half4 frag (v2f i) : SV_Target
			{
				half4 tex = tex2D(_MainTex, i.uv_MainTex);
			    clip(tex.a - 0.1);
				half3 normal = UnpackNormal(tex2D(_BumpMap, i.uv_BumpMap));
				half2 offset = normal.rg * _BumpAmt * _GrabTexture_TexelSize.xy * i.color.a * i.uvgrab.z;
				i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
				half4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
				half3 emission = col.xyz + col.rgb * i.color.rgb * _TintColor.rgb * tex.rgb;
				return half4 (emission, _TintColor.a * i.color.a * tex.a);
			}
			ENDCG 
		}
	}	
}
}