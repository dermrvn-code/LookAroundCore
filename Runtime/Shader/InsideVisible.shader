Shader "Unlit/Pano360Shader"
{
   Properties
   {
       _MainTex ("Base (RGB)", 2D) = "white" {}
       _Color ("Main Color", Color) = (1,1,1,1)
       _OffsetX ("Horizontal Offset", Range(-1,1)) = 0
       _OffsetY ("Vertical Offset", Range(-1,1)) = 0
   }

   SubShader 
   {
      Tags { "RenderType" = "Opaque" }
      Cull Off

      CGPROGRAM
      #pragma surface surf SimpleLambert

      half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten)
      {
         half4 c = half4(0, 0, 0, 1);
         c.rgb = s.Albedo;
         return c;
      }

      sampler2D _MainTex;
      fixed4 _Color;
      float _OffsetX;
      float _OffsetY;

      struct Input
      {
         float2 uv_MainTex;
      };

      void surf (Input IN, inout SurfaceOutput o)
      {
         // Start with original UVs
         float2 uv = IN.uv_MainTex;

         // Mirror horizontally for inside-sphere projection
         uv.x = 1 - uv.x;

         // Apply user offsets
         uv.x = frac(uv.x + _OffsetX); // wrap horizontally
         uv.y = clamp(uv.y + _OffsetY, 0, 1); // clamp vertically to avoid distortion

         fixed4 tex = tex2D(_MainTex, uv);
         tex *= _Color;

         o.Albedo = tex.rgb;
         o.Alpha  = tex.a;
      }
      ENDCG
   }

   Fallback "Diffuse"
}
