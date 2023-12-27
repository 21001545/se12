Shader "MapBox/LineNoStencilTexture"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    { 
        ScreenMask("ScreenMask", Vector) = (0.2, 0.2, 0.8, 0.8)
        _MainTex("Texture", 2D) = "white" {}

        _Color("Color",Color) = (1,1,1,1)
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

        //Stencil
        //{
        //    Ref [_Stencil]
        //    Comp Equal
        //}

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
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)

            float _Offset;
            float _Width;
            float _GapWidth;
            float _DevicePixelRatio;
            half4 _Color;
            sampler2D _MainTex;

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

                float4 pos = TransformObjectToHClip(IN.positionOS.xyz);
                float4 pos_uv = TransformObjectToHClip2(float4(IN.uv.x, IN.uv.x, 0, 0));

                float2 screenExtrudeDir = TransformWorldToHClipDir(TransformObjectToWorldDir(float3(a_extrude, 0), false), true).xy;

                float screenThickness = _GapWidth + _Width * (_GapWidth > 0.0f ? 2.0 : 1.0);

                float2 screenExtrude = screenThickness * screenExtrudeDir;
                screenExtrude *= _ScreenParams.zw - 1;

                float4 projected_extrude = float4(screenExtrude * pos.w, 0, 0);
                float2 uv = IN.uv;
                uv.x = length(pos_uv.xy);
                uv.x /= (screenThickness * (_ScreenParams.z - 1) * 2 * pos.w);

                OUT.positionHCS = pos + projected_extrude;
                OUT.color = IN.color * _Color;
                OUT.uv = uv;

                return OUT;
            }


            // The fragment shader definition.
            half4 frag(Varyings IN) : SV_Target
            {
                return tex2D(_MainTex, IN.uv) * IN.color;
            }
            ENDHLSL
        }
    }
}