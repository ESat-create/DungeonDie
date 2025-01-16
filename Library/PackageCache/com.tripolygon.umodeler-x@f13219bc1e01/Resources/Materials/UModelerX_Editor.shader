Shader "UModelerX_Editor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_WireframeThick("WireframeThick", float) = 1.5
		_WireframeColor("WireframeColor", Color) = (0,0,0,1)
		_SelectedWireframeThick("SelectedWireframeThick", float) = 2
		_SelectWireframeColor("SelectWireframeColor", Color) = (1,1,0,1)
		_HighlightWireframeColor("HighlightWireframeColor", Color) = (1,1,0,1)
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _CullMode("CullMode", Float) = 0
	}
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass //"Wireframe" 0
        {
			Blend One OneMinusSrcAlpha
			ZWrite Off
			ZTest [_ZTest]
			Cull [_CullMode]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ _SELECTED _HIGHLIGHT _SELECTEDANDUVSEAM
			#pragma multi_compile __ _USE_MASKTEXTURE
			#pragma multi_compile __ _EDGEDISABLE _FACEDISABLE
			#pragma multi_compile __ _SOFTSELECTION
            #pragma multi_compile __ _USE_TEMPERATURE
            #define WIREFRAME_1

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float4 barycentric1 : TEXCOORD0;
				float4 barycentric2 : TEXCOORD1;
				float4 barycentric3 : TEXCOORD2;
                float3 barycentric4 : TEXCOORD5;
				float4 position : TEXCOORD3;
				float4 facecolor : TEXCOORD6;
				float4 vertex : SV_POSITION;
#ifdef _SOFTSELECTION
				float selection : TEXCOORD4;
#endif
			};

			float4 _WireframeColor;
			float4 _SelectWireframeColor;
			float4 _HighlightWireframeColor;

			float _LineAlpha;

			float _WireframeThick;
			float _SelectedWireframeThick = 1;
			float4 _SeamLineColor;

			sampler2D _SoftSelectTexture;
			int _SoftSelectTextureSize;

			sampler2D _Texture;
			int _TextureSize;

			v2f vert (appdata v)
			{
				v2f o;
				float zbias = 0.00001f;
				o.vertex = UnityObjectToClipPos(v.vertex) + float4(0, 0, zbias, 0);

				int edge = floor(v.uv2.y);

				o.barycentric1 = 1;
				o.barycentric2 = 1;
				o.barycentric3 = 1;
                o.barycentric4 = 1;

#ifndef _HIGHLIGHT
				o.barycentric1 = float4((edge & 1) == 0 ? 1 : 0, (edge & 2) == 0 ? 1 : 0, (edge & 4) == 0 ? 1 : 0, (edge & 8) == 0 ? 1 : 0);
                o.barycentric4.x = (edge & 16) == 0 ? 1 : 0;
#endif
#if defined(_SELECTED) || defined(_SELECTEDANDUVSEAM)
				o.barycentric2 = float4((edge & (32|1024)) == 32 ? 0 : 1, (edge & (64|2048)) == 64 ? 0 : 1, (edge & (128|4096)) == 128 ? 0 : 1, (edge & (256|8192)) == 256 ? 0 : 1);
                o.barycentric4.y = (edge & (512|16384)) == 512 ? 0 : 1;
#endif
#ifdef _SELECTEDANDUVSEAM
				o.barycentric3 = float4((edge & (32|1024)) == (32|1024) ? 0 : 1, (edge & (64|2048)) == (64|2048) ? 0 : 1, (edge & (128|4096)) == (128|4096) ? 0 : 1, (edge & (256|8192)) == (256|8192) ? 0 : 1);
                o.barycentric4.z = (edge & (512|16384)) == (512|16384) ? 0 : 1;
#endif
#ifdef _HIGHLIGHT
				o.barycentric3 = float4((edge & (32|1024)) == 1024 ? 0 : 1, (edge & (64|2048)) == 2048 ? 0 : 1, (edge & (128|4096)) == 4096 ? 0 : 1, (edge & (256|8192)) == 8192 ? 0 : 1);
                o.barycentric4.z = (edge & (512|16384)) == 16384 ? 0 : 1;
#endif
				o.facecolor = 0;
#if defined(_SELECTED) || defined(_SELECTEDANDUVSEAM)
				if ((edge & 32768) != 0)
					o.facecolor = _SelectWireframeColor * float4(1, 1, 1, 0.5);
#endif
#ifdef _HIGHLIGHT
				if ((edge & 65536) != 0)
					o.facecolor = _HighlightWireframeColor * float4(1, 1, 1, 0.5);
#endif
#ifdef _EDGEDISABLE
				o.barycentric1 = 1;
                o.barycentric4.x = 1;
#endif
#ifdef _FACEDISABLE
				o.facecolor = 0;
#endif
#ifdef _USE_MASKTEXTURE
				uint uid = floor(v.uv2.x);
				uint off = uid / 32;
				uint ch = (uid / 8) & 3;
				uint bit = 1 << (uid & 7);
				float2 uv2 = (float2(fmod(off, _TextureSize), floor(off / _TextureSize)) + 0.5) / _TextureSize;
				fixed4 tex = tex2Dlod(_Texture, float4(uv2, 0, 0));
				float c = ch < 2 ? (ch < 1 ? tex.r : tex.g) : (ch < 3 ? tex.b : tex.a);
				int mask = floor(c * 255.0f);
				if ((mask&bit) != 0)
					o.vertex = 0;
#endif
#ifdef _SOFTSELECTION
				uint uid2 = floor(v.uv2.z);
				uint off2 = uid2 / 4;
				uint ch2 = uid2 & 3;
				float2 uv3 = (float2(fmod(off2, _SoftSelectTextureSize), floor(off2 / _SoftSelectTextureSize)) + 0.5) / _SoftSelectTextureSize;
				fixed4 tex2 = tex2Dlod(_SoftSelectTexture, float4(uv3, 0, 0));
				o.selection = ch2 < 2 ? (ch2 < 1 ? tex2.r : tex2.g) : (ch2 < 3 ? tex2.b : tex2.a);
#endif
				o.position = o.vertex;
				return o;
			}
			
			float3 Temperature(float w)
			{
				// 0 1,0,0 -> 0.5,0.5,0 -> 0, 1, 0 -> 0, 0.5, 0.5 -> 0,0,1
				float t = (1 - w) * 2;
				return saturate(float3(1.5f - t, 1.5f - abs(t - 1), 1.5f - abs(t - 2)));
			}

            fixed4 frag(v2f i) : SV_Target
            {
                float WireframeThick = _WireframeThick;
                float SelectedWireframeThick = _SelectedWireframeThick;
#ifdef WIREFRAME_1
                float4 barycentric1 = i.barycentric1;
                float4 barycentric2 = i.barycentric2;
                float4 barycentric3 = i.barycentric3;
                float3 barycentric4 = i.barycentric4;

                float3 minBary = float3(min(min(min(barycentric1.x, barycentric1.y), min(barycentric1.z, barycentric1.w)), barycentric4.x),
                    min(min(min(barycentric2.x, barycentric2.y), min(barycentric2.z, barycentric2.w)), barycentric4.y),
                    min(min(min(barycentric3.x, barycentric3.y), min(barycentric3.z, barycentric3.w)), barycentric4.z));

                float3 ddx2 = ddx(minBary);
                float3 ddy2 = ddy(minBary);
                float3 delta = abs(ddx2) + abs(ddy2);
                //float3 delta = sqrt(ddx2* ddx2 + ddy2* ddy2);

                float w1 = 1 - smoothstep(0, WireframeThick * delta.x, minBary.x);
                float w2 = 1 - smoothstep(0, SelectedWireframeThick * delta.y, minBary.y);
                float w3 = 1 - smoothstep(0, SelectedWireframeThick * delta.z, minBary.z);
#else
				float4 barycentric1 = smoothstep(0, WireframeThick * (abs(ddx(i.barycentric1)) + abs(ddy(i.barycentric1))), i.barycentric1); // wireframe
				float4 barycentric2 = smoothstep(0, SelectedWireframeThick * (abs(ddx(i.barycentric2)) + abs(ddy(i.barycentric2))), i.barycentric2); // wireframe_selected
				float4 barycentric3 = smoothstep(0, SelectedWireframeThick * (abs(ddx(i.barycentric3)) + abs(ddy(i.barycentric3))), i.barycentric3); // wireframe_hightlight
                float3 barycentric4 = smoothstep(0, float3(WireframeThick, SelectedWireframeThick, SelectedWireframeThick)* (abs(ddx(i.barycentric4)) + abs(ddy(i.barycentric4))), i.barycentric4);

				float w1 = 1 - min(min(min(barycentric1.x, barycentric1.y), min(barycentric1.z, barycentric1.w)), barycentric4.x);
                float w2 = 1 - min(min(min(barycentric2.x, barycentric2.y), min(barycentric2.z, barycentric2.w)), barycentric4.y);
                float w3 = 1 - min(min(min(barycentric3.x, barycentric3.y), min(barycentric3.z, barycentric3.w)), barycentric4.z);
#endif

				float4 SelectWireframeColor = _SelectWireframeColor;
				float4 WireframeColor = _WireframeColor;
#ifdef _SOFTSELECTION
#ifdef _USE_TEMPERATURE
                WireframeColor.xyz = i.selection > 0.99 ? SelectWireframeColor : Temperature(i.selection);
#else
                WireframeColor.xyz = lerp(_WireframeColor, SelectWireframeColor, i.selection);
#endif
#endif
				float4 c1 = lerp(WireframeColor* _LineAlpha, SelectWireframeColor, step(w1, w2));
				float w12 = max(w1, w2);

#ifdef _HIGHLIGHT
				float4 c = lerp(c1, _HighlightWireframeColor, step(w12, w3));
#else
				float4 c = lerp(c1, _SeamLineColor* _LineAlpha, step(w12, w3));
#endif
				c.w *= max(w12, w3);

				float4 facecolor = i.facecolor;
#ifdef _SOFTSELECTION
#ifndef _FACEDISABLE
#ifdef _USE_TEMPERATURE
                facecolor = float4(Temperature(i.selection), 0.125);
#else
				facecolor = lerp(_WireframeColor, SelectWireframeColor, i.selection);
#endif
#endif
#endif
				return float4(c.xyz * c.w + facecolor.xyz * facecolor.w, c.w + facecolor.w * (1 - c.w));
			}
			ENDCG
        }

    /// 되도록 삭제 아마도 안씀
        Pass //"FaceId" 1 
        {
			Blend One Zero

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv2 : TEXCOORD1;
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
                o.color = float4(0, 0, 0, 0);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}

		Pass //"VertexRect" 2
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZTest [_ZTest]
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma multi_compile __ _USE_MASKTEXTURE _USE_HEATTEXTURE
            #pragma multi_compile __ _USE_TEMPERATURE

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				//float2 uv2 : TEXCOORD0;
				float type : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			float _BoxSize;
			float4 _Color;
			float4 _SelectedColor;

            float _Ortho;

			sampler2D _Texture;
			int _TextureSize;

			float _Alpha;

			sampler2D _SoftSelectTexture;
			int _SoftSelectTextureSize;

			v2f vert(appdata v)
			{
				v2f o;
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float4 viewPos = mul(UNITY_MATRIX_V, worldPos);
				float zbias = lerp(0.002f, 0.0001f, _Ortho);

                viewPos.xy += v.uv * (_BoxSize * lerp(viewPos.z, 1, _Ortho));
                o.vertex = mul(UNITY_MATRIX_P, viewPos) + float4(0, 0, zbias, 0);

				float type = 1;
#if defined(_USE_MASKTEXTURE) || defined(_USE_HEATTEXTURE)
				uint uid = floor(v.uv2.x);
				uint off = uid / 32;
				uint ch = (uid / 8) & 3;
				uint bit = 1 << (uid & 7);
				float2 uv2 = (float2(fmod(off, _TextureSize), floor(off / _TextureSize)) + 0.5) / _TextureSize;
				fixed4 tex = tex2Dlod(_Texture, float4(uv2, 0, 0));
				float c = ch < 2 ? (ch < 1 ? tex.r : tex.g) : (ch < 3 ? tex.b : tex.a);
				int mask = floor(c * 255.0f);
				if ((mask & bit) == 0)
				{
#if defined(_USE_HEATTEXTURE)
					uid -= 1;
					off = uid / 4;
					ch = uid & 3;
					uv2 = (float2(fmod(off, _SoftSelectTextureSize), floor(off / _SoftSelectTextureSize)) + 0.5) / _SoftSelectTextureSize;
					tex = tex2Dlod(_SoftSelectTexture, float4(uv2, 0, 0));
					c = ch < 2 ? (ch < 1 ? tex.r : tex.g) : (ch < 3 ? tex.b : tex.a);
					type = c;
#else
					type = 0;
#endif
				}
#endif
				o.type = type;
				return o;
			}

			float3 Temperature(float w)
			{
				// 0 1,0,0 -> 0.5,0.5,0 -> 0, 1, 0 -> 0, 0.5, 0.5 -> 0,0,1
				float t = (1 - w) * 2;
				return saturate(float3(1.5f - t, 1.5f - abs(t - 1), 1.5f - abs(t - 2)));
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 c;
#if defined(_USE_HEATTEXTURE) && defined(_USE_TEMPERATURE)
				c = i.type > 0 ? (i.type > 0.99 ? _SelectedColor : float4(Temperature(i.type), _SelectedColor.a)) : _Color;
#else
				c = lerp(_Color, _SelectedColor, i.type);
#endif
				return float4(c.rgb, c.a*_Alpha);
			}
			ENDCG
		}

		Pass //"VertexRect" Inner 3
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZTest Greater
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ _USE_MASKTEXTURE _USE_HEATTEXTURE

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				//float2 uv2 : TEXCOORD0;
				float weight : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			float _BoxSize;
			float4 _Color;
			float4 _SelectedColor;

            float _Ortho;

			sampler2D _Texture;
			int _TextureSize;

			sampler2D _SoftSelectTexture;
			int _SoftSelectTextureSize;

			v2f vert(appdata v)
			{
				v2f o;
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float4 viewPos = mul(UNITY_MATRIX_V, worldPos);
                float zbias = lerp(0.002f, 0.0001f, _Ortho);

				viewPos.xy += v.uv * (_BoxSize * lerp(viewPos.z, 1, _Ortho));
				o.vertex = mul(UNITY_MATRIX_P, viewPos) + float4(0,0, zbias, 0);

				float weight = 1;
#if defined(_USE_MASKTEXTURE) || defined(_USE_HEATTEXTURE)
				int uid = floor(v.uv2.x);
				int off = uid / 32;
				int ch = (uid / 8) & 3;
				int bit = 1 << (uid & 7);
				float2 uv2 = (float2(fmod(off, _TextureSize), floor(off / _TextureSize)) + 0.5) / _TextureSize;
				fixed4 tex = tex2Dlod(_Texture, float4(uv2, 0, 0));
				float c = ch < 2 ? (ch < 1 ? tex.r : tex.g) : (ch < 3 ? tex.b : tex.a);
				int mask = floor(c * 255.0f);
				if ((mask & bit) == 0)
				{
#if defined(_USE_HEATTEXTURE)
					off = uid / 4;
					ch = uid & 3;
					uv2 = (float2(fmod(off, _SoftSelectTextureSize), floor(off / _SoftSelectTextureSize)) + 0.5) / _SoftSelectTextureSize;
					tex = tex2Dlod(_SoftSelectTexture, float4(uv2, 0, 0));
					c = ch < 2 ? (ch < 1 ? tex.r : tex.g) : (ch < 3 ? tex.b : tex.a);
					weight = c;
#else
					weight = 0;
#endif
				}
#endif
				o.weight = weight;
				return o;
			}

			float3 Temperature(float w)
			{
				// 0 1,0,0 -> 0.5,0.5,0 -> 0, 1, 0 -> 0, 0.5, 0.5 -> 0,0,1
				float t = (1 - w) * 2;
				return saturate(float3(1.5f - t, 1.5f - abs(t - 1), 1.5f - abs(t - 2)));
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 color;
#if defined(_USE_HEATTEXTURE)
				color = i.weight > 0 ? (i.weight > 0.99 ? _SelectedColor : float4(Temperature(i.weight), _SelectedColor.a)) : _Color;
#else
				color = lerp(_Color, _SelectedColor, i.weight);
#endif
				return color * float4(1, 1, 1, 0.25);
			}
			ENDCG
		}

		Pass //"Edge" 4
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			//ZTest Always
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ _USE_MASKTEXTURE

			#include "UnityCG.cginc"
			#define CLIPCAMERAPLANE

			struct appdata
			{
				float4 vertex1 : POSITION;
				float2 uv : TEXCOORD0;
				float4 vertex2 : TEXCOORD1; // position, edgeid
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 color : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			float _BoxSize;
			float4 _Color;
			float4 _SelectedColor;

			float4 _WireframeColor;
			float4 _SelectWireframeColor;
			float4 _HighlightWireframeColor;

			float _HightlightId;

			float _WireframeThick;

			sampler2D _Texture;
			int _TextureSize;

			v2f vert(appdata v)
			{
				v2f o;
				float4 worldPos1 = mul(unity_ObjectToWorld, v.vertex1);
				float4 worldPos2 = mul(unity_ObjectToWorld, float4(v.vertex2.xyz, 1));
				int uid = floor(v.vertex2.w);

#ifdef CLIPCAMERAPLANE
				float3 cameraForward = mul((float3x3)unity_CameraToWorld, float3(0, 0, 1));
				float3 cameraPosition = mul(unity_CameraToWorld, float4(0, 0, 0, 1));
				float4 plane = float4(cameraForward, -dot(cameraForward, cameraPosition));

				float nearplane = 0.1f;
				float d1 = dot(float4(worldPos1.xyz, 1), plane) - nearplane;
				float d2 = dot(float4(worldPos2.xyz, 1), plane) - nearplane;
				
				if (d1 * d2 < 0)
				{
					if (d1 > 0)
						worldPos2 = worldPos1 + (worldPos2 - worldPos1) * (d1 / (d1 - d2));
					else
						worldPos1 = worldPos2 + (worldPos1 - worldPos2) * (d2 / (d2 - d1));
				}
#endif
				float4 viewPos1 = mul(UNITY_MATRIX_V, worldPos1);
				float4 viewPos2 = mul(UNITY_MATRIX_V, worldPos2);
				float zbias = 0.00001f;
				float2 uv = v.uv;

				float2 dir = normalize((viewPos1.xy / viewPos1.z - viewPos2.xy / viewPos2.z));

				float4 viewPos = lerp(viewPos1, viewPos2, uv.y);
				viewPos.xy += float2(-dir.y, dir.x) * (uv.x * _BoxSize * viewPos.z * 1);

				o.vertex = mul(UNITY_MATRIX_P, viewPos) + float4(0,0, zbias, 0);
				o.uv = float2(abs(uv.x) * 2, 1);

				o.color = _WireframeColor;

#ifdef _USE_MASKTEXTURE
				int off = (uid+1) / 32;
				int ch = ((uid+1) / 8) & 3;
				int bit = 1 << ((uid+1) & 7);
				float2 uv2 = (float2(fmod(off, _TextureSize), floor(off / _TextureSize)) + 0.5) / _TextureSize;
				fixed4 tex = tex2Dlod(_Texture, float4(uv2, 0, 0));
				float c = ch < 2 ? (ch < 1 ? tex.r : tex.g) : (ch < 3 ? tex.b : tex.a);
				int mask = floor(c * 255.0f);
				if ((mask&bit) != 0)
					o.color = _SelectWireframeColor;
#endif
				if (uid == floor(_HightlightId))
					o.color = _HighlightWireframeColor;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return lerp(_Color, _SelectedColor, i.type) * float4(1, 1, 1, 1-i.uv.x);
				float width = _WireframeThick;
				float2 ddxy = smoothstep(0, width * (abs(ddx(i.uv)) + abs(ddy(i.uv))), abs(i.uv));
				float w = 1 - min(ddxy.x, ddxy.y);
				return i.color * float4(1, 1, 1, w);
			}
			ENDCG
		}
		Pass //"Edge" 5. UV Render
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZTest Always
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define _USE_MASKTEXTURE

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex1 : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1; // position, edgeid
			};

			struct v2f
			{
				float4 facecolor : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			float4 _FaceColor;

			sampler2D _Texture;
			int _TextureSize;

			uint _HighlightedIndex;
			float4 _HighlitedColor;

			v2f vert(appdata v)
			{
				v2f o;

				float4 worldPos = float4(v.uv, 1, 1);
				worldPos = mul(UNITY_MATRIX_VP, worldPos);
				o.vertex = float4(worldPos.xy, 0.5, 1);

				o.facecolor = _FaceColor;

#ifdef _USE_MASKTEXTURE
				uint uid = floor(v.uv2.x);
				uint off = uid / 32;
				uint ch = (uid / 8) & 3;
				uint bit = 1 << (uid & 7);
				float2 uv2 = (float2(fmod(off, _TextureSize), floor(off / _TextureSize)) + 0.5) / _TextureSize;
				fixed4 tex = tex2Dlod(_Texture, float4(uv2, 0, 0));
				float c = ch < 2 ? (ch < 1 ? tex.r : tex.g) : (ch < 3 ? tex.b : tex.a);
				int mask = floor(c * 255.0f);
				if ((mask & bit) == 0)
					o.vertex = 0;
#endif
				if (uid > 0 && uid - 1 == _HighlightedIndex)
					o.facecolor = _HighlitedColor;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.facecolor;
			}
			ENDCG
		}
		Pass //"Edge" 6. UV Faceid Render
		{
			Blend One Zero
			Cull Off
			ZTest Always
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile __ _USE_MASKTEXTURE
			#define _USE_MASKTEXTURE

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex1 : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1; // position, edgeid
			};

			struct v2f
			{
				float4 color : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _Texture;
			int _TextureSize;

			float _SelectedWireframeThick = 1;

			v2f vert(appdata v)
			{
				v2f o;

				float4 worldPos = float4(v.uv, 1, 1);
				worldPos = mul(UNITY_MATRIX_VP, worldPos);
				o.vertex = float4(worldPos.xy, 0.5, 1);

				uint uid = floor(v.uv2.x);
# ifdef _USE_MASKTEXTURE
                uint off = uid / 32;
                uint ch = (uid / 8) & 3;
                uint bit = 1 << (uid & 7);
                float2 uv2 = (float2(fmod(off, _TextureSize), floor(off / _TextureSize)) + 0.5) / _TextureSize;
                fixed4 tex = tex2Dlod(_Texture, float4(uv2, 0, 0));
                float c = ch < 2 ? (ch < 1 ? tex.r : tex.g) : (ch < 3 ? tex.b : tex.a);
                int mask = floor(c * 255.0f);
                if ((mask & bit) == 0)
                    o.vertex = 0;

                int v0 = uid % 256;
                int v1 = (uid / 256) % 256;
                int v2 = (uid / (256 * 256)) % 256;
                o.color = float4(v0 / 255.0f, v1 / 255.0f, v2 / 255.0f, 1);
#endif
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
		Pass //"Edge" 7. UVEdgeLine
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZTest Always
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile __ _USE_MASKTEXTURE
			#pragma multi_compile __ _USE_SELECT_TEXTURE
			//#define _USE_MASKTEXTURE

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex1 : POSITION;	// position, edgeid1, idx
				float4 vertex2 : TEXCOORD0; // nextposition, edgeid2, idx, 
			};

			struct v2f
			{
				float4 color : TEXCOORD0;
				float2 barycentric : TEXCOORD1;
				float thick : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _Texture;
			sampler2D _SelectedTexture;
			sampler2D _SharedTexture;
			sampler2D _SeamTexture;
			int _TextureSize;

			float _LineGeomWidth;
			float4 _LineColor;
			float4 _SelectedLineColor;
			float4 _SharedLineColor;
			float4 _SeamLineColor;
			float _ScreenRatio;

			float _WireframeThick;
			float _SelectedWireframeThick = 1;

			uint _HighlightedIndex;
			float4 _HighlitedColor;

			v2f vert(appdata v)
			{
				v2f o;

				float4 v1 = mul(UNITY_MATRIX_VP, float4(v.vertex1.xy, 1, 1));
				float4 v2 = mul(UNITY_MATRIX_VP, float4(v.vertex2.xy, 1, 1));
				float2 v3 = normalize(v2 - v1);
				v3 = float2(-v3.y / _ScreenRatio, v3.x) * _LineGeomWidth;

				uint index = floor(v.vertex2.w);

				float2 barycentric = float2(floor(index/2) - 1, (index&1) == 0 ? 0 : 1);

				o.vertex = float4(lerp(v1.xy, v2.xy, barycentric.y) + v3.xy * barycentric.x, 0.5, 1);
				o.barycentric = abs(barycentric);
				o.color = _LineColor;
				o.thick = _WireframeThick;

				uint uid1 = floor(v.vertex1.z); // edgeid
				uint off = uid1 / 32;

				uint ch1 = (uid1 / 8) & 3; // ch1, edge1, bit1
				float2 edge1 = (float2(fmod(off, _TextureSize), floor(off / _TextureSize)) + 0.5) / _TextureSize;
				uint bit1 = 1 << (uid1 & 7);

				uint uid2 = floor(v.vertex2.z); // edgeid
				off = uid2 / 32;

				uint ch2 = (uid2 / 8) & 3; // ch2, edge2, bits2
				float2 edge2 = (float2(fmod(off, _TextureSize), floor(off / _TextureSize)) + 0.5) / _TextureSize;
				uint bit2 = 1 << (uid2 & 7);

				fixed4 tex;
				float c;
				int mask;

				tex = tex2Dlod(_SeamTexture, float4(edge1, 0, 0)); // edge1, ch1, bit1
				c = ch1 < 2 ? (ch1 < 1 ? tex.r : tex.g) : (ch1 < 3 ? tex.b : tex.a);
				mask = floor(c * 255.0f);
				if ((mask & bit1) != 0)
				{
					o.color = _SeamLineColor;
					o.thick = _SelectedWireframeThick;
				}

				tex = tex2Dlod(_SeamTexture, float4(edge2, 0, 0)); // edge2, ch2, bit2
				c = ch2 < 2 ? (ch2 < 1 ? tex.r : tex.g) : (ch2 < 3 ? tex.b : tex.a);
				mask = floor(c * 255.0f);
				if ((mask & bit2) != 0)
				{
					o.color = _SeamLineColor;
					o.thick = _SelectedWireframeThick;
				}

#ifdef _USE_SELECT_TEXTURE
				tex = tex2Dlod(_SharedTexture, float4(edge1, 0, 0));
				c = ch1 < 2 ? (ch1 < 1 ? tex.r : tex.g) : (ch1 < 3 ? tex.b : tex.a);
				mask = floor(c * 255.0f);
				if ((mask & bit1) != 0)
				{
					o.color = _SharedLineColor;
					o.thick = _SelectedWireframeThick;
				}
				tex = tex2Dlod(_SharedTexture, float4(edge2, 0, 0));
				c = ch2 < 2 ? (ch2 < 1 ? tex.r : tex.g) : (ch2 < 3 ? tex.b : tex.a);
				mask = floor(c * 255.0f);
				if ((mask & bit2) != 0)
				{
					o.color = _SharedLineColor;
					o.thick = _SelectedWireframeThick;
				}

				tex = tex2Dlod(_SelectedTexture, float4(edge1, 0, 0));
				c = ch1 < 2 ? (ch1 < 1 ? tex.r : tex.g) : (ch1 < 3 ? tex.b : tex.a);
				mask = floor(c * 255.0f);
				if ((mask & bit1) != 0)
				{
					o.color = _SelectedLineColor;
					o.thick = _SelectedWireframeThick;
				}

				tex = tex2Dlod(_SelectedTexture, float4(edge2, 0, 0));
				c = ch2 < 2 ? (ch2 < 1 ? tex.r : tex.g) : (ch2 < 3 ? tex.b : tex.a);
				mask = floor(c * 255.0f);
				if ((mask & bit2) != 0)
				{
					o.color = _SelectedLineColor;
					o.thick = _SelectedWireframeThick;
				}
#endif
				if ((uid1 > 0 && uid1 - 1 == _HighlightedIndex) ||
					(uid2 > 0 && uid2 - 1 == _HighlightedIndex))
				{
					o.color = _HighlitedColor;
					o.thick = _SelectedWireframeThick;
				}
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float WireframeThick = i.thick;
				float barycentric = i.barycentric.x;
				float w1 = 1 - smoothstep(0, WireframeThick * (abs(ddx(barycentric)) + abs(ddy(barycentric))), barycentric);
				return float4(i.color.xyz, i.color.w * w1);
			}
			ENDCG
		}
		Pass //"Edge" 8. UVVertex
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZTest Always
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile __ _USE_MASKTEXTURE
			#pragma multi_compile __ _USE_SELECT_TEXTURE
			//#define _USE_MASKTEXTURE

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex1 : POSITION;	// position, edgeid1, idx
				float4 uv : TEXCOORD0; // uvid, idx
			};

			struct v2f
			{
				float4 color : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _Texture;
			sampler2D _SelectedTexture;
			sampler2D _SharedTexture;
			int _TextureSize;

			float _BoxSize;
			float4 _Color;
			float4 _SelectedColor;
			float4 _SharedColor;

			uint _HighlightedIndex;
			float4 _HighlitedColor;

			v2f vert(appdata v)
			{
				v2f o;

				uint index = floor(v.uv.y);

				float2 offset = float2((index / 2) == 0 ? -1 : 1, (index & 1) == 0 ? -1 : 1) * _BoxSize;
				float4 v1 = mul(UNITY_MATRIX_VP, float4(v.vertex1.xy + offset, 1, 1));

				o.vertex = float4(v1.xy, 0.5, 1);
				o.color = _Color;

				uint uid = floor(v.uv.x); // edgeid
				uint off = uid / 32;

				uint ch1 = (uid / 8) & 3;
				float2 edge1 = (float2(fmod(off, _TextureSize), floor(off / _TextureSize)) + 0.5) / _TextureSize;
				uint bit1 = 1 << (uid & 7);

				fixed4 tex;
				float c;
				int mask;
#ifdef _USE_SELECT_TEXTURE
				tex = tex2Dlod(_SharedTexture, float4(edge1, 0, 0));
				c = ch1 < 2 ? (ch1 < 1 ? tex.r : tex.g) : (ch1 < 3 ? tex.b : tex.a);
				mask = floor(c * 255.0f);
				if ((mask & bit1) != 0)
				{
					o.color = _SharedColor;
				}

				tex = tex2Dlod(_SelectedTexture, float4(edge1, 0, 0));
				c = ch1 < 2 ? (ch1 < 1 ? tex.r : tex.g) : (ch1 < 3 ? tex.b : tex.a);
				mask = floor(c * 255.0f);
				if ((mask & bit1) != 0)
				{
					o.color = _SelectedColor;
				}
#endif
				if (uid-1 == _HighlightedIndex)
					o.color = _HighlitedColor;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}
