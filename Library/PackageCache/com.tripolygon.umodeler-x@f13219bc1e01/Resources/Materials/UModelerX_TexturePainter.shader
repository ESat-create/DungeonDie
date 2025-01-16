Shader "Hidden/UModelerX_TexturePainter"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"}
        LOD 100

        Pass // 0 페인트 브러시
        {
            Cull off
            ZTest always
            Blend [_SrcBlend] OneMinusSrcAlpha, [_SrcAlphaBlend] OneMinusSrcAlpha
            Colormask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 worldpos : TEXCOORD0;
                float3 worldnormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            float3 _BrushPosition;
            float _BrushRadius;
            float _BrushHardness;
            float4 _BrushColor;
			float _FrontOnly;
            float _BrushShape; // 0 sphere 1 square

            float _UVSpace;
            float2 _TextureSpace;

            float3 _CameraPosition;
            float3 _CameraUp;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.vertex = float4(v.uv*float2(2,-2)+float2(-1,1), 1, 1);
                o.worldnormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                if (_UVSpace == 1)
                    o.worldpos = float3(v.uv * _TextureSpace, 0);
                else
                    o.worldpos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 v = _BrushPosition.xyz - i.worldpos.xyz;
                float3 worldpos = i.worldpos;
                float3 normal = i.worldnormal;
                float3 cameradir = _CameraPosition - worldpos;
                float front = dot(cameradir, normal) >= 0 ? 1 : 0;
                float3 right = normalize(cross(_CameraUp, cameradir));
                float3 up = normalize(cross(cameradir, right));

                if (_UVSpace == 1)
                {
                    right = float3(1, 0, 0);
                    up = float3(0, 1, 0);
                }

                float alpha;

                if (_BrushShape == 1)
                {
                    float3 forward = cross(up, right);
                    float w = max(abs(dot(right, v)), abs(dot(up, v))) / _BrushRadius;
                    alpha = saturate(1 - lerp(w, max(0, (w - 0.8) / 0.2), _BrushHardness)) *saturate(1 - (abs(dot(forward, v)) / _BrushRadius - 0.7) / 0.3);
                }
                else
                {
                    float w = length(v) / _BrushRadius;
                    alpha = saturate(1 - lerp(w, max(0, (w - 0.8) / 0.2), _BrushHardness)) * saturate(1 - (abs(dot(normal, v)) / _BrushRadius - 0.7) / 0.3);
                }
                
                float4 c = float4(_BrushColor.rgb, _BrushColor.a * alpha * lerp(1, front, _FrontOnly));
                return c;
            }
            ENDCG
        }

        Pass // 1 - Decal Paint
        {
            Cull off
            ZTest always
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            Colormask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 worldpos : TEXCOORD0;
                float3 worldnormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            float3 _BrushPosition;
            float3 _BrushTangent;
            float3 _BrushBinormal;
            float3 _BrushNormal;
            float4 _BrushColor;
            float _FrontOnly;
            sampler2D _MaskTex;
            float _MaskTexType;
            float _UVSpace;

            float3 _CameraPosition;

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = float4(v.uv * float2(2,-2) + float2(-1,1), 1, 1);// UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldnormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.worldpos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 worldpos = i.worldpos;
                float3 uvw;
                float front = 1;

                if (_UVSpace == 0)
                {
                    float3 normal = i.worldnormal;// -normalize(cross(ddx(worldpos), ddy(worldpos)));
                    float front = dot((_CameraPosition - worldpos), normal) >= 0 ? 1 : 0;

                    float3 v = worldpos.xyz - _BrushPosition;
                    uvw = float3(dot(v, _BrushTangent), dot(v, _BrushBinormal), dot(v, _BrushNormal));
                }
                else
                {
                    float2 p = i.uv - _BrushPosition.xy;
                    uvw = float3(dot(p, _BrushTangent.xy), dot(p, _BrushBinormal.xy), 0);
                }

                uvw.y *= _MainTex_TexelSize.z / _MainTex_TexelSize.w;

                if (uvw.x >= -1 && uvw.x <= 1.0f && uvw.y >= -1 && uvw.y <= 1.0f)
                {
                    fixed4 col = tex2D(_MainTex, uvw.xy * float2(-0.5, 0.5) + 0.5) * _BrushColor;
                    col.a *= 1 - saturate((abs(uvw.z) - 0.6) / 0.4f);
                    col.a *= lerp(1, front, _FrontOnly);

                    col.a *= _MaskTexType == 1 ? tex2D(_MaskTex, i.uv).r : 1;
                    return col;
                }
                return 0;
            }
            ENDCG
        }

        Pass // 2 그려진 채널 레이어 텍스처 합성
        {
            Cull off
            ZTest always
            Blend One Zero
            Colormask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _Color;
            float4 _TilingOffset;
            float _MaskTexType;

            sampler2D _BlendMainTex2;
            float _BlendMode;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 c;
                float2 uv = i.uv * _TilingOffset.xy + _TilingOffset.zw;
                float m = _MaskTexType == 1 ? tex2D(_MaskTex, i.uv).r : 1;
                float4 t = tex2Dlod(_MainTex, float4(uv, 0, 0));
                float4 alphatex = t.a;

                float alpha = m * _Color.a;

                c.rgb = (t.xyz * _Color.rgb) * alpha; // _MainTex에는 alpha(alphatex) 가 곱해진 상태
                c.a = alphatex * alpha;

                float4 src = tex2Dlod(_BlendMainTex2, float4(i.uv, 0, 0));

                if (_BlendMode != 0)
                {
                    float src_alpha = src.a;
                    if (src_alpha > 0)
                    {
                        float3 src_rgb = src.rgb;// src_alpha 가 곱해진 상태
                        float3 dest_rgb = 0;

                        if (_BlendMode == 10) // Darken = min(Target,Blend)
                            //c.rgb = min(c.rgb / c.a, src_rgb / src_alpha) * c.a;
                            dest_rgb = lerp(c.rgb, min(c.rgb, src_rgb * c.a / src_alpha), src_alpha);
                        else if (_BlendMode == 11) // Multiply
                            dest_rgb = c.rgb * src_rgb / src_alpha;
                        else if (_BlendMode == 12) // Color Burn = 1 - (1-Target) / Blend
                            dest_rgb = c.a - (c.a - c.rgb) * src_alpha / src_rgb;
                        else if (_BlendMode == 13) // Linear Burn = Target + Blend - 1
                            dest_rgb = c.rgb + src_rgb * c.a / src_alpha - c.a;
                        else if (_BlendMode == 20) // Lighten = max(Target,Blend)
                            dest_rgb = max(c.rgb, src_rgb * c.a / src_alpha);
                        else if (_BlendMode == 21) // Screen = 1 - (1-Target) * (1-Blend)
                            dest_rgb = c.a - (c.a - c.rgb) * (1 - src_rgb / src_alpha);
                        else if (_BlendMode == 22) // Color Dodge = Target / (1-Blend)
                            dest_rgb = c.rgb / (1 - src_rgb / src_alpha);
                        else if (_BlendMode == 23) // Linear Dodge = Target + Blend
                            dest_rgb = c.rgb + src_rgb * c.a / src_alpha;
                        else if (_BlendMode == 30) // Overlay = (Target > 0.5 ? (1 - (1-2*(Target-0.5)) * (1-Blend)), (2*Target) * Blend)
                            dest_rgb = lerp(c.a - (c.a - 2 * (c.rgb - c.a)) * (1 - src_rgb / src_alpha), 2 * c.rgb * src_rgb / src_alpha, 1-step(c.rgb, c.a * 0.5));
                        else if (_BlendMode == 31) // Soft Light = (Blend > 0.5, (1 - (1-Target) * (1-(Blend-0.5))) , (Target * (Blend+0.5))
                            dest_rgb = lerp(c.a - (c.a - (c.a - c.rgb)) * (1 - (src_rgb / src_alpha-0.5)), c.rgb * (src_rgb / src_alpha+0.5), 1-step(c.rgb, c.a * 0.5));
                        else if (_BlendMode == 32) // Hard Light = (Blend > 0.5, (1 - (1-Target) * (1-2*(Blend-0.5))) , (Target * (2*Blend))
                            dest_rgb = lerp(c.a - (c.a - (c.a - c.rgb)) * (1 - 2*(src_rgb / src_alpha - 0.5)), c.rgb * 2 * src_rgb / src_alpha, 1 - step(c.rgb, c.a * 0.5));
                        else if (_BlendMode == 33) // Vivid Light = (Blend > 0.5, (Target / (1-2*(Blend-0.5))) , (1 - (1-Target) / (2*Blend)) )
                            dest_rgb = lerp(c.rgb / (1 - 2 * (src_rgb / src_alpha - 0.5)), c.a - (c.a - c.rgb) / (2 * src_rgb / src_alpha), 1 - step(c.rgb, c.a * 0.5));
                        else if (_BlendMode == 34) // Linear Light = (Blend > 0.5, (Target + 2*(Blend-0.5)) , (Target + 2*Blend - 1)
                            dest_rgb = lerp(c.rgb + 2 * (src_rgb / src_alpha - 0.5) * c.a, c.rgb + 2 * src_rgb * c.a / src_alpha - c.a, 1 - step(c.rgb, c.a * 0.5));
                        else if (_BlendMode == 35) // Pin Light = (Blend > 0.5, (max(Target,2*(Blend-0.5))) , (min(Target,2*Blend)))
                            dest_rgb = lerp(max(c.rgb, 2 * (src_rgb / src_alpha - 0.5) * c.a), min(c.rgb, 2 * src_rgb * c.a / src_alpha), 1 - step(c.rgb, c.a * 0.5));
                        else if (_BlendMode == 40) // Difference = | Target - Blend |
                            dest_rgb = abs(c.rgb - src_rgb * c.a / src_alpha);
                        else if (_BlendMode == 41) // Exclusion = 0.5 - 2*(Target-0.5)*(Blend-0.5)
                            dest_rgb = 0.5 * c.a - (2 * c.rgb - c.a) * (src_rgb / src_alpha - 0.5);

                        c.rgb = lerp(c.rgb, dest_rgb, src_alpha);
                    }
                }

                return c + src * (1 - c.a);
            }
            ENDCG
        }

        Pass // 3 값쓰기 (rgb혹은 a 혹은 r) => rgb에
        {
            Cull off
            ZTest always
            Blend One Zero

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _AlphaType;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 c = tex2D(_MainTex, i.uv);
                if (_AlphaType == 2)
                    return float4(c.aaa, 1);;
                if (_AlphaType == 1)
                    return float4(c.rrr, 1);
                if (_AlphaType == 3)
                    return float4(c.rrr * c.a, 1);
                return float4(c.rgb, 1);
            }
            ENDCG
        }

        Pass // 4 텍스터 합성 (마스크 컬러 채널 복사)
        {
            Cull off
            ZTest always
            Blend One Zero
            Colormask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _SrcTex;
            float4 _Mask;
            float _ChannelType;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 c1 = tex2D(_MainTex, i.uv);
                float4 c2 = tex2D(_SrcTex, i.uv);
                if (_ChannelType == 1)
                    c1 = c1.xxxx;
                return lerp(c1, c2, 1-_Mask);
            }
            ENDCG
        }

        Pass // 5 클리어 (미사용)
        {
            Cull off
            ZTest always
            Blend One Zero, One Zero

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
        Pass // 6 thumnbnail 용 배경
        {
            Cull off
            ZTest always
            Blend One Zero
            Colormask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _ChannelType;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 c = tex2D(_MainTex, i.uv);
                float4 a = c.a;

                float check = abs(frac(dot(floor(saturate(i.uv) * 8) * 0.5, 1)) - 0.5) * 0.5 + 0.25;
                return float4(check.rrr, 1);
            }
            ENDCG
        }
        Pass // 7 bit mask (Fillrect용 컬러 칠하기)
        {
            Cull off
            ZTest always
            Blend [_SrcBlend] OneMinusSrcAlpha,[_SrcAlphaBlend] OneMinusSrcAlpha
            Colormask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float alpha : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _BitMaskTex;
            float4 _BitMaskTex_TexelSize;

            float4 _Color;

            sampler2D _BlendMainTex7;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = float4(v.uv.xy*float2(2,-2)+float2(-1,1), 1, 1);

                int3 index = floor(v.uv2.xxx * float3(1/32.0f, 1/8.0f, 1));

                float2 uv = floor(index.xx * float2(1, 1.0f / _BitMaskTex_TexelSize.z)) / _BitMaskTex_TexelSize.zw;
                float4 bitmask = tex2Dlod(_BitMaskTex, float4(frac(uv), 0, 0));

                int index1 = (index.y&3);
                int index2 = (index.z&7);
                int bitmaks8 = (int)((index1 < 2 ? (index1 == 0 ? bitmask.r : bitmask.g) : (index1 == 2 ? bitmask.b : bitmask.a)) * 255);
                o.alpha = (bitmaks8 & (1 << index2)) != 0 ? 1 : 0;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return float4(_Color.rgb, _Color.a * i.alpha);
            }
            ENDCG
        }
		Pass //"Edge" 8. UVEdgeLine
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZTest Always
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

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
				float2 uvcoord : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			float _LineGeomWidth;
			float4 _LineColor;
			float _ScreenRatio;
			float _WireframeThick;
            float4 _ClipRect;

			v2f vert(appdata v)
			{
				v2f o;

				float4 v1 = UnityObjectToClipPos(float4(float2(0,1) + v.vertex1.xy * float2(1,-1), 0, 1));
                float4 v2 = UnityObjectToClipPos(float4(float2(0, 1) + v.vertex2.xy * float2(1, -1), 0, 1));
				float2 v3 = normalize(v2 - v1);
				v3 = float2(-v3.y / _ScreenRatio, v3.x) * _LineGeomWidth;

				uint index = floor(v.vertex2.w);

				float2 barycentric = float2(floor(index/2) - 1, (index&1) == 0 ? 0 : 1);

				o.vertex = float4(lerp(v1.xy, v2.xy, barycentric.y) + v3.xy * barycentric.x, 0, 1);
                o.uvcoord = o.vertex.xy * 0.5 + 0.5;
                //o.vertex = float4(barycentric.x*0.5, barycentric.y, 0.1, 1);
                o.barycentric = abs(barycentric);
				o.color = _LineColor;
				return o;
			}

            fixed4 frag(v2f i) : SV_Target
            {
                if (i.uvcoord.x < _ClipRect.x || i.uvcoord.x > _ClipRect.z || 1-i.uvcoord.y < _ClipRect.y)
                    discard;

                float WireframeThick = _WireframeThick;
				float barycentric = i.barycentric.x;
				float w1 = 1 - smoothstep(0, WireframeThick * (abs(ddx(barycentric)) + abs(ddy(barycentric))), barycentric);
				return float4(i.color.xyz, i.color.w * w1);
			}
			ENDCG
		}
		Pass // 9. duplicate
		{
			Blend One Zero
			Cull Off
			ZTest Always
			ZWrite Off
            Colormask RGBA

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
		}
		Pass // 10. Clear (A포함 단색으로 칠하기)
		{
			Blend One Zero
			Cull Off
			ZTest Always
			ZWrite Off
            Colormask RGBA

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _ClearColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _ClearColor;
            }
            ENDCG
		}
        Pass // 11 단색 컬러로 칠하기 (마스킹)
        {
            Cull off
            ZTest always
            Blend One Zero
            Colormask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                //float alpha : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = float4(v.uv.xy*float2(2,-2)+float2(-1,1), 1, 1);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
               return 1;
            }
            ENDCG
        }
        Pass // 12 확장
        {
            Cull off
            ZTest always
            Blend One Zero
            Colormask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 c = tex2D(_MainTex, i.uv);
                if (c.a < 1)
                {
                    float4 c1 =
                        tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(-1, -1)) +
                        tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(0, -1)) +
                        tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(1, -1)) +

                        tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(-1, 0)) +
                        tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(1, 0)) +

                        tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(-1, 1)) +
                        tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(0, 1)) +
                        tex2D(_MainTex, i.uv + _MainTex_TexelSize.xy * float2(1, 1));

                    if (c1.a > 0.1f)
                        return float4(c1.rgb / c1.a, 1);
                    else
                        return 0;
                }
                return c;
            }
            ENDCG
        }
        Pass // 13 확장
        {
            Cull off
            ZTest always
            Blend One Zero
            Colormask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MaskTex;
            sampler2D _DilationTex;

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 m = tex2D(_MaskTex, i.uv);
                float4 c = tex2D(_MainTex, i.uv);
                if (m.r < 1)
                {
                    float4 d = tex2D(_DilationTex, i.uv);
                    return d;// +c * (1 - d.a);
                }
                return c;
            }
            ENDCG
        }
        Pass // 14 마스킹
        {
            Cull off
            ZTest always
            Blend One Zero
            Colormask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MaskTex;
            sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 m = tex2D(_MaskTex, i.uv);
                if (m.r > 0)
                    return tex2D(_MainTex, i.uv);
                else
                    return 0;
            }
            ENDCG
        }

        Pass // 15 - texture 덮어씌우기
        {
            Cull off
            ZTest always
            Blend [_SrcBlend] OneMinusSrcAlpha,[_SrcAlphaBlend] OneMinusSrcAlpha
            Colormask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = float4(v.uv * float2(2,-2) + float2(-1,1), 1, 1);// UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
               return col;
            }
            ENDCG
        }
        Pass // 16 페인트 브러시 (어레이)
        {
            Cull off
            ZTest always
            Blend[_SrcBlend] OneMinusSrcAlpha,[_SrcAlphaBlend] OneMinusSrcAlpha
            Colormask RGBA

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float3 worldpos : TEXCOORD0;
                float3 worldnormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            float3 _BrushPositionArray[64];
            float _BrushRadiusArray[64];
            int _BrushCount;

            float _BrushHardness;
            float4 _BrushColor;
            float _FrontOnly;
            float _BrushShape; // 0 sphere 1 square

            float _UVSpace;
            float2 _TextureSpace;

            float3 _CameraPosition;
            float3 _CameraUp;

            v2f vert(appdata v)
            {
                v2f o;
                o.uv = v.uv;
                o.vertex = float4(v.uv * float2(2,-2) + float2(-1,1), 1, 1);
                o.worldnormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                if (_UVSpace == 1)
                    o.worldpos = float3(v.uv * _TextureSpace, 0);
                else
                    o.worldpos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 worldpos = i.worldpos;
                float3 normal = i.worldnormal;
                float3 cameradir = _CameraPosition - worldpos;
                float front = dot(cameradir, normal) >= 0 ? 1 : 0;
                float3 right = normalize(cross(_CameraUp, cameradir));
                float3 up = normalize(cross(cameradir, right));

                if (_UVSpace == 1)
                {
                    right = float3(1, 0, 0);
                    up = float3(0, 1, 0);
                }

                float alpha = 0;

                for (int j = 0; j < _BrushCount; j++)
                {
                    float3 v = _BrushPositionArray[j].xyz - i.worldpos.xyz;
                    float brushradius = _BrushRadiusArray[j];

                    if (_BrushShape == 1)
                    {
                        float3 forward = cross(up, right);
                        float w = max(abs(dot(right, v)), abs(dot(up, v))) / brushradius;
                        alpha += (1-alpha) * saturate(1 - lerp(w, max(0, (w - 0.8) / 0.2), _BrushHardness)) * saturate(1 - (abs(dot(forward, v)) / brushradius - 0.7) / 0.3);
                    }
                    else
                    {
                        float w = length(v) / brushradius;
                        alpha += (1 - alpha) * saturate(1 - lerp(w, max(0, (w - 0.8) / 0.2), _BrushHardness)) * saturate(1 - (abs(dot(normal, v)) / brushradius - 0.7) / 0.3);
                    }
                }

                return float4(_BrushColor.rgb, _BrushColor.a * alpha * lerp(1, front, _FrontOnly));
            }
            ENDCG
        }
    }
}
