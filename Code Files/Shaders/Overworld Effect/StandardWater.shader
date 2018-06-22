Shader "Custom/StandardWater"
{
    Properties
    {
        _PrimaryWaterTexture("PrimaryWaterTexture", 2D) = "white" {}
        _SecondaryWaterTexture("SecondaryWaterTexture", 2D) = "whtie" {}
        _Alpha("Alpha Channel", float) = 1.0
    }
        SubShader
    {
        Tags{ "RenderType" = "Opaque" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
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

        sampler2D _PrimaryWaterTexture;
        float4 _PrimaryWaterTexture_ST;
        sampler2D _SecondaryWaterTexture;
        float _Alpha;
        fixed _WaterSpeed;
        fixed2 _WaterDirection;


        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _PrimaryWaterTexture);
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            //Offset the texture UVs by the _WaterSpeed and the _WaterDirection in order to get the actual displacement of the water.
            i.uv.x += _WaterDirection.x * (_Time.x * _WaterSpeed);
            i.uv.y += _WaterDirection.y * (_Time.x * _WaterSpeed);
            //now get the color of the primary texture at the offset location
            fixed4 primary = tex2D(_PrimaryWaterTexture, i.uv);

            //divid the UVs by 2 which will make the secondary texture move slower then the primary texture
            i.uv *= 0.5;
            //get the color of the secondary texture at the new uv
            fixed4 secondary = tex2D(_SecondaryWaterTexture, i.uv);

            //clamp the result of the two colors added togeather in order to get the desired color
            fixed4 color = clamp(primary + secondary, 0, 1);
            color.a = _Alpha;
            return color;
        }
            ENDCG
        }
    }
}
