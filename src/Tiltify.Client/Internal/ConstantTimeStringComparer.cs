using System.Runtime.CompilerServices;

namespace Tiltify.Client.Internal;

/// <summary>
/// Provides constant-time string comparison to prevent timing attacks when comparing secrets.
/// </summary>
internal static class ConstantTimeStringComparer
{
    /// <summary>
    /// Compares two strings in constant time to prevent timing-based attacks.
    /// </summary>
    /// <param name="a">The first string.</param>
    /// <param name="b">The second string.</param>
    /// <returns><see langword="true"/> when both strings are equal; otherwise <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool Equals(string? a, string? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        int length = a.Length;
        if (length != b.Length)
        {
            return false;
        }

        int result = 0;
        for (int i = 0; i < length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}
