Shader "KriptoFX/RFX4/CutoutBorderMobile" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[HDR]_EmissionColor("Emission Color", Color) = (1,1,1,1)
		_EmissionTex("Emission (A)", 2D) = "black" {}
		_Cutoff ("_Cutoff", Range(0,1)) = 0
		//_Cutout2 ("Cutout2", Range(0,1)) = 0
		[HDR]_BorderColor ("Border Color", Color) = (1,1,1,1)
		_CutoutThickness ("Cutout Thickness", Range(0,1)) = 0.03
	}
	SubShader {
		Tags { "RenderType"="Tranparent"  "IgnoreProjector" = "True" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert noforwardadd

		sampler2D _MainTex;
		sampler2D _EmissionTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpTex; 
			float2 uv_EmissionTex;
		};

		half4 _Color;
		half4 _BorderColor;
		half4 _EmissionColor;
		half _CutoutThickness;
		half _Cutoff;

		void surf(Input IN, inout SurfaceOutput o) {

			// Albedo comes from a texture tinted by color
			half4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Alpha = c.a;
			clip(c.a - _Cutoff);
			//if(c.a < _Cutout) discard;
			if(c.a < _Cutoff + _CutoutThickness) o.Emission = _BorderColor;
			else o.Emission = tex2D(_EmissionTex, IN.uv_EmissionTex) * _EmissionColor;
		}
		ENDCG
	}
	 Fallback "Transparent/Cutout/Diffuse"
}
