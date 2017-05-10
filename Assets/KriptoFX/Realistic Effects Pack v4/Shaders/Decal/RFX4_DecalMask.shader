// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "KriptoFX/RFX4/Decal/Mask" {
Properties {
	[HDR]_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_Cutout("Cutout", Range(0, 1.1)) = 1.1
	_MainTex ("Particle Texture", 2D) = "white" {}
	_Mask ("Mask", 2D) = "white" {}
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
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
			half4 _TintColor;
			float _Cutout;
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			
			struct appdata_t {
				float4 vertex : POSITION;
				half4 color : COLOR0;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half4 color : COLOR0;
				float2 texcoord : TEXCOORD0;
				float4 uvShadow : TEXCOORD1;
				float4 uvMainTex : TEXCOORD2;
				UNITY_FOG_COORDS(3)

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
				o.color = v.color;
				o.uvMainTex = mul(unity_Projector, v.vertex);
				o.texcoord = TRANSFORM_TEX(o.uvMainTex.xyz, _MainTex);
				o.uvShadow = mul(unity_Projector, v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			sampler2D_float _CameraDepthTexture;
			float _InvFade;
			
			half4 frag (v2f i) : SV_Target
			{
				float clampMutliplier = 1 - step(i.uvMainTex.x, 0);
				clampMutliplier *= 1 - step(1, i.uvMainTex.x);
				clampMutliplier *= 1 - step(i.uvMainTex.y, 0);
				clampMutliplier *= 1 - step(1, i.uvMainTex.y);
				//clip(col.a - 0.1);

				float4 tex = tex2D(_MainTex, i.texcoord);
				float mask = tex2D(_Mask, i.texcoord).r;
				half4 col = 2.0f * i.color * _TintColor * tex;
				UNITY_APPLY_FOG(i.fogCoord, col);
				float m = saturate(_Cutout - mask);

				
				float projectedCordZ = i.uvShadow.z;
				clampMutliplier *= step(projectedCordZ, 1);
				clampMutliplier *= step(-1, projectedCordZ);
				col.a = tex.a * saturate(m*m * 100) * _TintColor.a * clampMutliplier;
				
				return half4(col.rgb, col.a);
			}
			ENDCG 
		}
	}	
}
}
