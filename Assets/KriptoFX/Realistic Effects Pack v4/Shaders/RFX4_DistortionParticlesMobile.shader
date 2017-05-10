// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with '_Object2World'

Shader "KriptoFX/RFX4/DistortionParticlesMobile" {
Properties {
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
			//Tags { "LightMode" = "Always" }
			
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_particles
#pragma multi_compile DISTORT_ON DISTORT_OFF
#include "UnityCG.cginc"

struct appdata_t {
	float4 vertex : POSITION;
#if DISTORT_ON
	float2 texcoord: TEXCOORD0;
	half4 color : COLOR;
#endif
};

struct v2f {
	float4 vertex : SV_POSITION;
#if DISTORT_ON
	float4 uvgrab : TEXCOORD0;
	float2 uvbump : TEXCOORD1;
	half4 color : COLOR;
	#ifdef SOFTPARTICLES_ON
		float4 projPos : TEXCOORD3;
	#endif
#endif
};

sampler2D _BumpTex;
float _BumpAmt;
sampler2D _GrabTextureMobile;
half _GrabTextureMobileScale;
float4 _GrabTextureMobile_TexelSize;
float4 _BumpTex_ST;

v2f vert (appdata_t v)
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
	#ifdef SOFTPARTICLES_ON
		o.projPos = ComputeScreenPos (o.vertex);
		COMPUTE_EYEDEPTH(o.projPos.z);
	#endif
	o.color = v.color;
	
	o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*_ProjectionParams.x) + o.vertex.w) * 0.5;
	o.uvgrab.zw = o.vertex.w;
#if UNITY_SINGLE_PASS_STEREO
	o.uvgrab.xy = TransformStereoScreenSpaceTex(o.uvgrab.xy, o.uvgrab.w);
#endif
	o.uvgrab.z /= distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex));
	o.uvbump = TRANSFORM_TEX( v.texcoord, _BumpTex );
	return o;
#endif
}

sampler2D _CameraDepthTexture;
half _InvFade;

half4 frag( v2f i ) : SV_Target
{
#if DISTORT_OFF
	discard;
	return 0;
#else
	#ifdef SOFTPARTICLES_ON
		float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
		float partZ = i.projPos.z;
		float fade = saturate (_InvFade * (sceneZ-partZ));
		float fadeStep = step(0.001, _InvFade);
		i.color.a *= lerp(1, fade, step(0.001, _InvFade));
	#endif
	
	half3 bump = UnpackNormal(tex2D( _BumpTex, i.uvbump));
	half2 offset = bump.rg;
	half alphaBump = (abs(bump.r + bump.g)-0.01)*25;
	clip(alphaBump - 0.1);
	offset = offset * _BumpAmt * _GrabTextureMobile_TexelSize.xy * i.color.a * _GrabTextureMobileScale;
	i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
	half4 col = tex2Dproj( _GrabTextureMobile, UNITY_PROJ_COORD(i.uvgrab));
	
    col.a = saturate(col.a * alphaBump) * i.color.a;
	return col;
#endif
}
ENDCG
		}
	}
}

}
