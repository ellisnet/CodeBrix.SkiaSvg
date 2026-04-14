using System;
using CodeBrix.SvgParse;

namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

/// <summary>Provides data for animation clock time-changed events.</summary>
public sealed class SvgAnimationClockChangedEventArgs : EventArgs
{
    internal SvgAnimationClockChangedEventArgs(TimeSpan time)
    {
        Time = time;
    }

    /// <summary>Gets the new clock time.</summary>
    public TimeSpan Time { get; }
}

/// <summary>Tracks animation playback time and notifies subscribers of time changes.</summary>
public sealed class SvgAnimationClock
{
    private TimeSpan _currentTime;

    /// <summary>Gets the current animation time.</summary>
    public TimeSpan CurrentTime => _currentTime;

    /// <summary>Occurs when the animation time changes.</summary>
    public event EventHandler<SvgAnimationClockChangedEventArgs> TimeChanged;

    /// <summary>Resets the clock to zero.</summary>
    public void Reset()
    {
        Seek(TimeSpan.Zero);
    }

    /// <summary>Seeks the clock to the specified time.</summary>
    /// <param name="time">The time to seek to.</param>
    public void Seek(TimeSpan time)
    {
        if (time < TimeSpan.Zero)
        {
            time = TimeSpan.Zero;
        }

        if (_currentTime == time)
        {
            return;
        }

        _currentTime = time;
        TimeChanged?.Invoke(this, new SvgAnimationClockChangedEventArgs(time));
    }

    /// <summary>Advances the clock by the specified time delta.</summary>
    /// <param name="delta">The time delta to advance by.</param>
    public void AdvanceBy(TimeSpan delta)
    {
        TimeSpan next;
        if (delta >= TimeSpan.Zero)
        {
            next = delta >= TimeSpan.MaxValue - _currentTime
                ? TimeSpan.MaxValue
                : _currentTime + delta;
        }
        else
        {
            next = delta <= -_currentTime
                ? TimeSpan.Zero
                : _currentTime + delta;
        }

        Seek(next);
    }
}
