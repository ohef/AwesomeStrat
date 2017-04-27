Shader "Custom/NewSurfaceShader" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_GradientScale("Gradient Scale", Range(0.0,10.0)) = 1.0
		_GradientColor("Gradient Color", Color) = (1,1,1,1)
	}

	SubShader{
		Pass{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			Tags { "Queue" = "Transparent" "PreviewType" = "Plane" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			// vertex shader inputs
			struct appdata
			{
				float4 vertex : POSITION; // vertex position
				float2 uv : TEXCOORD0; // texture coordinate
			};

			// vertex shader outputs ("vertex to fragment")
			struct v2f
			{
				float2 uv : TEXCOORD0; // texture coordinate
				float4 vertex : SV_POSITION; // clip space position
			};

			// vertex shader
			v2f vert(appdata v)
			{
				v2f o;
				// transform position to clip space
				// (multiply with model*view*projection matrix)
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				// just pass the texture coordinate
				o.uv = v.uv;
				return o;
			}

			// texture we will sample
			sampler2D _MainTex;
			fixed _GradientScale;
			fixed4 _GradientColor;

			// pixel shader; returns low precision ("fixed4" type)
			// color ("SV_Target" semantic)
			fixed4 frag(v2f i) : SV_Target
			{
				// Albedo comes from a texture tinted by color
				float2 uv = i.uv;

				//Integer Portion isn't really used, just a place holder
				float ip = 0;
				//The offset for the sin given the time
				float offset = modf(_Time.y / 4.0, ip);

				//The gradient moves along the v direction in UV space
				float t = abs(sin((uv.y + offset) * 3.14 * 2 * _GradientScale));

				//fixed4 c = lerp(tex2D(_MainTex, IN.uv_MainTex), _Color, t);
				//fixed4 c = max(tex2D(_MainTex, IN.uv_MainTex), _Color * t);
				//fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color * t;
				fixed4 c = 1 - (1 - tex2D(_MainTex, uv)) * (1 - _GradientColor * t);
				fixed4 ret = { c.rgb, tex2D(_MainTex, uv).a };
				return ret;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}