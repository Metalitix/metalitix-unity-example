Shader "Metalitix/DashboardMaterial"
{
    Properties
    {

        _Color("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "DisableBatching" = "True"
        }

        Pass
        {
            Cull Off
            ZTest off
            ZWrite Off
            ColorMask RGB

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct VertexInput
            {
                float4 Position : POSITION;
            };

            struct VertexOutput
            {
                float4 Position : SV_POSITION;
                float3 WorldPos : TEXCOORD1;
                float4 Color : COLOR;
            };

            uniform fixed4 _Color;

            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;
                output.Position = UnityObjectToClipPos(input.Position);
                output.WorldPos = mul(unity_ObjectToWorld, input.Position).xyz;
                output.Color = _Color;
                return output;
            }

            fixed4 frag(VertexOutput input) : SV_Target
            {
                return input.Color;
            }
            ENDCG
        }
    }
}