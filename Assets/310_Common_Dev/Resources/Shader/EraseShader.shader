Shader "DoDoEng/EraseShader"
{
	Properties
	{
		_MainTex("Brush", 2D) = "white" {}
	}
	SubShader
	{
		Tags {"Queue"="Transparent"  "DisableBatching" = "True" }
		Cull Off 
		Lighting Off
		ZWrite Off 
		ZTest Off

		Pass
		{
			//AlphaToMask Off
			//Blend Zero Zero
			BlendOp RevSub
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert_img
			#pragma fragment frag


			sampler2D _MainTex;

			float4 frag(v2f_img i) : SV_Target
			{
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
