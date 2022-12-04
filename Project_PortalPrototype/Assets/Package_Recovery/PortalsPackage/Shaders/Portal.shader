Shader "Custom/Portal"
{
	// Properties in Inspector
	Properties
	{
		// Data Type ("Inspector Name", Specify Type) = Default Value
		_InactiveColor("Inactive Colour", Color) = (1, 1, 1, 1)
	}

		// What information to Pass at runtime
			SubShader
		{
			Tags { "RenderType"="Opaque"}
			LOD 100
			Cull Off

			Pass
			{
				// This is in the CG shader language --v
				CGPROGRAM

				// Initialise Functions like a header in C++
				#pragma vertex VertexFunc
				#pragma fragment FragmentFunc

				#include "UnityCG.cginc"

				// Create new data type to grab object's mesh information
				struct appdata
				{
				// DataType --> variable name : info of pixel currently being worked on
					float4 vertex : POSITION;
				};

				// Create new data type to grab object's colour information
				struct v2f
				{
					// DataType --> variable name : info of pixel currently being worked on
					float4 vertex : SV_POSITION;
					//float2 uv : TEXCOORD0;
					float4 screenPos : TEXCOORD0;
				};
		
				// Variable defined in Properties
				float4 _InactiveColor;

				sampler2D _MainTex;
				int DisplayMask; // Set to 1 to draw texture, otherwise draw the inactive colour

				v2f VertexFunc(appdata IN)							// Where Vertices get calculated
				{
					v2f OUT;										// Return this variable at the end of the Function

					OUT.vertex = UnityObjectToClipPos(IN.vertex); // Makes shader look correct on perspective camera
					OUT.screenPos = ComputeScreenPos(OUT.vertex);

					return OUT;
				}

				fixed4 FragmentFunc(v2f IN) : SV_Target				// SV_Target is Target screen to render towards
				{
					float2 uv = IN.screenPos.xy / IN.screenPos.w;
					fixed4 portalColor = tex2D(_MainTex, uv); // Set pixel color to the initial Texture first
					return portalColor * DisplayMask + _InactiveColor * (1 - DisplayMask);
				}

				ENDCG
			}
		}
}