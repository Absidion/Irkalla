Shader "Custom/SimpleEffect"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Scale("Scale", float) = 1
        _Speed("Speed", float) = 1
        _Frequency("Frequency", float) = 1
    }
        SubShader
        {
            // No culling or depth
            Cull Off ZWrite Off ZTest Always

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
                    float4 screenPosition : TEXCOORD2;
                    float heightFromCenter : TEXCOORD1;
                };


                sampler2D _MainTex;
                fixed4 _EffectColor;
                float _Scale, _Speed, _Frequency;

                v2f vert(appdata v)
                {
                    half offsetvert = -((v.vertex.x*v.vertex.x) + (v.vertex.y*v.vertex.y));
                    half value = _Scale * sin(_Time.w * _Speed + offsetvert * _Frequency);

                    //v.vertex.z -= value;

                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    o.screenPosition = ComputeScreenPos(o.vertex);
                    o.heightFromCenter = abs(value * 1.5);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    i.screenPosition /= i.screenPosition.w;

                    fixed4 col = tex2D(_MainTex, float2(i.screenPosition.x, i.screenPosition.y));
                    
                    col.x += i.heightFromCenter * _EffectColor.x;
                    col.y += i.heightFromCenter * _EffectColor.y;
                    col.z += i.heightFromCenter * _EffectColor.z;

                    return col;
                }
                ENDCG
            }
        }
}
