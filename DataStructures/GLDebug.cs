using System.Diagnostics;

namespace DataStructures;


public static class GLDebug
{
	internal const string UseAsserts = "USE_ASSERTS";
	
	[Conditional(UseAsserts)]
	public static void Assert(bool condition) => Debug.Assert(condition);
	
	[Conditional(UseAsserts)]
	public static void Assert(bool condition, string message) => Debug.Assert(condition, message);
}
