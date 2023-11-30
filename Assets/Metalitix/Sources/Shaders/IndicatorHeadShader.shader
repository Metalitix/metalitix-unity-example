Shader "Metalitix/IndicatorHeadShader"
{
    Properties
    {
        _Scale ("Scale", float) = 1
        _IncreaseValue ("IncreaseValue", float) = 3
        _Brightness ("Brightness", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        ZWrite off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100
        cull off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct VertexOutput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _Opacity;
            float _Power;
            float _Scale;
            float _IncreaseValue;
            float _Brightness;
            
            VertexOutput vert(VertexInput vertexInput)
            {
                VertexOutput vertex_output;
                vertexInput.vertex.xyz += vertexInput.normal.xyz * (_Scale * _IncreaseValue);
                
                vertex_output.vertex = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, vertexInput.vertex));
                vertex_output.uv = vertexInput.uv;
                return vertex_output;
            }

            float getOpacity()
            {
                return pow(cos(pow(_Power - 1, 2) * 3.14) / 2 + 0.5, 1/(_Brightness));
            }
            
            fixed4 frag(VertexOutput vertexOutput) : SV_Target
            {
                float4 color = float4(_Color.xyz, _Opacity * getOpacity());
                return color;
            }
            
            ENDCG
        }
    }
    FallBack "Diffuse"
}
