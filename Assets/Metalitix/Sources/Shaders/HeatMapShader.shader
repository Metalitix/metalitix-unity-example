// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Metalitix/HeatMapShader"
{
    Properties
    {
        _FillGradientBottomColor ("Fill Gradient Bottom", Color) = (0, 0, 0, 1)
        _FillGradientTopColor ("Fill Gradient Top", Color) = (0, 0, 0, 1)
        _MaxDistance ("MaxDistance", float) = 1
        _SpotScale ("SpotScale", float) = 1
        _SpotBrightness ("SpotBrightness", float) = 1
        _ModelBrightness ("ModelBrightness", float) = 0.09
    }
    SubShader
    {
        Tags {"IgnoreProjector"="True" "RenderType"="Transparent"}
        Tags{ "DisableBatching" = "true"}

        ZWrite off
        ZTest off 
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200
        cull off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 Position : POSITION;
            };

            struct VertexOutput
            {
                float4 Position : SV_POSITION;
                float3 WorldPos : TEXCOORD1;
            };

            float _MaxDistance;
            float _SpotScale;
            float _SpotBrightness;
            float _ModelBrightness;
            float4 _FillGradientBottomColor;
            float4 _FillGradientTopColor;
            
            uniform int pointSize = 1100;
            uniform float4 pointsData[1100];

            float smotht(float t, float scale)
            {
                return abs(1 / (1 + exp(- scale * t)) * 2 - 1);
            }

            float3 turbocmap(float t)
            {
                float3 color;

                float3 c1 = lerp(_FillGradientBottomColor.xyz, _FillGradientTopColor.xyz, 0.6);
                float3 c2 = lerp(_FillGradientBottomColor.xyz, _FillGradientTopColor.xyz, 0.3);

                float3 c3 = lerp(float3(255. / 255., 99. / 255., 99. / 255.), c1, 0.3);

                float3 c5 = float3(255. / 255., 255. / 255., 192. / 255.);
                float3 c4 = lerp(float3(252. / 255., 180. / 255., 91. / 255.), c5, 0.3);

                float s1 = 0.;
                float s2 = 0.2;
                float s3 = 0.5;
                float s4 = 0.96;
                float s5 = 1.02;

                if (t <= s2)
                {
                    color = lerp(c1, c2, (t - s1) / (s2 - s1));
                    
                }
                else if (t <= s3)
                {
                    color = lerp(c2, c3, (t - s2) / (s3 - s2));
                }
                else if (t <= s4)
                {
                    color = lerp(c3, c4, (t - s3) / (s4 - s3));
                }
                else if (t <= s5)
                {
                    color = lerp(c4, c5, (t - s4) / (s5 - s4));
                }
                else
                {
                    color = c5;
                }

                return color;
            }

            float4 fragColorHelper(float3 vPos)
            {
                float powers = 0;

                for (int i = 0; i < pointSize; i++)
                {
                    powers += (pointsData[i][3] / _MaxDistance) * (1 - smotht(
                        distance(vPos, pointsData[i].xyz), (1 / _SpotScale) * 1.5));
                }

                powers = sqrt(smotht(powers, _SpotBrightness * 12));

                return float4(turbocmap(powers), powers * 0.85 + 0.06 - 0.06 * smotht(vPos.y, 1) + _ModelBrightness);
            }
            
            VertexOutput vert(VertexInput vertexInput)
            {
                VertexOutput vertex_output;
                vertex_output.Position = UnityObjectToClipPos(vertexInput.Position);
                vertex_output.WorldPos = mul(unity_ObjectToWorld, vertexInput.Position).xyz;
                return vertex_output;
            }

            fixed4 frag(VertexOutput vertexOutput) : SV_Target
            {
                return fragColorHelper(vertexOutput.WorldPos);
            }
            
            ENDCG
        }
    }
    FallBack Off
}