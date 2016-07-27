Shader "Custom/MovementShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
	}
		SubShader{
			Tags {}
			Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			//ZWrite Off
			//ZTest Always
				GLSLPROGRAM
		#ifdef VERTEX // here begins the vertex shader

			uniform vec4 _Color;

			void main()
			{
				gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
			}

	#endif // here ends the vertex shader

	#ifdef FRAGMENT // here begins the fragment shader

			uniform vec4 _Color;

			void main() {
				gl_FragColor = _Color;
			}

	#endif // here ends the fragment shader

			ENDGLSL
		}
	}
	FallBack "Diffuse"
}
