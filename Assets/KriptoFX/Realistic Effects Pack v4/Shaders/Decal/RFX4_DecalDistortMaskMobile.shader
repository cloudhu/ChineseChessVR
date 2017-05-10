Shader "KriptoFX/RFX4/Decal/DistortMaskMobile" {
Properties {
	[HDR]_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_Cutoff("Cutoff", Range(0, 1.1)) = 1.1
	_MainTex ("Main Texture", 2D) = "white" {}
	_DistortTex("Distort Texture", 2D) = "white" {}
	_Mask ("Mask", 2D) = "white" {}
	_Speed("Distort Speed", Float) = 1
	_Scale("Distort Scale", Float) = 1
	_MaskPow("Mask pow", Float) = 1
	_AlphaPow("Alpha pow", Float) = 1
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
			sampler2D _DistortTex;
			half4 _TintColor;
			half _Cutoff;
			half _Speed;
			half _Scale;
			half _MaskPow;
			half _AlphaPow;
			half4 _Tex_NextFrame;
			half InterpolationValue;

			
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
				UNITY_FOG_COORDS(2)

			};
			
			float4 _MainTex_ST;
			float4 _DistortTex_ST;
			float4 _Mask_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
#if UNITY_VERSION >= 550
				o.vertex = UnityObjectToClipPos(v.vertex);
#else 
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
				o.color = v.color;
				o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.texcoord.zw = TRANSFORM_TEX(v.texcoord, _DistortTex);
				o.uvMask = TRANSFORM_TEX(o.texcoord, _Mask);

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			half4 frag (v2f i) : SV_Target
			{
				half4 distort = tex2D(_DistortTex, i.texcoord.zw)*2-1;
				half4 tex = tex2D(_MainTex, i.texcoord.xy + distort.xy / 10 * _Scale + _Speed * _Time.xx);
				half4 tex2 = tex2D(_MainTex, i.texcoord.xy - distort.xy / 7 * _Scale - _Speed * _Time.xx * 1.4 + float2(0.4, 0.6));

				tex *= tex2;
				half mask = tex2D(_Mask, i.uvMask).a;
				mask = pow(mask, _MaskPow);
				
				half4 col = 2.0f * i.color * _TintColor * tex;

				UNITY_APPLY_FOG(i.fogCoord, col);
				
				half m = saturate(mask - _Cutoff);
				half alpha = saturate(tex.a * m * _TintColor.a * 2);
				return half4(col.rgb * pow(alpha, _AlphaPow), alpha);
			}
			ENDCG 
		}
	}	
}
}
