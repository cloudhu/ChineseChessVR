// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "KriptoFX/RFX4/Decal/DistortMaskMul" {
Properties {
	[HDR]_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_Cutoff("Cutoff", Range(0, 1.1)) = 1.1
	_MainTex ("Main Texture", 2D) = "white" {}
	_DistortTex("Distort Texture", 2D) = "white" {}
	_Mask ("Mask", 2D) = "white" {}
	_Speed("Distort Speed", Float) = 1
	_Scale("Distort Scale", Float) = 1
	_MaskPow("Mask pow", Float) = 1
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend DstColor Zero
	Cull Off Lighting Off ZWrite Off
	Offset -1, -1

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _Mask;
			sampler2D _DistortTex;
			half4 _TintColor;
			half _Cutoff;
			half _Speed;
			half _Scale;
			half _MaskPow;
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			
			struct appdata_t {
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half4 color : COLOR;
				float4 texcoord : TEXCOORD0;
				float2 uvMask : TEXCOORD1;
				float4 uvShadow : TEXCOORD2;
				float4 uvMainTex : TEXCOORD3;
				UNITY_FOG_COORDS(4)

			};
			
			float4 _MainTex_ST;
			float4 _Mask_ST;
			float4 _DistortTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;

				o.uvMainTex = mul(unity_Projector, v.vertex);
				o.texcoord.xy = TRANSFORM_TEX(o.uvMainTex.xyz, _MainTex);
				o.texcoord.zw = TRANSFORM_TEX(v.texcoord, _DistortTex);
				o.uvMask = TRANSFORM_TEX(o.uvMainTex.xyz, _Mask);
				o.uvShadow = mul(unity_Projector, v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			sampler2D_float _CameraDepthTexture;
			float _InvFade;
			
			half4 frag (v2f i) : SV_Target
			{
				half4 distort = tex2D(_DistortTex, i.texcoord.zw)*2-1;
				half4 tex = tex2D(_MainTex, i.texcoord.xy + distort.xy / 10 * _Scale + _Speed * _Time.xx);
				half4 tex2 = tex2D(_MainTex, i.texcoord.xy - distort.xy / 7 * _Scale - _Speed * _Time.xx * 1.4 + half2(0.4, 0.6));
				tex.rgba *= tex2.rgba;
				half mask = tex2D(_Mask, i.uvMask).a;
				mask = pow(mask, _MaskPow);
				half4 col = i.color * _TintColor * tex;
				UNITY_APPLY_FOG(i.fogCoord, col);
				half m = saturate(mask - _Cutoff);
				half alpha = saturate(tex.a * m * _TintColor.a * 2);

				half clampMutliplier = 1 - step(i.uvMainTex.x, 0);
				clampMutliplier *= 1 - step(1, i.uvMainTex.x);
				clampMutliplier *= 1 - step(i.uvMainTex.y, 0);
				clampMutliplier *= 1 - step(1, i.uvMainTex.y);
				half projectedCordZ = i.uvShadow.z;
				clampMutliplier *= step(projectedCordZ, 1);
				clampMutliplier *= step(-1, projectedCordZ);
				clampMutliplier = clampMutliplier;
				clip(alpha - 0.02);
				return saturate(col * (1-col) + col * (1.2 - alpha*alpha) - alpha + 2 - clampMutliplier);
			}
			ENDCG 
		}
	}	
}
}
