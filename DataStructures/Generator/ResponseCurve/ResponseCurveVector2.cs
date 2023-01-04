// Copyright Gamelogic (c) http://www.gamelogic.co.za

using System.Collections.Generic;
using System.Numerics;

namespace Gamelogic.Extensions.Algorithms
{
	/// <summary>
	/// A response curve with outputs of Vector2.
	/// </summary>
	public class ResponseCurveVector2 : ResponseCurveBase<Vector2>
	{
		#region Constructors

		public ResponseCurveVector2(IEnumerable<float> inputSamples, IEnumerable<Vector2> outputSamples)
			: base(inputSamples, outputSamples)
		{
		}

		#endregion

		#region Protected Methods

		protected override Vector2 Lerp(Vector2 outputSampleMin, Vector2 outputSampleMax, float t)
		{
			return Vector2.Lerp(outputSampleMin, outputSampleMax, t);
		}

		#endregion
	}
}
