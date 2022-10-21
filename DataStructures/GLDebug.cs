using System.Diagnostics;

namespace DataStructures;


public static class GLDebug
{
	internal const string UseAsserts = "USE_ASSERTS";
	internal const string Debug = "DEBUG";
	
	[Conditional(UseAsserts)]
	public static void Assert(bool condition) => System.Diagnostics.Debug.Assert(condition);
	
	[Conditional(UseAsserts)]
	public static void Assert(bool condition, string message) => System.Diagnostics.Debug.Assert(condition, message);
}
