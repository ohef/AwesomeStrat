Shader "Custom/MovementShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
	}
		SubShader{
			Tags {}
			Pass {
				Fog{ Mode Off }

				//ZWrite Off
				ZTest Always
				//Blend One One

				CGPROGRAM
				#pragma target 3.0
				#pragma vertex vert
				#pragma fragment frag
				#pragma exclude_renderers nomrt
				half4 _Color;

				float4 vert(float4 vertex : POSITION) : SV_POSITION
				{
					return mul(UNITY_MATRIX_MVP, vertex);
				}

				half4 frag() : SV_Target
				{
					return half4(_Color.rgb, 0.33f);
				}
				ENDCG
		}
	}
	FallBack "Diffuse"
}
