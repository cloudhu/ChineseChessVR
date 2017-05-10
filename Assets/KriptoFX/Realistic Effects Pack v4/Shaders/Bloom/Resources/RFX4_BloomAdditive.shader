Shader "Hidden/KriptoFX/PostEffects/RFX4_BloomAdditive" {
Properties {
}

SubShader {
		
Pass {
	Tags{ "Queue" = "Transparent " "IgnoreProjector" = "True" "RenderType" = "Transparent" }
	Blend SrcAlpha One
	ZTest Always
			
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

struct appdata_t {
	float4 vertex : POSITION;
	float2 texcoord: TEXCOORD0;
};

struct v2f {
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
};

sampler2D _MainTex;
float4 _MainTex_ST;

v2f vert (appdata_t v)
{
	v2f o;
	o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
	o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
	return o;
}


half4 frag( v2f i ) : SV_Target
{
	half4 col = tex2D(_MainTex, i.uv);
	col.a = 1;
	return col;
}
ENDCG
		}
	}
}


