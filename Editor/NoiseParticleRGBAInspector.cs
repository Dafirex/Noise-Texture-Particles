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


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
public class NoiseParticleRGBAInspector : ShaderGUI 
{
	public enum ActiveTex{
		Noise,
		Mask
	}

	public enum ColorMode{
		Shader,
		Particle
	}

	public enum BlendMode{
		Additive,
		SoftAdditive,
		Multiplicative,
		Transparency,
		PremultipliedTransparency,
		Advanced
	}
	ActiveTex activeTex;

	MaterialProperty blendMode;
	MaterialProperty srcBlend;
	MaterialProperty dstBlend;
	MaterialProperty invCol;

	MaterialProperty noiseTex;
	MaterialProperty noiseTexOffsetScale;
	MaterialProperty noiseTexMod;
	MaterialProperty secondTexOffsetScale;
	MaterialProperty secondTexMod;

	MaterialProperty customDataSpeed;
	MaterialProperty texMul;
	MaterialProperty texPow;

	MaterialProperty maskTexMod;
	MaterialProperty maskTexOffsetScale;
	MaterialProperty maskTex2Mod;
	MaterialProperty maskTex2OffsetScale;

	MaterialProperty rampAsAlpha;
	MaterialProperty maskPow;
	MaterialProperty maskMul;

	MaterialProperty edgeSoft;
	MaterialProperty edgeMul;
	MaterialProperty edgePow;

	MaterialProperty color1;
	MaterialProperty color2;
	MaterialProperty colorMode;
	MaterialProperty partColMul1;
	MaterialProperty partColMul2;

	MaterialProperty rampOffset;

	MaterialProperty finAlphaMul;

	MaterialProperty rgbaChannelCheck;

	int combinedTexWidth = 1024;
	int combinedTexHeight = 1024;

    public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] prop)
    {
			blendMode = ShaderGUI.FindProperty("_BlendMode", prop);
			srcBlend = ShaderGUI.FindProperty("_SrcBlend", prop);
			dstBlend = ShaderGUI.FindProperty("_DstBlend", prop);
			invCol = ShaderGUI.FindProperty("_InvertCol", prop);

			noiseTex = ShaderGUI.FindProperty("_NoiseTex", prop);
			noiseTexMod = ShaderGUI.FindProperty("_TexMod", prop);
			secondTexMod = ShaderGUI.FindProperty("_TexMod2", prop);

			noiseTexOffsetScale = ShaderGUI.FindProperty("_NoiseTexOffsetScale", prop);
			secondTexOffsetScale = ShaderGUI.FindProperty("_SecondTexOffsetScale", prop);

			customDataSpeed = ShaderGUI.FindProperty("_UseCustomData", prop);
			texMul = ShaderGUI.FindProperty("_TexMul", prop);
			texPow = ShaderGUI.FindProperty("_TexPow", prop);

			maskTexMod = ShaderGUI.FindProperty("_MaskMod", prop);
			maskTex2Mod = ShaderGUI.FindProperty("_MaskMod2", prop);
			maskTexOffsetScale = ShaderGUI.FindProperty("_MaskTextureOffsetScale", prop);
			maskTex2OffsetScale = ShaderGUI.FindProperty("_MaskTexture2OffsetScale", prop);
			
			rampAsAlpha = ShaderGUI.FindProperty("_NoiseRampAsAlpha", prop);
			maskPow = ShaderGUI.FindProperty("_MaskThreshold", prop);
			maskMul = ShaderGUI.FindProperty("_MaskMultiplier", prop);

			edgeSoft = ShaderGUI.FindProperty("_EdgeFallOff", prop);
			edgeMul = ShaderGUI.FindProperty("_EdgeIntensity", prop);
			edgePow = ShaderGUI.FindProperty("_EdgePow", prop);

			color1 = ShaderGUI.FindProperty("_Color1", prop);
			color2 = ShaderGUI.FindProperty("_Color2", prop);
			colorMode = ShaderGUI.FindProperty("_ColorMode", prop);

			partColMul1 = ShaderGUI.FindProperty("_PartColMul1", prop);
			partColMul2 = ShaderGUI.FindProperty("_PartColMul2", prop);

			rampOffset = ShaderGUI.FindProperty("_RampOffset", prop);

			finAlphaMul = ShaderGUI.FindProperty("_FinalAlphaMul", prop);
			
			rgbaChannelCheck = ShaderGUI.FindProperty("_RGBAChecks", prop);
			
		Material material = materialEditor.target as Material;
		//Blend Options
		EditorGUI.BeginChangeCheck();
		BlendMode bMode = (BlendMode) blendMode.floatValue;
		bMode = (BlendMode) EditorGUILayout.EnumPopup("Blend Mode", bMode);

		switch (bMode){
			case BlendMode.Advanced:
				materialEditor.ShaderProperty(srcBlend, "Src", 2);
				materialEditor.ShaderProperty(dstBlend, "Dst", 2);
				materialEditor.ShaderProperty(invCol, "Invert Ramp", 2);
				break;
			default:
				break;
		}
		if(EditorGUI.EndChangeCheck()){
			materialEditor.RegisterPropertyChangeUndo("Blend Mode");
			blendMode.floatValue = (int) bMode;

			foreach(Material mat in invCol.targets)
				SetUpBlendMode(mat, bMode, srcBlend, dstBlend);


		}
		GUILayout.Space(20);





		//Main Tex
		materialEditor.TexturePropertySingleLine(new GUIContent("Main Texture", "Offsets X Y controls scroll speed"), noiseTex);
		GUILayout.Space(20);

		//Red Channel (Noise Tex)
		EditorGUI.BeginChangeCheck();
		bool useRed = FloatToBool(rgbaChannelCheck.vectorValue.x);
		useRed = EditorGUILayout.Toggle("Use Red Channel (Noise 1)", useRed);
		if(EditorGUI.EndChangeCheck()){
			materialEditor.RegisterPropertyChangeUndo("Red Channel");
			foreach(Material mat in rgbaChannelCheck.targets){
				mat.SetVector("_RGBAChecks", new Vector4(BoolToFloat(useRed), rgbaChannelCheck.vectorValue.y, rgbaChannelCheck.vectorValue.z, rgbaChannelCheck.vectorValue.w));
				if(useRed)
					mat.EnableKeyword("USING_NOISE_TEX1");
				else
					mat.DisableKeyword("USING_NOISE_TEX1");
			}
		}

		//Texture 1 Properties
		SetUpProperties("Red", noiseTexOffsetScale, noiseTexMod, materialEditor);


		//Green Channel (Noise Tex 2)
		EditorGUI.BeginChangeCheck();
		bool useGreen = FloatToBool(rgbaChannelCheck.vectorValue.y);
		useGreen = EditorGUILayout.Toggle("Use Green Channel (Noise 2)", useGreen);
		if(EditorGUI.EndChangeCheck()){
			materialEditor.RegisterPropertyChangeUndo("Green Channel");
			foreach(Material mat in rgbaChannelCheck.targets){
				mat.SetVector("_RGBAChecks", new Vector4(rgbaChannelCheck.vectorValue.x, BoolToFloat(useGreen), rgbaChannelCheck.vectorValue.z, rgbaChannelCheck.vectorValue.w));
				if(useGreen)
					mat.EnableKeyword("USING_NOISE_TEX2");
				else
					mat.DisableKeyword("USING_NOISE_TEX2");
			}
		}
		//Texture 2 Properties
		SetUpProperties("Green", secondTexOffsetScale, secondTexMod, materialEditor);


		//Custom Data Speed Option
		GUILayout.Space(20);
		materialEditor.ShaderProperty(customDataSpeed, new GUIContent("Custom Data 1 speed mod", "Allows use of custom data 1 to modify speed offset speed, X -> Noise 1 | Y -> Noise 2 | Z -> Mask"));


		//Noise/Ramp Options
		materialEditor.FloatProperty(texMul, "Noise Multiplier");
		materialEditor.FloatProperty(texPow, "Noise Power");


		GUILayout.Space(20);
		//Blue Channel (Alpha Mask)
		EditorGUI.BeginChangeCheck();
		bool useBlue = FloatToBool(rgbaChannelCheck.vectorValue.z);
		useBlue = EditorGUILayout.Toggle("Use Blue Channel (Alpha Mask)", useBlue);
		if(EditorGUI.EndChangeCheck()){
			materialEditor.RegisterPropertyChangeUndo("Blue Channel");
			foreach(Material mat in rgbaChannelCheck.targets){
				mat.SetVector("_RGBAChecks", new Vector4(rgbaChannelCheck.vectorValue.x, rgbaChannelCheck.vectorValue.y, BoolToFloat(useBlue), rgbaChannelCheck.vectorValue.w));
				if(useBlue)
					mat.EnableKeyword("USING_TEX_MASK");
				else
					mat.DisableKeyword("USING_TEX_MASK");
			}
		}
		//Mask Tex 1 Properties
		SetUpProperties("Blue", maskTexOffsetScale, maskTexMod, materialEditor);

		//Alpha Channel (Alpha Mask 2)
		EditorGUI.BeginChangeCheck();
		bool useAlpha = FloatToBool(rgbaChannelCheck.vectorValue.w);
		useAlpha = EditorGUILayout.Toggle("Use Alpha Channel (Alpha Mask 2)", useAlpha);
		if(EditorGUI.EndChangeCheck()){
			materialEditor.RegisterPropertyChangeUndo("Alpha Channel");
			foreach(Material mat in rgbaChannelCheck.targets){
				mat.SetVector("_RGBAChecks", new Vector4(rgbaChannelCheck.vectorValue.x, rgbaChannelCheck.vectorValue.y, rgbaChannelCheck.vectorValue.z, BoolToFloat(useAlpha)));
				if(useAlpha)
					mat.EnableKeyword("USING_TEX_MASK2");
				else
					mat.DisableKeyword("USING_TEX_MASK2");
			}
		}
		//Mask Tex 2 Properties
		SetUpProperties("Alpha", maskTex2OffsetScale, maskTex2Mod, materialEditor);
		
		GUILayout.Space(20);
		materialEditor.FloatProperty(maskMul, "Mask Intensity");
		materialEditor.FloatProperty(maskPow, "Mask Power");
		GUILayout.Space(20);

		materialEditor.ShaderProperty(rampAsAlpha, "Noise Times Alpha");
		materialEditor.ShaderProperty(edgeMul, "Mask Edge Intensity (Multiply)");
		materialEditor.ShaderProperty(edgePow, "Mask Edge Exponent (Power)");
		materialEditor.ShaderProperty(edgeSoft, "Mask Edge Brightness (Add)");


		//Color Mode Option
		GUILayout.Space(20);
		materialEditor.RangeProperty(rampOffset, "Color Ramp Offset");
		EditorGUI.BeginChangeCheck();
		ColorMode cMode = (ColorMode) colorMode.floatValue;
		cMode = (ColorMode) EditorGUILayout.EnumPopup("Color Mode", cMode);

		if (EditorGUI.EndChangeCheck()){
			materialEditor.RegisterPropertyChangeUndo("Color Mode");
			colorMode.floatValue = (int) cMode;
		}
		switch (cMode){
			case ColorMode.Particle: //Use Color from particles
				materialEditor.ShaderProperty(partColMul1, "Color 1 Multiplier", 2);
				materialEditor.ShaderProperty(partColMul2, "Color 2 Multiplier", 2);
				break;
			case ColorMode.Shader: //Use color from shader
			default:
				materialEditor.ShaderProperty(color1, "Color 1", 2);
				materialEditor.ShaderProperty(color2, "Color 2", 2);
				//Swap Colors Button
				if(GUILayout.Button("Swap Colors")){
					Color tempCol = color1.colorValue;
					foreach(Material mat in color1.targets)
						mat.SetColor(color1.name, color2.colorValue);
					foreach(Material mat in color2.targets)
						mat.SetColor(color2.name, tempCol);
				}
				break;
		}
		materialEditor.ShaderProperty(finAlphaMul, "Final Multiplier");




		


    }

	public static void SetUpBlendMode(Material mat, BlendMode mode, MaterialProperty srcBlend, MaterialProperty dstBlend){

				switch (mode){
					case BlendMode.Advanced:
						srcBlend.floatValue = mat.GetFloat("_SrcBlend");
						dstBlend.floatValue = mat.GetFloat("_DstBlend");
						break;

					case BlendMode.Additive:
						srcBlend.floatValue = (int) UnityEngine.Rendering.BlendMode.One;
						dstBlend.floatValue = (int) UnityEngine.Rendering.BlendMode.One;
						mat.SetFloat("_InvertCol", 0);
						break;
					case BlendMode.SoftAdditive:
						srcBlend.floatValue = (int) UnityEngine.Rendering.BlendMode.OneMinusDstColor;
						dstBlend.floatValue = (int) UnityEngine.Rendering.BlendMode.One;
						mat.SetFloat("_InvertCol", 0);
						break;
					case BlendMode.Multiplicative:
						srcBlend.floatValue = (int) UnityEngine.Rendering.BlendMode.DstColor;
						dstBlend.floatValue = (int) UnityEngine.Rendering.BlendMode.Zero;
						mat.SetFloat("_InvertCol", 1);
						break;
					case BlendMode.Transparency:
						srcBlend.floatValue = (int) UnityEngine.Rendering.BlendMode.SrcAlpha;
						dstBlend.floatValue = (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
						mat.SetFloat("_InvertCol", 0);
						break;
					case BlendMode.PremultipliedTransparency:
						srcBlend.floatValue = (int) UnityEngine.Rendering.BlendMode.One;
						dstBlend.floatValue = (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
						mat.SetFloat("_InvertCol", 0);
						break;
					default:
						srcBlend.floatValue = (int) UnityEngine.Rendering.BlendMode.One;
						dstBlend.floatValue = (int) UnityEngine.Rendering.BlendMode.One;
						mat.SetFloat("_InvertCol", 0);
						break;
				}
			
	}

	public static void SetUpProperties(String name, MaterialProperty offsetScale, MaterialProperty prop, MaterialEditor materialEditor){

		bool showOption = FloatToBool(prop.vectorValue.w);
		EditorGUI.indentLevel += 2;
		EditorGUI.BeginChangeCheck();
		showOption = EditorGUILayout.Foldout(showOption, name + " Channel Properties" ); 
		if(EditorGUI.EndChangeCheck()){
			//Set foldout boolean
			foreach(Material mat in prop.targets)
				mat.SetVector(prop.name, new Vector4(prop.vectorValue.x, prop.vectorValue.y, prop.vectorValue.z, BoolToFloat(showOption)));
		}
		//Show foldout options
		if(FloatToBool(prop.vectorValue.w)){

			materialEditor.VectorProperty(offsetScale, "Offset Scale");
			//materialEditor.TextureScaleOffsetProperty(texProp);


			//Texture modifiers
			float noiseTexMul = prop.vectorValue.x;
			float noiseTexPow = prop.vectorValue.y;
			float noiseTexAdd = prop.vectorValue.z; 
			EditorGUI.BeginChangeCheck();
			noiseTexMul = EditorGUILayout.FloatField("Intensity (Multiply)", noiseTexMul);
			noiseTexPow = EditorGUILayout.FloatField("Power (Exponent)", noiseTexPow);
			noiseTexAdd = EditorGUILayout.Slider( "Brightness (Add)", noiseTexAdd, -1, 1);
			if(EditorGUI.EndChangeCheck()){
				materialEditor.RegisterPropertyChangeUndo("Change Texture Property");
				foreach(Material mat in prop.targets)
					mat.SetVector(prop.name, new Vector4(noiseTexMul, noiseTexPow, noiseTexAdd, prop.vectorValue.w));
			}
			GUILayout.Space(20);
		}
		EditorGUI.indentLevel -= 2;
	}

	private static bool FloatToBool(float x){
		return (int) x == 1 ? true : false;
	}

	private static float BoolToFloat(bool x){
		return x == true ? 1 : 0;
	}


}
