Shader "KriptoFX/RFX4/Lava" {
	Properties {
		[HDR]_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Main Texture", 2D) = "white" {}
		_DistortTex("Distort Texture", 2D) = "white" {}
		_Speed("Distort Speed", Float) = 1
		_Scale("Distort Scale", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		//#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _DistortTex;
		half _Glossiness;
		half _Metallic;
		half4 _TintColor;
		float _Speed;
		float _Scale;

		struct Input {
			float2 uv_MainTex;
			float2 uv_DistortTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			float4 distort = tex2D(_DistortTex, IN.uv_DistortTex) * 2 - 1;
			float4 tex = tex2D(_MainTex, IN.uv_MainTex + distort / 10 * _Scale + _Speed * _Time.xx);
			float4 tex2 = tex2D(_MainTex, IN.uv_MainTex - distort / 7 * _Scale - _Speed * _Time.xx * 1.4 + float2(0.4, 0.6));
			tex.rgba *= tex2.rgba;
			o.Emission = _TintColor * tex;
			o.Albedo = o.Emission;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
