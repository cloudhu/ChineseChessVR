Shader "KriptoFX/RFX4/LightningMobile" {
Properties {
	[HDR]_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Main Texture", 2D) = "white" {}
	_DistortTex1("Distort Texture1", 2D) = "white" {}
	_DistortTex2("Distort Texture2", 2D) = "white" {}
	_DistortSpeed("Distort Speed Scale (xy/zw)", Vector) = (1,.1,1,.1) 
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	Cull Off Lighting Off ZWrite Off
	Offset -1, -1

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _DistortTex1;
			sampler2D _DistortTex2;
			half4 _TintColor;
			half _Cutoff;
			half4 _DistortSpeed;
			
			struct appdata_t {
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half4 color : COLOR;
				float2 uvMain : TEXCOORD0;
				float4 uvDistort : TEXCOORD1;
				UNITY_FOG_COORDS(3)

			};
			
			float4 _MainTex_ST;
			float4 _DistortTex1_ST;
			float4 _DistortTex2_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
#if UNITY_VERSION >= 550
				o.vertex = UnityObjectToClipPos(v.vertex);
#else 
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
				o.color = v.color;
				o.uvMain.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uvDistort.xy = TRANSFORM_TEX(v.texcoord, _DistortTex1);
				o.uvDistort.zw = TRANSFORM_TEX(v.texcoord, _DistortTex2);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			sampler2D_float _CameraDepthTexture;
			half _InvFade;
			
			half4 frag (v2f i) : SV_Target
			{
				half4 distort1 = tex2D(_DistortTex1, i.uvDistort.xy + _DistortSpeed.x * _Time.xx) * 2 - 1;
				half4 distort2 = tex2D(_DistortTex1, i.uvDistort.xy - _DistortSpeed.x * _Time.xx * 1.4 + float2(0.4, 0.6)) * 2 - 1;
				half4 distort3 = tex2D(_DistortTex2, i.uvDistort.zw + _DistortSpeed.z * _Time.xx) * 2 - 1;
				half4 distort4 = tex2D(_DistortTex2, i.uvDistort.zw - _DistortSpeed.z * _Time.xx * 1.25 + float2(0.3, 0.7)) * 2 - 1;
				half4 tex = tex2D(_MainTex, i.uvMain + (distort1.xy + distort2.xy) * _DistortSpeed.y + (distort3.xy + distort4.xy) * _DistortSpeed.w);
				
				half4 col = 2.0f * i.color * _TintColor * tex;
				
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG 
		}
	}	
}
}
