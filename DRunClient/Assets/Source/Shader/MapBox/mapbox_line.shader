Shader "MapBox/Line"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    { 
        ScreenMask("ScreenMask", Vector) = (0.2, 0.2, 0.8, 0.8)

        _Opacity("Opacity", Float) = 1
        _Stencil("Stencil ID", Float) = 0
        _Width("Width", Float) = 1
        _GapWidth("GapWidth",Float) = 1

        _Offset("Offset", Float) = 0
        _DevicePixelRatio("DevicePixelRatio",Float) = 1
    }

        // The SubShader block containing the Shader code.
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags 
        { 
            "Queue" = "Transparent"
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref [_Stencil]
            Comp Equal
        }

        Pass
        {
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader.
            #pragma vertex vert
            // This line defines the name of the fragment shader.
            #pragma fragment frag

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                half4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)

            float _Offset;
            float _Width;
            float _GapWidth;
            float _DevicePixelRatio;

            CBUFFER_END

            const float scale = 0.015873016;

            float4 TransformObjectToHClip2(float4 positionOS)
            {
                return mul(GetWorldToHClipMatrix(), mul(GetObjectToWorldMatrix(), positionOS));
            }

            // The vertex shader definition with properties defined in the Varyings
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;

                float ANTIALIASING = 1.0 / _DevicePixelRatio / 2.0;

                float2 a_extrude = IN.normal.xy;// floor(IN.normal.xy * 0.5);
                float a_direction = (IN.uv.x % 4.0) - 1.0;

                //OUT.linesofar = (floor(IN.uv.x / 4.0) + IN.uv.y * 64.0) * 2.0;

                float4 pos = TransformObjectToHClip(IN.positionOS.xyz);

                float2 sc_extrude = TransformWorldToHClipDir(TransformObjectToWorldDir(float3(a_extrude, 0), false), true).xy;

                //float2 normal = IN.normal.xy - floor(IN.normal.xy * 0.5);
                //normal.y = normal.y * 2.0 - 1.0;

                //OUT
                //float gapwidth = _GapWidth / 2.0;
                //float halfwidth = _Width / 2.0;
                //float offset = -1.0 * _Offset;

                //float inset = gapwidth + (gapwidth > 0.0 ? ANTIALIASING : 0.0);
                //float outset = gapwidth + halfwidth * (gapwidth > 0.0 ? 2.0 : 1.0);// +(halfwidth == 0.0 ? 0.0 : ANTIALIASING);
                float outset = _GapWidth + _Width * (_GapWidth > 0.0f ? 2.0 : 1.0);

                float2 dist = outset * sc_extrude;

                //float u = 0.5f * a_direction;
                //float t = 1.0 - abs(u);
                //float2 ss = a_extrude * offset * normal.y;

                //float2 offset2 = mul(float2x2(t, -u, u, t), ss);
                
                dist *= _ScreenParams.zw - 1;

                float4 projected_extrude = float4(dist * pos.w, 0, 0);//float4(dist * pow(pos.w, 1 - 0), 0, 0);

                OUT.positionHCS = pos + projected_extrude;
                //OUT.positionHCS = TransformObjectToHClip2(float4(pos, 0, 1));
                //OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.color = IN.color;// float4(sc_extrude, 0, 1);

                // Returning the output.
                return OUT;
            }

            // The fragment shader definition.
            half4 frag(Varyings IN) : SV_Target
            {
                return IN.color;
            }
            ENDHLSL
        }
    }
}