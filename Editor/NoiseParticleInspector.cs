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
public class NoiseParticleInspector : ShaderGUI 
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
	MaterialProperty secondTex;
	MaterialProperty noiseTexMod;
	MaterialProperty secondTexMod;

	MaterialProperty customDataSpeed;
	MaterialProperty texMul;
	MaterialProperty texPow;

	MaterialProperty maskTex;
	MaterialProperty maskTex2;
	MaterialProperty maskTexMod;
	MaterialProperty maskTex2Mod;

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


	//For RGBA Combined Shader
	MaterialProperty rgbaChannelCheck;
	MaterialProperty noiseTexOffsetScale;
	MaterialProperty secondTexOffsetScale;
	MaterialProperty maskTexOffsetScale;
	MaterialProperty maskTex2OffsetScale;

	int combinedTexWidth = 1024;
	int combinedTexHeight = 1024;

    public override void OnGUI (MaterialEditor materialEditor, MaterialProperty[] prop)
    {
			blendMode = ShaderGUI.FindProperty("_BlendMode", prop);
			srcBlend = ShaderGUI.FindProperty("_SrcBlend", prop);
			dstBlend = ShaderGUI.FindProperty("_DstBlend", prop);
			invCol = ShaderGUI.FindProperty("_InvertCol", prop);

			noiseTex = ShaderGUI.FindProperty("_NoiseTex", prop);
			secondTex = ShaderGUI.FindProperty("_SecondTex", prop);
			noiseTexMod = ShaderGUI.FindProperty("_TexMod", prop);
			secondTexMod = ShaderGUI.FindProperty("_TexMod2", prop);

			customDataSpeed = ShaderGUI.FindProperty("_UseCustomData", prop);
			texMul = ShaderGUI.FindProperty("_TexMul", prop);
			texPow = ShaderGUI.FindProperty("_TexPow", prop);

			maskTex = ShaderGUI.FindProperty("_MaskTexture", prop);
			maskTex2 = ShaderGUI.FindProperty("_MaskTexture2", prop);
			maskTexMod = ShaderGUI.FindProperty("_MaskMod", prop);
			maskTex2Mod = ShaderGUI.FindProperty("_MaskMod2", prop);

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
			
			noiseTexOffsetScale = ShaderGUI.FindProperty("_NoiseTexOffsetScale", prop);
			secondTexOffsetScale = ShaderGUI.FindProperty("_SecondTexOffsetScale", prop);
			maskTexOffsetScale = ShaderGUI.FindProperty("_MaskTextureOffsetScale", prop);
			maskTex2OffsetScale = ShaderGUI.FindProperty("_MaskTexture2OffsetScale", prop);
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





		//Texture Options
		//Texture 1 block
		EditorGUI.BeginChangeCheck();
		materialEditor.TexturePropertySingleLine(new GUIContent("Noise Texture 1", "Offsets X Y controls scroll speed"), noiseTex);
		if(EditorGUI.EndChangeCheck()){
			foreach(Material mat in noiseTex.targets){
				if(noiseTex.textureValue == null)
					mat.DisableKeyword("USING_NOISE_TEX1");
				else{
					mat.EnableKeyword("USING_NOISE_TEX1");
				}
			}

		}
		//Texture 1 Properties
		SetUpProperties(noiseTex, noiseTexMod, materialEditor);

		//Texture 2 Block
		EditorGUI.BeginChangeCheck();
		materialEditor.TexturePropertySingleLine(new GUIContent("Noise Texture 2", "Offsets X Y controls scroll speed"), secondTex);
		if(EditorGUI.EndChangeCheck()){
			foreach(Material mat in secondTex.targets){
				if(secondTex.textureValue == null)
					mat.DisableKeyword("USING_NOISE_TEX2");
				else{
					mat.EnableKeyword("USING_NOISE_TEX2");
				}
			}
		}
		//Texture 2 Properties
		SetUpProperties(secondTex, secondTexMod, materialEditor);


		//Custom Data Speed Option
		materialEditor.ShaderProperty(customDataSpeed, new GUIContent("Custom Data 1 speed mod", "Allows use of custom data 1 to modify speed offset speed, X -> Noise 1 | Y -> Noise 2 | Z -> Mask"));


		//Noise/Ramp Options
		materialEditor.FloatProperty(texMul, "Noise Multiplier");
		materialEditor.FloatProperty(texPow, "Noise Power");
		if(GUILayout.Button("Show Preview Window")){
			NoiseTexturePreview.GetWindow<NoiseTexturePreview>("Texture Prview");
			NoiseTexturePreview.SendMaterialTextures((Texture2D) noiseTex.textureValue, (Texture2D) secondTex.textureValue);
			activeTex = ActiveTex.Noise;
		}


		//Texture Mask
		GUILayout.Space(20);
		EditorGUI.BeginChangeCheck();
		materialEditor.TexturePropertySingleLine(new GUIContent("Mask Texture", ""), maskTex);
		//Using Texturemask
		if(EditorGUI.EndChangeCheck()){
			foreach(Material mat in maskTex.targets){
				if(maskTex.textureValue == null)
					mat.DisableKeyword("USING_TEX_MASK");
				else{
					mat.EnableKeyword("USING_TEX_MASK");
				}
			}
		}
		//Mask Tex 1 Properties
		SetUpProperties(maskTex, maskTexMod, materialEditor);

		//Mask Tex 2
		EditorGUI.BeginChangeCheck();
		materialEditor.TexturePropertySingleLine(new GUIContent("Mask Texture 2", ""), maskTex2);
		if(EditorGUI.EndChangeCheck()){
			foreach(Material mat in maskTex2.targets){
				if(maskTex2.textureValue == null)
					mat.DisableKeyword("USING_TEX_MASK2");
				else{
					mat.EnableKeyword("USING_TEX_MASK2");
				}
			}
		}
		//Mask Tex 2 Properties
		SetUpProperties(maskTex2, maskTex2Mod, materialEditor);
		

		materialEditor.FloatProperty(maskMul, "Mask Intensity");
		materialEditor.FloatProperty(maskPow, "Mask Power");
		GUILayout.Space(20);

		materialEditor.ShaderProperty(rampAsAlpha, "Noise Times Alpha");
		materialEditor.ShaderProperty(edgeMul, "Mask Edge Intensity (Multiply)");
		materialEditor.ShaderProperty(edgePow, "Mask Edge Exponent (Power)");
		materialEditor.ShaderProperty(edgeSoft, "Mask Edge Brightness (Add)");

		if(GUILayout.Button("Show Preview Window")){
			NoiseTexturePreview.GetWindow<NoiseTexturePreview>("Texture Prview");
			NoiseTexturePreview.SendMaterialTextures((Texture2D) maskTex.textureValue, (Texture2D) maskTex2.textureValue);
			activeTex = ActiveTex.Mask;
		}

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

		//End Check for all values
		if(NoiseTexturePreview.IsActive()){
			switch(activeTex){
				case ActiveTex.Noise:
					NoiseTexturePreview.SendMaterialTextures((Texture2D) noiseTex.textureValue, (Texture2D) secondTex.textureValue);
					NoiseTexturePreview.SendMaterialValues(noiseTexMod.vectorValue, noiseTex.textureScaleAndOffset, secondTexMod.vectorValue, secondTex.textureScaleAndOffset, texMul.floatValue, texPow.floatValue);
					break;
				case ActiveTex.Mask:
					NoiseTexturePreview.SendMaterialTextures((Texture2D) maskTex.textureValue, (Texture2D) maskTex2.textureValue);
					NoiseTexturePreview.SendMaterialValues(maskTexMod.vectorValue, maskTex.textureScaleAndOffset, maskTex2Mod.vectorValue, maskTex2.textureScaleAndOffset, maskMul.floatValue, maskPow.floatValue, edgeSoft.floatValue, edgeMul.floatValue, edgePow.floatValue);
					break;
				default:
					break;

			}
		}


		GUILayout.Space(20);
		int widthTarget = combinedTexWidth;
		combinedTexWidth = EditorGUILayout.IntField("Combined Texture Width", combinedTexWidth);
		combinedTexHeight = EditorGUILayout.IntField("Combined Texture Height", combinedTexHeight);

		combinedTexWidth = combinedTexWidth > 4096 ? 4096 : combinedTexWidth <= 0 ? 1 : combinedTexWidth;
		combinedTexHeight= combinedTexHeight > 4096 ? 4096 : combinedTexHeight <= 0 ? 1 : combinedTexHeight;

		if (GUILayout.Button("Combine Textures and Switch Shaders")) {
			if(EditorUtility.DisplayDialog("Are you sure?", "This option will create a new texture and change certain values; this cannot be undone.\nThis is recommeneded to be done after setting up all texture properties. \nMake sure that ALL textures have read/write enabled in the texture's import settings.", "Yes", "Cancel")){
				bool readable;
				string path = EditorUtility.SaveFilePanelInProject("Save Texture", "Combined RGBA.png", "png", "Please enter a filename");

				if(path.Length == 0)
					Debug.Log("Cancelled");
				else{
					//Jank method of checking if texture is readable
					try{
						Texture2D check;
						if(noiseTex.textureValue != null){
							check = (Texture2D) noiseTex.textureValue;
							check.GetPixel(0, 0);
						}
						if(secondTex.textureValue != null){
							check = (Texture2D) secondTex.textureValue;
							check.GetPixel(0, 0);
						}
						if(maskTex.textureValue != null){
							check = (Texture2D) maskTex.textureValue;
							check.GetPixel(0, 0);
						}
						if(maskTex2.textureValue != null){
							check = (Texture2D) maskTex2.textureValue;
							check.GetPixel(0, 0);
						}
						readable = true;
					}catch{
						EditorUtility.DisplayDialog("Failed to read textures!", "Did you enable Read/Write in the import settings for all textures?", "Ok");
						readable = false;
					}

					if(readable){
						
						Texture2D noiseTexCombine = SetTex((Texture2D) noiseTex.textureValue);
						Texture2D noiseTexTwoCombine = SetTex((Texture2D) secondTex.textureValue);
						Texture2D maskTexCombine = SetTex((Texture2D) maskTex.textureValue);
						Texture2D maskTexTwoCombine = SetTex((Texture2D) maskTex2.textureValue);

						Texture2D finalTex = CombineImages(noiseTexCombine, noiseTexTwoCombine, maskTexCombine, maskTexTwoCombine, combinedTexWidth, combinedTexHeight);
						byte[] pngData = finalTex.EncodeToPNG();
						if(pngData != null){
							System.IO.File.WriteAllBytes(path, pngData);
							AssetDatabase.Refresh();
						}

						//Use Tex Channel
						Vector4 setTexActive = new Vector4(
							BoolToFloat(noiseTex.textureValue != null),
							BoolToFloat(secondTex.textureValue != null),
							BoolToFloat(maskTex.textureValue != null),
							BoolToFloat(maskTex2.textureValue != null)
							);
						foreach(Material mat in rgbaChannelCheck.targets)
							mat.SetVector("_RGBAChecks", setTexActive);

						//Texture OffsetScale
						foreach(Material mat in noiseTexOffsetScale.targets)
							mat.SetVector("_NoiseTexOffsetScale", noiseTex.textureScaleAndOffset);
						foreach(Material mat in secondTexOffsetScale.targets)
							mat.SetVector("_SecondTexOffsetScale", secondTex.textureScaleAndOffset);
						foreach(Material mat in maskTexOffsetScale.targets)
							mat.SetVector("_MaskTextureOffsetScale", maskTex.textureScaleAndOffset);
						foreach(Material mat in maskTex2OffsetScale.targets)
							mat.SetVector("_MaskTexture2OffsetScale", maskTex2.textureScaleAndOffset);
						
						//Set Texture
						Texture2D finalTextureTex = (Texture2D) AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
						TextureImporter finalTextureImport = (TextureImporter) TextureImporter.GetAtPath(path);
						finalTextureImport.alphaIsTransparency = true;
						AssetDatabase.Refresh();
						foreach(Material mat in noiseTex.targets)
							mat.SetTexture("_NoiseTex", finalTextureTex);
						//Finally Switch Shaders
						material.shader = Shader.Find("Dafirex/Particles/NoiseParticles RGBA Tex");

					}

				}

			}

		}


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

	public static void SetUpProperties(MaterialProperty texProp, MaterialProperty prop, MaterialEditor materialEditor){

		bool showOption = FloatToBool(prop.vectorValue.w);
		EditorGUI.indentLevel += 2;
		EditorGUI.BeginChangeCheck();
		showOption = EditorGUILayout.Foldout(showOption, "Properties"); 
		if(EditorGUI.EndChangeCheck()){
			//Set foldout boolean
			foreach(Material mat in prop.targets)
				mat.SetVector(prop.name, new Vector4(prop.vectorValue.x, prop.vectorValue.y, prop.vectorValue.z, BoolToFloat(showOption)));
		}
		//Show foldout options
		if(FloatToBool(prop.vectorValue.w)){

			EditorGUI.indentLevel -= 1;
			materialEditor.TextureScaleOffsetProperty(texProp);
			EditorGUI.indentLevel += 1;

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

	//1 = True ; 0 = False
	private static bool FloatToBool(float x){
		return (int) x == 1 ? true : false;
	}

	private static float BoolToFloat(bool x){
		return x == true ? 1 : 0;
	}

	//Combine Four Texture2D objects into one 
	private static Texture2D CombineImages(Texture2D red, Texture2D green, Texture2D blue, Texture2D alpha, int width, int height){
		Texture2D combinedImage = new Texture2D(width, height);

		//Resize if the dimensions do not match 
		if(red.width != width || red.height != height)
			red = ScaleTexture(red, width, height, "red channel");

		if(green.width != width || green.height != height)
			green = ScaleTexture(green, width, height, "green channel");

		if(blue.width != width || blue.height != height)
			blue = ScaleTexture(blue, width, height, "blue channel");

		if(alpha.width != width || alpha.height != height)
			alpha = ScaleTexture(alpha, width, height, "alpha channel");

		//Set Colors
		EditorUtility.DisplayProgressBar("Combining Channels", "" , 0);
		for(int w = 0; w < combinedImage.width; w++){
			EditorUtility.DisplayProgressBar("Combining Channels", "" , (float) w / (float) width);
			for(int h = 0; h < combinedImage.height; h++){
				//Read each pixel from each image and set the corresponding pixel
				combinedImage.SetPixel(w, h, new Color(red.GetPixel(w, h).grayscale, green.GetPixel(w, h).grayscale, blue.GetPixel(w, h).grayscale, alpha.GetPixel(w, h).grayscale));
			}
		}
		EditorUtility.ClearProgressBar();
		combinedImage.Apply();
		return combinedImage;
	}


	//Scales the image
	private static Texture2D ScaleTexture(Texture2D src, int width, int height, string name){
		Texture2D result = new Texture2D(width, height);
		EditorUtility.DisplayProgressBar("Resizing " + name, "Resizing Image", 0);
		for (int i = 0; i < result.height; i++){
			EditorUtility.DisplayProgressBar("Resizing " + name, "Resizing Image", (float) i / (float) height);
			for (int j = 0; j < result.width; j++){
				Color newColor = src.GetPixelBilinear((float) j / (float) result.width, (float) i / (float) result.height);
				result.SetPixel(j, i, newColor);
			}
		 }
		EditorUtility.ClearProgressBar();
		result.Apply();
		return result;
	}


	private Texture2D SetTex(Texture2D src){
		Texture2D result;
		if(src != null)
			result = src;
		else{
			result = new Texture2D(0, 0);
			result.SetPixel(0, 0, Color.white);
		}
		return result;
	}
}
