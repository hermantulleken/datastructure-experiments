namespace DataStructures;

public class GLMath
{
	//Only works for both positive integers.
	public static int CeilDiv(int m, int n) => (n + n - 1) / n;

	public static int RoundDiv(int m, int n) => (m + n / 2) / n;

	public static Int2 RoundDiv(Int2 m, int n) => new Int2(RoundDiv(m.X, n), RoundDiv(m.Y, n));
}
