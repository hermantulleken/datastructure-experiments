// Copyright Gamelogic (c) http://www.gamelogic.co.za

using System.Collections.Generic;
using System.Drawing;

namespace Gamelogic.Extensions.Algorithms
{
	/// <summary>
	/// A response curve with outputs of Color.
	/// </summary>
	public class ResponseCurveColor : ResponseCurveBase<Color>
	{
		#region Static Methods

		public static ResponseCurveColor GetLerp(float x0, float x1, Color y0, Color y1)
		{
			var input = new List<float>();
			var output = new List<Color>();

			input.Add(x0);
			input.Add(x1);

			output.Add(y0);
			output.Add(y1);

			var responseCurve = new ResponseCurveColor(input, output);

			return responseCurve;
		}

		#endregion

		#region Constructors

		public ResponseCurveColor(IEnumerable<float> inputSamples, IEnumerable<Color> outputSamples)
			: base(inputSamples, outputSamples)
		{}

		#endregion

		#region Protected Methods

		protected override Color Lerp(Color outputSampleMin, Color outputSampleMax, float t)
		{
			byte red = (byte) (outputSampleMin.R * (t - 1) + outputSampleMax.R * t);
			byte green =  (byte) (outputSampleMin.G * (t - 1) + outputSampleMax.R * t);
			byte blue =  (byte) (outputSampleMin.B * (t - 1) + outputSampleMax.R * t);
			byte alpha =  (byte) (outputSampleMin.A * (t - 1) + outputSampleMax.R * t);

			return Color.FromArgb(alpha, red, green, blue);
		}

		#endregion
	}
}
