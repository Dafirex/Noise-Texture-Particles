/*
	Copyright (C) 2019 Dafirex

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

Shader "Dafirex/Particles/NoiseParticles"
{
	Properties
	{
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		_SecondTex("2nd Texture", 2D) = "white" {}
		_TexMod ("Texture Modifier", Vector) = (1, 1, 0, 0)
		_TexMod2 ("Texture Modifier 2", Vector) = (1, 1, 0, 0)

		_TexMul ("Texture Strength", Float) = 1
		_TexPow ("Texture Power", Float) = 1


		_MaskTexture("Mask", 2D) = "white" {}
		_MaskTexture2("Mask2 ", 2D) = "white" {}
		_MaskMod ("Mask Mod", Vector) = (1, 1, 0, 0)
		_MaskMod2 ("Mask Mod", Vector) = (1, 1, 0, 0)

		_MaskThreshold("Mask Threshold", Float) = 1
		_MaskMultiplier("Mask Multiplier", Float) = 1


		//Used to store values for combined texture shader
		_NoiseTexOffsetScale("Noise Tex Offset/Texture", Vector) = (1, 1, 0, 0)
		_SecondTexOffsetScale("Second Tex Offset/Texture", Vector) = (1, 1, 0, 0)
		_MaskTextureOffsetScale("Mask Tex Offset/Texture", Vector) = (1, 1, 0, 0)
		_MaskTexture2OffsetScale("Mask Tex2 Offset/Texture", Vector) = (1, 1, 0, 0)

		_EdgeFallOff("Edge Softness", Range(0, 1)) = 0
		_EdgeIntensity("Edge Intensity", Float) = 1
		_EdgePow("Edge Pow", Float) = 1

		[HDR]_Color1 ("Color 1", Color) = (1, 1, 1, 1)
		[HDR]_Color2 ("Color 2", Color) = (0, 0, 0, 0)

		_PartColMul1 ("Particle Color Mul", Float) = 1
		_PartColMul2 ("Particle Color Mul", Float) = 1

		_RampOffset ("Ramp Offset", Range(-1, 1)) = 0

		_FinalAlphaMul ("Final Alpha Multiplier", Float) = 1 

		//Inspector Options
		[Toggle] _ColorMode("Use Particle Colors", Int) = 0
		[Toggle] _UseCustomData("CustomData1 is speed", Int) = 0
		[Toggle] _InvertCol("Invert Col", Int) = 0
		[Toggle] _NoiseRampAsAlpha("Noise Ramp As Alpha", Int) = 0
		_RGBAChecks("RGBA Checkmarks", Vector) = (1, 1, 1, 1)
		//[Toggle] _UsingMaskTex("Use Mask Tex", Int) = 0


		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 1         
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DstBlend", Float) = 1   

		_BlendMode("Blend Mode", Int) = 0
	}
	SubShader
	{
		Tags { 
			"RenderType"="Transparent" 
			"Queue" = "Transparent"
			}
		LOD 100
		Blend [_SrcBlend] [_DstBlend]
		Zwrite Off
		Cull Off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature USING_NOISE_TEX1
			#pragma shader_feature USING_NOISE_TEX2
			#pragma shader_feature USING_TEX_MASK
			#pragma shader_feature USING_TEX_MASK2
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
				fixed4 particleTiming : TEXCOORD1;
				fixed4 color2 : TEXCOORD2;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
				fixed4 particleTiming : TEXCOORD1;
				fixed4 color2 : TEXCOORD2;
			};

			sampler2D _NoiseTex;
			fixed4 _NoiseTex_ST;
			fixed3 _TexMod;

			sampler2D _SecondTex;
			fixed4 _SecondTex_ST;
			fixed3 _TexMod2;

			fixed _TexMul;
			fixed _TexPow;

			sampler2D _MaskTexture;
			fixed4 _MaskTexture_ST;
			fixed3 _MaskMod;

			sampler2D _MaskTexture2;
			fixed4 _MaskTexture2_ST;
			fixed3 _MaskMod2;

			fixed _MaskThreshold;
			fixed _MaskMultiplier;

			fixed _EdgeIntensity;
			fixed _EdgeFallOff;
			fixed _EdgePow;

			fixed4 _Color1;
			fixed4 _Color2;

			fixed _PartColMul1;
			fixed _PartColMul2;

			fixed _RampOffset;

			fixed _FinalAlphaMul;

			//Using as few keywords as possible due to keyword limit 
			uniform bool _NoiseRampAsAlpha;
			uniform bool _InvertCol;
			uniform bool _ColorMode;
			uniform bool _UseCustomData;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				o.color.rgb *= _PartColMul1;
				o.particleTiming = v.particleTiming;
				o.color2 = v.color2;
				o.color2.rgb *= _PartColMul2;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//Noise Textures
				#if USING_NOISE_TEX1
					if(_UseCustomData == true)
						_NoiseTex_ST.zw *= i.particleTiming.x;
					float2 uvMainTex = i.uv * _NoiseTex_ST.xy + float2(_Time.x * _NoiseTex_ST.z, _Time.x * _NoiseTex_ST.w);
					fixed noise1 = pow(tex2D(_NoiseTex, uvMainTex) * _TexMod.x, _TexMod.y) + _TexMod.z;
				#else
					fixed noise1 = 1;
				#endif

				#if USING_NOISE_TEX2
					if(_UseCustomData == true)
						_SecondTex_ST.zw *= i.particleTiming.y;
					float2 uvSecondNoise = i.uv * _SecondTex_ST.xy + float2(_Time.x * _SecondTex_ST.z, _Time.x * _SecondTex_ST.w);
					fixed noise2 = pow(tex2D(_SecondTex, uvSecondNoise) * _TexMod2.x, _TexMod2.y) + _TexMod2.z;
				#else
					fixed noise2 = 1;
				#endif


				fixed ramp = clamp(pow(noise1 * noise2 * _TexMul, _TexPow), 0, 10);


				//Texture Mask
				#if USING_TEX_MASK
					if(_UseCustomData == true)
						_MaskTexture_ST.zw *= i.particleTiming.z;
					float2 uvAlphaMask =  i.uv * _MaskTexture_ST.xy + float2(_Time.x * _MaskTexture_ST.z, _Time.x * _MaskTexture_ST.w);
					fixed texMask = pow(tex2D(_MaskTexture, uvAlphaMask) * _MaskMod.x, _MaskMod.y) + _MaskMod.z;
				#else
					fixed texMask = 1;
				#endif

				#if USING_TEX_MASK2
					if(_UseCustomData == true)
						_MaskTexture2_ST.zw *= i.particleTiming.w;
					float2 uvAlphaMask2 = i.uv * _MaskTexture2_ST.xy + float2(_Time.x * _MaskTexture2_ST.z, _Time.x * _MaskTexture2_ST.w);
					fixed texMask2 = pow(tex2D(_MaskTexture2, uvAlphaMask2) * _MaskMod2.x, _MaskMod2.y) + _MaskMod2.z;
				#else
					fixed texMask2 = 1;
				#endif

				//Color
				fixed4 col;
				if(_ColorMode == true) //Particle
					col = lerp(i.color2, i.color, saturate(ramp + _RampOffset));
				else
					col = lerp(_Color2, _Color1, saturate(ramp + _RampOffset));



				if(_InvertCol == true)
					col = 1 - col;


				fixed border = saturate(pow(length(i.uv -.5) * 2 * _EdgeIntensity, _EdgePow) + _EdgeFallOff);
				fixed alphaMasks = clamp(texMask * texMask2, 0, 10);

				if(_NoiseRampAsAlpha == false)
					ramp = 1;

				col.a = saturate(ramp * pow(alphaMasks * _MaskMultiplier, _MaskThreshold)) * i.color.a * (1 - border) * _FinalAlphaMul;
				col.rgb *= col.a;


				if(_InvertCol == true)
					return clamp(1 - col, 0, 2);
				else
					return clamp(col, 0, 2);
			}
			ENDCG
		}
	}

		CustomEditor"NoiseParticleInspector"
}
