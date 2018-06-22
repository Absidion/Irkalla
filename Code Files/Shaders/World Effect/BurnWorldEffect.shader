Shader "Unlit/BurnWorldEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color Gradient", 2D) = "white" {}
        _Opacity ("Opacity", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		ZWrite Off
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
            #pragma target 3.0
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
                float2 uv_Opacity : TEXCOORD1;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			sampler2D _Color;
            sampler2D _Opacity;
			float4 _MainTex_ST;
            float4 _Opacity_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);

				//make the vertices bend around the origin
				float4 outputVertex = v.vertex;
				float curveAmount = 7.0;
				outputVertex.y = -exp2(abs(outputVertex.x)) / curveAmount;
				
				//add wave to distort uvs more 
				outputVertex.xy += sin(_Time.z - outputVertex.xy) * 0.5;
				
				o.vertex = UnityObjectToClipPos(outputVertex);

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_Opacity = TRANSFORM_TEX(v.uv, _Opacity);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				i.uv.y -= _Time.z / 2;
                i.uv_Opacity.x += _Time.y / 2;
				float noise = saturate(tex2D(_MainTex, i.uv));
                float noiseAlpha = 1.0f;

				if (noise < 0.2)
				{
					discard;
				}
				else if (noise < 1.0)
				{
                    noiseAlpha = 0.5;
				}

				// sample the texture
				fixed4 col = tex2D(_Color, float2(noise, 0.5));
                fixed4 opacityMask = tex2D(_Opacity, i.uv_Opacity);
                col.a = opacityMask.a * noiseAlpha;

				return col;
			}
			ENDCG
		}
	}
}
