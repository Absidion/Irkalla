// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Texture Splatting"
{
	/*
	NoScaleOffset is an attribute that makes sure we can't change the tiling and offset of the texture assigned with the attribute.
	For splat mapping, we want only the splat map itself to be able to be tiled. 
	It will control the other textures.
	*/
	
	Properties{
		_MainTex("Splat Map", 2D) = "white" {}
		[NoScaleOffset] _Texture1("Red Splat", 2D) = "white"{}
		[NoScaleOffset] _Texture2("Green Splat", 2D) = "white"{}
		[NoScaleOffset] _Texture3("Blue Splat", 2D) = "white" {}
		[NoScaleOffset] _Texture4("Alpha Splat", 2D) = "white" {}
	}

		SubShader{

		Pass{
		CGPROGRAM

#pragma vertex MyVertexProgram
#pragma fragment MyFragmentProgram

#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_ST;

		sampler2D _Texture1, _Texture2, _Texture3, _Texture4;

	struct VertexData {
		float4 position : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct Interpolators {
		float4 position : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uvSplat : TEXCOORD1;
	};

	Interpolators MyVertexProgram(VertexData v) {
		Interpolators i;
		i.position = UnityObjectToClipPos(v.position);
		i.uv = TRANSFORM_TEX(v.uv, _MainTex);
		i.uvSplat = v.uv;
		return i;
	}

	/* Information about Texture Splatting
	We want to use a splat map to have multiple textures appear on a terrain. 
	We can use a Binary Splat Map (Grayscale) or RGBA Splat Map.
	First we sample the splat map. 
	Now if we use an RGBA splat map, it means we can have 4 textures take over for each colour slot.
	Sampling Texture 1 and multplying that by our splat.r, means that the texture takes over all red space from our splat map.
	This happens again with G and B. 
	For the fourth texture (Alpha), we need to complete the interpolation by multiplying it by (1 - rgb).
	This is so we use any space that does not belong to red, blue or green. 
	When using a binary map, (white and black), the texture you pass in for white can apply to any of the RGB slots.
	The second map should go into the alpha slot then. 

	Add all of the texture samples together once they have been appropriately multiplied by the splat maps colour slot.
	Return the result, and you've now splatted a texture.

	//http://catlikecoding.com/unity/tutorials/rendering/part-3/

	*/

	float4 MyFragmentProgram(Interpolators i) : SV_TARGET
	{
		float4 splat = tex2D(_MainTex, i.uvSplat);
		return tex2D(_Texture1, i.uv) * splat.r +
				tex2D(_Texture2, i.uv) * splat.g +
				tex2D(_Texture3, i.uv) * splat.b +
				tex2D(_Texture4, i.uv) * (1 - splat.r - splat.g - splat.b);
	}

		ENDCG
	}
	}
}
