Shader "Custom/myshader" {  
    Properties {  
        _MainTex ("Base (RGB)", 2D) = "black" {}  
		_RimColor ("边缘光颜色", Color) = (0.12, 0.31, 0.47, 1.0)  
		_RimPower ("边缘光强度", Range(0.5, 8.0)) = 3.0  
		_SimulationLightOpen("模拟光阀值 0为关闭",Range(0,1)) = 0
		_SimulationLightColor ("模拟光颜色", Color) = (0.12, 0.31, 0.47, 1.0)  
		_SimulationLightPower("模拟光强度",Range(0,2))=0
		_SimulationLightX("模拟光方向X",Range(-2.0,2.0))=0
		_SimulationLightY("模拟光方向Y",Range(-2.0,2.0))=0
		_SimulationLightZ("模拟光方向Z",Range(-2.0,2.0))=0
		_SimulationLightHard("模拟光硬度",Range(2.0,4.0))=2
		_Glow("Glow",2D) = "black"{}
		_GlowPower("Glow 强度",Range(0,2.0))=1
    }  
    SubShader {  
        Tags { "RenderType"="Opaque" }  
		Fog{Mode Linear Color (0.87,0.87,0.87,1) Density 0.1  Range 0,300}
        LOD 200  
          
        CGPROGRAM  
        #pragma surface surf Lambert    

		sampler2D _MainTex; 
		sampler2D _Glow;

		fixed4 _RimColor; 
		fixed4 _SimulationLightColor; 
		half _RimPower;  
		half _SimulationLightX;
		half _SimulationLightY;
		half _SimulationLightZ;
		half _SimulationLightPower;
		half _SimulationLightHard;
		half _SimulationLightOpen;
		half _GlowPower;

  
        struct Input {  
            float2 uv_MainTex;  
			half3 viewDir;
        };  
  
        void surf (Input IN, inout SurfaceOutput o) {  
    fixed4 c = tex2D (_MainTex, IN.uv_MainTex);  
	o.Emission = c.rgb;
    o.Alpha = c.a;
	if(_SimulationLightOpen>0){
	half3 aaa = half3(_SimulationLightX,_SimulationLightY,_SimulationLightZ);
	half simulationDirt =  (dot (normalize(aaa+normalize(IN.viewDir)), normalize(o.Normal)));
	o.Emission =_SimulationLightOpen*o.Emission+ abs(o.Emission*_SimulationLightColor.rgb*_SimulationLightPower*pow(simulationDirt,_SimulationLightHard));

	}
	  fixed rim = 1.0 - saturate (dot (normalize(IN.viewDir), normalize(o.Normal)));  
		fixed4 glow = tex2D(_Glow,IN.uv_MainTex);
    o.Emission = o.Emission+(_RimColor.rgb * pow (rim, _RimPower))+glow.rgb*_GlowPower;
        }
        ENDCG  
    }   
    FallBack "Diffuse"  
}  
