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

Shader "Hidden/Dafirex/NoiseTexPreview"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SecondTex ("Texture 2", 2D) = "white" {}
		_MainTexMod("Tex Mod", Vector) = (1, 1, 0, 0)
		_SecondTexMod("Tex2 Mod", Vector) = (1, 1, 0, 0)
		_ColMul("Col Mul", Float) = 1
		_ColPow("Col Pow", Float) = 1
		_EdgeSoft("Edge Soft", Float) = 1
		_EdgeMul("Edge Mul", FLoat) = 1
		_EdePow("Edge Pow", Float) = 1
		
		[Toggle]_ShowEdge("Show Edge", Int) = 0
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
			fixed4 _MainTex_ST;
			
			sampler2D _SecondTex;
			fixed4 _SecondTex_ST;

			fixed4 _MainTexMod;
			fixed4 _SecondTexMod;
			fixed _ColMul;
			fixed _ColPow;

			fixed _EdgeSoft;
			fixed _EdgeMul;
			fixed _EdgePow;

			int _ShowEdge;

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
				if(_ShowEdge == 1)
					border = 1 - (saturate(pow(length(i.uv -.5) * 2 * _EdgeMul, _EdgePow) + _EdgeSoft));

				fixed tex1 = saturate(pow(tex2D(_MainTex, i.uv * _MainTex_ST.xy + float2(_Time.x * _MainTex_ST.z, _Time.x * _MainTex_ST.w)) * _MainTexMod.x, _MainTexMod.y) + _MainTexMod.z);
				fixed tex2 = saturate(pow(tex2D(_SecondTex, i.uv * _SecondTex_ST.xy + float2(_Time.x * _SecondTex_ST.z, _Time.x * _SecondTex_ST.w)) * _SecondTexMod.x, _SecondTexMod.y) + _SecondTexMod.z);
				fixed4 col = saturate(pow(tex1 * tex2 * _ColMul, _ColPow) * border);
				return col;
			}
			ENDCG
		}
	}
}
