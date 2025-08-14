Shader "Custom/GridPuzzleMask"
{
    Properties
    {
        _MainTex ("Puzzle Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _Rows ("Rows", Float) = 4
        _Cols ("Cols", Float) = 4
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float _Rows;
            float _Cols;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Which piece is this pixel in?
                float2 gridPos = floor(float2(i.uv.x * _Cols, i.uv.y * _Rows));

                // Normalize to [0,1] for mask lookup
                float2 maskUV = (gridPos + 0.5) / float2(_Cols, _Rows);

                // Read mask value
                fixed maskVal = tex2D(_MaskTex, maskUV).r;

                // If mask pixel is black => hide piece
                if (maskVal < 0.5) discard;

                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
