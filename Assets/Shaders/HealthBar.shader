Shader "Custom/HealthBar" {
	Properties {
		_StrokeWidth ("Stroke Width", Float ) = 0.1
		_Color ("Color", Color) = (1,1,1,1)
		_BarGrad ("Bar Gradient", 2D) = "white" {}
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _BarGrad;

		struct Input {
			float2 uv_BarGrad;
		};

		fixed4 _Color;
		float _StrokeWidth;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 g = tex2D(_BarGrad, IN.uv_BarGrad) * _Color;
			o.Albedo = g.rgb * _Color;
			if (IN.uv_BarGrad.x <= _StrokeWidth || 
				IN.uv_BarGrad.y <= _StrokeWidth || 
				IN.uv_BarGrad.x >= 1.0f - _StrokeWidth || 
				IN.uv_BarGrad.y >= 1.0f - _StrokeWidth )
			{
				float3 v = { 0.0f,0.0f,0.0f };
				o.Albedo = v;
			}
			// Metallic and smoothness come from slider variables
			o.Alpha = g.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
