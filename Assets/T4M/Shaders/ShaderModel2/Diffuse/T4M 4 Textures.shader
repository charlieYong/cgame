Shader "T4MShaders/ShaderModel2/Diffuse/T4M 4 Textures" {
Properties {
	_Splat0 ("Layer 1", 2D) = "white" {}
	_Splat1 ("Layer 2", 2D) = "white" {}
	_Splat2 ("Layer 3", 2D) = "white" {}
	_Splat3 ("Layer 4", 2D) = "white" {}
	_Tiling3("_Tiling4 x/y", Vector)=(1,1,0,0)
	_Control ("Control (RGBA)", 2D) = "white" {}
	_MainTex ("Never Used", 2D) = "white" {}
	_fogMaxDistance ("雾的最大加深距离", Range(0.0, 1000.0)) = 1000.0
}
                
SubShader {
	Tags {
   "SplatCount" = "4"
   "RenderType" = "Opaque"
	}

	// ------------------------------------------------------------
	// Surface shader code generated out of a CGPROGRAM block:
	

	// ---- forward rendering base pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardBase" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma exclude_renderers xbox360 ps3
#pragma multi_compile_fog
#pragma multi_compile_fwdbase
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
// Surface shader code generated based on:
// writes to per-pixel normal: no
// writes to emission: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: no
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: no
// 4 texcoords actually used
//   float2 _Control
//   float2 _Splat0
//   float2 _Splat1
//   float2 _Splat2
#define UNITY_PASS_FORWARDBASE
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 15 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

//#pragma surface surf Lambert
//#pragma exclude_renderers xbox360 ps3


struct Input {
	float2 uv_Control : TEXCOORD0;
	float2 uv_Splat0 : TEXCOORD1;
	float2 uv_Splat1 : TEXCOORD2;
	float2 uv_Splat2 : TEXCOORD3;
	float2 uv_Splat3 : TEXCOORD4;
};
 
sampler2D _Control;
sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
float _fogMaxDistance;
float4 _Tiling3;
void surf (Input IN, inout SurfaceOutput o) {
	fixed4 splat_control = tex2D (_Control, IN.uv_Control).rgba;
		
	fixed3 lay1 = tex2D (_Splat0, IN.uv_Splat0);
	fixed3 lay2 = tex2D (_Splat1, IN.uv_Splat1);
	fixed3 lay3 = tex2D (_Splat2, IN.uv_Splat2);
	fixed3 lay4 = tex2D (_Splat3, IN.uv_Control*_Tiling3.xy);
	o.Alpha = 0.0;
	o.Albedo.rgb = (lay1 * splat_control.r + lay2 * splat_control.g + lay3 * splat_control.b + lay4 * splat_control.a);
}


// vertex-to-fragment interpolation data
// no lightmaps:
#ifdef LIGHTMAP_OFF
struct v2f_surf {
  float4 pos : SV_POSITION;
  float4 pack0 : TEXCOORD0; // _Control _Splat0
  float4 pack1 : TEXCOORD1; // _Splat1 _Splat2
  half3 worldNormal : TEXCOORD2;
  float3 worldPos : TEXCOORD3;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD4; // SH
  #endif
  SHADOW_COORDS(5)
  UNITY_FOG_COORDS(6)
  #if SHADER_TARGET >= 30
  float4 lmap : TEXCOORD7;
  #endif
};
#endif
// with lightmaps:
#ifndef LIGHTMAP_OFF
struct v2f_surf {
  float4 pos : SV_POSITION;
  float4 pack0 : TEXCOORD0; // _Control _Splat0
  float4 pack1 : TEXCOORD1; // _Splat1 _Splat2
  half3 worldNormal : TEXCOORD2;
  float3 worldPos : TEXCOORD3;
  float4 lmap : TEXCOORD4;
  SHADOW_COORDS(5)
  UNITY_FOG_COORDS(6)
  #ifdef DIRLIGHTMAP_COMBINED
  fixed3 tSpace0 : TEXCOORD7;
  fixed3 tSpace1 : TEXCOORD8;
  fixed3 tSpace2 : TEXCOORD9;
  #endif
};
#endif
float4 _Control_ST;
float4 _Splat0_ST;
float4 _Splat1_ST;
float4 _Splat2_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _Control);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord, _Splat0);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord, _Splat1);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord, _Splat2);
  float3 worldPos = mul(_Object2World, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  #if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  #endif
  #if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  #endif
  o.worldPos = worldPos;
  o.worldNormal = worldNormal;
  #ifndef DYNAMICLIGHTMAP_OFF
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
  #endif
  #ifndef LIGHTMAP_OFF
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifdef LIGHTMAP_OFF
    #if UNITY_SHOULD_SAMPLE_SH
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // LIGHTMAP_OFF

  TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
	if(_fogMaxDistance >= distance(0,o.pos)){
		UNITY_TRANSFER_FOG(o,o.pos);
	}
	else{
		UNITY_TRANSFER_FOG(o,normalize(o.pos)*_fogMaxDistance);
	} // pass fog coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_Control.x = 1.0;
  surfIN.uv_Splat0.x = 1.0;
  surfIN.uv_Splat1.x = 1.0;
  surfIN.uv_Splat2.x = 1.0;
  surfIN.uv_Splat3.x = 1.0;
  surfIN.uv_Control = IN.pack0.xy;
  surfIN.uv_Splat0 = IN.pack0.zw;
  surfIN.uv_Splat1 = IN.pack1.xy;
  surfIN.uv_Splat2 = IN.pack1.zw;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  #if !defined(LIGHTMAP_ON)
      gi.light.color = _LightColor0.rgb;
      gi.light.dir = lightDir;
      gi.light.ndotl = LambertTerm (o.Normal, gi.light.dir);
  #endif
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #if UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingLambert_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingLambert (o, gi);
  UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}

ENDCG

}

	// ---- forward rendering additive lights pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardAdd" }
		ZWrite Off Blend One One

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma exclude_renderers xbox360 ps3
#pragma multi_compile_fog
#pragma multi_compile_fwdadd
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
// Surface shader code generated based on:
// writes to per-pixel normal: no
// writes to emission: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: no
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: no
// 4 texcoords actually used
//   float2 _Control
//   float2 _Splat0
//   float2 _Splat1
//   float2 _Splat2
#define UNITY_PASS_FORWARDADD
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 15 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

//#pragma surface surf Lambert
//#pragma exclude_renderers xbox360 ps3


struct Input {
	float2 uv_Control : TEXCOORD0;
	float2 uv_Splat0 : TEXCOORD1;
	float2 uv_Splat1 : TEXCOORD2;
	float2 uv_Splat2 : TEXCOORD3;
	float2 uv_Splat3 : TEXCOORD4;
};
 
sampler2D _Control;
sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
float _fogMaxDistance;
float4 _Tiling3;
void surf (Input IN, inout SurfaceOutput o) {
	fixed4 splat_control = tex2D (_Control, IN.uv_Control).rgba;
		
	fixed3 lay1 = tex2D (_Splat0, IN.uv_Splat0);
	fixed3 lay2 = tex2D (_Splat1, IN.uv_Splat1);
	fixed3 lay3 = tex2D (_Splat2, IN.uv_Splat2);
	fixed3 lay4 = tex2D (_Splat3, IN.uv_Control*_Tiling3.xy);
	o.Alpha = 0.0;
	o.Albedo.rgb = (lay1 * splat_control.r + lay2 * splat_control.g + lay3 * splat_control.b + lay4 * splat_control.a);
}


// vertex-to-fragment interpolation data
struct v2f_surf {
  float4 pos : SV_POSITION;
  float4 pack0 : TEXCOORD0; // _Control _Splat0
  float4 pack1 : TEXCOORD1; // _Splat1 _Splat2
  half3 worldNormal : TEXCOORD2;
  float3 worldPos : TEXCOORD3;
  SHADOW_COORDS(4)
  UNITY_FOG_COORDS(5)
};
float4 _Control_ST;
float4 _Splat0_ST;
float4 _Splat1_ST;
float4 _Splat2_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _Control);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord, _Splat0);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord, _Splat1);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord, _Splat2);
  float3 worldPos = mul(_Object2World, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos = worldPos;
  o.worldNormal = worldNormal;

  TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
  	if(_fogMaxDistance >= distance(0,o.pos)){
		UNITY_TRANSFER_FOG(o,o.pos);
	}
	else{
		UNITY_TRANSFER_FOG(o,normalize(o.pos)*_fogMaxDistance);
	};// pass fog coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_Control.x = 1.0;
  surfIN.uv_Splat0.x = 1.0;
  surfIN.uv_Splat1.x = 1.0;
  surfIN.uv_Splat2.x = 1.0;
  surfIN.uv_Splat3.x = 1.0;
  surfIN.uv_Control = IN.pack0.xy;
  surfIN.uv_Splat0 = IN.pack0.zw;
  surfIN.uv_Splat1 = IN.pack1.xy;
  surfIN.uv_Splat2 = IN.pack1.zw;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  #if !defined(LIGHTMAP_ON)
      gi.light.color = _LightColor0.rgb;
      gi.light.dir = lightDir;
      gi.light.ndotl = LambertTerm (o.Normal, gi.light.dir);
  #endif
  gi.light.color *= atten;
  c += LightingLambert (o, gi);
  c.a = 0.0;
  UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}

ENDCG

}

	// ---- deferred lighting base geometry pass:
	Pass {
		Name "PREPASS"
		Tags { "LightMode" = "PrePassBase" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma exclude_renderers xbox360 ps3
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
// Surface shader code generated based on:
// writes to per-pixel normal: no
// writes to emission: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: no
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: YES
// 0 texcoords actually used
#define UNITY_PASS_PREPASSBASE
#include "UnityCG.cginc"
#include "Lighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 15 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

//#pragma surface surf Lambert
//#pragma exclude_renderers xbox360 ps3


struct Input {
	float2 uv_Control : TEXCOORD0;
	float2 uv_Splat0 : TEXCOORD1;
	float2 uv_Splat1 : TEXCOORD2;
	float2 uv_Splat2 : TEXCOORD3;
	float2 uv_Splat3 : TEXCOORD4;
};
 
sampler2D _Control;
sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
float _fogMaxDistance;
float4 _Tiling3;
void surf (Input IN, inout SurfaceOutput o) {
	fixed4 splat_control = tex2D (_Control, IN.uv_Control).rgba;
		
	fixed3 lay1 = tex2D (_Splat0, IN.uv_Splat0);
	fixed3 lay2 = tex2D (_Splat1, IN.uv_Splat1);
	fixed3 lay3 = tex2D (_Splat2, IN.uv_Splat2);
	fixed3 lay4 = tex2D (_Splat3, IN.uv_Control*_Tiling3.xy);
	o.Alpha = 0.0;
	o.Albedo.rgb = (lay1 * splat_control.r + lay2 * splat_control.g + lay3 * splat_control.b + lay4 * splat_control.a);
}


// vertex-to-fragment interpolation data
struct v2f_surf {
  float4 pos : SV_POSITION;
  half3 worldNormal : TEXCOORD0;
  float3 worldPos : TEXCOORD1;
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
  float3 worldPos = mul(_Object2World, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos = worldPos;
  o.worldNormal = worldNormal;
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_Control.x = 1.0;
  surfIN.uv_Splat0.x = 1.0;
  surfIN.uv_Splat1.x = 1.0;
  surfIN.uv_Splat2.x = 1.0;
  surfIN.uv_Splat3.x = 1.0;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);

  // output normal and specular
  fixed4 res;
  res.rgb = o.Normal * 0.5 + 0.5;
  res.a = o.Specular;
  return res;
}

ENDCG

}

	// ---- deferred lighting final pass:
	Pass {
		Name "PREPASS"
		Tags { "LightMode" = "PrePassFinal" }
		ZWrite Off

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma exclude_renderers xbox360 ps3
#pragma multi_compile_fog
#pragma multi_compile_prepassfinal
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
// Surface shader code generated based on:
// writes to per-pixel normal: no
// writes to emission: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: no
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: no
// 4 texcoords actually used
//   float2 _Control
//   float2 _Splat0
//   float2 _Splat1
//   float2 _Splat2
#define UNITY_PASS_PREPASSFINAL
#include "UnityCG.cginc"
#include "Lighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 15 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

//#pragma surface surf Lambert
//#pragma exclude_renderers xbox360 ps3


struct Input {
	float2 uv_Control : TEXCOORD0;
	float2 uv_Splat0 : TEXCOORD1;
	float2 uv_Splat1 : TEXCOORD2;
	float2 uv_Splat2 : TEXCOORD3;
	float2 uv_Splat3 : TEXCOORD4;
};
 
sampler2D _Control;
sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
float _fogMaxDistance;
float4 _Tiling3;
void surf (Input IN, inout SurfaceOutput o) {
	fixed4 splat_control = tex2D (_Control, IN.uv_Control).rgba;
		
	fixed3 lay1 = tex2D (_Splat0, IN.uv_Splat0);
	fixed3 lay2 = tex2D (_Splat1, IN.uv_Splat1);
	fixed3 lay3 = tex2D (_Splat2, IN.uv_Splat2);
	fixed3 lay4 = tex2D (_Splat3, IN.uv_Control*_Tiling3.xy);
	o.Alpha = 0.0;
	o.Albedo.rgb = (lay1 * splat_control.r + lay2 * splat_control.g + lay3 * splat_control.b + lay4 * splat_control.a);
}


// vertex-to-fragment interpolation data
struct v2f_surf {
  float4 pos : SV_POSITION;
  float4 pack0 : TEXCOORD0; // _Control _Splat0
  float4 pack1 : TEXCOORD1; // _Splat1 _Splat2
  float3 worldPos : TEXCOORD2;
  float4 screen : TEXCOORD3;
  float4 lmap : TEXCOORD4;
#ifdef LIGHTMAP_OFF
  float3 vlight : TEXCOORD5;
#else
#ifdef DIRLIGHTMAP_OFF
  float4 lmapFadePos : TEXCOORD5;
#endif
#endif
  UNITY_FOG_COORDS(6)
  #if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
  fixed3 tSpace0 : TEXCOORD7;
  fixed3 tSpace1 : TEXCOORD8;
  fixed3 tSpace2 : TEXCOORD9;
  #endif
};
float4 _Control_ST;
float4 _Splat0_ST;
float4 _Splat1_ST;
float4 _Splat2_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _Control);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord, _Splat0);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord, _Splat1);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord, _Splat2);
  float3 worldPos = mul(_Object2World, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  #if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  #endif
  #if !defined(LIGHTMAP_OFF) && defined(DIRLIGHTMAP_COMBINED)
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  #endif
  o.worldPos = worldPos;
  o.screen = ComputeScreenPos (o.pos);
#ifndef DYNAMICLIGHTMAP_OFF
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifndef LIGHTMAP_OFF
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(_Object2World, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-mul(UNITY_MATRIX_MV, v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
  float3 worldN = UnityObjectToWorldNormal(v.normal);
  o.vlight = ShadeSH9 (float4(worldN,1.0));
#endif
  	if(_fogMaxDistance >= distance(0,o.pos)){
		UNITY_TRANSFER_FOG(o,o.pos);
	}
	else{
		UNITY_TRANSFER_FOG(o,normalize(o.pos)*_fogMaxDistance);
	}; // pass fog coordinates to pixel shader
  return o;
}
sampler2D _LightBuffer;
#if defined (SHADER_API_XBOX360) && defined (UNITY_HDR_ON)
sampler2D _LightSpecBuffer;
#endif
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_Control.x = 1.0;
  surfIN.uv_Splat0.x = 1.0;
  surfIN.uv_Splat1.x = 1.0;
  surfIN.uv_Splat2.x = 1.0;
  surfIN.uv_Splat3.x = 1.0;
  surfIN.uv_Control = IN.pack0.xy;
  surfIN.uv_Splat0 = IN.pack0.zw;
  surfIN.uv_Splat1 = IN.pack1.xy;
  surfIN.uv_Splat2 = IN.pack1.zw;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  half4 light = tex2Dproj (_LightBuffer, UNITY_PROJ_COORD(IN.screen));
#if defined (SHADER_API_MOBILE)
  light = max(light, half4(0.001, 0.001, 0.001, 0.001));
#endif
#ifndef UNITY_HDR_ON
  light = -log2(light);
#endif
#if defined (SHADER_API_XBOX360) && defined (UNITY_HDR_ON)
  light.w = tex2Dproj (_LightSpecBuffer, UNITY_PROJ_COORD(IN.screen)).r;
#endif
  #ifndef LIGHTMAP_OFF
    #ifdef DIRLIGHTMAP_OFF
      // single lightmap
      fixed4 lmtex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.lmap.xy);
      fixed3 lm = DecodeLightmap (lmtex);
      light.rgb += lm;
    #elif DIRLIGHTMAP_COMBINED
      // directional lightmaps
      fixed4 lmtex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.lmap.xy);
      half4 lm = half4(DecodeLightmap(lmtex), 0);
      light += lm;
    #elif DIRLIGHTMAP_SEPARATE
      // directional with specular - no support
    #endif // DIRLIGHTMAP_OFF
  #else
    light.rgb += IN.vlight;
  #endif // !LIGHTMAP_OFF

  #ifndef DYNAMICLIGHTMAP_OFF
  fixed4 dynlmtex = UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, IN.lmap.zw);
  light.rgb += DecodeRealtimeLightmap (dynlmtex);
  #endif

  half4 c = LightingLambert_PrePass (o, light);
  UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}

ENDCG

}

	// ---- deferred shading pass:
	Pass {
		Name "DEFERRED"
		Tags { "LightMode" = "Deferred" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma exclude_renderers xbox360 ps3
#pragma exclude_renderers nomrt
#pragma multi_compile_prepassfinal
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
// Surface shader code generated based on:
// writes to per-pixel normal: no
// writes to emission: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: no
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: YES
// 4 texcoords actually used
//   float2 _Control
//   float2 _Splat0
//   float2 _Splat1
//   float2 _Splat2
#define UNITY_PASS_DEFERRED
#include "UnityCG.cginc"
#include "Lighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 15 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

//#pragma surface surf Lambert
//#pragma exclude_renderers xbox360 ps3


struct Input {
	float2 uv_Control : TEXCOORD0;
	float2 uv_Splat0 : TEXCOORD1;
	float2 uv_Splat1 : TEXCOORD2;
	float2 uv_Splat2 : TEXCOORD3;
	float2 uv_Splat3 : TEXCOORD4;
};
 
sampler2D _Control;
sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
float _fogMaxDistance;
float4 _Tiling3;
void surf (Input IN, inout SurfaceOutput o) {
	fixed4 splat_control = tex2D (_Control, IN.uv_Control).rgba;
		
	fixed3 lay1 = tex2D (_Splat0, IN.uv_Splat0);
	fixed3 lay2 = tex2D (_Splat1, IN.uv_Splat1);
	fixed3 lay3 = tex2D (_Splat2, IN.uv_Splat2);
	fixed3 lay4 = tex2D (_Splat3, IN.uv_Control*_Tiling3.xy);
	o.Alpha = 0.0;
	o.Albedo.rgb = (lay1 * splat_control.r + lay2 * splat_control.g + lay3 * splat_control.b + lay4 * splat_control.a);
}


// vertex-to-fragment interpolation data
struct v2f_surf {
  float4 pos : SV_POSITION;
  float4 pack0 : TEXCOORD0; // _Control _Splat0
  float4 pack1 : TEXCOORD1; // _Splat1 _Splat2
  half3 worldNormal : TEXCOORD2;
  float3 worldPos : TEXCOORD3;
  float4 lmap : TEXCOORD4;
#ifdef LIGHTMAP_OFF
  #if UNITY_SHOULD_SAMPLE_SH
    half3 sh : TEXCOORD5; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD5;
  #endif
#endif
};
float4 _Control_ST;
float4 _Splat0_ST;
float4 _Splat1_ST;
float4 _Splat2_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _Control);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord, _Splat0);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord, _Splat1);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord, _Splat2);
  float3 worldPos = mul(_Object2World, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos = worldPos;
  o.worldNormal = worldNormal;
#ifndef DYNAMICLIGHTMAP_OFF
  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#else
  o.lmap.zw = 0;
#endif
#ifndef LIGHTMAP_OFF
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(_Object2World, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-mul(UNITY_MATRIX_MV, v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outDiffuse : SV_Target0,
    out half4 outSpecSmoothness : SV_Target1,
    out half4 outNormal : SV_Target2,
    out half4 outEmission : SV_Target3) {
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_Control.x = 1.0;
  surfIN.uv_Splat0.x = 1.0;
  surfIN.uv_Splat1.x = 1.0;
  surfIN.uv_Splat2.x = 1.0;
  surfIN.uv_Splat3.x = 1.0;
  surfIN.uv_Control = IN.pack0.xy;
  surfIN.uv_Splat0 = IN.pack0.zw;
  surfIN.uv_Splat1 = IN.pack1.xy;
  surfIN.uv_Splat2 = IN.pack1.zw;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = IN.worldNormal;
  normalWorldVertex = IN.worldNormal;

  // call surface function
  surf (surfIN, o);
fixed3 originalNormal = o.Normal;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  gi.light.ndotl = LambertTerm (o.Normal, gi.light.dir);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #if UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingLambert_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingLambert_Deferred (o, gi, outDiffuse, outSpecSmoothness, outNormal);
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
  UNITY_OPAQUE_ALPHA(outDiffuse.a);
}

ENDCG

}

	// ---- meta information extraction pass:
	Pass {
		Name "Meta"
		Tags { "LightMode" = "Meta" }
		Cull Off

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma exclude_renderers xbox360 ps3
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
// Surface shader code generated based on:
// writes to per-pixel normal: no
// writes to emission: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: no
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: no
// reads from normal: no
// 4 texcoords actually used
//   float2 _Control
//   float2 _Splat0
//   float2 _Splat1
//   float2 _Splat2
#define UNITY_PASS_META
#include "UnityCG.cginc"
#include "Lighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal

// Original surface shader snippet:
#line 15 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

//#pragma surface surf Lambert
//#pragma exclude_renderers xbox360 ps3


struct Input {
	float2 uv_Control : TEXCOORD0;
	float2 uv_Splat0 : TEXCOORD1;
	float2 uv_Splat1 : TEXCOORD2;
	float2 uv_Splat2 : TEXCOORD3;
	float2 uv_Splat3 : TEXCOORD4;
};
 
sampler2D _Control;
sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
float _fogMaxDistance;
float4 _Tiling3;
void surf (Input IN, inout SurfaceOutput o) {
	fixed4 splat_control = tex2D (_Control, IN.uv_Control).rgba;
		
	fixed3 lay1 = tex2D (_Splat0, IN.uv_Splat0);
	fixed3 lay2 = tex2D (_Splat1, IN.uv_Splat1);
	fixed3 lay3 = tex2D (_Splat2, IN.uv_Splat2);
	fixed3 lay4 = tex2D (_Splat3, IN.uv_Control*_Tiling3.xy);
	o.Alpha = 0.0;
	o.Albedo.rgb = (lay1 * splat_control.r + lay2 * splat_control.g + lay3 * splat_control.b + lay4 * splat_control.a);
}

#include "UnityMetaPass.cginc"

// vertex-to-fragment interpolation data
struct v2f_surf {
  float4 pos : SV_POSITION;
  float4 pack0 : TEXCOORD0; // _Control _Splat0
  float4 pack1 : TEXCOORD1; // _Splat1 _Splat2
  float3 worldPos : TEXCOORD2;
};
float4 _Control_ST;
float4 _Splat0_ST;
float4 _Splat1_ST;
float4 _Splat2_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _Control);
  o.pack0.zw = TRANSFORM_TEX(v.texcoord, _Splat0);
  o.pack1.xy = TRANSFORM_TEX(v.texcoord, _Splat1);
  o.pack1.zw = TRANSFORM_TEX(v.texcoord, _Splat2);
  float3 worldPos = mul(_Object2World, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos = worldPos;
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_Control.x = 1.0;
  surfIN.uv_Splat0.x = 1.0;
  surfIN.uv_Splat1.x = 1.0;
  surfIN.uv_Splat2.x = 1.0;
  surfIN.uv_Splat3.x = 1.0;
  surfIN.uv_Control = IN.pack0.xy;
  surfIN.uv_Splat0 = IN.pack0.zw;
  surfIN.uv_Splat1 = IN.pack1.xy;
  surfIN.uv_Splat2 = IN.pack1.zw;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UnityMetaInput metaIN;
  UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
  metaIN.Albedo = o.Albedo;
  metaIN.Emission = o.Emission;
  return UnityMetaFragment(metaIN);
}

ENDCG

}

	// ---- end of surface shader generated code

#LINE 43
 
}
// Fallback to Diffuse
Fallback "Diffuse"
}
