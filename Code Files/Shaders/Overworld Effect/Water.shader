Shader "Custom/Water" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
        _SecondTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_xWaveRotation ("X Rotation", Range(-2.5,2.5)) = 1.0
		_zWaveRotation ("Z Rotation", Range(-2.5,2.5)) = 1.0
		_Alpha ("Alpha Value", Range(0,1)) = 1.0
		_TotalWaveAmount ("TotalWaveAmount", Float) = 0.2
		_WaveHeight ("Wave Height", Float) = 1.0
		_WaveSpeed ("Wave Speed", Float) = 1.0

	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent"}
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert alpha
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		//#REGION VERTEX SHADER DATA
			half _xWaveRotation;					//amount that the waves will rotate around the x-axis
			half _zWaveRotation;					//amount that the waves will rotate around the z-axis
			half _TotalWaveAmount;					//total amount of waves, ontop of wave x and wave z
			half _WaveHeight;						//height value of the waves
			half _WaveSpeed;						//speed at which the texture will move as well as the speed at which the waves move

			void vert (inout appdata_full v)
			{
				//create a wave effect by increasing the vertex's y value by sin of Time passed as well as influencing it by other numbers
				v.vertex.y = (sin(((_Time.w * _WaveSpeed) + (v.vertex.z * _zWaveRotation) + (v.vertex.x * _xWaveRotation)) *  (0.2 * _TotalWaveAmount)) * _WaveHeight);
			}
		//#END REGION

			sampler2D _MainTex;
			sampler2D _SecondTex;

			struct Input 
			{
				float2 uv_MainTex;
			    float2 uv_SecondTex;
			};

			half _Glossiness;			//glossiness value 
			half _Metallic;				//metallic value 
			fixed4 _Color;				//color which the texture color get multiplied by		
			half _Alpha;				//sets the alpha channel of the shader				

			UNITY_INSTANCING_CBUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_CBUFFER_END
			

			void surf (Input IN, inout SurfaceOutputStandard o) 
			{
				//offset the primary texture by an 8th of timepassed. Then multiply by 0.5 in order to 
				//slow this texture down so it moves slower then the second texture.
			    IN.uv_MainTex.x += (_Time.x * 0.5) * _WaveSpeed;
			    IN.uv_MainTex.y += (_Time.x * 0.5) * _WaveSpeed;

				//offset the secondary texture by an 8th of delta time. This texture moves at full speed.
			    IN.uv_SecondTex.x += _Time.x * _WaveSpeed;
			    IN.uv_SecondTex.y += _Time.x * _WaveSpeed;			   

			    // Albedo comes from a texture tinted by color
			    fixed4 firstTex = tex2D(_MainTex, IN.uv_MainTex);
			    fixed4 secondTex = tex2D(_SecondTex, IN.uv_SecondTex);			   

				o.Albedo = firstTex.rgb * secondTex.rgb * _Color.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = _Alpha;
			}
		ENDCG
	}
	FallBack "Diffuse"
}
