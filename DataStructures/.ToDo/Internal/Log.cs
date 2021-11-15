using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = System.Object;

// ReSharper disable InvocationIsSkipped
// ReSharper disable RedundantAssignment

namespace SimuladorCP
{
	/// <summary>
	/// A class useful for debug logging. 
	/// </summary>
	/// <remarks>For these methods to be generated
	/// the script define <c>DEBUG_LOG</c> must be set in the player settings.</remarks>
	public static class Log
	{
		private const string DebugDefine = "DEBUG_LOG";
		
		#region Colors
		private const string Purple = "#9933ff";
		private const string Green = "#33ff00";
		private const string Pink = "#ff3366";
		private const string White = "white";
		private const string Blue = "#3366ff";
		private const string Orange = "#ff9900";
		private const string BlueGreen = "#3399ff";
		#endregion

		private static readonly Dictionary<string, int> SequenceTracking = new Dictionary<string, int>();

		/// <summary>
		/// Logs a string with an label, essentially <c>Label: message</c>.
		/// </summary>
		/// <param name="label">The label to log.</param>
		/// <param name="message">The message to log.</param>
		/// <param name="color">The color to use for the message, as one of the color
		/// constants Unity supports, such as <c>red</c>, or a CSS color string such as <c>#ff0000</c>.</param>
		[Conditional(DebugDefine)]
		public static void LabelledInfo(string label, object message, UnityEngine.Object context=null, string color=null)
		{
			if (Application.isEditor && color != null)
			{
				Debug.Log($"<color={color}>{label}: {message}</color>", context);	
			}
			else
			{
				Debug.Log($"{label}: {message}", context);
			}
		}

		/// <summary>
		/// Logs the name of the method from where this method is called. 
		/// </summary>
		/// <example> In the snippet below, "MyMethod" is logged.
		/// <![CDATA[
		/// public void MyMethod()
		/// {
		///		Log.Method();
		/// }
		/// ]]>
		/// </example>
		[Conditional(DebugDefine)]
		public static void Method(UnityEngine.Object context = null)
		{
			var methodName = GetMethodName();

			LabelledInfo("Method", methodName, context, Purple);
		}
		
		/// <summary>
		/// Logs an information message.
		/// </summary>
		[Conditional(DebugDefine)]
		public static void Info(object message, UnityEngine.Object context = null)
		{
			LabelledInfo("Info", message, context, White);
		}

		/// <summary>
		/// Logs an warning.
		/// </summary>
		[Conditional(DebugDefine)]
		public static void Warning(object message)
		{
			Debug.LogWarning("Warn: " + message);
		}

		/// <summary>
		/// Logs an error.
		/// </summary>
		[Conditional(DebugDefine)]
		public static void Error(object message)
		{
			Debug.LogError("Error: " + message);
		}

		/// <summary>
		/// Logs an assertion failure.
		/// </summary>
		[Conditional(DebugDefine)]
		public static void Assertion(object message)
		{
			Debug.LogAssertion("Assertion: " + message);
		}

		/// <summary>
		/// Logs variable and its value.
		/// </summary>
		[Conditional(DebugDefine)]
		[PublicAPI]
		public static void Value<T>(string name, T value, UnityEngine.Object context = null)
		{
			if (value == null)
			{
				LabelledInfo($"Value of {name}", "null", context, Pink);
			}
			else
			{
				LabelledInfo($"Value of {name} <{value.GetType()}>", value.ToString(), context, Pink);
			}
		}

		/// <summary>
		/// Logs a list variable and its contents.
		/// </summary>
		[Conditional(DebugDefine)]
		public static void List<T>(string name, IEnumerable<T> list)
		{
			var values = string.Join(", ", list.Select(v => v.ToString()));
			Value(name, $"[{values}]");
		}

		/// <summary>
		/// Logs the name of the current scene (as reported by the SceneManager).
		/// </summary>
		[Conditional(DebugDefine)]
		public static void CurrentScene(UnityEngine.Object context = null)
		{
			var sceneName = SceneManager.GetActiveScene().name;
			LabelledInfo("Scene", sceneName, context, Green);
		}

		[Conditional(DebugDefine)]
		[PublicAPI]
		public static void Sequence(string name, UnityEngine.Object context = null)
		{
			if (!SequenceTracking.ContainsKey(name))
			{
				SequenceTracking[name] = 0;
			}

			LabelledInfo(name, SequenceTracking[name], context, Blue);
		}

		[Conditional(DebugDefine)]
		[PublicAPI]
		public static void ResetSequence(string name)
		{
			SequenceTracking[name] = 0;
		}

		[Conditional(DebugDefine)]
		public static void MethodSequence()
		{
			var methodName = GetMethodName();
			Sequence(methodName);
		}

		[Conditional(DebugDefine)]
		public static void ResetMethodSequence()
		{
			var methodName = GetMethodName();
			ResetSequence(methodName);
		}

		[Conditional(DebugDefine)]
		public static void IsActive(GameObject go)
		{
			LabelledInfo($"{go.name} is active", go.activeInHierarchy, go, Orange);
		}

		[Conditional(DebugDefine)]
		public static void IsNull(object obj, string name, UnityEngine.Object context = null)
		{
			LabelledInfo($"{name} is null", obj == null, context, Orange);
		}

		[Conditional(DebugDefine)]
		public static void LogType(object obj, string name, UnityEngine.Object context = null)
		{
			LabelledInfo($"{name} has type", obj.GetType(), context, Pink);
		}

		[Conditional(DebugDefine)]
		public static void Stack()
		{
			
			var stackTrace = new StackTrace();

			foreach (var frame in stackTrace.GetFrames().Skip(1))
			{
				if (Application.isEditor)
				{
					Debug.Log($"<color={BlueGreen}>{frame}</color>");
				}
			}
		}

		/// <summary>
		/// Prints an error message that the thing with the given name is not in
		/// any of the active scenes. 
		/// </summary>
		/// <remarks>The list of scenes is also mentioned in the message.</remarks>
		[Conditional(DebugDefine)]
		public static void NotInScenes(string name)
		{
			Log.Error($"No {name} in scenes ({GetActiveSceneNames()}).");
		}

		/// <summary>
		/// Gets a string representation of the list of active scenes. 
		/// </summary>
		/// <returns></returns>
		public static string GetActiveSceneNames()
		{
			int sceneCount = SceneManager.sceneCount;
			var scenes = new string[sceneCount];

			for (int i = 0; i < sceneCount; i++)
			{
				scenes[i] = SceneManager.GetSceneAt(i).name;
			}

			return string.Join(", ", scenes);
		}

		/*
			This magical method is used to get the name of a method from which it is called, \
			so that we can log it. To do this, we examine the third (at index 2) frame of the callstack.
			The first two frames are ignored; the first one represents this method, and the next 
			represents another (public) method in this class that is called by the user. 
			
			It will break if there are more methods in the chain.   
		 */
		private static string GetMethodName()
		{
			var stackTrace = new StackTrace();
			var frame = stackTrace.GetFrame(2);
			var method = frame.GetMethod();
			var type = method.DeclaringType;
			var methodName = $"{type.FullName}.{method.Name} in {frame.GetFileName()}[{frame.GetFileLineNumber()}]";

			return methodName;
		}

		public static void AssertUnreachable()
		{
			Assertion($"Should not be reachable in {GetMethodName()}.");
		}
	}
}