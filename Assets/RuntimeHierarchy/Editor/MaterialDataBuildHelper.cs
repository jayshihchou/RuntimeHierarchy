using UnityEngine;

namespace RHierarchy
{
#if UNITY_2018_1_OR_NEWER
	public class MaterialDataBuildHelper : UnityEditor.Build.IPreprocessBuildWithReport
#else
	public class MaterialDataBuildHelper : UnityEditor.Build.IPreprocessBuild
#endif
	{
		public int callbackOrder { get { return 0; } }
#if UNITY_2018_1_OR_NEWER
		public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
#else
		public void OnPreprocessBuild(UnityEditor.BuildTarget target, string path)
#endif
		{
			if (MaterialData.Instance != null && MaterialData.Instance.AutoCollectShaderPropertyWhenBuild)
			{
				MaterialData.Instance.Editor_GetAllShaderPropertys();
			}
		}
	}
}