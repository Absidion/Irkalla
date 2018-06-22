Shader "Unlit/PortalShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_GlowColor ("Glow Color", Color) = (1,1,1,1)
	}
	SubShader
	{
        Tags
		{
			"Queue" = "Transparent" 
			"IgnoreProjector" = "True" 
			"RenderType" = "Transparent"			
		}

        Lighting off
        Cull Back
        ZWrite on
        ZTest Less


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
				float4 screenPosition : TEXCOORD1;
				float4 vertex : SV_POSITION;
				float edgeDistance : TEXCOORD2;
			};
	

			sampler2D _MainTex;			
			
			v2f vert (appdata v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPosition = ComputeScreenPos(o.vertex);

                //distort the vertices by dividing the sind/cosd value of them by the distance. This will make it so that values closer to the center will be more distorted
                o.edgeDistance = length(o.vertex);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                i.screenPosition.x += sin(_Time.w / i.edgeDistance) / 80;
                i.screenPosition.y += cos(_Time.w / i.edgeDistance) / 50;
                i.screenPosition /= i.screenPosition.w;

				fixed4 col = tex2D(_MainTex, float2(i.screenPosition.x, i.screenPosition.y));

				return col;				
			}
			ENDCG
		}
	}
}
