// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with '_Object2World'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "KriptoFX/RFX4/Ice" {
Properties {
        [HDR]_Color ("Main Color", Color) = (1,1,1,1)
        _ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
        _MainTex ("Base (RGB) Emission Tex (A)", 2D) = "white" {}
        _Cube ("Reflection Cubemap", Cube) = "" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _FPOW("FPOW Fresnel", Float) = 5.0
        _R0("R0 Fresnel", Float) = 0.05
		_BumpAmt ("Distortion", Float) = 10
		_RefractiveStrength ("Refractive Strength", Float) = 10
}

Category{

			Tags{ "Queue" = "Transparent"  "IgnoreProjector" = "True"  "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Back
			Lighting Off
			ZWrite Off
			Fog{ Mode Off }

			SubShader{
			GrabPass{ "_GrabTextureIce" }
			Pass{
			
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile DISTORT_ON DISTORT_OFF
#include "UnityCG.cginc"

		struct appdata_t {
			float4 vertex : POSITION;
			float2 texcoord: TEXCOORD0;
			half4 color : COLOR;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			float4 uv_MainCutOut : TEXCOORD0;
			float4 uv_BumpFresnel : TEXCOORD1;
			float4 uvgrab : TEXCOORD2;
			float3 refl : TEXCOORD3;
		};

		sampler2D _MainTex;
		sampler2D _CutoutTex;
		sampler2D _BumpMap;
		float4 _MainTex_ST;
		float4 _CutoutTex_ST;
		float4 _BumpMap_ST;
		samplerCUBE _Cube;

		half4 _Color;
		half4 _ReflectColor;
		half _RefractiveStrength;
		half _FPOW;
		half _R0;
		half _Cutoff;
		half _BumpAmt;
		sampler2D _GrabTextureIce;
		float4 _GrabTextureIce_TexelSize;

		v2f vert(appdata_t v)
		{
			v2f o;
#if UNITY_VERSION >= 550
			o.vertex = UnityObjectToClipPos(v.vertex);
#else 
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
			float3 binormal = cross(v.normal, v.tangent.xyz) * v.tangent.w;
			float3x3 rotation = float3x3(v.tangent.xyz, binormal, v.normal);


			o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*_ProjectionParams.x) + o.vertex.w) * 0.5;
			o.uvgrab.xy += refract(normalize(mul(rotation, ObjSpaceViewDir(v.vertex))), 0, 1.0 / _RefractiveStrength);
			o.uvgrab.zw = o.vertex.w;
#if UNITY_SINGLE_PASS_STEREO
			o.uvgrab.xy = TransformStereoScreenSpaceTex(o.uvgrab.xy, o.uvgrab.w);
#endif
			o.uvgrab.z /= distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex));


			o.uv_MainCutOut.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.uv_MainCutOut.zw = TRANSFORM_TEX(v.texcoord, _CutoutTex);
			o.uv_BumpFresnel.xy = TRANSFORM_TEX(v.texcoord, _BumpMap);
			o.uv_BumpFresnel.zw = 1;
			o.uv_BumpFresnel.z = (1 - dot(normalize(v.normal), normalize(ObjSpaceViewDir(v.vertex))));
			o.uv_BumpFresnel.z = pow(o.uv_BumpFresnel.z, _FPOW);
			o.uv_BumpFresnel.z = saturate(_R0 + (1.0 - _R0) * o.uv_BumpFresnel.z);
			float3 viewDir = mul(unity_ObjectToWorld, v.vertex).xyz - _WorldSpaceCameraPos;
			float3 normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
			o.refl = reflect(viewDir, normalDir);
			return o;
		}

		half4 frag(v2f i) : SV_Target
		{
			half3 bump = UnpackNormal(tex2D(_BumpMap, i.uv_BumpFresnel.xy));
			half2 offset = bump.rg * _BumpAmt * _GrabTextureIce_TexelSize.xy;
			half4 tex = tex2D(_MainTex, i.uv_MainCutOut.xy + offset / 10);
			half4 c = tex * _Color;
			half reflcol = dot(texCUBE(_Cube, i.refl*(bump * 2 - 1)), 0.33);
			reflcol *= tex.a;
			reflcol = lerp(c, reflcol, i.uv_BumpFresnel.z);

			i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
			half4 col = tex2Dproj(_GrabTextureIce, UNITY_PROJ_COORD(i.uvgrab));
			
			//gray = saturate(gray);
			half3 refl = lerp(tex.rgb * reflcol, reflcol, i.uv_BumpFresnel.z);
			
			col.rgb = col * _Color + refl * _ReflectColor * col * 4;
			col.a = _Color.a;
			return col;
		}
			ENDCG
		}
	}
	}

}
