using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RHierarchy
{
	public class DemoClass : MonoBehaviour
	{
		// serializable just to make editor draw this field
		[System.Serializable]
		public struct SubStructure
		{
			public int ivalue;
			public string strValue;
		}

		// serializable just to make editor draw this field
		[System.Serializable]
		public class SubClass
		{
			public string strValue = "str";
			public List<int> array;
			public Dictionary<string, int> dict = new Dictionary<string, int>();
		}

		public SubStructure subStructure = new SubStructure();
		public SubClass subClass = new SubClass();

		public int intValue = 20;
		public long longValue = (long)int.MaxValue + 1L;
		public float floatValue = 20.1f;
		public double doubleValue = 40.00013;

		public Color color;

		string str1 = "readonly property";
		[RHierarchy.Draw]
		public string ReadonlyProperty
		{
			get { return str1; }
		}

		string str2 = "property can be set";
		[RHierarchy.Draw]
		public string SetProperty
		{
			get { return str2; }
			set { str2 = value; }
		}

		public SubStructure GetSubStructure { get { return subStructure; } set { subStructure = value; } }
		public Texture tex;
		public Sprite sprite;

		public enum AEnum
		{
			e1, e2, e3
		}

		public AEnum e;

		public Material mat;

		/// <summary>
		/// test method you can try in runtime hierarchy
		/// </summary>
		void TestMethod()
		{
			Debug.Log("test method!");
		}

		/// <summary>
		/// test method you can try in runtime hierarchy
		/// </summary>
		public void TestPublicMethod()
		{
			Debug.Log("test public method!");
		}

		/// <summary>
		/// a demo method for RuntimeHierarchy.AddProperty()
		/// </summary>
		public void DemoAddProperty()
		{
			GameObject go = new GameObject();
			var tc = go.AddComponent<DemoClass>();
			var rh = FindObjectOfType<RuntimeHierarchy>();
			rh.AddProperty(go, tc, "intValue");
		}
	}
}