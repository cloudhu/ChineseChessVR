// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with '_Object2World'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "KriptoFX/RFX4/Decal/IceMobile" {
	Properties{
			_Color("Main Color", Color) = (1,1,1,1)
			[HDR]_ReflectColor("Reflection Color", Color) = (1,1,1,0.5)
			_MainTex("Base (RGB) Emission Tex (A)", 2D) = "white" {}
			_Cube("Reflection Cubemap", Cube) = "" {}
			_BumpMap("Normalmap", 2D) = "bump" {}
			_FPOW("FPOW Fresnel", Float) = 5.0
			_R0("R0 Fresnel", Float) = 0.05
			_Cutoff("Cutoff", Range(0, 1)) = 0.5
			_BumpAmt("Distortion", range(0,1500)) = 10
	}

		Category{

					Tags{ "Queue" = "Transparent"  "IgnoreProjector" = "True"  "RenderType" = "Transparent" }
					Blend SrcAlpha OneMinusSrcAlpha
					Cull Off
					Lighting Off
					ZWrite Off
					Fog{ Mode Off }
					Offset -1, -1

					SubShader{
					Pass{
				//Name "BASE"
				//Tags { "LightMode" = "Always" }

				CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma fragmentoption ARB_precision_hint_fastest
	#pragma multi_compile DISTORT_ON DISTORT_OFF
	#include "UnityCG.cginc"

				struct appdata_t {
				float4 vertex : POSITION;
	#if DISTORT_ON
				float2 texcoord: TEXCOORD0;
				half4 color : COLOR;
				float3 normal : NORMAL;
	#endif
			};

			struct v2f {
				float4 vertex : SV_POSITION;
	#if DISTORT_ON
				float2 uv_Main : TEXCOORD0;
				float4 uv_BumpFresnel : TEXCOORD1;
				float4 uvgrab : TEXCOORD2;
				float3 refl : TEXCOORD3;
	#endif
			};

			sampler2D _MainTex;
			sampler2D _BumpMap;
			float4 _MainTex_ST;
			float4 _BumpMap_ST;
			samplerCUBE _Cube;

			half4 _Color;
			half4 _ReflectColor;
			half _FPOW;
			half _R0;
			half _Cutoff;
			half _BumpAmt;
			sampler2D _GrabTextureMobile;
			half  _GrabTextureMobileScale;
			float4 _GrabTextureMobile_TexelSize;

			v2f vert(appdata_t v)
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

				o.uv_Main = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv_BumpFresnel.xy = TRANSFORM_TEX(v.texcoord, _BumpMap);
				o.uv_BumpFresnel.zw = 1;
				o.uv_BumpFresnel.z = (1 - dot(normalize(v.normal), normalize(ObjSpaceViewDir(v.vertex))));
				o.uv_BumpFresnel.z = pow(o.uv_BumpFresnel.z, _FPOW);
				o.uv_BumpFresnel.z = saturate(_R0 + (1.0 - _R0) * o.uv_BumpFresnel.z);
				float3 viewDir = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
				float3 normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				o.refl = reflect(viewDir, normalDir);
				return o;
	#endif
			}

			half4 frag(v2f i) : SV_Target
			{
	#if DISTORT_OFF
				discard;
			return 0;
	#else
			half3 bump = UnpackNormal(tex2D(_BumpMap, i.uv_BumpFresnel.xy));
			half2 offset = bump.rg * _BumpAmt * _GrabTextureMobile_TexelSize.xy * _GrabTextureMobileScale;
			half4 tex = tex2D(_MainTex, i.uv_Main + offset / 10);
			half4 c = tex * _Color;
			half reflcol = dot(texCUBE(_Cube, i.refl*(bump*2-1)), 0.33);
			reflcol *= tex.a;
			reflcol = lerp(c, reflcol, i.uv_BumpFresnel.z);
			
			i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
			half4 col = tex2Dproj(_GrabTextureMobile, UNITY_PROJ_COORD(i.uvgrab));
			col.rgb = col * _Color + reflcol * _ReflectColor.rgb * col * 4 * _Cutoff;
			col.a = tex.a * saturate(step(1 - tex.a, _Cutoff));
			return col;
	#endif
			}
				ENDCG
			}
			}
			}

}
