// Upgrade NOTE: commented out 'float4 unity_LightmapST', a built-in variable
// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable
// Upgrade NOTE: replaced tex2D unity_Lightmap with UNITY_SAMPLE_TEX2D

// Unlit alpha-cutout shader.
// - no lighting
// - no per-material color

Shader "Unlit/Transparent_Cutout_LightMap" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	_fogMaxDistance ("雾的最大加深距离", Range(0.0, 1000.0)) = 1000.0
}
SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 100

	Lighting Off

	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
			
			#include "UnityCG.cginc"

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			#ifdef LIGHTMAP_ON
				half2 uvLM : TEXCOORD5;
			#endif	
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _Cutoff;
			float _fogMaxDistance;
			// float4 unity_LightmapST;	
			// sampler2D unity_Lightmap;

			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				if(_fogMaxDistance >= distance(0,o.vertex)){
					UNITY_TRANSFER_FOG(o,o.vertex);
				}
				else{
					UNITY_TRANSFER_FOG(o,normalize(o.vertex)*_fogMaxDistance);
					}
			#ifdef LIGHTMAP_ON
				o.uvLM = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
			#endif
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
				clip(col.a - _Cutoff);
				#ifdef LIGHTMAP_ON
					fixed3 lm = ( DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uvLM)));
					col.rgb *= lm;
				#endif
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
		ENDCG
	}
}

}
