Shader "Custom/ScrollingClouds"
{
    Properties
    {
        _MainTex ("Cloud Texture", 2D) = "white" {}
        _Speed ("Scroll Speed", Vector) = (0.01, 0.01, 0, 0)
        _Color ("Tint", Color) = (1,1,1,1)
        _Transparency ("Alpha", Range(0,1)) = 0.5
        _FadeRange ("Edge Fade Range", Range(0.0, 1.0)) = 0.4
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Speed;
            float4 _Color;
            float _Transparency;
            float _FadeRange;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 quadLocalXY : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv + _Time.y * _Speed.xy, _MainTex);

                // 取 Quad 的 XY 轴（适用于已旋转 -90 的 Quad）
                o.quadLocalXY = v.vertex.xy * 2.0; // [-0.5, 0.5] ➝ [-1, 1]
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 pos = abs(i.quadLocalXY); // [0,1]
                float fade = smoothstep(1.0, 1.0 - _FadeRange, max(pos.x, pos.y));

                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.a *= _Transparency * fade;
                return col;
            }
            ENDCG
        }
    }
}