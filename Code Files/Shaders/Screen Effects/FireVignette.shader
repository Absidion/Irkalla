Shader "Hidden/FireVignette"
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
		Blend SrcAlpha OneMinusSrcAlpha					//Standard transparency blending mode

		Tags 
		{	
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

            uniform float _CenterAmount;			//The vignette's center circle amount           
			uniform float _Height;					//The max height that the vignette will draw fragments at
			uniform float _TimeScale;				//Time scale for noise calculation and texture offsetting
			uniform float _Alpha;					//Alpha multiplier

			uniform float _Amplitude;				//Amplitude of the Noise logic
			uniform float _Frequency;				//Frequency of the Noise logic
			uniform int _Octaves;					//Octaves of the Noise logic
			uniform float _Lacunarity;				//Lacunarity of the Noise logic
			uniform float _Gain;					//Gain of each pass in the Noise loop

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);			
				
				//create an offset value and set it to the current y of the UV minus height uniform
				float offsetValue = i.uv.y - _Height;
				//the distance of the current fragment from the center of the screen
                fixed distance = length(fixed2(0.5, 0.5) - i.uv);   

				//calculate the texture offset of the Noise texture that is used in the Noise loop
				float2 textureOffset = i.uv;
				textureOffset.x -= _Time.x * 0.1;				//offset the x of the value that way it doesn't appear to be a loop visually in game
				textureOffset.y -= _Time.x * _TimeScale;		//offset the y by time and the Time scale uniform to make the texture move up like fire rising to the sky

				fixed gradiantValue = tex2D(_EffectTex, textureOffset).r;

				//the Noise algorithm
				for(int x; x < _Octaves; x++)
				{
					//increase the offsetvalue each time by the Amplitude and frequency multiplied by the noise texture's calculated value for this fragment
					offsetValue += _Amplitude * gradiantValue * _Frequency;
					
					_Frequency *= _Lacunarity;		//increase the frequency by the Lacunarity value
					_Amplitude *= _Gain;			//increase the amplitude by the gain
				}

				//1. check to see if the distance from the center value is less then the centeramount plus the offset value. This will make sure that this fragment
				//	 is actually inside of the vignette's center value after the noise offset calculation
				//						OR			||
				//2. check to see if the current fragment's is greater then the height plus the offset value. This make's sure that the fragment's height isn't to high
                if ((distance < (_CenterAmount + offsetValue)) || (i.uv.y >= _Height + offsetValue))
                {
                    return col;
                }               
							
				//keep the originalHeight since the uv's values are going to offset							
				float originalHeight = i.uv.y;
				i.uv.y -= _Time.x * _TimeScale;
				
				fixed4 effectCol = tex2D(_EffectTex, i.uv);	

				//set the alpha channel of the effectCol to be divided by the originalHeight * the alpha channel. This will create a fade effect based on the height of the fragment relative to the original height of the fragment
				effectCol.w = sqrt(effectCol.xyz) / (originalHeight * _Alpha);
                effectCol.w = clamp(effectCol.w, 0.0, 1.0);
				
                return effectCol;
            }
		
            ENDCG
        }
    }
}
