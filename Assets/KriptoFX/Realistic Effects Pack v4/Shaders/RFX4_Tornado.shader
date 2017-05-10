// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with '_Object2World'

Shader "KriptoFX/RFX4/Tornado" {
Properties {
	[HDR]_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_TwistScale("Twist Scale (XY) Time(Z) Pivot(W)", Vector) = (1, 0.2, 2, 0)
	_WavesScale("Waves Scale (XY) Time(Z)", Vector) = (10, 0.08, 10, 0)
	_FireOffsetSpeed("Fire Offset Speed (XY)", Vector) = (0.3, 0.75, 0, 0)
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	Cull Back 
	Lighting Off 
	ZWrite Off

	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _PerlinNoise;
			half4 _TintColor;
			float4 _TwistScale;
			float4 _WavesScale;
			float4 _FireOffsetSpeed;
			
			struct appdata_t {
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float4 normal : NORMAL;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD2;
				#endif
				float height : TEXCOORD3;
			};
			
			float4 _MainTex_ST;
			float4 _PerlinNoise_ST;

			v2f vert (appdata_t v)
			{
				v2f o;

				///////////////////////////////////////////////////////////////////////////////////////////////////////////
				float3 wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
				float4 pivot = mul(unity_ObjectToWorld, float4(0,0,0,1));
				//v.vertex.xyz += offsetNoise;
				float height = (wpos.y - pivot.y + _TwistScale.w) * _TwistScale.y;
				v.vertex.x += sin(_Time.y*_TwistScale.z + wpos.y * _TwistScale.x) * height;
				v.vertex.z += sin(_Time.y*_TwistScale.z + wpos.y * _TwistScale.x + 3.1415/2) * height;
				v.vertex.xz += (v.normal.xz/_WavesScale.x + v.normal.xz * sin(-_Time.y * _WavesScale.z + wpos.y*_WavesScale.x)*_WavesScale.y)* height;
				///////////////////////////////////////////////////////////////////////////////////////////////////////////
				o.height = height;
#if UNITY_VERSION >= 550
				o.vertex = UnityObjectToClipPos(v.vertex);
#else 
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
				#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				#endif
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			half4 frag (v2f i) : SV_Target
			{
				half4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord - _FireOffsetSpeed.xy * _Time.y);
				//clip(_TintColor.a - col.r / 3);
				//col.rgb = lerp(col.rgb  + col.rgb *  (0.85-_TintColor.a)*5, col.rgb, _TintColor.a);
				UNITY_APPLY_FOG(i.fogCoord, col);
				//col.a = 1;
				col.a = saturate(col.a);
				return col;
			}
			ENDCG 
		}
	}	
}
}
