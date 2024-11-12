using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewUI.Graphics;
using StardewUI.Input;
using StardewUI.Layout;

namespace StardewUI.Widgets;

/// <summary>
/// Simple unidirectional layout that draws multiple child views in a row or column arrangement.
/// </summary>
public class Lane : View
{
    /// <summary>
    /// Child views to display in this layout.
    /// </summary>
    public IList<IView> Children
    {
        get => this.children;
        set
        {
            if (this.children.SetItems(value))
            {
                this.OnPropertyChanged(nameof(this.Children));
                this.OnPropertyChanged(nameof(this.VisibleChildren));
            }
        }
    }

    /// <summary>
    /// Specifies how to align the <see cref="Children"/> horizontally within the lane's area. Only has an effect if the
    /// total content area is larger than the content size, i.e. when <see cref="LayoutParameters.Width"/> does
    /// <i>not</i> use <see cref="LengthType.Content"/>.
    /// </summary>
    public Alignment HorizontalContentAlignment
    {
        get => this.horizontalContentAlignment.Value;
        set
        {
            if (this.horizontalContentAlignment.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.HorizontalContentAlignment));
            }
        }
    }

    /// <summary>
    /// The layout orientation.
    /// </summary>
    public Orientation Orientation
    {
        get => this.orientation.Value;
        set
        {
            if (this.orientation.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.Orientation));
            }
        }
    }

    /// <summary>
    /// Specifies how to align the <see cref="Children"/> vertically within the lane's area. Only has an effect if the
    /// total content area is larger than the content size, i.e. when <see cref="LayoutParameters.Height"/> does
    /// <i>not</i> use <see cref="LengthType.Content"/>.
    /// </summary>
    public Alignment VerticalContentAlignment
    {
        get => this.verticalContentAlignment.Value;
        set
        {
            if (this.verticalContentAlignment.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.VerticalContentAlignment));
            }
        }
    }

    /// <summary>
    /// The children that have received layout and have at least some content visible.
    /// </summary>
    public IEnumerable<IView> VisibleChildren => this.Children.Take(this.visibleChildCount);

    private readonly DirtyTrackingList<IView> children = [];
    private readonly DirtyTracker<Alignment> horizontalContentAlignment = new(Alignment.Start);
    private readonly DirtyTracker<Orientation> orientation = new(Orientation.Horizontal);
    private readonly DirtyTracker<Alignment> verticalContentAlignment = new(Alignment.Start);
    private readonly List<ViewChild> visibleChildPositions = [];

    private Vector2 childrenSize;
    private int visibleChildCount;

    /// <inheritdoc />
    protected override FocusSearchResult? FindFocusableDescendant(Vector2 contentPosition, Direction direction)
    {
        // No matter what the navigation direction is, if the cursor is already within one of the children, then we
        // should perform a recursive focus search on that child before doing anything else, so that delegation works
        // properly.
        int nearestChildIndex = this.FindNearestChildIndex(contentPosition);
        if (nearestChildIndex < 0)
        {
            this.LogFocusSearch("Couldn't find any matching children; ending search.");
            return null;
        }

        var nearestChild = this.visibleChildPositions[nearestChildIndex];
        this.LogFocusSearch(
            $"Nearest child '{nearestChild.View.Name}' is at index {nearestChildIndex} with bounds: "
                + $"[{nearestChild.Position}, {nearestChild.View.OuterSize}]"
        );
        var nearestResult = nearestChild.FocusSearch(contentPosition, direction);
        var nearestTarget = nearestResult?.Target;
        if (nearestTarget is not null)
        {
            this.LogFocusSearch(
                $"Nearest child '{nearestChild.View.Name}' matched the query: '{nearestTarget.View.Name}' with "
                    + $"bounds: [{nearestTarget.Position}, {nearestTarget.View.OuterSize}]"
            );
        }
        else
        {
            this.LogFocusSearch($"Nearest child '{nearestChild.View.Name}' does NOT match the current query.");
        }
        // The search result from the nearest child is always where we want to start, but not always where we want to
        // finish. If the cursor is actually within the child bounds, then it's always correct, but otherwise we have
        // to account for the fact that distance to "descendant focusable" may not be the same as distance to to the
        // child itself; that is, if the children are themselves layout views, then the nearest child may have its only
        // focusable element be much farther away than the second-nearest child.
        //
        // Fortunately it is always going to be either the nearest or second-nearest match, if we assume non-overlapping
        // layout.
        if (nearestTarget is not null && nearestChild.ContainsPoint(contentPosition))
        {
            this.LogFocusSearch(
                $"Returning nearest child's result '{nearestTarget.View.Name}' since it contains the specified point "
                    + "and has its own match."
            );
            return nearestResult;
        }

        // At this point we either didn't find anything focusable in the nearest child, or aren't sure if it's the best
        // match. We proceed differently depending on whether the direction is along the orientation axis, or orthogonal
        // to it. The parallel case is easier; we only need to traverse the list in the specified direction until
        // something finds focus - which could be the result we already have, the only catch being that the "nearest"
        // element might be on the wrong side, so we have to check.
        if (direction.GetOrientation() == this.Orientation)
        {
            this.LogFocusSearch($"Nearest child wasn't exact match; searching ALONG the orientation ({this.Orientation}) axis.");
            if (nearestTarget is not null && IsCorrectDirection(contentPosition, nearestTarget, direction))
            {
                this.LogFocusSearch(
                    $"Nearest child's result '{nearestTarget.View.Name}' is already in the requested direction "
                        + $"({direction} of {contentPosition}); returning it."
                );
                return nearestResult;
            }
            int searchStep = IsReverseDirection(direction) ? -1 : 1;
            for (int i = nearestChildIndex + searchStep; i >= 0 && i < this.visibleChildCount; i += searchStep)
            {
                var searchChild = this.visibleChildPositions[i];
                this.LogFocusSearch($"Searching next child '{searchChild.View.Name}' at index {i}");
                var childResult = searchChild.FocusSearch(contentPosition, direction);
                if (childResult is not null)
                {
                    this.LogFocusSearch(
                        $"Found a match: '{searchChild.View.Name}' matched the query: "
                            + $"'{childResult.Target.View.Name}' with bounds: "
                            + $"[{childResult.Target.Position}, {childResult.Target.View.OuterSize}]"
                    );
                    return childResult;
                }

                this.LogFocusSearch("No result at this position.");
            }

            this.LogFocusSearch("No matches anywhere in this lane.");
            return null;
        }

        // Perpendicular to the orientation is more intuitive visually, but trickier to implement. We have to search in
        // both directions to be sure we've found the closest point. If we're willing to accept a bit of redundancy then
        // a relatively simple approach is to just fan out until we find we're getting farther away.
        //
        // One useful caveat is that if the cursor is already inside the bounds of one of the children when moving this
        // way, as opposed to entering the bounds of the entire lane from a different view entirely, then we don't allow
        // the movement, otherwise nonintuitive things can happen like moving UP several views when the RIGHT button is
        // pressed.
        this.LogFocusSearch($"Nearest child wasn't exact match; searching OPPOSITE the orientation ({this.Orientation}) axis.");
        if (nearestChild.ContainsPoint(contentPosition))
        {
            this.LogFocusSearch("Nearest child already contains the requested position; no more possible results.");
            return null;
        }
        float nearestDistance = nearestTarget is not null
            ? GetDistance(contentPosition, nearestTarget, this.Orientation)
            : float.PositiveInfinity;
        var ahead = (index: nearestChildIndex, distance: nearestDistance, result: nearestResult);
        var behind = ahead;
        this.LogFocusSearch("Starting distance search in forward direction");
        for (int i = nearestChildIndex + 1; i < this.visibleChildCount; i++)
        {
            var nextChild = this.visibleChildPositions[i];
            this.LogFocusSearch($"Next candidate is '{nextChild.View.Name}' at position {i}");
            var nextResult = nextChild.FocusSearch(contentPosition, direction);
            float nextDistance = nextResult is not null
                ? MathF.Abs(GetDistance(contentPosition, nextResult.Target, this.Orientation))
                : float.PositiveInfinity;
            if (nextResult is not null)
            {
                this.LogFocusSearch(
                    $"Found possible match: '{nextResult.Target.View.Name}' with distance {nextDistance} and bounds: "
                        + $"[{nextResult.Target.Position}, {nextResult.Target.View.OuterSize}]"
                );
            }
            else
            {
                this.LogFocusSearch("No matches in this child.");
            }
            if (nextDistance < ahead.distance)
            {
                this.LogFocusSearch($"Distance {nextDistance} is smaller than best ahead distance {ahead.distance}.");
                ahead = (i, nextDistance, nextResult);
            }
            else if (!float.IsPositiveInfinity(nextDistance))
            {
                // We found something to focus, but it's farther away, so stop searching since everything else we
                // subsequently find will be even farther.
                break;
            }
        }

        this.LogFocusSearch("Starting distance search in reverse direction");
        for (int i = nearestChildIndex - 1; i >= 0; i--)
        {
            var nextChild = this.visibleChildPositions[i];
            this.LogFocusSearch($"Next candidate is {nextChild.View.Name} at position {i}");
            var prevResult = nextChild.FocusSearch(contentPosition, direction);
            float prevDistance = prevResult is not null
                ? MathF.Abs(GetDistance(contentPosition, prevResult.Target, this.Orientation))
                : float.PositiveInfinity;
            if (prevResult is not null)
            {
                this.LogFocusSearch(
                    $"Found possible match: '{prevResult.Target.View.Name}' with distance {prevDistance} and bounds: "
                        + $"[{prevResult.Target.Position}, {prevResult.Target.View.OuterSize}]"
                );
            }
            else
            {
                this.LogFocusSearch("No matches in this child.");
            }
            if (prevDistance < behind.distance)
            {
                this.LogFocusSearch($"Distance {prevDistance} is smaller than best behind distance {behind.distance}.");
                behind = (i, prevDistance, prevResult);
            }
            else if (!float.IsPositiveInfinity(prevDistance))
            {
                break;
            }
        }
        if (ahead.result is null && behind.result is null)
        {
            this.LogFocusSearch("Didn't find any matches either behind or ahead of the nearest position");
            return null;
        }

        this.LogFocusSearch(
            "Matches found:\n"
                + $"  Ahead: '{ahead.result?.Target.View.Name}' [{ahead.result?.Target.Position}, "
                + $"{ahead.result?.Target.View.OuterSize}] distance={ahead.distance}\n"
                + $"  Behind: '{behind.result?.Target.View.Name}' [{behind.result?.Target.Position}, "
                + $"{behind.result?.Target.View.OuterSize}] distance={behind.distance}"
        );
        return ahead.distance < behind.distance ? ahead.result : behind.result;
    }

    /// <inheritdoc />
    protected override IEnumerable<ViewChild> GetLocalChildren()
    {
        return this.visibleChildPositions;
    }

    /// <inheritdoc />
    protected override bool IsContentDirty()
    {
        return this.orientation.IsDirty
            || this.horizontalContentAlignment.IsDirty
            || this.verticalContentAlignment.IsDirty
            || this.children.IsDirty
            || this.visibleChildPositions.Any(child => child.View.IsDirty());
    }

    /// <inheritdoc />
    protected override void OnDrawContent(ISpriteBatch b)
    {
        foreach (var (child, position) in this.visibleChildPositions.OrderBy(child => child.View.ZIndex))
        {
            using var _ = b.SaveTransform();
            b.Translate(position);
            child.Draw(b);
        }
    }

    /// <inheritdoc />
    protected override void OnMeasure(Vector2 availableSize)
    {
        var limits = this.Layout.GetLimits(availableSize);
        var swapOrientation = this.Orientation.Swap();
        this.childrenSize = Vector2.Zero;
        int previousVisibleChildCount = this.visibleChildCount;
        this.visibleChildCount = 0;

        void measureChild(IView child, Vector2 childLimits, bool isDeferred)
        {
            bool hasSwapOrientationStretch = swapOrientation.Length(child.Layout).Type == LengthType.Stretch;
            if (hasSwapOrientationStretch && (!isDeferred || swapOrientation.Length(this.Layout).Type == LengthType.Content))
            {
                float swapLength = swapOrientation.Get(this.childrenSize);
                if (swapLength > 0)
                {
                    swapOrientation.Set(ref childLimits, swapLength);
                }
            }
            child.Measure(childLimits);
            float outerLength = this.Orientation.Get(child.OuterSize);
            this.Orientation.Update(ref limits, v => v - outerLength);
            this.Orientation.Update(ref this.childrenSize, v => v + outerLength);
            if (!hasSwapOrientationStretch)
            {
                swapOrientation.Update(ref this.childrenSize, v => MathF.Max(v, swapOrientation.Get(child.OuterSize)));
            }

            this.visibleChildCount++;
        }

        var deferredChildren = new List<IView>();
        foreach (var child in this.Children)
        {
            if (child.Layout.Width.Type == LengthType.Stretch || child.Layout.Height.Type == LengthType.Stretch)
            {
                // Stretched views have special treatment. A lane should be able to have fixed-size views and then one
                // or more stretched views that use the *remaining* space (instead of greedily consuming the entire
                // space). We'll first process all the other children, then figure out how long the stretches can be.
                //
                // Children stretched along the opposite axis also receive special, but different treatment. Instead of
                // expanding to fill all the available size, they should be limited to the max size of other children.
                //
                // It's possible to end up with combinations that aren't unambiguously resolvable, e.g. if one child
                // uses stretched width and has a content height that depends on width, and another child uses stretched
                // height with width depending on height. We aren't going to implement a huge system of byzantine rules
                // here like CSS, just enough to get the majority of cases correct; unusual/exceptional cases can be
                // handled by simply requiring the caller to be more precise, i.e. specify more fixed dimensions.
                deferredChildren.Add(child);
                continue;
            }
            measureChild(child, limits, false);
        }
        if (deferredChildren.Count > 0)
        {
            var deferredLimits = limits;
            if (this.childrenSize != Vector2.Zero)
            {
                float swapLimit = swapOrientation
                    .Length(this.Layout)
                    .Resolve(swapOrientation.Get(availableSize), () => swapOrientation.Get(this.childrenSize));
                swapOrientation.Set(ref deferredLimits, swapLimit);
            }
            foreach (var child in deferredChildren)
            {
                this.Orientation.Set(ref deferredLimits, this.Orientation.Get(limits));
                measureChild(child, deferredLimits, true);
            }
        }

        if (swapOrientation.Get(this.childrenSize) == 0 && this.Children.Count > 0)
        {
            swapOrientation.Set(ref this.childrenSize, this.Children.Max(c => swapOrientation.Get(c.OuterSize)));
        }

        this.ContentSize = this.Layout.Resolve(availableSize, () => this.childrenSize);
        this.UpdateVisibleChildPositions();

        if (this.visibleChildCount != previousVisibleChildCount)
        {
            this.OnPropertyChanged(nameof(this.VisibleChildren));
        }
    }

    /// <inheritdoc />
    protected override void ResetDirty()
    {
        this.horizontalContentAlignment.ResetDirty();
        this.verticalContentAlignment.ResetDirty();
        this.children.ResetDirty();
        this.orientation.ResetDirty();
    }

    private int FindNearestChildIndex(Vector2 position)
    {
        // Child positions are sorted, so we could technically do a binary search. For the number of elements typically
        // found in a lane, it's probably not worth the extra complexity.
        float axisPosition = this.Orientation.Get(position);
        int bestIndex = -1;
        float maxDistance = float.PositiveInfinity;
        for (int i = 0; i < this.visibleChildCount; i++)
        {
            var child = this.visibleChildPositions[i];
            float minExtent = this.Orientation.Get(child.Position);
            float maxExtent = this.Orientation.Get(child.Position + child.View.OuterSize);
            float distance =
                (axisPosition >= minExtent && axisPosition < maxExtent)
                    ? 0
                    : MathF.Min(MathF.Abs(axisPosition - minExtent), MathF.Abs(axisPosition - maxExtent));
            if (distance < maxDistance)
            {
                maxDistance = distance;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    private static float GetDistance(Vector2 position, ViewChild target, Orientation orientation)
    {
        float axisPosition = orientation.Get(position);
        float minExtent = orientation.Get(target.Position);
        float maxExtent = orientation.Get(target.Position + target.View.OuterSize);
        if (axisPosition >= minExtent && axisPosition < maxExtent)
        {
            return 0;
        }
        float distanceToMin = axisPosition - minExtent;
        float distanceToMax = axisPosition - maxExtent;
        // Note: Doing it this way preserves the sign, so other helpers like IsCorrectDirection can determine which side
        // the child is on.
        return MathF.Abs(distanceToMin) < MathF.Abs(distanceToMax) ? distanceToMin : distanceToMax;
    }

    private static bool IsCorrectDirection(Vector2 position, ViewChild child, Direction direction)
    {
        float distance = GetDistance(position, child, direction.GetOrientation());
        // The distance is measured from child to position, so a negative distance corresponds to a positive direction.
        return float.IsNegative(distance) ^ IsReverseDirection(direction);
    }

    // Whether a direction is the reverse of the iteration order of the child list.
    private static bool IsReverseDirection(Direction direction)
    {
        return direction == Direction.North || direction == Direction.West;
    }

    private void UpdateVisibleChildPositions()
    {
        this.visibleChildPositions.Clear();
        if (this.Orientation == Orientation.Horizontal)
        {
            float x = this.HorizontalContentAlignment.Align(this.childrenSize.X, this.ContentSize.X);
            foreach (var child in this.VisibleChildren)
            {
                float y = this.VerticalContentAlignment.Align(child.OuterSize.Y, this.ContentSize.Y);
                this.visibleChildPositions.Add(new(child, new(x, y)));
                x += child.OuterSize.X;
            }
        }
        else
        {
            float y = this.VerticalContentAlignment.Align(this.childrenSize.Y, this.ContentSize.Y);
            foreach (var child in this.VisibleChildren)
            {
                float x = this.HorizontalContentAlignment.Align(child.OuterSize.X, this.ContentSize.X);
                this.visibleChildPositions.Add(new(child, new(x, y)));
                y += child.OuterSize.Y;
            }
        }
    }
}
