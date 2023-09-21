namespace Csharp.Common.Unsafe;

public static class MathExtensions
{
    const int FastInverseHex   = 0x5f3759df;
    const int BetterInverseHex = 0x5f375a86;

    public static unsafe float FastInverseSqrt(this float number)
    {
        long i;
        float x2, y;
        x2 = number * 0.5f;
        y = number;
        i = *(long*) &y;
        i = FastInverseHex - (i >> 1);
        y = *(float*) &i;
        y *= (1.5f - (x2 * y * y));
        return y;
    }
    
    public static unsafe float BetterInverseSqrt(this float number)
    {
        long i;
        float x2, y;
        x2 = number * 0.5f;
        y = number;
        i = *(long*) &y;
        i = BetterInverseHex - (i >> 1);
        y = *(float*) &i;
        y *= (1.5f - (x2 * y * y));
        return y;
    }
}