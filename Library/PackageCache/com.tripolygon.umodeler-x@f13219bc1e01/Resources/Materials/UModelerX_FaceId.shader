Shader "UModelerX_FaceId"
{
    SubShader
    {
        Tags { 
            "RenderType" = "Opaque"  
            "LightMode" = "Always" 
            "UModelerXPicker" = "FaceID" 
        }
        LOD 100

        Pass //"FaceId"
        {
            ZTest LEqual
            Lighting Off
            Blend Off
			Cull Back

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
                float4 color : COLOR;
			};

			struct v2f
			{
				float4 color : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
    }
}
