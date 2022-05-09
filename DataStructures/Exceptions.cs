using System;

namespace DataStructures;

public static class Exceptions
{
	public static NotImplementedException TypeCaseNotImplemented<T>(T type, string variableName)
		=> new NotImplementedException($"The case type {type.GetType()} of variable {variableName} is not implemented.");
	
	public static NotImplementedException TypeCaseNotImplemented(Type type, string variableName)
		=> new NotImplementedException($"The case type {type} of variable {variableName} is not implemented.");
	
	public static NotImplementedException CaseNotImplemented<T>(T value, string variableName)
		=> new NotImplementedException($"The case {value} of variable {variableName} is not implemented.");
}
