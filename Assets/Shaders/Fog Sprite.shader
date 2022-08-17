// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Sprites/Diffuse with Fog (URP)"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
      "RenderPipeline" = "UniversalRenderPipeline"
    }
      
    Cull Off
    Lighting Off
    ZWrite Off
    Blend One OneMinusSrcAlpha


    HLSLPROGRAM
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

    #pragma surface surf Lambert vertex:vert nolightmap nodynlightmap keepalpha noinstancing finalcolor:applyFog
    #pragma multi_compile _ PIXELSNAP_ON
    #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
    #pragma multi_compile_fog

    struct Attributes
    {
      float2 uv_MainTex;
      fixed4 color;
      float fogCoord; 
    };
    
    void vert (inout appdata_full v, out Attributes o)
    {
      v.vertex.xy *= _Flip.xy;

      #if defined(PIXELSNAP_ON)
      v.vertex = UnityPixelSnap (v.vertex);
      #endif
      
      UNITY_INITIALIZE_OUTPUT(Attributes, o);

      o.color = v.color * _Color * _RendererColor;
      //o.fogCoord = UnityObjectToClipPos(v.vertex).z;
      UNITY_TRANSFER_FOG(o, UnityObjectToClipPos(v.vertex));
    }

    void surf (Attributes IN, inout SurfaceOutput o)
    {
      fixed4 c = SampleSpriteTexture (IN.uv_MainTex) * IN.color;
      o.Albedo = c.rgb * c.a;
      o.Alpha = c.a;
    }

    void applyFog (Attributes IN, SurfaceOutput o, inout fixed4 color)
    {
      // apply fog
      UNITY_APPLY_FOG(IN.fogCoord, color.rgb);

      color.rgb *= o.Alpha;
    }


    ENDHLSL
	}
//Fallback "Transparent/VertexLit"
}