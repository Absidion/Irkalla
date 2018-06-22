Shader "Hidden/Vignette"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
		_EffectTex("EffectTexture", 2D)  = "Effect"{}
    }
        SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Tags {	
				"Queue" = "Transparent" 
				"RenderType" = "Transparent" 
		     }

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;					//The main texture / destination color being used
			sampler2D _EffectTex;				//The secondary fire texture being used

            uniform float _Intensity;
            uniform float _Falloff;
			uniform float _Height;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);		
                fixed distance = length(fixed2(0.5, 0.5) - i.uv);   								

                if (distance < _Intensity || i.uv.y >= _Height)
                {
                    return col;
                }   					
				
                fixed falloffAlpha = distance * _Falloff;

				fixed4 effectCol = tex2D(_EffectTex, i.uv);	
                effectCol.w = clamp(falloffAlpha, 0, 1);
				
                return effectCol;
            }

            ENDCG
        }
    }
}
