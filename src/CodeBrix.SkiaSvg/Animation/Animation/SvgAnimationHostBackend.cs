using System;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

/// <summary>Specifies the backend used to drive SVG animation playback.</summary>
public enum SvgAnimationHostBackend
{
    /// <summary>Automatically selects the best available backend.</summary>
    Default,
    /// <summary>Animation time is advanced manually by the caller.</summary>
    Manual,
    /// <summary>Animation is driven by a dispatcher timer.</summary>
    DispatcherTimer,
    /// <summary>Animation is driven by a continuous render loop.</summary>
    RenderLoop,
    /// <summary>Animation is driven by the platform's native composition layer.</summary>
    NativeComposition
}

/// <summary>Describes the animation backend capabilities of a host environment.</summary>
public sealed class SvgAnimationHostBackendCapabilities
{
    /// <summary>Initializes a new instance of the <see cref="SvgAnimationHostBackendCapabilities"/> class.</summary>
    /// <param name="isHostReady">Whether the host is attached and ready.</param>
    /// <param name="supportsDispatcherTimer">Whether dispatcher timer playback is available.</param>
    /// <param name="supportsRenderLoop">Whether render loop playback is available.</param>
    /// <param name="supportsNativeComposition">Whether native composition playback is available.</param>
    public SvgAnimationHostBackendCapabilities(
        bool isHostReady,
        bool supportsDispatcherTimer,
        bool supportsRenderLoop,
        bool supportsNativeComposition)
    {
        IsHostReady = isHostReady;
        SupportsDispatcherTimer = supportsDispatcherTimer;
        SupportsRenderLoop = supportsRenderLoop;
        SupportsNativeComposition = supportsNativeComposition;
    }

    /// <summary>Gets a value indicating whether the UI host is attached and ready.</summary>
    public bool IsHostReady { get; }

    /// <summary>Gets a value indicating whether dispatcher timer playback is supported.</summary>
    public bool SupportsDispatcherTimer { get; }

    /// <summary>Gets a value indicating whether render loop playback is supported.</summary>
    public bool SupportsRenderLoop { get; }

    /// <summary>Gets a value indicating whether native composition playback is supported.</summary>
    public bool SupportsNativeComposition { get; }
}

/// <summary>Contains the result of resolving an animation host backend.</summary>
public sealed class SvgAnimationHostBackendResolution
{
    /// <summary>Initializes a new instance of the <see cref="SvgAnimationHostBackendResolution"/> class.</summary>
    /// <param name="requestedBackend">The originally requested backend.</param>
    /// <param name="actualBackend">The backend that was actually selected.</param>
    /// <param name="fallbackReason">The reason for falling back, or <c>null</c>.</param>
    public SvgAnimationHostBackendResolution(
        SvgAnimationHostBackend requestedBackend,
        SvgAnimationHostBackend actualBackend,
        string fallbackReason)
    {
        RequestedBackend = requestedBackend;
        ActualBackend = actualBackend;
        FallbackReason = fallbackReason;
    }

    /// <summary>Gets the originally requested backend.</summary>
    public SvgAnimationHostBackend RequestedBackend { get; }

    /// <summary>Gets the backend that was actually selected.</summary>
    public SvgAnimationHostBackend ActualBackend { get; }

    /// <summary>Gets the reason for falling back, or <c>null</c>.</summary>
    public string FallbackReason { get; }

    /// <summary>Gets a value indicating whether a fallback to a different backend occurred.</summary>
    public bool IsFallback =>
        !string.IsNullOrWhiteSpace(FallbackReason) &&
        RequestedBackend != ActualBackend;
}

/// <summary>Resolves the best animation host backend given requested preferences and host capabilities.</summary>
public static class SvgAnimationHostBackendResolver
{
    /// <summary>Resolves the animation backend to use.</summary>
    /// <param name="requestedBackend">The requested animation backend.</param>
    /// <param name="capabilities">The capabilities of the host environment.</param>
    /// <param name="hasAnimations">Whether the SVG source contains animations.</param>
    /// <returns>The resolved backend selection.</returns>
    public static SvgAnimationHostBackendResolution Resolve(
        SvgAnimationHostBackend requestedBackend,
        SvgAnimationHostBackendCapabilities capabilities,
        bool hasAnimations)
    {
        if (requestedBackend == SvgAnimationHostBackend.Manual)
        {
            return new SvgAnimationHostBackendResolution(
                requestedBackend,
                SvgAnimationHostBackend.Manual,
                null);
        }

        if (!hasAnimations)
        {
            return new SvgAnimationHostBackendResolution(
                requestedBackend,
                SvgAnimationHostBackend.Manual,
                "SVG source does not contain animation elements.");
        }

        if (!capabilities.IsHostReady)
        {
            return new SvgAnimationHostBackendResolution(
                requestedBackend,
                SvgAnimationHostBackend.Manual,
                "Animation playback requires an attached UI host.");
        }

        switch (requestedBackend)
        {
            case SvgAnimationHostBackend.DispatcherTimer:
                {
                    return capabilities.SupportsDispatcherTimer
                        ? new SvgAnimationHostBackendResolution(
                            requestedBackend,
                            SvgAnimationHostBackend.DispatcherTimer,
                            null)
                        : new SvgAnimationHostBackendResolution(
                            requestedBackend,
                            SvgAnimationHostBackend.Manual,
                            "Dispatcher timer animation playback is unavailable.");
                }
            case SvgAnimationHostBackend.NativeComposition:
                {
                    if (capabilities.SupportsNativeComposition && SupportsAutomaticTicks(capabilities))
                    {
                        return new SvgAnimationHostBackendResolution(
                            requestedBackend,
                            SvgAnimationHostBackend.NativeComposition,
                            null);
                    }

                    return ResolveNativeCompositionFallback(requestedBackend, capabilities);
                }
            case SvgAnimationHostBackend.RenderLoop:
                {
                    if (capabilities.SupportsRenderLoop)
                    {
                        return new SvgAnimationHostBackendResolution(
                            requestedBackend,
                            SvgAnimationHostBackend.RenderLoop,
                            null);
                    }

                    if (capabilities.SupportsDispatcherTimer)
                    {
                        return new SvgAnimationHostBackendResolution(
                            requestedBackend,
                            SvgAnimationHostBackend.DispatcherTimer,
                            "Render-loop animation playback is unavailable; falling back to dispatcher timer.");
                    }

                    return new SvgAnimationHostBackendResolution(
                        requestedBackend,
                        SvgAnimationHostBackend.Manual,
                        "Render-loop animation playback is unavailable.");
                }
            case SvgAnimationHostBackend.Default:
            default:
                {
                    if (capabilities.SupportsNativeComposition && SupportsAutomaticTicks(capabilities))
                    {
                        return new SvgAnimationHostBackendResolution(
                            requestedBackend,
                            SvgAnimationHostBackend.NativeComposition,
                            null);
                    }

                    if (capabilities.SupportsRenderLoop)
                    {
                        return new SvgAnimationHostBackendResolution(
                            requestedBackend,
                            SvgAnimationHostBackend.RenderLoop,
                            null);
                    }

                    if (capabilities.SupportsDispatcherTimer)
                    {
                        return new SvgAnimationHostBackendResolution(
                            requestedBackend,
                            SvgAnimationHostBackend.DispatcherTimer,
                            null);
                    }

                    return new SvgAnimationHostBackendResolution(
                        requestedBackend,
                        SvgAnimationHostBackend.Manual,
                        "Automatic animation playback backends are unavailable.");
                }
        }
    }

    private static bool SupportsAutomaticTicks(SvgAnimationHostBackendCapabilities capabilities)
    {
        return capabilities.SupportsRenderLoop || capabilities.SupportsDispatcherTimer;
    }

    private static SvgAnimationHostBackendResolution ResolveNativeCompositionFallback(
        SvgAnimationHostBackend requestedBackend,
        SvgAnimationHostBackendCapabilities capabilities)
    {
        if (capabilities.SupportsRenderLoop)
        {
            return new SvgAnimationHostBackendResolution(
                requestedBackend,
                SvgAnimationHostBackend.RenderLoop,
                "Native composition animation playback is unavailable; falling back to render loop.");
        }

        if (capabilities.SupportsDispatcherTimer)
        {
            return new SvgAnimationHostBackendResolution(
                requestedBackend,
                SvgAnimationHostBackend.DispatcherTimer,
                "Native composition animation playback is unavailable; falling back to dispatcher timer.");
        }

        return new SvgAnimationHostBackendResolution(
            requestedBackend,
            SvgAnimationHostBackend.Manual,
            "Native composition animation playback is unavailable.");
    }
}
