Shader "Custom/NewSurfaceShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_GradientScale ("Gradient Scale", Range(0.0,1.0)) = 1.0 
	}
	SubShader {
		Tags { "RenderType" = "TransparentCutout" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _GradientScale;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			float2 uv = IN.uv_MainTex;
			fixed texAlph = tex2D(_MainTex, IN.uv_MainTex).a;
			float ip = 0;
			float offset = modf(_Time.y / 4.0, ip);
			//float t = abs(sin((abs(_SinTime.z) - uv.y) * 3.14 * 2));
			float t = abs(sin((uv.y + offset) * 3.14 * 2 * _GradientScale));
			clip(texAlph - 0.5);
			fixed3 c = lerp(tex2D(_MainTex, IN.uv_MainTex), _Color, t);
			o.Albedo = fixed4(c.rgb, texAlph);

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
