Shader "Custom/EyeLaserShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PulseSpeed ("Pulsesation Speed", float) = 1.0
		_PulseSize ("Pulse Size", float) = 2.0
		_PulseSizeMultiplier ("Pulse Size Multiplier", float) = 0.05
		_Alpha ("Transparency", float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		cull off
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed _PulseSpeed;
			fixed _PulseSize;
			fixed _PulseSizeMultiplier;
			fixed _Alpha;

			v2f vert (appdata v)
			{  
				v.vertex.x *=((sin(_Time.w * _PulseSpeed + v.vertex.y) + _PulseSize) * _PulseSizeMultiplier);
				v.vertex.z *=((sin(_Time.w * _PulseSpeed + v.vertex.y) + _PulseSize) * _PulseSizeMultiplier);
			      	      	
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);	

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				i.uv.y -= _Time.w;

				fixed4 col = tex2D(_MainTex, i.uv);
				col.w = _Alpha;

				return col;
			}
			ENDCG
		}
	}
}
