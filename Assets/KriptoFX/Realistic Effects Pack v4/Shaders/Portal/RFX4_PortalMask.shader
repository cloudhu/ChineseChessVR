// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with '_Object2World'


Shader "KriptoFX/RFX4/Portal/PortalMask"
{
Properties {
	_TurbulenceMask ("Turbulence Mask", 2D) = "white" {}
	_NoiseScale("Noize Scale (XYZ) Height (W)", Vector) = (1, 1, 1, 0.2)
}
	SubShader 
	{
		Tags { "RenderType"="Tranperent" "Queue"="Geometry-100" "IgnoreProjector" = "True" }
		ColorMask 0
		ZWrite off
		Stencil 
		{
			Ref 2
			Comp always
			Pass replace
		}
		
		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			float4 _NoiseScale;
			sampler2D _TurbulenceMask;

			struct appdata 
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			
			struct v2f 
			{
				float4 vertex : SV_POSITION;
			};
			
			v2f vert(appdata v) 
			{
				v2f o;
				float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float4 coordNoise = float4(wpos * _NoiseScale.xyz, 0);
				float4 tex1 = tex2Dlod (_TurbulenceMask, coordNoise + float4(_Time.x*3, _Time.x * 5, _Time.x * 2.5, 0));
				v.vertex.xyz += v.normal* 0.005 + tex1.rgb * _NoiseScale.w - _NoiseScale.w/2;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}
			
			half4 frag(v2f i) : SV_Target 
			{
				return 0;
			}
		ENDCG
		}
	}
}
