Shader "Unlit/myShade"
{
    Properties
    {
    }
    SubShader
    {
      Blend SrcAlpha OneMinusSrcAlpha

      Tags {"RenderType" = "Transparent"}

      CGPROGRAM
      #pragma surface surf Lambert vertex:vert alpha:fade

      struct Input
      {
          float4 vertColor;
      };

      void vert(inout appdata_full v, out Input o)
      {
          UNITY_INITIALIZE_OUTPUT(Input, o);
          o.vertColor = v.color;
      }

      void surf(Input IN, inout SurfaceOutput o)
      {
          o.Albedo = IN.vertColor.rgb;
          o.Alpha = IN.vertColor.a;
      }
      ENDCG
    }
}
