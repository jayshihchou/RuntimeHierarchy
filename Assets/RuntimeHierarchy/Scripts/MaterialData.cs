using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RHierarchy
{
	/// <summary>
	/// Record datas for draw material viewer.
	/// </summary>
	public class MaterialData : ScriptableObject
	{
		static MaterialData instance;
		/// <summary>
		/// Get material data form resources.
		/// </summary>
		public static MaterialData Instance
		{
			get
			{
				if (instance == null)
				{
					instance = Resources.Load<MaterialData>("MaterialData");
					if (instance != null) instance.OnEnable();
				}
				return instance;
			}
		}

		/// <summary>
		/// Get properties from material.
		/// </summary>
		public PropertyData[] GetPropertyData(Material mat)
		{
			Shader shader = mat.shader;
			if (shader != null)
			{
				if (!propertyMap.ContainsKey(shader.GetInstanceID()))
				{
					List<PropertyData> list = new List<PropertyData>();

					foreach (var prop in propertyDatas)
					{
						if (mat.HasProperty(prop.propertyId))
						{
							list.Add(prop);
						}
					}

					propertyMap[shader.GetInstanceID()] = list.ToArray();
				}
				return propertyMap[shader.GetInstanceID()];
			}
			return null;
		}

		#region ===== Helper Data Structures =====
		/// <summary>
		/// Type of property.
		/// </summary>
		public enum PropertyType
		{
			Color = 0,
			Vector = 1,
			Float = 2,
			Texture = 4
		}

		/// <summary>
		/// Shader property data.
		/// </summary>
		[System.Serializable]
		public struct PropertyData
		{
			/// <summary>
			/// Property name.
			/// </summary>
			public string propertyName;
			/// <summary>
			/// Type of property.
			/// </summary>
			public PropertyType propertyType;
			/// <summary>
			/// Property id.
			/// </summary>
			[System.NonSerialized] public int propertyId;
		}

		/// <summary>
		/// Helper class contains unity built-in shader properties.
		/// </summary>
		[System.Serializable]
		public class BuiltinProperties
		{
			public const string jsonData = "{\"propertyDatas\":[{\"propertyName\":\"_MainTex\",\"propertyType\":4},{\"propertyName\":\"_Color\",\"propertyType\":0},{" +
				"\"propertyName\":\"_SrcBlend\",\"propertyType\":2},{\"propertyName\":\"_DstBlend\",\"propertyType\":2},{\"propertyName\":\"_ZWrite\",\"propertyType\":2},{\"p" +
				"ropertyName\":\"_ZTest\",\"propertyType\":2},{\"propertyName\":\"_Cull\",\"propertyType\":2},{\"propertyName\":\"_ZBias\",\"propertyType\":2},{\"propertyName" +
				"\":\"_BumpMap\",\"propertyType\":4},{\"propertyName\":\"_SpecColor\",\"propertyType\":0},{\"propertyName\":\"_Shininess\",\"propertyType\":2},{\"propertyName" +
				"\":\"_Parallax\",\"propertyType\":2},{\"propertyName\":\"_ParallaxMap\",\"propertyType\":4},{\"propertyName\":\"_Emission\",\"propertyType\":0},{\"propertyNa" +
				"me\":\"_Cutoff\",\"propertyType\":2},{\"propertyName\":\"_DecalTex\",\"propertyType\":4},{\"propertyName\":\"_Illum\",\"propertyType\":4},{\"propertyName\":" +
				"\"_LightTexture0\",\"propertyType\":4},{\"propertyName\":\"_LightTextureB0\",\"propertyType\":4},{\"propertyName\":\"_ShadowMapTexture\",\"propertyType\":4}," +
				"{\"propertyName\":\"_LightMap\",\"propertyType\":4},{\"propertyName\":\"_Detail\",\"propertyType\":4},{\"propertyName\":\"_TintColor\",\"propertyType\":0},{" +
				"\"propertyName\":\"_InvFade\",\"propertyType\":2},{\"propertyName\":\"_EmisColor\",\"propertyType\":0},{\"propertyName\":\"_ReflectColor\",\"propertyType\":0" +
				"},{\"propertyName\":\"_Cube\",\"propertyType\":4},{\"propertyName\":\"_Tint\",\"propertyType\":0},{\"propertyName\":\"_Exposure\",\"propertyType\":2},{\"prop" +
				"ertyName\":\"_Rotation\",\"propertyType\":2},{\"propertyName\":\"_Tex\",\"propertyType\":4},{\"propertyName\":\"_SunDisk\",\"propertyType\":2},{\"propertyNam" +
				"e\":\"_SunSize\",\"propertyType\":2},{\"propertyName\":\"_AtmosphereThickness\",\"propertyType\":2},{\"propertyName\":\"_SkyTint\",\"propertyType\":0},{\"pro" +
				"pertyName\":\"_GroundColor\",\"propertyType\":0},{\"propertyName\":\"_FrontTex\",\"propertyType\":4},{\"propertyName\":\"_BackTex\",\"propertyType\":4},{\"pr" +
				"opertyName\":\"_LeftTex\",\"propertyType\":4},{\"propertyName\":\"_RightTex\",\"propertyType\":4},{\"propertyName\":\"_UpTex\",\"propertyType\":4},{\"propert" +
				"yName\":\"_DownTex\",\"propertyType\":4},{\"propertyName\":\"PixelSnap\",\"propertyType\":2},{\"propertyName\":\"_RendererColor\",\"propertyType\":0},{\"prop" +
				"ertyName\":\"_Flip\",\"propertyType\":1},{\"propertyName\":\"_AlphaTex\",\"propertyType\":4},{\"propertyName\":\"_EnableExternalAlpha\",\"propertyType\":2},{" +
				"\"propertyName\":\"_Glossiness\",\"propertyType\":2},{\"propertyName\":\"_GlossMapScale\",\"propertyType\":2},{\"propertyName\":\"_SmoothnessTextureC" +
				"hannel\",\"propertyType\":2},{\"propertyName\":\"_Metallic\",\"propertyType\":2},{\"propertyName\":\"_MetallicGlossMap\",\"propertyType\":4},{\"propertyName" +
				"\":\"_SpecularHighlights\",\"propertyType\":2},{\"propertyName\":\"_GlossyReflections\",\"propertyType\":2},{\"propertyName\":\"_BumpScale\",\"propertyType\"" +
				":2},{\"propertyName\":\"_OcclusionStrength\",\"propertyType\":2},{\"propertyName\":\"_OcclusionMap\",\"propertyType\":4},{\"propertyName\":\"_Emissio" +
				"nColor\",\"propertyType\":0},{\"propertyName\":\"_EmissionMap\",\"propertyType\":4},{\"propertyName\":\"_DetailMask\",\"propertyType\":4},{\"propertyName\":\"" +
				"_DetailAlbedoMap\",\"propertyType\":4},{\"propertyName\":\"_DetailNormalMapScale\",\"propertyType\":2},{\"propertyName\":\"_DetailNormalMap\",\"propertyType\"" +
				":4},{\"propertyName\":\"_UVSec\",\"propertyType\":2},{\"propertyName\":\"_Mode\",\"propertyType\":2},{\"propertyName\":\"_SecondTex\",\"propertyType\":4},{\"" +
				"propertyName\":\"_ThirdTex\",\"propertyType\":4},{\"propertyName\":\"_TexA\",\"propertyType\":4},{\"propertyName\":\"_TexB\",\"propertyType\":4},{\"propertyN" +
				"ame\":\"_value\",\"propertyType\":2},{\"propertyName\":\"_Texel\",\"propertyType\":2},{\"propertyName\":\"_Level\",\"propertyType\":2},{\"propertyName\":\"_S" +
				"cale\",\"propertyType\":2},{\"propertyName\":\"_LightTexture\",\"propertyType\":4},{\"propertyName\":\"_HueVariation\",\"propertyType\":0},{\"propertyName\":" +
				"\"_DetailTex\",\"propertyType\":4},{\"propertyName\":\"_WindQuality\",\"propertyType\":2},{\"propertyName\":\"_BaseLight\",\"propertyType\":2},{\"propertyNam" +
				"e\":\"_AO\",\"propertyType\":2},{\"propertyName\":\"_TreeInstanceColor\",\"propertyType\":1},{\"propertyName\":\"_TreeInstanceScale\",\"propertyType\":1},{\"" +
				"propertyName\":\"_SquashAmount\",\"propertyType\":2},{\"propertyName\":\"_Occlusion\",\"propertyType\":2},{\"propertyName\":\"_HalfOverCutoff\",\"typ" +
				"e\":2},{\"propertyName\":\"_GlossMap\",\"propertyType\":4},{\"propertyName\":\"_BumpSpecMap\",\"propertyType\":4},{\"propertyName\":\"_TranslucencyMa" +
				"p\",\"propertyType\":4},{\"propertyName\":\"_ShadowOffset\",\"propertyType\":4},{\"propertyName\":\"_TranslucencyColor\",\"propertyType\":0},{\"propertyName\"" +
				":\"_TranslucencyViewDependency\",\"propertyType\":2},{\"propertyName\":\"_ShadowStrength\",\"propertyType\":2},{\"propertyName\":\"_ShadowTex\",\"typ" +
				"e\":4},{\"propertyName\":\"_ShadowOffsetScale\",\"propertyType\":2},{\"propertyName\":\"_WavingTint\",\"propertyType\":0},{\"propertyName\":\"_WaveAn" +
				"dDistance\",\"propertyType\":1},{\"propertyName\":\"_Control\",\"propertyType\":4},{\"propertyName\":\"_Splat3\",\"propertyType\":4},{\"propertyName\":\"_Spl" +
				"at2\",\"propertyType\":4},{\"propertyName\":\"_Splat1\",\"propertyType\":4},{\"propertyName\":\"_Splat0\",\"propertyType\":4},{\"propertyName\":\"_Normal3\"," +
				"\"propertyType\":4},{\"propertyName\":\"_Normal2\",\"propertyType\":4},{\"propertyName\":\"_Normal1\",\"propertyType\":4},{\"propertyName\":\"_Normal0\",\"ty" +
				"pe\":4},{\"propertyName\":\"_Metallic0\",\"propertyType\":2},{\"propertyName\":\"_Metallic1\",\"propertyType\":2},{\"propertyName\":\"_Metallic2\",\"" +
				"propertyType\":2},{\"propertyName\":\"_Metallic3\",\"propertyType\":2},{\"propertyName\":\"_Smoothness0\",\"propertyType\":2},{\"propertyName\":\"_Smoothness" +
				"1\",\"propertyType\":2},{\"propertyName\":\"_Smoothness2\",\"propertyType\":2},{\"propertyName\":\"_Smoothness3\",\"propertyType\":2},{\"propertyName\":\"_Me" +
				"tallicTex\",\"propertyType\":4},{\"propertyName\":\"_StencilComp\",\"propertyType\":2},{\"propertyName\":\"_Stencil\",\"propertyType\":2},{\"propertyName\":\"" +
				"_StencilOp\",\"propertyType\":2},{\"propertyName\":\"_StencilWriteMask\",\"propertyType\":2},{\"propertyName\":\"_StencilReadMask\",\"propertyType\":2},{\"pr" +
				"opertyName\":\"_ColorMask\",\"propertyType\":2},{\"propertyName\":\"_UseUIAlphaClip\",\"propertyType\":2},{\"propertyName\":\"_Specular\",\"propertyType\":0}" +
				",{\"propertyName\":\"_MainBump\",\"propertyType\":4},{\"propertyName\":\"_DetailBump\",\"propertyType\":4},{\"propertyName\":\"_Strength\",\"propertyType\":2" +
				"},{\"propertyName\":\"_Mask\",\"propertyType\":4},{\"propertyName\":\"_Focus\",\"propertyType\":2},{\"propertyName\":\"_WireThickness\",\"propertyType\":2}]}";
			public PropertyData[] propertyDatas;
		}

		/// <summary>
		/// Build in shaders.
		/// </summary>
		[System.Serializable]
		public class BuiltinShaders
		{
			public string[] shaderNames;
			public const string jsonData = "{\"shaderNames\":[\"GUI/Text Shader\",\"Hidden/InternalClear\"," +
				"\"Hidden/Internal-Colored\",\"Hidden/InternalErrorShader\",\"Hidden/FrameDebuggerRenderTargetDisplay\"," +
				"\"Legacy Shaders/Transparent/Bumped Diffuse\",\"Legacy Shaders/Transparent/Bumped Specular\"," +
				"\"Legacy Shaders/Transparent/Diffuse\",\"Legacy Shaders/Transparent/Specular\",\"Legacy Shaders/Transparent/Parallax Diffuse\"," +
				"\"Legacy Shaders/Transparent/Parallax Specular\",\"Legacy Shaders/Transparent/VertexLit\"," +
				"\"Legacy Shaders/Transparent/Cutout/Bumped Diffuse\",\"Legacy Shaders/Transparent/Cutout/Bumped Specular\"," +
				"\"Legacy Shaders/Transparent/Cutout/Diffuse\",\"Legacy Shaders/Transparent/Cutout/Specular\"," +
				"\"Legacy Shaders/Transparent/Cutout/Soft Edge Unlit\",\"Legacy Shaders/Transparent/Cutout/VertexLit\"," +
				"\"Hidden/Compositing\",\"Legacy Shaders/Decal\",\"FX/Flare\",\"Legacy Shaders/Self-Illumin/Bumped Diffuse\"," +
				"\"Legacy Shaders/Self-Illumin/Bumped Specular\",\"Legacy Shaders/Self-Illumin/Diffuse\"," +
				"\"Legacy Shaders/Self-Illumin/Specular\",\"Legacy Shaders/Self-Illumin/Parallax Diffuse\"," +
				"\"Legacy Shaders/Self-Illumin/Parallax Specular\",\"Legacy Shaders/Self-Illumin/VertexLit\"," +
				"\"Hidden/BlitCopy\",\"Hidden/BlitCopyDepth\",\"Hidden/BlitCopyWithDepth\",\"Hidden/BlitToDepth\"," +
				"\"Hidden/BlitToDepth_MSAA\",\"Hidden/Internal-CombineDepthNormals\",\"Hidden/ConvertTexture\"," +
				"\"Hidden/Internal-CubemapToEquirect\",\"Hidden/Internal-DeferredReflections\",\"Hidden/Internal-DeferredShading\"," +
				"\"Hidden/Internal-DepthNormalsTexture\",\"Hidden/Internal-Flare\",\"Hidden/Internal-GUIRoundedRect\"," +
				"\"Hidden/Internal-GUITexture\",\"Hidden/Internal-GUITextureBlit\",\"Hidden/Internal-GUITextureClip\"," +
				"\"Hidden/Internal-GUITextureClipText\",\"Hidden/Internal-Halo\",\"Hidden/Internal-MotionVectors\"," +
				"\"Hidden/Internal-ODSWorldTexture\",\"Hidden/Internal-PrePassLighting\",\"Hidden/Internal-ScreenSpaceShadows\"," +
				"\"Hidden/Internal-StencilWrite\",\"Legacy Shaders/Lightmapped/Bumped Diffuse\",\"Legacy Shaders/Lightmapped/Bumped Specular\"," +
				"\"Legacy Shaders/Lightmapped/Diffuse\",\"Legacy Shaders/Lightmapped/Specular\",\"Legacy Shaders/Lightmapped/VertexLit\"," +
				"\"Legacy Shaders/Bumped Diffuse\",\"Legacy Shaders/Bumped Specular\",\"Legacy Shaders/Diffuse\",\"Legacy Shaders/Diffuse Detail\"," +
				"\"Legacy Shaders/Diffuse Fast\",\"Legacy Shaders/Specular\",\"Legacy Shaders/Parallax Diffuse\",\"Legacy Shaders/Parallax Specular\"," +
				"\"Legacy Shaders/VertexLit\",\"Particles/Additive\",\"Particles/~Additive-Multiply\",\"Particles/Additive(Soft)\"," +
				"\"Particles/Alpha Blended\",\"Particles/Anim Alpha Blended\",\"Particles/Blend\",\"Particles/Multiply\"," +
				"\"Particles/Multiply(Double)\",\"Particles/Alpha Blended Premultiply\",\"Particles/Standard Surface\"," +
				"\"Particles/Standard Unlit\",\"Particles/VertexLit Blended\",\"Legacy Shaders/Reflective/Bumped Diffuse\"," +
				"\"Legacy Shaders/Reflective/Bumped Unlit\",\"Legacy Shaders/Reflective/Bumped Specular\"," +
				"\"Legacy Shaders/Reflective/Bumped VertexLit\",\"Legacy Shaders/Reflective/Diffuse\",\"Legacy Shaders/Reflective/Specular\"," +
				"\"Legacy Shaders/Reflective/Parallax Diffuse\",\"Legacy Shaders/Reflective/Parallax Specular\"," +
				"\"Legacy Shaders/Reflective/VertexLit\",\"Skybox/Cubemap\",\"Skybox/Panoramic\",\"Skybox/Procedural\"," +
				"\"Skybox/6 Sided\",\"Sprites/Default\",\"Sprites/Diffuse\",\"Sprites/Mask\",\"Standard\",\"Standard (Roughness setup)\"," +
				"\"Standard (Specular setup)\",\"Hidden/VideoDecode\",\"Hidden/VideoDecodeAndroid\",\"Hidden/VideoDecodeOSX\"," +
				"\"AR/TangoARRender\",\"Hidden/CubeBlend\",\"Hidden/CubeBlur\",\"Hidden/CubeBlurOdd\",\"Hidden/CubeCopy\"," +
				"\"Hidden/GIDebug/ShowLightMask\",\"Hidden/GIDebug/TextureUV\",\"Hidden/GIDebug/UV1sAsPositions\"," +
				"\"Hidden/GIDebug/VertexColors\",\"Mobile/Bumped Diffuse\",\"Mobile/Bumped Specular(1 Directional Light)\"," +
				"\"Mobile/Bumped Specular\",\"Mobile/Diffuse\",\"Mobile/Unlit(Supports Lightmap)\",\"Mobile/Particles/Additive\"," +
				"\"Mobile/Particles/VertexLit Blended\",\"Mobile/Particles/Alpha Blended\",\"Mobile/Particles/Multiply\"," +
				"\"Mobile/Skybox\",\"Mobile/VertexLit(Only Directional Lights)\",\"Mobile/VertexLit\",\"Nature/SpeedTree\"," +
				"\"Nature/SpeedTree Billboard\",\"Nature/Tree Soft Occlusion Bark\",\"Hidden/Nature/Tree Soft Occlusion Bark Rendertex\"," +
				"\"Nature/Tree Soft Occlusion Leaves\",\"Hidden/Nature/Tree Soft Occlusion Leaves Rendertex\"," +
				"\"Hidden/Nature/Tree Creator Albedo Rendertex\",\"Nature/Tree Creator Bark\",\"Hidden/Nature/Tree Creator Bark Optimized\"," +
				"\"Hidden/Nature/Tree Creator Bark Rendertex\",\"Nature/Tree Creator Leaves\",\"Nature/Tree Creator Leaves Fast\"," +
				"\"Hidden/Nature/Tree Creator Leaves Fast Optimized\",\"Hidden/Nature/Tree Creator Leaves Optimized\"," +
				"\"Hidden/Nature/Tree Creator Leaves Rendertex\",\"Hidden/Nature/Tree Creator Normal Rendertex\"," +
				"\"Hidden/TerrainEngine/Details/Vertexlit\",\"Hidden/TerrainEngine/Details/WavingDoublePass\"," +
				"\"Hidden/TerrainEngine/Details/BillboardWavingDoublePass\",\"Hidden/TerrainEngine/Splatmap/Diffuse-AddPass\"," +
				"\"Nature/Terrain/Diffuse\",\"Hidden/TerrainEngine/Splatmap/Specular-AddPass\",\"Hidden/TerrainEngine/Splatmap/Specular-Base\"," +
				"\"Nature/Terrain/Specular\",\"Hidden/TerrainEngine/Splatmap/Standard-AddPass\",\"Hidden/TerrainEngine/Splatmap/Standard-Base\"," +
				"\"Nature/Terrain/Standard\",\"Hidden/TerrainEngine/BillboardTree\",\"Hidden/TerrainEngine/CameraFacingBillboardTree\"," +
				"\"Hidden/UI/CompositeOverdraw\",\"UI/Default\",\"UI/DefaultETC1\",\"UI/Default Font\",\"UI/Lit/Bumped\",\"UI/Lit/Detail\"," +
				"\"UI/Lit/Refraction\",\"UI/Lit/Refraction Detail\",\"UI/Lit/Transparent\",\"Hidden/UI/Overdraw\",\"UI/Unlit/Detail\"," +
				"\"UI/Unlit/Text\",\"UI/Unlit/Text Detail\",\"UI/Unlit/Transparent\",\"Unlit/Transparent\",\"Unlit/Transparent Cutout\"," +
				"\"Unlit/Color\",\"Unlit/Texture\",\"Hidden/VR/BlitTexArraySlice\",\"Hidden/VR/BlitTexArraySliceToDepth\"," +
				"\"Hidden/VR/BlitTexArraySliceToDepth_MSAA\",\"Hidden/VR/ClippingMask\",\"Hidden/VR/Internal-VRDistortion\"," +
				"\"VR/SpatialMapping/Occlusion\",\"VR/SpatialMapping/Wireframe\",\"Hidden/VR/VideoBackground\"]}";
		}
		#endregion
		#region ===== Parameters =====
		/// <summary>
		/// Runtime property map.
		/// </summary>
		Dictionary<int, PropertyData[]> propertyMap = new Dictionary<int, PropertyData[]>();

		/// <summary>
		/// Saved property datas.
		/// </summary>
		PropertyData[] propertyDatas;

		string[] shaderNames;
		public string[] ShaderNames { get { return shaderNames; } }
		#endregion
		#region ===== Helper Methods =====
		private void OnEnable()
		{
			if (propertyDatas == null || propertyDatas.Length == 0)
			{
				GetBuiltinProperties();
			}

			if (shaderNames == null || shaderNames.Length == 0)
			{
				GetBuildinShaders();
			}

			if (Application.isPlaying)
			{
				GetPropertiesIds();
			}
		}

		/// <summary>
		/// Refind built-in properties.
		/// </summary>
		public void GetBuiltinProperties()
		{
			BuiltinProperties builtin = JsonUtility.FromJson<BuiltinProperties>(BuiltinProperties.jsonData);
			propertyDatas = builtin.propertyDatas;
		}

		/// <summary>
		/// Refind build-in shaders.
		/// </summary>
		public void GetBuildinShaders()
		{
			shaderNames = null;
			BuiltinShaders builtin = JsonUtility.FromJson<BuiltinShaders>(BuiltinShaders.jsonData);
			List<string> shaders = new List<string>();
			foreach (var s in builtin.shaderNames)
			{
				if (Shader.Find(s) != null)
				{
					shaders.Add(s);
				}
				//else
				//{
				//	Debug.Log("cannot find " + s);
				//}
			}
			shaderNames = shaders.ToArray();
		}

		void GetPropertiesIds()
		{
			for (int i = propertyDatas.Length - 1; i >= 0; --i)
			{
				propertyDatas[i].propertyId = Shader.PropertyToID(propertyDatas[i].propertyName);
			}
		}

		void ParseShaderNameInEditor(string path, HashSet<string> results)
		{
			string line;
			StreamReader file = new StreamReader(path);
			while ((line = file.ReadLine()) != null)
			{
				if (line.Trim().ToLower().StartsWith("shader"))
				{
					int st = line.IndexOf("\"") + 1;
					int ed = line.LastIndexOf("\"");
					results.Add(line.Substring(st, ed - st));
					break;
				}
			}
		}

		void GetShaderNamesRecursively(string path, HashSet<string> results)
		{
			if (File.Exists(path))
			{
				if (Path.GetExtension(path) == ".shader")
				{
					ParseShaderNameInEditor(path, results);
				}
			}
			else if (Directory.Exists(path))
			{
				foreach (var dir in Directory.GetFiles(path))
				{
					GetShaderNamesRecursively(dir, results);
				}
				foreach (var dir in Directory.GetDirectories(path))
				{
					GetShaderNamesRecursively(dir, results);
				}
			}
		}

		bool ContainsProperty(List<PropertyData> list, string property)
		{
			for (int i = list.Count - 1; i >= 0; --i)
			{
				if (list[i].propertyName == property)
					return true;
			}
			return false;
		}
		#endregion
#if UNITY_EDITOR
		#region ===== Editor Methods =====
		/// <summary>
		/// If set to true, MaterialData will automatically call Editor_GetAllShaderPropertys() when building player.
		/// </summary>
		[Header("If set to true, MaterialData will automatically")]
		[Header("call Editor_GetAllShaderPropertys() when building player.")]
		public bool AutoCollectShaderPropertyWhenBuild = false;

		public void Editor_GetAllShaderPropertys()
		{
			propertyDatas = null;
			GetBuiltinProperties();
			shaderNames = null;
			GetBuildinShaders();

			HashSet<string> shaderList = new HashSet<string>();
			HashSet<string> customShaderNameList = new HashSet<string>(shaderNames);
			GetShaderNamesRecursively(Application.dataPath, shaderList);
			List<PropertyData> customPropertyList = new List<PropertyData>(propertyDatas);
			foreach (var shaderName in shaderList)
			{
				Shader shader = Shader.Find(shaderName);
				if (shader != null)
				{
					customShaderNameList.Add(shaderName);
					int count = UnityEditor.ShaderUtil.GetPropertyCount(shader);
					for (int i = 0; i < count; ++i)
					{
						var pname = UnityEditor.ShaderUtil.GetPropertyName(shader, i);
						if (!ContainsProperty(customPropertyList, pname))
						{
							PropertyData propertyData = new PropertyData() { propertyName = pname };
							var type = UnityEditor.ShaderUtil.GetPropertyType(shader, i);
							switch (type)
							{
								case UnityEditor.ShaderUtil.ShaderPropertyType.Color:
									propertyData.propertyType = PropertyType.Color;
									break;
								case UnityEditor.ShaderUtil.ShaderPropertyType.Float:
								case UnityEditor.ShaderUtil.ShaderPropertyType.Range:
									propertyData.propertyType = PropertyType.Float;
									break;
								case UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv:
									propertyData.propertyType = PropertyType.Texture;
									break;
								case UnityEditor.ShaderUtil.ShaderPropertyType.Vector:
									propertyData.propertyType = PropertyType.Vector;
									break;
							}
							customPropertyList.Add(propertyData);
						}
					}
				}
			}
			shaderNames = new string[customShaderNameList.Count];
			customShaderNameList.CopyTo(shaderNames);

			propertyDatas = customPropertyList.ToArray();
			Debug.Log("MaterialData: properties collected!");
		}
		#endregion
#endif
	}
}