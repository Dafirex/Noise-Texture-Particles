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

Shader "Hidden/Dafirex/NoiseTexPreviewRGBA"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TextureOffsetScale("Tex 1 Offset Scale", Vector) = (1, 1, 0, 0)
		_Texture2OffsetScale("Tex 2 Offset Scale", Vector) = (1, 1, 0, 0)
		_MainTexMod("Tex Mod", Vector) = (1, 1, 0, 0)
		_SecondTexMod("Tex2 Mod", Vector) = (1, 1, 0, 0)
		_ColMul("Col Mul", Float) = 1
		_ColPow("Col Pow", Float) = 1
		_EdgeSoft("Edge Soft", Float) = 1
		_EdgeMul("Edge Mul", FLoat) = 1
		_EdePow("Edge Pow", Float) = 1
		
		[Toggle]_ShowEdge("Show Edge", Int) = 0
		[Toggle]_Mask("Mask", Int) = 1
		[Toggle]_UseTex1("Use Tex 1", Int) = 1
		[Toggle]_UseTex2("ues Tex 2", Int) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
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
			fixed4 _TextureOffsetScale;
			fixed4 _Texture2OffsetScale;

			fixed4 _MainTexMod;
			fixed4 _SecondTexMod;
			fixed _ColMul;
			fixed _ColPow;

			fixed _EdgeSoft;
			fixed _EdgeMul;
			fixed _EdgePow;

			bool _ShowEdge;
			bool _Mask;
			bool _UseTex1;
			bool _UseTex2;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				o.uv = v.uv;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed border = 1;
				if(_ShowEdge == true)
					border = 1 - (saturate(pow(length(i.uv -.5) * 2 * _EdgeMul, _EdgePow) + _EdgeSoft));
				
				fixed tex1;
				fixed tex2;
				if(_Mask == true){
					tex1 = saturate(pow(tex2D(_MainTex, i.uv * _TextureOffsetScale.xy + float2(_Time.x * _TextureOffsetScale.z, _Time.x * _TextureOffsetScale.w)) * _MainTexMod.x, _MainTexMod.y) + _MainTexMod.z).r;
					tex2 = saturate(pow(tex2D(_MainTex, i.uv * _Texture2OffsetScale.xy + float2(_Time.x * _Texture2OffsetScale.z, _Time.x * _Texture2OffsetScale.w)) * _SecondTexMod.x, _SecondTexMod.y) + _SecondTexMod.z).g;
				}
				else{
					tex1 = saturate(pow(tex2D(_MainTex, i.uv * _TextureOffsetScale.xy + float2(_Time.x * _TextureOffsetScale.z, _Time.x * _TextureOffsetScale.w)) * _MainTexMod.x, _MainTexMod.y) + _MainTexMod.z).b;
					tex2 = saturate(pow(tex2D(_MainTex, i.uv * _Texture2OffsetScale.xy + float2(_Time.x * _Texture2OffsetScale.z, _Time.x * _Texture2OffsetScale.w)) * _SecondTexMod.x, _SecondTexMod.y) + _SecondTexMod.z).a;
				}
				if(_UseTex1 == false)
					tex1 = 1;
				if(_UseTex2 == false)
					tex2 = 1;

				fixed4 col = saturate(pow(tex1 * tex2 * _ColMul, _ColPow) * border);
				return col;
			}
			ENDCG
		}
	}
}
