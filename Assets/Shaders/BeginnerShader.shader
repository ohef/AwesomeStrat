Shader "Custom/BeginnerShader" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader{
	//	Pass {
	//		GLSLPROGRAM
	//#ifdef VERTEX // here begins the vertex shader

	//		varying vec4 vertex;
	//		// this is a varying variable in the vertex shader

	//		void main()
	//		{
	//			vertex = gl_Vertex;
	//			gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
	//		}

	//#endif // here ends the vertex shader

	//#ifdef FRAGMENT // here begins the fragment shader

	//		varying vec4 vertex;
	//		// this is a varying variable in the fragment shader

	//		void main() {
	//			// Pick a coordinate to visualize in a grid
	//			vec2 coord = vertex.xz;

	//			// Compute anti-aliased world-space grid lines
	//			vec2 grid = abs(fract(coord - 0.5) - 0.5) / fwidth(coord);
	//			float line = min(grid.x, grid.y);

	//			// Just visualize the grid lines directly
	//			gl_FragColor = vec4(vec3(1.0 - min(line, 1.0)), 1);
	//		}

	//#endif // here ends the fragment shader

	//		ENDGLSL
	//	}
//		Pass {
//		CGPROGRAM
//#pragma vertex vert
//#pragma fragment frag
//#include "UnityCG.cginc"
//
//		struct vertexInput {
//			float4 vertex : POSITION;
//		};
//
//		struct vertexOutput {
//			float4 vertex : SV_POSITION;
//		};
//
//		vertexOutput vert(vertexInput v)
//		{
//			vertexOutput o;
//			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
//			return o;
//		}
//
//		float4 frag(vertexOutput v) : SV_Target{
//			return v.vertex / length(v.vertex);
//		}
//
//		ENDCG
//	}
		UsePass "Custom/Standard/FORWARD" 
		//UsePass "Custom/Standard/FORWARD_DELTA" 
		//UsePass "Custom/Standard/ShadowCaster" 
		//UsePass "Custom/Standard/DEFERRED" 
		//UsePass "Custom/StandardMETA" 
	}
}
