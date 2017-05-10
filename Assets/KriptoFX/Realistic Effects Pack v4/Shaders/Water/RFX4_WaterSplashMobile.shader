// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with '_Object2World'

Shader "KriptoFX/RFX4/WaterSplashMobile" {
Properties {
        _TintColor ("Main Color", Color) = (1,1,1,1)
		_RimColor("Rim Color", Color) = (1,1,1,0.5)
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _FPOW("FPOW Fresnel", Float) = 5.0
        _R0("R0 Fresnel", Float) = 0.05
		_BumpAmt ("Distortion Scale", Float) = 10
}
Category {
	
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
				Blend SrcAlpha OneMinusSrcAlpha
				ZWrite Off
				Cull Back

	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DISTORT_ON DISTORT_OFF

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			sampler2D _BumpMap;
			sampler2D _GrabTextureMobile;

			half  _GrabTextureMobileScale;
			float4 _GrabTextureMobile_TexelSize;
			half4 _TintColor;
			half4 _RimColor;
			float4 _BumpMap_ST;

			half _BumpAmt;
			half _FPOW;
			half _R0;
			
			struct appdata_t {
				float4 vertex : POSITION;
#if DISTORT_ON
				float3 normal : NORMAL;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
#endif
			};

			struct v2f {
				float4 vertex : SV_POSITION;
#if DISTORT_ON
				half2 uv_BumpMap : TEXCOORD0;
				float4 uvgrab : TEXCOORD1;
				half fresnel : TEXCOORD2;
				half4 color : COLOR;
#endif
			};

			v2f vert (appdata_full v)
			{

				v2f o;
#if UNITY_VERSION >= 550
				o.vertex = UnityObjectToClipPos(v.vertex);
#else 
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
#if DISTORT_OFF
				return o;
#else
				o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*_ProjectionParams.x) + o.vertex.w) * 0.5;
				o.uvgrab.zw = o.vertex.w;
#if UNITY_SINGLE_PASS_STEREO
				o.uvgrab.xy = TransformStereoScreenSpaceTex(o.uvgrab.xy, o.uvgrab.w);
#endif
				o.uvgrab.z /= distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex));

				o.uv_BumpMap = TRANSFORM_TEX(v.texcoord, _BumpMap);
				o.color = v.color;
				o.color.rgb *= _LightColor0.rgb * _LightColor0.w;
				o.fresnel = (1 - dot(normalize(v.normal), normalize(ObjSpaceViewDir(v.vertex))));
				o.fresnel = pow(o.fresnel, _FPOW);
				o.fresnel = saturate(_R0 + (1.0 - _R0) * o.fresnel);

				return o;
#endif
			}

			half4 frag (v2f i) : SV_Target
			{ 
#if DISTORT_OFF
				discard;
			return 0;
#else
				half3 normal = UnpackNormal(tex2D(_BumpMap, i.uv_BumpMap));
				half2 offset = normal.rg * _BumpAmt * _GrabTextureMobile_TexelSize.xy * i.color.a * _GrabTextureMobileScale;
				i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
				half4 col = tex2Dproj(_GrabTextureMobile, UNITY_PROJ_COORD(i.uvgrab));
				
				half3 emission = col.xyz * _TintColor.xyz + col.rgb * i.fresnel *_RimColor * i.color.rgb * _RimColor.a * 3;
				return half4 (emission, _TintColor.a * i.color.a);
#endif
			}
			ENDCG 
		}
	}	
}
}