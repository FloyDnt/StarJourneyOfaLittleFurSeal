// Soft-edged ring on a quad that lies on the sand under a prop. Unlit and alpha-blended. Drawn after the opaque scene
// and before the water (queue 3000), which writes depth, so the water covers every ring under its surface.
Shader "StarJourney/PropRing"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 0.85)
        _Fade ("Fade", Range(0, 1)) = 1
        _Inner ("Inner Radius", Range(0, 1)) = 0.6
        _Soft ("Edge Softness", Range(0.001, 0.5)) = 0.1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent-10" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Off
        Offset -1, -1

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            half _Fade;
            half _Inner;
            half _Soft;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * 2.0 - 1.0;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half d = length(i.uv);
                half a = smoothstep(_Inner - _Soft, _Inner, d) * (1.0 - smoothstep(1.0 - _Soft, 1.0, d));
                return fixed4(_Color.rgb, _Color.a * a * _Fade);
            }
            ENDCG
        }
    }
}
