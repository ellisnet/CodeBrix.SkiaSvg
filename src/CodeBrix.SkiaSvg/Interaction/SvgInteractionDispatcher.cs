using System;
using System.Collections.Generic;
using CodeBrix.SkiaSvg.ShimSkiaSharp;

using CodeBrix.SvgParse;
namespace CodeBrix.SkiaSvg; //Was previously: namespace Svg.Skia;

/// <summary>Identifies the type of pointer device.</summary>
public enum SvgPointerDeviceType
{
    /// <summary>An unknown device type.</summary>
    Unknown,
    /// <summary>A mouse device.</summary>
    Mouse,
    /// <summary>A touch device.</summary>
    Touch,
    /// <summary>A pen or stylus device.</summary>
    Pen
}

/// <summary>Identifies a mouse button.</summary>
public enum SvgMouseButton
{
    /// <summary>No button.</summary>
    None,
    /// <summary>The left mouse button.</summary>
    Left,
    /// <summary>The middle mouse button.</summary>
    Middle,
    /// <summary>The right mouse button.</summary>
    Right,
    /// <summary>The first extended mouse button.</summary>
    XButton1,
    /// <summary>The second extended mouse button.</summary>
    XButton2
}

/// <summary>Identifies the routing phase of a pointer event.</summary>
public enum SvgPointerEventRoutePhase
{
    /// <summary>The tunneling (preview) phase, propagating from root toward target.</summary>
    Tunnel,
    /// <summary>The event has reached the target element.</summary>
    Target,
    /// <summary>The bubbling phase, propagating from target toward root.</summary>
    Bubble
}

/// <summary>Describes input state for a pointer event.</summary>
public sealed class SvgPointerInput
{
    /// <summary>Initializes a new instance of the <see cref="SvgPointerInput"/> class.</summary>
    /// <param name="picturePoint">The point in picture coordinates.</param>
    /// <param name="pointerDeviceType">The type of pointer device.</param>
    /// <param name="button">The mouse button involved.</param>
    /// <param name="clickCount">The number of clicks.</param>
    /// <param name="wheelDelta">The mouse wheel delta.</param>
    /// <param name="altKey">Whether the Alt key is pressed.</param>
    /// <param name="shiftKey">Whether the Shift key is pressed.</param>
    /// <param name="ctrlKey">Whether the Ctrl key is pressed.</param>
    /// <param name="sessionId">An identifier for the pointer session.</param>
    public SvgPointerInput(
        SKPoint picturePoint,
        SvgPointerDeviceType pointerDeviceType,
        SvgMouseButton button,
        int clickCount,
        int wheelDelta,
        bool altKey,
        bool shiftKey,
        bool ctrlKey,
        string sessionId)
    {
        PicturePoint = picturePoint;
        PointerDeviceType = pointerDeviceType;
        Button = button;
        ClickCount = clickCount;
        WheelDelta = wheelDelta;
        AltKey = altKey;
        ShiftKey = shiftKey;
        CtrlKey = ctrlKey;
        SessionId = sessionId ?? string.Empty;
    }

    /// <summary>Gets the point in picture coordinates.</summary>
    public SKPoint PicturePoint { get; }

    /// <summary>Gets the type of pointer device.</summary>
    public SvgPointerDeviceType PointerDeviceType { get; }

    /// <summary>Gets the mouse button involved.</summary>
    public SvgMouseButton Button { get; }

    /// <summary>Gets the number of clicks.</summary>
    public int ClickCount { get; }

    /// <summary>Gets the mouse wheel delta.</summary>
    public int WheelDelta { get; }

    /// <summary>Gets a value indicating whether the Alt key is pressed.</summary>
    public bool AltKey { get; }

    /// <summary>Gets a value indicating whether the Shift key is pressed.</summary>
    public bool ShiftKey { get; }

    /// <summary>Gets a value indicating whether the Ctrl key is pressed.</summary>
    public bool CtrlKey { get; }

    /// <summary>Gets the pointer session identifier.</summary>
    public string SessionId { get; }
}

/// <summary>Provides data for SVG pointer events.</summary>
public sealed class SvgPointerEventArgs : EventArgs
{
    internal SvgPointerEventArgs(
        SvgPointerEventType eventType,
        SvgElement element,
        SvgElement targetElement,
        SvgElement relatedElement,
        SvgPointerEventRoutePhase routePhase,
        SvgPointerInput input,
        string cursor)
    {
        EventType = eventType;
        Element = element;
        TargetElement = targetElement;
        RelatedElement = relatedElement;
        RoutePhase = routePhase;
        Input = input;
        Cursor = cursor;
    }

    /// <summary>Gets the type of pointer event.</summary>
    public SvgPointerEventType EventType { get; }

    /// <summary>Gets the element receiving the event in the current routing phase.</summary>
    public SvgElement Element { get; }

    /// <summary>Gets the original target element of the event.</summary>
    public SvgElement TargetElement { get; }

    /// <summary>Gets the related element, such as the element being entered or left.</summary>
    public SvgElement RelatedElement { get; }

    /// <summary>Gets the current routing phase of the event.</summary>
    public SvgPointerEventRoutePhase RoutePhase { get; }

    /// <summary>Gets the pointer input state.</summary>
    public SvgPointerInput Input { get; }

    /// <summary>Gets the resolved cursor for the target element.</summary>
    public string Cursor { get; }

    /// <summary>Gets or sets a value indicating whether the event has been handled.</summary>
    public bool Handled { get; set; }

    /// <summary>Gets the point in picture coordinates from the input.</summary>
    public SKPoint PicturePoint => Input.PicturePoint;
}

/// <summary>Contains the result of dispatching a pointer interaction.</summary>
public sealed class SvgInteractionDispatchResult
{
    internal SvgInteractionDispatchResult(SvgElement targetElement, string cursor, bool handled)
    {
        TargetElement = targetElement;
        Cursor = cursor;
        Handled = handled;
    }

    /// <summary>Gets the target element of the interaction.</summary>
    public SvgElement TargetElement { get; }

    /// <summary>Gets the resolved cursor for the target element.</summary>
    public string Cursor { get; }

    /// <summary>Gets a value indicating whether the event was handled.</summary>
    public bool Handled { get; }
}

/// <summary>Dispatches pointer events to SVG elements with tunneling, targeting, and bubbling phases.</summary>
public sealed class SvgInteractionDispatcher
{
    private readonly SvgEventCallerRegistry _eventCallerRegistry = new();
    private SvgElement _registeredRoot;
    private SvgElement _hoveredElement;
    private SvgElement _pressedElement;
    private SvgElement _capturedElement;

    /// <summary>Gets or sets a value indicating whether SVG element events are raised.</summary>
    public bool RaiseSvgElementEvents { get; set; } = true;

    /// <summary>Gets the currently hovered SVG element.</summary>
    public SvgElement HoveredElement => _hoveredElement;

    /// <summary>Gets the currently pressed SVG element.</summary>
    public SvgElement PressedElement => _pressedElement;

    /// <summary>Gets the currently captured SVG element.</summary>
    public SvgElement CapturedElement => _capturedElement;

    /// <summary>Gets the currently resolved cursor string.</summary>
    public string CurrentCursor { get; private set; }

    /// <summary>Occurs when a pointer event is dispatched to an element.</summary>
    public event EventHandler<SvgPointerEventArgs> Dispatched;

    /// <summary>Hit-tests to find the topmost SVG element at the specified point.</summary>
    /// <param name="svg">The SVG instance to hit-test.</param>
    /// <param name="picturePoint">The point in picture coordinates.</param>
    /// <returns>The topmost element at the point, or <c>null</c> if none.</returns>
    public SvgElement HitTestTopmostElement(SKSvg svg, SKPoint picturePoint)
    {
        return svg?.HitTestTopmostElement(picturePoint);
    }

    /// <summary>Handles a pointer moved event, dispatching it through the SVG element tree.</summary>
    /// <param name="svg">The SVG instance.</param>
    /// <param name="input">The pointer input state.</param>
    public void HandlePointerMoved(SKSvg svg, SvgPointerInput input)
    {
        _ = DispatchPointerMoved(svg, input);
    }

    /// <summary>Handles a pointer pressed event, dispatching it through the SVG element tree.</summary>
    /// <param name="svg">The SVG instance.</param>
    /// <param name="input">The pointer input state.</param>
    public void HandlePointerPressed(SKSvg svg, SvgPointerInput input)
    {
        _ = DispatchPointerPressed(svg, input);
    }

    /// <summary>Handles a pointer released event, dispatching it through the SVG element tree.</summary>
    /// <param name="svg">The SVG instance.</param>
    /// <param name="input">The pointer input state.</param>
    public void HandlePointerReleased(SKSvg svg, SvgPointerInput input)
    {
        _ = DispatchPointerReleased(svg, input);
    }

    /// <summary>Handles a pointer wheel changed event, dispatching it through the SVG element tree.</summary>
    /// <param name="svg">The SVG instance.</param>
    /// <param name="input">The pointer input state.</param>
    public void HandlePointerWheelChanged(SKSvg svg, SvgPointerInput input)
    {
        _ = DispatchPointerWheelChanged(svg, input);
    }

    /// <summary>Handles a pointer exited event.</summary>
    /// <param name="input">The pointer input state.</param>
    public void HandlePointerExited(SvgPointerInput input)
    {
        _ = DispatchPointerExited(input);
    }

    /// <summary>Dispatches a pointer moved event and returns the result.</summary>
    /// <param name="svg">The SVG instance.</param>
    /// <param name="input">The pointer input state.</param>
    /// <returns>The dispatch result.</returns>
    public SvgInteractionDispatchResult DispatchPointerMoved(SKSvg svg, SvgPointerInput input)
    {
        EnsureEventBridge(svg);

        var animationFrameDirty = false;
        var hitTarget = svg?.HitTestTopmostElement(input.PicturePoint);
        var routeTarget = _capturedElement ?? hitTarget;
        var handled = false;

        if (_capturedElement is null)
        {
            handled = UpdateHover(svg, hitTarget, input, ref animationFrameDirty);
        }
        else
        {
            CurrentCursor = ResolveCursor(routeTarget);
        }

        handled |= DispatchRoutedEvent(svg, SvgPointerEventType.Move, routeTarget, null, input, "onmousemove", ref animationFrameDirty);
        RefreshAnimationFrame(svg, animationFrameDirty);

        return CreateResult(_hoveredElement ?? routeTarget, handled);
    }

    /// <summary>Dispatches a pointer pressed event and returns the result.</summary>
    /// <param name="svg">The SVG instance.</param>
    /// <param name="input">The pointer input state.</param>
    /// <returns>The dispatch result.</returns>
    public SvgInteractionDispatchResult DispatchPointerPressed(SKSvg svg, SvgPointerInput input)
    {
        EnsureEventBridge(svg);

        var animationFrameDirty = false;
        var target = svg?.HitTestTopmostElement(input.PicturePoint);
        var handled = UpdateHover(svg, target, input, ref animationFrameDirty);
        _pressedElement = target;
        _capturedElement = target;
        handled |= DispatchRoutedEvent(svg, SvgPointerEventType.Press, target, null, input, "onmousedown", ref animationFrameDirty);
        RefreshAnimationFrame(svg, animationFrameDirty);

        return CreateResult(_hoveredElement ?? target, handled);
    }

    /// <summary>Dispatches a pointer released event and returns the result.</summary>
    /// <param name="svg">The SVG instance.</param>
    /// <param name="input">The pointer input state.</param>
    /// <returns>The dispatch result.</returns>
    public SvgInteractionDispatchResult DispatchPointerReleased(SKSvg svg, SvgPointerInput input)
    {
        EnsureEventBridge(svg);

        var animationFrameDirty = false;
        var hitTarget = svg?.HitTestTopmostElement(input.PicturePoint);
        var routeTarget = _capturedElement ?? hitTarget;
        var captureWasActive = _capturedElement is not null;
        var handled = false;

        if (_capturedElement is null)
        {
            handled = UpdateHover(svg, hitTarget, input, ref animationFrameDirty);
        }
        else
        {
            CurrentCursor = ResolveCursor(routeTarget);
        }

        handled |= DispatchRoutedEvent(svg, SvgPointerEventType.Release, routeTarget, null, input, "onmouseup", ref animationFrameDirty);

        if (routeTarget is not null && ReferenceEquals(hitTarget, _pressedElement))
        {
            handled |= DispatchRoutedEvent(svg, SvgPointerEventType.Click, routeTarget, null, input, "onclick", ref animationFrameDirty);
        }

        _pressedElement = null;
        _capturedElement = null;

        if (captureWasActive)
        {
            handled |= UpdateHover(svg, hitTarget, input, ref animationFrameDirty);
        }

        RefreshAnimationFrame(svg, animationFrameDirty);
        return CreateResult(_hoveredElement ?? hitTarget, handled);
    }

    /// <summary>Dispatches a pointer wheel changed event and returns the result.</summary>
    /// <param name="svg">The SVG instance.</param>
    /// <param name="input">The pointer input state.</param>
    /// <returns>The dispatch result.</returns>
    public SvgInteractionDispatchResult DispatchPointerWheelChanged(SKSvg svg, SvgPointerInput input)
    {
        EnsureEventBridge(svg);

        var animationFrameDirty = false;
        var hitTarget = svg?.HitTestTopmostElement(input.PicturePoint);
        var routeTarget = _capturedElement ?? hitTarget;
        var handled = false;

        if (_capturedElement is null)
        {
            handled = UpdateHover(svg, hitTarget, input, ref animationFrameDirty);
        }
        else
        {
            CurrentCursor = ResolveCursor(routeTarget);
        }

        handled |= DispatchRoutedScroll(svg, routeTarget, input, ref animationFrameDirty);
        RefreshAnimationFrame(svg, animationFrameDirty);

        return CreateResult(_hoveredElement ?? routeTarget, handled);
    }

    /// <summary>Dispatches a pointer exited event and returns the result.</summary>
    /// <param name="input">The pointer input state.</param>
    /// <returns>The dispatch result.</returns>
    public SvgInteractionDispatchResult DispatchPointerExited(SvgPointerInput input)
    {
        return DispatchPointerExited(null, input);
    }

    /// <summary>Dispatches a pointer exited event with an optional SVG context and returns the result.</summary>
    /// <param name="svg">The SVG instance, or <c>null</c>.</param>
    /// <param name="input">The pointer input state.</param>
    /// <returns>The dispatch result.</returns>
    public SvgInteractionDispatchResult DispatchPointerExited(SKSvg svg, SvgPointerInput input)
    {
        if (_capturedElement is not null)
        {
            CurrentCursor = null;
            return CreateResult(_capturedElement, handled: false);
        }

        var target = _hoveredElement;
        var handled = false;
        var animationFrameDirty = false;

        if (target is { })
        {
            handled = DispatchRoutedEvent(svg, SvgPointerEventType.Leave, target, null, input, "onmouseout", ref animationFrameDirty);
        }

        _hoveredElement = null;
        CurrentCursor = null;
        RefreshAnimationFrame(svg, animationFrameDirty);

        return CreateResult(null, handled);
    }

    /// <summary>Resets the dispatcher state, clearing all tracked elements and event registrations.</summary>
    public void Reset()
    {
        _hoveredElement = null;
        _pressedElement = null;
        _capturedElement = null;
        _registeredRoot = null;
        CurrentCursor = null;
        _eventCallerRegistry.Clear();
    }

    private bool UpdateHover(SKSvg svg, SvgElement target, SvgPointerInput input, ref bool animationFrameDirty)
    {
        if (ReferenceEquals(target, _hoveredElement))
        {
            CurrentCursor = ResolveCursor(target);
            return false;
        }

        var previous = _hoveredElement;
        var handled = false;

        if (previous is { })
        {
            handled |= DispatchRoutedEvent(svg, SvgPointerEventType.Leave, previous, target, input, "onmouseout", ref animationFrameDirty);
        }

        _hoveredElement = target;
        CurrentCursor = ResolveCursor(target);

        if (target is { })
        {
            handled |= DispatchRoutedEvent(svg, SvgPointerEventType.Enter, target, previous, input, "onmouseover", ref animationFrameDirty);
        }

        return handled;
    }

    private SvgInteractionDispatchResult CreateResult(SvgElement target, bool handled)
    {
        return new SvgInteractionDispatchResult(target, CurrentCursor, handled);
    }

    private bool DispatchRoutedScroll(SKSvg svg, SvgElement target, SvgPointerInput input, ref bool animationFrameDirty)
    {
        var cursor = ResolveCursor(target);
        if (target is null)
        {
            return DispatchShared(
                SvgPointerEventType.Wheel,
                null,
                null,
                null,
                SvgPointerEventRoutePhase.Target,
                input,
                cursor);
        }

        if (DispatchTunnelEvent(
                SvgPointerEventType.Wheel,
                target,
                null,
                input,
                cursor))
        {
            return true;
        }

        foreach (var element in BuildRoute(target))
        {
            animationFrameDirty |= svg?.RecordAnimationPointerEvent(element, SvgPointerEventType.Wheel) == true;
            DispatchSvgMouseScroll(element, input);
            var routePhase = ReferenceEquals(element, target)
                ? SvgPointerEventRoutePhase.Target
                : SvgPointerEventRoutePhase.Bubble;

            if (DispatchShared(SvgPointerEventType.Wheel, element, target, null, routePhase, input, cursor))
            {
                return true;
            }
        }

        return false;
    }

    private bool DispatchRoutedEvent(
        SKSvg svg,
        SvgPointerEventType eventType,
        SvgElement target,
        SvgElement relatedElement,
        SvgPointerInput input,
        string svgEventName,
        ref bool animationFrameDirty)
    {
        var cursor = ResolveCursor(target);
        if (target is null)
        {
            return DispatchShared(
                eventType,
                null,
                null,
                relatedElement,
                SvgPointerEventRoutePhase.Target,
                input,
                cursor);
        }

        if (DispatchTunnelEvent(
                eventType,
                target,
                relatedElement,
                input,
                cursor))
        {
            return true;
        }

        foreach (var element in BuildRoute(target))
        {
            animationFrameDirty |= svg?.RecordAnimationPointerEvent(element, eventType) == true;
            DispatchSvgMouseEvent(element, svgEventName, input);
            var routePhase = ReferenceEquals(element, target)
                ? SvgPointerEventRoutePhase.Target
                : SvgPointerEventRoutePhase.Bubble;

            if (DispatchShared(eventType, element, target, relatedElement, routePhase, input, cursor))
            {
                return true;
            }
        }

        return false;
    }

    private bool DispatchTunnelEvent(
        SvgPointerEventType eventType,
        SvgElement target,
        SvgElement relatedElement,
        SvgPointerInput input,
        string cursor)
    {
        var route = BuildRoute(target);
        for (var index = route.Count - 1; index > 0; index--)
        {
            if (DispatchShared(
                    eventType,
                    route[index],
                    target,
                    relatedElement,
                    SvgPointerEventRoutePhase.Tunnel,
                    input,
                    cursor))
            {
                return true;
            }
        }

        return false;
    }

    private static void RefreshAnimationFrame(SKSvg svg, bool animationFrameDirty)
    {
        if (animationFrameDirty)
        {
            svg?.RefreshCurrentAnimationFrame(bypassThrottle: true);
        }
    }

    private static List<SvgElement> BuildRoute(SvgElement target)
    {
        var route = new List<SvgElement>();
        for (var current = target; current is not null; current = current.Parent)
        {
            route.Add(current);
        }

        return route;
    }

    private bool DispatchShared(
        SvgPointerEventType eventType,
        SvgElement element,
        SvgElement targetElement,
        SvgElement relatedElement,
        SvgPointerEventRoutePhase routePhase,
        SvgPointerInput input,
        string cursor)
    {
        var dispatched = Dispatched;
        if (dispatched is null)
        {
            return false;
        }

        var args = new SvgPointerEventArgs(
            eventType,
            element,
            targetElement,
            relatedElement,
            routePhase,
            input,
            cursor);

        dispatched(this, args);
        return args.Handled;
    }

    private void DispatchSvgMouseEvent(SvgElement element, string eventName, SvgPointerInput input)
    {
        var elementId = element?.ID;
        if (!RaiseSvgElementEvents || string.IsNullOrWhiteSpace(elementId))
        {
            return;
        }

        var registeredElementId = elementId!;
        _eventCallerRegistry.Invoke(
            registeredElementId + "/" + eventName,
            input.PicturePoint.X,
            input.PicturePoint.Y,
            ToSvgMouseButtonValue(input.Button),
            input.ClickCount,
            input.AltKey,
            input.ShiftKey,
            input.CtrlKey,
            input.SessionId);
    }

    private void DispatchSvgMouseScroll(SvgElement element, SvgPointerInput input)
    {
        var elementId = element?.ID;
        if (!RaiseSvgElementEvents || string.IsNullOrWhiteSpace(elementId))
        {
            return;
        }

        var registeredElementId = elementId!;
        _eventCallerRegistry.Invoke(
            registeredElementId + "/onmousescroll",
            input.WheelDelta,
            input.AltKey,
            input.ShiftKey,
            input.CtrlKey,
            input.SessionId);
    }

    private void EnsureEventBridge(SKSvg svg)
    {
        if (!RaiseSvgElementEvents)
        {
            return;
        }

        var root = GetRootElement(svg);
        if (ReferenceEquals(root, _registeredRoot))
        {
            return;
        }

        _eventCallerRegistry.Clear();
        _registeredRoot = root;

        if (root is { })
        {
            RegisterTree(root);
        }
    }

    private static SvgElement GetRootElement(SKSvg svg)
    {
        return svg?.SourceDocument ?? svg?.RetainedSceneGraph?.SourceDocument;
    }

    private void RegisterTree(SvgElement element)
    {
        if (!string.IsNullOrWhiteSpace(element.ID))
        {
            element.RegisterEvents(_eventCallerRegistry);
        }

        foreach (var child in element.Children)
        {
            RegisterTree(child);
        }
    }

    private static string ResolveCursor(SvgElement target)
    {
        for (var current = target; current is not null; current = current.Parent)
        {
            if (current.TryGetAttribute("cursor", out var cursor) &&
                !string.IsNullOrWhiteSpace(cursor))
            {
                var normalizedCursor = cursor.Trim();
                if (string.Equals(normalizedCursor, "inherit", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return normalizedCursor;
            }
        }

        return null;
    }

    private static int ToSvgMouseButtonValue(SvgMouseButton button)
    {
        return button switch
        {
            SvgMouseButton.Left => 1,
            SvgMouseButton.Middle => 2,
            SvgMouseButton.Right => 3,
            SvgMouseButton.XButton1 => 4,
            SvgMouseButton.XButton2 => 5,
            _ => 0
        };
    }
}

internal sealed class SvgEventCallerRegistry : ISvgEventCaller
{
    private readonly Dictionary<string, Delegate> _actions = new(StringComparer.Ordinal);

    public void Clear()
    {
        _actions.Clear();
    }

    public void RegisterAction(string rpcID, Action action)
    {
        _actions[rpcID] = action;
    }

    public void RegisterAction<T1>(string rpcID, Action<T1> action)
    {
        _actions[rpcID] = action;
    }

    public void RegisterAction<T1, T2>(string rpcID, Action<T1, T2> action)
    {
        _actions[rpcID] = action;
    }

    public void RegisterAction<T1, T2, T3>(string rpcID, Action<T1, T2, T3> action)
    {
        _actions[rpcID] = action;
    }

    public void RegisterAction<T1, T2, T3, T4>(string rpcID, Action<T1, T2, T3, T4> action)
    {
        _actions[rpcID] = action;
    }

    public void RegisterAction<T1, T2, T3, T4, T5>(string rpcID, Action<T1, T2, T3, T4, T5> action)
    {
        _actions[rpcID] = action;
    }

    public void RegisterAction<T1, T2, T3, T4, T5, T6>(string rpcID, Action<T1, T2, T3, T4, T5, T6> action)
    {
        _actions[rpcID] = action;
    }

    public void RegisterAction<T1, T2, T3, T4, T5, T6, T7>(string rpcID, Action<T1, T2, T3, T4, T5, T6, T7> action)
    {
        _actions[rpcID] = action;
    }

    public void RegisterAction<T1, T2, T3, T4, T5, T6, T7, T8>(string rpcID, Action<T1, T2, T3, T4, T5, T6, T7, T8> action)
    {
        _actions[rpcID] = action;
    }

    public void UnregisterAction(string rpcID)
    {
        _actions.Remove(rpcID);
    }

    public void Invoke<T1, T2, T3, T4, T5>(string rpcID, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        if (_actions.TryGetValue(rpcID, out var action) && action is Action<T1, T2, T3, T4, T5> typedAction)
        {
            typedAction(arg1, arg2, arg3, arg4, arg5);
        }
    }

    public void Invoke<T1, T2, T3, T4, T5, T6, T7, T8>(
        string rpcID,
        T1 arg1,
        T2 arg2,
        T3 arg3,
        T4 arg4,
        T5 arg5,
        T6 arg6,
        T7 arg7,
        T8 arg8)
    {
        if (_actions.TryGetValue(rpcID, out var action) &&
            action is Action<T1, T2, T3, T4, T5, T6, T7, T8> typedAction)
        {
            typedAction(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }
    }
}
