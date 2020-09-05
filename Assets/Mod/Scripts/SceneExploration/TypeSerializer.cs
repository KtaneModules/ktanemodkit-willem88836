using System;
using System.Reflection;
using UnityEngine;

public class TypeSerializer : MonoBehaviour
{
	public string ModuleName;
	public string[] Types;

	public void Start()
	{
		foreach(string t in Types)
		{
			try
			{
				Type type = Type.GetType(t);


				Debug.LogWarningFormat(@"[{0}] fields", ModuleName);
				FieldInfo[] fieldInfos = type.GetFields();
				Print(fieldInfos);

				Debug.LogWarningFormat(@"[{0}] methods", ModuleName);
				MethodInfo[] methodInfos = type.GetMethods();
				Print(methodInfos);
				
				Debug.LogWarningFormat(@"[{0}] properties", ModuleName);
				PropertyInfo[] propertyInfos = type.GetProperties();
				Print(propertyInfos);
				
			}
			catch(Exception ex)
			{
				Debug.LogErrorFormat(@"[{0}] {1}", ModuleName, ex.Message);
			}

		}
	}

	private void Print<T> (T[] array)
	{
		foreach(T e in array)
		{
			Debug.LogFormat(@"[{0}] {1}", ModuleName, e.ToString());
		}
	}
}
