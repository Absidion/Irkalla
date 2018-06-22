Shader "Custom/Explosion" 
{
	Properties 
	{
		_RampTex("Colour Ramp", 2D) = "white" {}			        //ramp texture sampled to get colour of explosion
		_RampOffset("Ramp Offset", Range(-0.5, 0.5)) = 0            //change to move colors between lighter or darker explosion
		_NoiseTex("Noise Texture", 2D) = "gray" {}			        //noise texture for visual effect and random vertex displacement
		_DistortionAmount("Distortion Amount", Range(0, 1.0)) = 0.3 //0 = no mesh distortion. 1 = maximum jagged mesh distortion 
		_ClipRange("Clip Range", Range(0, 1)) = 1                   //0 = all pixels clipped away. 1 = all pixels shown
	}
	SubShader 
		{
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf Standard vertex:vert 
		#pragma target 3.0

		sampler2D _RampTex;
		half _RampOffset;
		sampler2D _NoiseTex;
		half _DistortionAmount;
		half _ClipRange;

		struct Input 
		{
			float2 uv_NoiseTex;
		};

		void vert(inout appdata_full v)
		{
			//goal of the vertex shader is to distort and displace the vertices over time randomly
            
            //sample noise texture at vertex location for displacement value
            float3 displacement = tex2Dlod(_NoiseTex, float4(v.texcoord.xy, 0, 0)); 
			
            //offset displacement with a sinwave manipulated over time. multiply the displacement by arbitrary number for more vertex waves  
            float time = sin(_Time[3] + displacement.r * 10);                                                      
			
            //get direction from vertex normal. multiply by the displacement value, time value and the input distortion amount
            //offset the vertex position by final calculated multiplier
            v.vertex.xyz += v.normal * displacement.r * _DistortionAmount * time;   
		}

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
            //sample random position from noise texture
			float3 noise = tex2D(_NoiseTex, IN.uv_NoiseTex);    
            
            //clamp the random x value between 0 and 1. you can offset this value to skew random towards lighter/darker colors
            float n = saturate(noise.r + _RampOffset);          

            //with the clip function, fragments will be discarded if the number input is negative
            //when clip range is at 1 no fragments are discarded. when clip range is at 0, all fragments are discarded.
            //when clip range is in between 0 and 1, the pixels will starting being discarded in the range of the random number value to 1.
            //the clip range value is decreased from 1 over time in ExplosionEffect.cs. This means over time, the explosion will appear to dissipate. 
            clip(_ClipRange - n);                                                                                     
			
            //colour used from the ramp texture is determined by sampling along the x with clamped random number
			half4 color = tex2D(_RampTex, float2(n, 0.5));       
			
            //set albedo and emission color, set smoothness to 0 for no specular highlights
            o.Albedo = color.rgb;
            o.Emission = color.rgb;
            o.Smoothness = 0.0f;
		}
		ENDCG
	}
}
