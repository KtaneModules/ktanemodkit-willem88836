// Combination of Mobile/Diffuse and LightingLambert, but added in support
// for blending with a second unlit texture.

// Mobile/Diffuse:
// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color

Shader "KT/Blend Lit and Color" {
Properties {
    _Blend ("Blend", Range (0,1)) = 0 
	_MainTex ("Lit Base (RGB)", 2D) = "white" {}
    //_UnlitTex ("Unlit Base (RGB)", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
// Mobile improvement: noforwardadd
// http://answers.unity3d.com/questions/1200437/how-to-make-a-conditional-pragma-surface-noforward.html
// http://gamedev.stackexchange.com/questions/123669/unity-surface-shader-conditinally-noforwardadd
#pragma surface surf BlendLitUnlit

half _Blend;
sampler2D _MainTex;
sampler2D _UnlitTex;
float3 _Color;

struct Input {
	float2 uv_MainTex;
	float3 color;
};

struct SurfaceOutputCustom {
    half3 Albedo;
    half3 Normal;
    half3 Emission;
    half Specular;
    half Gloss;
    half Alpha;
    // Custom fields:
	float3 Color;
};

void surf (Input IN, inout SurfaceOutputCustom o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = c.rgb;
	o.Alpha = c.a;
	o.Color = _Color.rgb;
}

half4 LightingBlendLitUnlit (SurfaceOutputCustom s, half3 lightDir, half atten)
{
	fixed diff = max (0, dot (s.Normal, lightDir));
	
	fixed3 clr = s.Color.rgb * _Blend;
	
	fixed4 c;

	c.rgb = s.Albedo * _Blend + clr * (1 - _Blend);

	//c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten) * (1 - _Blend) + clr;
	c.a = s.Alpha;
	
	return c;
}

ENDCG
}

Fallback "Mobile/Diffuse"
}
