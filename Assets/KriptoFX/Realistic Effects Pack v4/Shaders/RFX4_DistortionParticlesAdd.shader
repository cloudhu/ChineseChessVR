// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with '_Object2World'

Shader "KriptoFX/RFX4/DistortionParticlesAdditive" {
	Properties {
		[HDR]_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_FrontColor ("Front Color", Range(0, 1)) = 0.2
		_AlphaDistort ("Alpha Distort", Range(0, 1)) = 0
		_MainTex ("Main Texture", 2D) = "black" {}
		_BumpTex ("Normalmap (RG) & Alpha (A)", 2D) = "black" {}
		_BumpAmt ("Distortion", Float) = 10
		_InvFade ("Soft Particles Factor", Float) = 0.5
	}

	Category {

		Tags { "Queue"="Transparent"  "IgnoreProjector"="True"  "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off 
		Lighting Off 
		ZWrite Off 
		Fog { Mode Off}

		SubShader {
		Pass {
				//Name "BASE"
				Tags { "LightMode" = "Always" }
				Blend SrcAlpha One
				
				CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_particles
#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord: TEXCOORD0;
					half4 color : COLOR;
					half4 normal : NORMAL;
				};

				struct v2f {
					float4 vertex : POSITION;
					float2 uvmain : TEXCOORD2;
					half4 color : COLOR;
	#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD3;
	#endif
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _TintColor;

				v2f vert (appdata_t v)
				{
					v2f o;
#if UNITY_VERSION >= 550
					o.vertex = UnityObjectToClipPos(v.vertex);
#else 
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
	#ifdef SOFTPARTICLES_ON
					o.projPos = ComputeScreenPos (o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
	#endif
					o.color = v.color;
					o.uvmain.rg = TRANSFORM_TEX( v.texcoord, _MainTex);
					return o;
				}

				sampler2D _CameraDepthTexture;
				float _InvFade;

				half4 frag( v2f i ) : COLOR
				{
	#ifdef SOFTPARTICLES_ON
					float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
					float partZ = i.projPos.z;
					float fade = saturate (_InvFade * (sceneZ-partZ));
					float fadeStep = step(0.001, _InvFade);
					i.color.a *= lerp(1, fade, fadeStep);
	#endif

					half4 tex = tex2D( _MainTex, i.uvmain.rg);
					return tex * _TintColor * 5 * i.color.a;
				}
				ENDCG
			}

			GrabPass {							
				//"_GrabTexture"
			}
			Pass {
				//Name "BASE"
				Tags { "LightMode" = "Always" }
				
				CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_particles
#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord: TEXCOORD0;
					half4 color : COLOR;
				};

				struct v2f {
					float4 vertex : POSITION;
					float4 uvgrab : TEXCOORD0;
					float2 uvbump : TEXCOORD1;
					float2 uvmain : TEXCOORD2;
					half4 color : COLOR;
	#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD3;
	#endif
				};

				sampler2D _MainTex;
				sampler2D _BumpTex;
				float _BumpAmt;
				sampler2D _GrabTexture;
				float4 _GrabTexture_TexelSize;
				float4 _BumpTex_ST;
				float4 _MainTex_ST;
				float4 _TintColor;
				float _FrontColor;
				float _AlphaDistort;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
	#ifdef SOFTPARTICLES_ON
					o.projPos = ComputeScreenPos (o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
	#endif
					o.color = v.color;
	#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
	#else
					float scale = 1.0;
	#endif
					o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
					o.uvgrab.zw = o.vertex.w;
#if UNITY_SINGLE_PASS_STEREO
					o.uvgrab.xy = TransformStereoScreenSpaceTex(o.uvgrab.xy, o.uvgrab.w);
#endif
					o.uvgrab.z /= distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex));
					o.uvbump = TRANSFORM_TEX( v.texcoord, _BumpTex );
					o.uvmain = TRANSFORM_TEX( v.texcoord, _MainTex );

					return o;
				}

				sampler2D _CameraDepthTexture;
				float _InvFade;

				half4 frag( v2f i ) : COLOR
				{
	#ifdef SOFTPARTICLES_ON
					float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
					float partZ = i.projPos.z;
					float fade = saturate (_InvFade * (sceneZ-partZ));
					float fadeStep = step(0.001, _InvFade);
					i.color.a *= lerp(1, fade, fadeStep);
	#endif

					half3 bump = UnpackNormal(tex2D( _BumpTex, i.uvbump));
					//half alphaBump = abs(bump.z-1)*255;
					half alphaBump = (abs(bump.r + bump.g) - 0.01) * 25;
					clip(alphaBump - 0.1);
					half2 offset = lerp(bump.rg, bump.rg, _AlphaDistort);

					offset = offset * _BumpAmt * _GrabTexture_TexelSize.xy * i.color.a * _TintColor.a;
					i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
					float4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
					col.a = saturate(col.a * alphaBump);
					return col;
				}
				ENDCG
			}
		}
	}
}
