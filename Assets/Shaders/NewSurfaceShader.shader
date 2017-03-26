Shader "Custom/NewSurfaceShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_GradientScale ("Gradient Scale", Range(0.0,10.0)) = 1.0 
	}

	SubShader {
		ZWrite On
		ZTest Always
		LOD 200
		
		Tags { "Queue" = "Transparent"  "PreviewType" = "Plane" }
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

			//Integer Portion isn't really used, just a place holder
			float ip = 0;

			//The offset for the sin given the time
			float offset = modf(_Time.y / 4.0, ip);

			//The gradient moves along the v direction in UV space
			float t = abs(sin((uv.y + offset) * 3.14 * 2 * _GradientScale));

			//Why doesn't clip discard a 0? Have to subtract then
			clip(texAlph - 0.5);

			//fixed4 c = lerp(tex2D(_MainTex, IN.uv_MainTex), _Color, t);
			//fixed4 c = max(tex2D(_MainTex, IN.uv_MainTex), _Color * t);
			//fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color * t;
			fixed4 c = 1 - (1 - tex2D(_MainTex, IN.uv_MainTex)) * (1 - _Color * t);

			//o.Albedo = fixed4(c.rgb, texAlph);
			o.Albedo = c;

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
