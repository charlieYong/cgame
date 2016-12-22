// Upgrade NOTE: commented out 'float4 unity_LightmapST', a built-in variable
// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D



Shader "Sgame/fackRefShader_Rim_LightMap" {
	
Properties {
	_MainTex ("Base", 2D) = "white" {}
	_Cube ("环境贴图", CUBE) = "black" {}
	_RefStrength ("反射强度", Range(0.0, 1.0)) = 0.0
	_RimColor ("边缘光颜色", Color) = (0.12, 0.31, 0.47, 1.0)  
	_RimPower ("边缘光强度", Range(0, 8.0)) = 3.0  
	_fogMaxDistance ("雾的最大加深距离", Range(0.0, 1000.0)) = 1000.0
}

CGINCLUDE		
	
#include "UnityCG.cginc"
			
ENDCG 

SubShader {
	Tags { "RenderType"="Opaque" }
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

		fixed4 _RimColor; 
		half _RimPower;
		float _fogMaxDistance;

		struct v2f_full
		{
			half4 pos : POSITION;
			float3 normal : TEXCOORD0;
			half2 uv : TEXCOORD1;
			float4 posWorld : TEXCOORD2;
			half3 reflDir : TEXCOORD3;
			UNITY_FOG_COORDS(4)
			#ifdef LIGHTMAP_ON
				half2 uvLM : TEXCOORD5;
			#endif	
		};
		
		v2f_full vert (appdata_full v) 
		{
			v2f_full o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.posWorld = mul(_Object2World, v.vertex);
			o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
			if(_fogMaxDistance >= distance(0,o.pos)){
				UNITY_TRANSFER_FOG(o,o.pos);
			}
			else{
				UNITY_TRANSFER_FOG(o,normalize(o.pos)*_fogMaxDistance);
			}
			o.normal = mul(float4(v.normal,0), _World2Object).xyz;
			o.reflDir = 0;
			if(_RefStrength>0){
				half3 viewDir = WorldSpaceViewDir (v.vertex);
				o.reflDir = -reflect (viewDir, normalize(o.normal));
			}


			#ifdef LIGHTMAP_ON
				o.uvLM = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
			#endif
				
			return o; 
		}
				
		fixed4 frag (v2f_full i) : COLOR0 
		{
			fixed4 tex = tex2D(_MainTex, i.uv);

			#ifdef LIGHTMAP_ON
				fixed3 lm = ( DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM)));
				tex.rgb *= lm;
			#endif	
			if(_RefStrength>0){
				fixed4 refl = texCUBE (_Cube, i.reflDir);
				tex  = tex*(1-_RefStrength) + refl*_RefStrength;
				}
			if(_RimPower>0)
			{
				float3 ViewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				half Rim = 1.0 - max(0, dot(normalize(i.normal), ViewDirection));
				tex= tex+(_RimColor* pow (Rim, _RimPower));
			}
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
