// Unlit shader. Simplest possible textured shader.
// - SUPPORTS lightmap
// - no lighting
// - no per-material color

Shader "MMSG/UnlitHighLight" {
Properties {
	_Color ("_Color",Color) = (1,1,1,1)
	_AddColor("_AddColor",Color) = (0,0,0,0)
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 100
	
	// Non-lightmapped
	Pass {
		Tags { "LightMode" = "Vertex" }
		Lighting Off
		
		SetTexture [_MainTex] { 
			constantColor [_Color]
			combine texture * constant
		 } 
		 SetTexture [_MainTex] { 
			constantColor [_AddColor]
			combine texture + constant
		 }
	}
	
	// Lightmapped, encoded as dLDR
	Pass {
		Tags { "LightMode" = "VertexLM" }
		Lighting Off
		BindChannels {
			Bind "Vertex", vertex
			Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
			Bind "texcoord", texcoord1 // main uses 1st uv
		}
		SetTexture [unity_Lightmap] {
			matrix [unity_LightmapMatrix]
			constantColor [_Color]
			combine texture * constant, texture * constant
		}
		SetTexture [_MainTex] {
			combine texture * previous DOUBLE, texture * primary
		}
	}
	
	// Lightmapped, encoded as RGBM
	Pass {
		Tags { "LightMode" = "VertexLMRGBM" }

		Lighting Off
		BindChannels {
			Bind "Vertex", vertex
			Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
			Bind "texcoord", texcoord1 // main uses 1st uv
		}

		SetTexture [unity_Lightmap] {
			matrix [unity_LightmapMatrix]
			constantColor [_Color]
			combine texture * texture alpha DOUBLE
		}
		SetTexture [_MainTex] {
			combine texture * previous QUAD, texture * primary
		}
	}
}
}