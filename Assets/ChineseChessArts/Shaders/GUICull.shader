Shader "Custom/GUIcull" {
Properties {
	
	_MainTex ("Particle Texture", 2D) = "white" {}
}
	
SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Tags { "LightMode" = "Vertex" }
	Cull Back
	Lighting Off

	Material {

	 }
	ColorMaterial AmbientAndDiffuse
	ZWrite On
	//ColorMask RGB
	Blend SrcAlpha OneMinusSrcAlpha
	AlphaTest Greater 0
	Pass { 
		SetTexture [_MainTex] { combine primary * texture }
	}
}
}