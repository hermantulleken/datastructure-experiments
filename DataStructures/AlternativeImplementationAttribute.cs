using System;

namespace DataStructures
{
	public enum  ExposedFor
	{
		Performance,
		PerformanceMonitoring,
		Testing,
		Debugging
	}
	
	/// <summary>
	/// Marks an alternative implementation of a method that is not used.
	/// </summary>
	[AttributeUsage(AttributeTargets.All)]
	public class AlternativeImplementationAttribute : Attribute
	{
		/// <summary>
		/// What is used instead of the marked target.
		/// </summary>
		public string UsedImplementation { get; set; } 
			
		/// <summary>
		/// Why the marked target is unsuitable. 
		/// </summary>
		public string Comment { get; set; }
		
	}

	[AttributeUsage(AttributeTargets.All)]
	public class PrivateAttribute : Attribute
	{
		public ExposedFor Reason { get; }

		public PrivateAttribute(ExposedFor reason)
		{
			Reason = reason;
		}
	}
}
