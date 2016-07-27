Shader "Custom/BeginnerShader" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader{
		Pass {
			GLSLPROGRAM
	#ifdef VERTEX // here begins the vertex shader

			varying vec4 vertex;
			// this is a varying variable in the vertex shader

			void main()
			{
				vertex = gl_Vertex;
				gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
			}

	#endif // here ends the vertex shader

	#ifdef FRAGMENT // here begins the fragment shader

			varying vec4 vertex;
			// this is a varying variable in the fragment shader

			void main() {
				// Pick a coordinate to visualize in a grid
				vec2 coord = vertex.xz;

				// Compute anti-aliased world-space grid lines
				vec2 grid = abs(fract(coord - 0.5) - 0.5) / fwidth(coord);
				float line = min(grid.x, grid.y);

				// Just visualize the grid lines directly
				gl_FragColor = vec4(vec3(1.0 - min(line, 1.0)), 1);
			}

	#endif // here ends the fragment shader

			ENDGLSL
		}
		UsePass "Custom/Standard/FORWARD" 
		//UsePass "Custom/Standard/FORWARD_DELTA" 
		//UsePass "Custom/Standard/ShadowCaster" 
		//UsePass "Custom/Standard/DEFERRED" 
		//UsePass "Custom/StandardMETA" 
	}
}
