Shader "Custom/CustomStandardShader" 
{
	Properties 
	{
		_MainTex("Albedo", 2D) = "white" {}             //main texture that contains all the details of the model. also can contain AO and GI/Lighting information
		_Color("Tint", Color) = (1, 1, 1, 1)            //single color that gets used to color the model if no main texture is given
		_RampTex ("Ramp", 2D) = "white" {}              //ramp texture which determines the color gradient for the model's diffuse lighting
		_BumpMap("Bump", 2D) = "bump" {}                //bump map used to add extra details to the model's surface
		_SpecMap("Specular", 2D) = "black" {}           //specular map used to give different areas of the model stronger/weaker specular highlights
		_Shininess("Shininess", Range(0, 1)) = 0.5      //flat value that determines the specularity of the entire model if no specular map is given
		_RimPower("Rim Power", Range(0.0, 5.0)) = 2.0   //determines how bright the rim lighting that appears on the edge of the model is
	}
		SubShader
		{
		Tags{ "RenderType" = "Opaque" }

		CGPROGRAM
		#pragma target 3.0
		#pragma shader_feature _HAS_TEXTURE 
		#pragma shader_feature _HAS_SPEC_MAP
        #pragma shader_feature _HAS_BUMP_MAP
		
		#pragma surface surf Ramp addshadow

		sampler2D _MainTex;
		sampler2D _RampTex;
		sampler2D _BumpMap;
		sampler2D _SpecMap;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_SpecMap;
			float3 viewDir;
		};

		fixed4 _Color;
		half _Shininess;
		half _RimPower;

		half4 LightingRamp(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			float3 nNormal = normalize(s.Normal); 							//normalized surface normal to remove artifacts

			//Diffuse lighting
			half nDotL = dot(nNormal, lightDir);							//get the dot product between the surface normal and light direction
			half halfLambert = nDotL * 0.5 + 0.5;                           //use a half lambert calculation rather than a normal one so models/textures don't lose as much definition in dark areas
			float4 ramp = tex2D(_RampTex, float2(halfLambert, 0.5));		//warp the diffuse light with the ramp texture by sampling along the gradient to add color to the diffuse
            half4 diff = ramp *_LightColor0 * atten;						//multiply the warped diffuse by the color of the light and attenuation for final diffuse value
		
			//Specular lighting
			half3 halfVector = normalize(lightDir + viewDir);				    //get the half vector for Blinn-Phong specular
			float nDotH = max(0, dot(nNormal, halfVector));					    //get the dot product between surface normal and halfvector.
            half4 spec = pow(nDotH, 64) * _LightColor0 * atten * s.Specular;    //get the specular by exponentially multiplying the dot product and multiplying by the specular amount

            //Rim lighting
            //calculate rim lighting term so lighting only gets shown when the angle between the camera view and surface normal is smaller
            //this causes the rim lighting to appear around the edges of the model when the light is behind the model
			float fresnelTerm = pow(1 - saturate(dot(nNormal, viewDir)), 6);	        
			half4 rim = fresnelTerm * pow(nDotH, 2) * _LightColor0 * atten * _RimPower;

            //only display the strongest lighting out of specular/rim so the model doesn't get too bright with both lighting applied in the same areas
			half4 specOrRim = max(spec, rim);

			//final color to return post-lighting calculation
			float4 c;
			c.rgb = s.Albedo * diff + specOrRim;	//multipy the model color by the final lighting calculation
			c.a = s.Alpha;
			return c;
		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			//Albedo comes from a texture tinted or a color
			fixed4 c;
			
            //if a texture is defined, use that. otherwise use the flat color provided
			#if defined (_HAS_TEXTURE)
				fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
				c = tex;
			#else
				c = _Color;
			#endif

            //if the specular map is defined, use that. otherwise apply the single specular value provided to the entire model
			#if defined (_HAS_SPEC_MAP)
				fixed4 specMap = tex2D(_SpecMap, IN.uv_SpecMap);
				o.Specular = specMap;
			#else
				o.Specular = _Shininess;
			#endif 
			
            //if the bump map is defined, unpack the normals from it
            #if defined (_HAS_BUMP_MAP)
                o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            #endif
            
			o.Albedo = c.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
	CustomEditor "CustomStandardShaderGUI"
}
