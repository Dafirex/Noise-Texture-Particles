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
public class NoiseTexturePreviewRGBA : EditorWindow {

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
	static Texture2D tex;
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

	static bool useTex1;
	static bool useTex2;

	private void Awake() {
		previewQuad = GameObject.CreatePrimitive(PrimitiveType.Cube);
		previewQuad.transform.position = new Vector3(1000000000, 1000000000, 1000000000);
		previewQuad.name = "Texture Preview";
		quadMat = new Material(Shader.Find("Hidden/Dafirex/NoiseTexPreviewRGBA"));
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

	public static void SetTex(Texture2D texture, bool tex1, bool tex2){
		tex = texture;
		useTex1 = tex1;
		useTex2 = tex2;
		timeToUpdate = true;
	}


	public static void SendMaterialValues(Vector4 mod1, Vector4 tex1OffsetScale, Vector4 mod2, Vector4 tex2OffsetScale, float colMulVal, float colPowVal){
		timeToUpdate = true;
		tex1Mod = mod1;
		tex1Offset = new Vector2(tex1OffsetScale.x, tex1OffsetScale.y);
		tex1Scale = new Vector2(tex1OffsetScale.z, tex1OffsetScale.w);
		tex2Mod = mod2;
		tex2Offset = new Vector2(tex2OffsetScale.x, tex2OffsetScale.y);
		tex2Scale = new Vector2(tex2OffsetScale.z, tex2OffsetScale.w);
		colMul = colMulVal;
		colPow = colPowVal;
		currentTex = ActiveTex.Noise;
	}
	public static void SendMaterialValues(Vector4 mod1, Vector4 tex1OffsetScale, Vector4 mod2, Vector4 tex2OffsetScale, float colMulVal, float colPowVal, float edgeSoftVal, float edgeMulVal, float edgePowVal){
		timeToUpdate = true;
		tex1Mod = mod1;
		tex1Offset = new Vector2(tex1OffsetScale.x, tex1OffsetScale.y);
		tex1Scale = new Vector2(tex1OffsetScale.z, tex1OffsetScale.w);
		tex2Mod = mod2;
		tex2Offset = new Vector2(tex2OffsetScale.x, tex2OffsetScale.y);
		tex2Scale = new Vector2(tex2OffsetScale.z, tex2OffsetScale.w);
		colMul = colMulVal;
		colPow = colPowVal;
		edgeSoft = edgeSoftVal;
		edgeMul = edgeMulVal;
		edgePow = edgePowVal;
		currentTex = ActiveTex.Mask;
	}

	private void UpdateValues(){
		timeToUpdate = false;
		quadMat.SetTexture("_MainTex", tex);
		quadMat.SetFloat("_UseTex1", BoolToFloat(useTex1));
		quadMat.SetFloat("_UseTex2", BoolToFloat(useTex2));

		Vector4 texOffsetScale1 = new Vector4(tex1Offset.x, tex1Offset.y, tex1Scale.x, tex1Scale.y);
		Vector4 texOffsetScale2 = new Vector4(tex2Offset.x, tex2Offset.y, tex2Scale.x, tex2Scale.y);
		quadMat.SetVector("_TextureOffsetScale", texOffsetScale1);
		quadMat.SetVector("_Texture2OffsetScale", texOffsetScale2);
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
				quadMat.SetFloat(Shader.PropertyToID("_Mask"), 0);
				break;
			case ActiveTex.Noise:
			default:
				quadMat.SetFloat(Shader.PropertyToID("_ShowEdge"), 0);
				quadMat.SetFloat(Shader.PropertyToID("_Mask"), 1);
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
	private static float BoolToFloat(bool x){
		return x == true ? 1 : 0;
	}


}
