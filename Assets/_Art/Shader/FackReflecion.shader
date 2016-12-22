// Upgrade NOTE: commented out 'float4 unity_LightmapST', a built-in variable
// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D



Shader "Sgame/fackRefShader" {
	
Properties {
	_MainTex ("Base", 2D) = "white" {}
	_Cube ("环境贴图", CUBE) = "black" {}
	_RefStrength ("反射强度", Range(0.0, 1.0)) = 0.1
}

CGINCLUDE		
	
#include "UnityCG.cginc"
			
ENDCG 

SubShader {
	Tags { "RenderType"="Opaque" }
	Fog{Mode Linear Color (0.87,0.87,0.87,1) Density 0.1  Range 0,300}
	LOD 300 

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_fog


		sampler2D _MainTex;
		uniform samplerCUBE _Cube;
		float4 _MainTex_ST;
		// float4 unity_LightmapST;	
		// sampler2D unity_Lightmap;
		float _RefStrength;

		struct v2f_full
		{
			half4 pos : POSITION;
			half2 uv : TEXCOORD0;
			half3 reflDir : TEXCOORD1;
			#ifdef LIGHTMAP_ON
				half2 uvLM : TEXCOORD2;
			#endif	
			UNITY_FOG_COORDS(4)
		};
		
		v2f_full vert (appdata_full v) 
		{
			v2f_full o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
			o.reflDir = WorldSpaceViewDir (v.vertex);
			o.reflDir = reflect (o.reflDir, normalize(mul((half3x3)_Object2World, v.normal.xyz)));
			#ifdef LIGHTMAP_ON
				o.uvLM = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
			#endif
			UNITY_TRANSFER_FOG(o,o.pos);
			return o; 
		}
				
		fixed4 frag (v2f_full i) : COLOR0 
		{
			fixed4 tex = tex2D(_MainTex, i.uv.xy);
			fixed4 refl = texCUBE (_Cube, i.reflDir);
		
			#ifdef LIGHTMAP_ON
				fixed3 lm = ( DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM)));
				tex.rgb *= lm;
			#endif	
			
			tex  = tex + refl*_RefStrength;
			UNITY_APPLY_FOG(i.fogCoord,tex);
			return tex;	
		}	
		
		#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
		#pragma fragmentoption ARB_precision_hint_fastest 
	
		ENDCG
	}
} 

FallBack "AngryBots/Fallback"
}
