Shader "Custom/CloudDome"
{
    Properties
    {
        _MainTex ("Cloud Texture", 2D) = "white" {}
        _Speed ("Scroll Speed", Vector) = (0.01, 0.01, 0, 0)
        _Color ("Tint", Color) = (1,1,1,1)
        _Transparency ("Alpha", Range(0,1)) = 0.5
        [HideInInspector]_MainTex_ST ("MainTex_ST", Vector) = (1,1,0,0)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Cull Front // 反转剔除方向以从内部看到球体
        Blend SrcAlpha OneMinusSrcAlpha

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
                float2 scrolledUV = TRANSFORM_TEX(v.uv, _MainTex) + _Time.y * _Speed.xy;
                o.uv = scrolledUV;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.a *= _Transparency;
                return col;
            }
            ENDCG
        }
    }
}