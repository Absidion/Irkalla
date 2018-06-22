Shader "Custom/HitscanShader"
{
    Properties
    {
        _MainTex("Albedo Texture", 2D) = "white" {}
        _Alpha("Alpha", Range(0, 1)) = 0
    }

        SubShader
    {
        Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
    {
        CGPROGRAM

#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog

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
        UNITY_FOG_COORDS(1)
    };

    sampler2D _MainTex;
    float4 _MainTex_ST;
    float _Alpha;

    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {        
        float4 col = (tex2D(_MainTex, i.uv), tex2D(_MainTex, i.uv).a *_Alpha);
        UNITY_APPLY_FOG(i.fogcoord, col);

        return col;
    }
        ENDCG
    }
    }
}
