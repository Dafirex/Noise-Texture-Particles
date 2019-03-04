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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(MeshFilter))]
public class NoiseTexturePreview : EditorWindow {

	public enum ActiveTex{
		Noise,
		Mask
	}

	static Editor gameObjectEditor;
	GUIStyle bgColor;

	GameObject previewQuad;
	MeshRenderer quadRenderer;
	Material quadMat;

	static bool isActive = false;

	static bool timeToUpdate;
	static Texture2D tex1;
	static Texture2D tex2;
	static Vector4 tex1Mod;
	static Vector2 tex1Scale;
	static Vector2 tex1Offset;

	static Vector4 tex2Mod;
	static Vector2 tex2Scale;
	static Vector2 tex2Offset;

	static float colMul;
	static float colPow;

	static float edgeSoft;
	static float edgeMul;
	static float edgePow;

	static ActiveTex currentTex;
	static bool RGBATex;

	private void Awake() {
		previewQuad = GameObject.CreatePrimitive(PrimitiveType.Cube);
		previewQuad.transform.position = new Vector3(1000000000, 1000000000, 1000000000);
		previewQuad.name = "Texture Preview";
		quadMat = new Material(Shader.Find("Hidden/Dafirex/NoiseTexPreview"));
		quadRenderer = previewQuad.GetComponent<MeshRenderer>();
		quadRenderer.material = quadMat;

		gameObjectEditor = Editor.CreateEditor(previewQuad);
		bgColor = new GUIStyle();
		bgColor.normal.background = Texture2D.blackTexture;

		isActive = true;
		timeToUpdate = false;
	}


	public static bool IsActive() {
		return isActive;
	}

	public static void SetTexOne(Texture2D texture){
		tex1 = texture;
		timeToUpdate = true;
	}

	public static void SetTexTwo(Texture2D texture){
		tex2 = texture;
		timeToUpdate = true;
	}

	public static void SendMaterialTextures(Texture2D texture1, Texture2D texture2){
		RGBATex = false;
		tex1 = texture1;
		tex2 = texture2;
		timeToUpdate = true;
	}

	public static void SendMaterialTextures(Texture2D texture1){
		RGBATex = true;
		tex1 = texture1;
		timeToUpdate = true;
	}

	public static void SendMaterialValues(Vector4 mod1, Vector4 tex1OffsetScale, Vector4 mod2, Vector4 tex2OffsetScale, float colMulVal, float colPowVal){
		timeToUpdate = true;
		tex1Mod = mod1;
		tex1Offset = new Vector2(tex1OffsetScale.z, tex1OffsetScale.w);
		tex1Scale = new Vector2(tex1OffsetScale.x, tex1OffsetScale.y);
		tex2Mod = mod2;
		tex2Offset = new Vector2(tex2OffsetScale.z, tex2OffsetScale.w);
		tex2Scale = new Vector2(tex2OffsetScale.x, tex2OffsetScale.y);
		colMul = colMulVal;
		colPow = colPowVal;
		currentTex = ActiveTex.Noise;
	}
	public static void SendMaterialValues(Vector4 mod1, Vector4 tex1OffsetScale, Vector4 mod2, Vector4 tex2OffsetScale, float colMulVal, float colPowVal, float edgeSoftVal, float edgeMulVal, float edgePowVal){
		timeToUpdate = true;
		tex1Mod = mod1;
		tex1Offset = new Vector2(tex1OffsetScale.z, tex1OffsetScale.w);
		tex1Scale = new Vector2(tex1OffsetScale.x, tex1OffsetScale.y);
		tex2Mod = mod2;
		tex2Offset = new Vector2(tex2OffsetScale.z, tex2OffsetScale.w);
		tex2Scale = new Vector2(tex2OffsetScale.x, tex2OffsetScale.y);
		colMul = colMulVal;
		colPow = colPowVal;
		edgeSoft = edgeSoftVal;
		edgeMul = edgeMulVal;
		edgePow = edgePowVal;
		currentTex = ActiveTex.Mask;
	}

	private void UpdateValues(){
		timeToUpdate = false;
		quadMat.SetTexture("_MainTex", tex1);
		quadMat.SetTextureOffset("_MainTex", tex1Offset);
		quadMat.SetTextureScale("_MainTex", tex1Scale);
		quadMat.SetTexture("_SecondTex", tex2);
		quadMat.SetTextureOffset("_SecondTex", tex2Offset);
		quadMat.SetTextureScale("_SecondTex", tex2Scale);
		quadMat.SetVector(Shader.PropertyToID("_MainTexMod"), tex1Mod);
		quadMat.SetVector(Shader.PropertyToID("_SecondTexMod"), tex2Mod);
		quadMat.SetFloat(Shader.PropertyToID("_ColMul"), colMul);
		quadMat.SetFloat(Shader.PropertyToID("_ColPow"), colPow);
		switch(currentTex){
			case ActiveTex.Mask:
				quadMat.SetFloat(Shader.PropertyToID("_EdgeSoft"), edgeSoft);
				quadMat.SetFloat(Shader.PropertyToID("_EdgeMul"), edgeMul);
				quadMat.SetFloat(Shader.PropertyToID("_EdgePow"), edgePow);
				quadMat.SetFloat(Shader.PropertyToID("_ShowEdge"), 1);
				break;
			case ActiveTex.Noise:
			default:
				quadMat.SetFloat(Shader.PropertyToID("_ShowEdge"), 0);
				break;
		}
	}


	private void OnGUI() {
		if(timeToUpdate)
			UpdateValues();
		gameObjectEditor.OnPreviewGUI(new Rect (0, 0, position.width,position.height), bgColor);
	}

	private void Update(){
		Repaint();
	}

	private void OnDestroy() {
		isActive = false;
		DestroyImmediate(previewQuad);
		bgColor = null;
		gameObjectEditor = null;

	}


}
