namespace Tiltify.Client.Models;

/// <summary>The values of <see cref="TiltifyFact.Status"/>, as Tiltify's API defines them.</summary>
public static class TiltifyFactStatus
{
    /// <summary>Not yet visible or taking donations.</summary>
    public const string Unpublished = "unpublished";

    /// <summary>Live and taking donations.</summary>
    public const string Published = "published";

    /// <summary>Ended.</summary>
    public const string Retired = "retired";
}
