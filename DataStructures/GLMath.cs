using System;

namespace DataStructures;

public class GLMath
{
	//Only works for both positive integers.
	public static int CeilDiv(int m, int n) => (n + n - 1) / n;

	public static int RoundDiv(int m, int n) => (m + n / 2) / n;

	public static Int2 RoundDiv(Int2 m, int n) => new Int2(RoundDiv(m.X, n), RoundDiv(m.Y, n));

	public static int Mod(int m, int n) => m >= 0 ? m % n : (m % n) + n;
	public static int FloorToInt(float f) => (int) MathF.Floor(f);
	public static int RoundToInt(float x) => (int)MathF.Round(x);
	
	public static int Clamp(int value, int min, int max) => value <= min ? min : value >= max ? max : value;
	public static float Clamp(float value, float min, float max) => value <= min ? min : value >= max ? max : value;
	public static float Clamp01(float value) => Clamp(value, 0.0f, 1.0f);
	
	public static float Lerp(float t, float a, float b) => (t - 1) * a + t * b;

	
}
