using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Input;
using StardewUI.Layout;

namespace StardewUI.Widgets;

/// <inheritdoc cref="DecoratorView{T}"/>
public abstract class DecoratorView : DecoratorView<IView> { }

/// <summary>
/// A view that owns and delegates to an inner view.
/// </summary>
/// <remarks>
/// <para>
/// Decorator views, while not abstract, are used as a base type for other composite views, and primarily intended for
/// framework use. Custom widgets should normally use <see cref="ComponentView{T}"/> instead, which incorporates lazy
/// loading and other conveniences for minimalistic implementations.
/// </para>
/// <para>
/// The inner view is considered to be owned by the decorator; it will be assigned any values that were assigned to the
/// decorator itself, such as <see cref="IView.Layout"/>, and if it implements <see cref="IDisposable"/>, then it will
/// be disposed along with the decorator.
/// </para>
/// </remarks>
/// <typeparam name="T">The specific type of view that the decorator owns.</typeparam>
public class DecoratorView<T> : IView, IDisposable
    where T : class, IView
{
    private interface IDecoratedProperty
    {
        void Init(DecoratorView<T> owner);
        void Update();
    }

    /// <summary>
    /// Helper for propagating a single property to and from the inner view.
    /// </summary>
    /// <remarks>
    /// Decorated properties must be initialized in the decorator's constructor by calling
    /// <see cref="DecoratorView{T}.RegisterDecoratedProperty{TValue}"/>, and have the following behavior:
    /// <list type="number">
    /// <item>The current value is tracked independently of the inner view;</item>
    /// <item>If the current value has <b>not</b> been set, then it is ignored when initializing a new view;</item>
    /// <item>If the current value <b>has</b> been set, the view's value is overwritten on initialization.</item>
    /// </list>
    /// </remarks>
    /// <typeparam name="TValue">The type of value tracked.</typeparam>
    /// <param name="getValue">Function to retrieve the current value from the inner view.</param>
    /// <param name="setValue">Delegate to change the current value on the inner view.</param>
    /// <param name="defaultValue">The initial value to return from <see cref="Get()"/> if no view exists and the value
    /// has not been changed. This is never written to the view, it is only used by <see cref="Get()"/> and is
    /// effectively a hack to allow <see cref="DecoratedProperty{TValue}"/> to deal with value (struct) types.</param>
    protected sealed class DecoratedProperty<TValue>(
        Func<T, TValue> getValue,
        Action<T, TValue> setValue,
        TValue defaultValue
    ) : IDecoratedProperty
    {
        private DecoratorView<T>? owner;
        private bool isValueSet;
        private TValue value = defaultValue;

        /// <summary>
        /// Gets the current value from the inner view.
        /// </summary>
        /// <returns>The value from the current view, if the view is non-null; otherwise, the default value configured
        /// for this property.</returns>
        public TValue Get()
        {
            this.EnsureInitialized();
            return this.owner.view is not null ? getValue(this.owner.view) : this.value;
        }

        /// <summary>
        /// Updates the property value, also updating the inner view if one exists.
        /// </summary>
        /// <remarks>
        /// If the inner view has not been created yet, then its corresponding property will be updated as soon as it is
        /// assigned to the <see cref="DecoratorView{T}.View"/>.
        /// </remarks>
        /// <param name="value">The new value.</param>
        public void Set(TValue value)
        {
            this.EnsureInitialized();
            this.value = value;
            this.isValueSet = true;
            if (this.owner.view is not null)
            {
                setValue(this.owner.view, value);
            }
        }

        /// <summary>
        /// Updates the inner view's property to the most recent value, if one has been set on the decorated property.
        /// </summary>
        /// <remarks>
        /// If there have been no calls to <see cref="Set(TValue)"/>, then the view is left untouched, to preserve any
        /// non-default settings on the inner view.
        /// </remarks>
        public void Update()
        {
            this.EnsureInitialized();
            if (this.owner.view is not null)
            {
                if (this.isValueSet)
                {
                    setValue(this.owner.view, this.value!);
                }
                else
                {
                    // Update our own value here, but *don't* change isValueSet.
                    //
                    // That way, if anything external reads the property using `Get`, it will have the same value as the
                    // inner view; but also, if the inner view is swapped out at some point without the property ever
                    // having been set explicitly, then values from the old view don't "stick" to the new view.
                    this.value = getValue(this.owner.view);
                }
            }
        }

        [MemberNotNull(nameof(owner))]
        private void EnsureInitialized()
        {
            if (this.owner is null)
            {
                throw new InvalidOperationException($"{nameof(DecoratedProperty<T>)} has not been initialized.");
            }
        }

        void IDecoratedProperty.Init(DecoratorView<T> owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Converts a <see cref="DecoratedProperty{TValue}"/> to its corresponding value type.
        /// </summary>
        /// <param name="property">The decorated property.</param>
        public static implicit operator TValue(DecoratedProperty<TValue> property) => property.Get();
    }

    /// <inheritdoc />
    public Bounds ActualBounds => this.view?.ActualBounds ?? Bounds.Empty;

    /// <inheritdoc />
    public Bounds ContentBounds => this.view?.ContentBounds ?? Bounds.Empty;

    /// <inheritdoc />
    public IEnumerable<Bounds> FloatingBounds => this.view?.FloatingBounds ?? [];

    /// <inheritdoc />
    public bool IsFocusable => this.view?.IsFocusable ?? false;

    /// <inheritdoc />
    public LayoutParameters Layout
    {
        get => this.layout;
        set => this.layout.Set(value);
    }

    /// <inheritdoc />
    public string Name
    {
        get => this.name;
        set => this.name.Set(value);
    }

    /// <inheritdoc />
    public Vector2 OuterSize => this.view?.OuterSize ?? Vector2.Zero;

    /// <inheritdoc />
    public bool PointerEventsEnabled
    {
        get => this.pointerEventsEnabled;
        set => this.pointerEventsEnabled.Set(value);
    }

    /// <inheritdoc />
    public Orientation? ScrollWithChildren
    {
        get => this.scrollWithChildren;
        set => this.scrollWithChildren.Set(value);
    }

    /// <inheritdoc />
    public Tags Tags => this.view?.Tags ?? Tags.Empty;

    /// <inheritdoc />
    public string Tooltip
    {
        get => this.tooltip;
        set => this.tooltip.Set(value);
    }

    /// <inheritdoc />
    public Visibility Visibility
    {
        get => this.visibility;
        set => this.visibility.Set(value);
    }

    /// <inheritdoc />
    public int ZIndex
    {
        get => this.zIndex;
        set => this.zIndex.Set(value);
    }

    /// <inheritdoc />
    public event EventHandler<ButtonEventArgs>? ButtonPress;

    /// <inheritdoc />
    public event EventHandler<ClickEventArgs>? Click;

    /// <inheritdoc />
    public event EventHandler<PointerEventArgs>? Drag;

    /// <inheritdoc />
    public event EventHandler<PointerEventArgs>? DragEnd;

    /// <inheritdoc />
    public event EventHandler<PointerEventArgs>? DragStart;

    /// <inheritdoc />
    public event EventHandler<ClickEventArgs>? LeftClick;

    /// <inheritdoc />
    public event EventHandler<PointerEventArgs>? PointerEnter;

    /// <inheritdoc />
    public event EventHandler<PointerEventArgs>? PointerLeave;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public event EventHandler<ClickEventArgs>? RightClick;

    /// <inheritdoc />
    public event EventHandler<WheelEventArgs>? Wheel;

    /// <summary>
    /// The inner view that is decorated by this view.
    /// </summary>
    protected T? View
    {
        get => this.view;
        set => this.SetInnerView(value);
    }

    private readonly List<IDecoratedProperty> decoratedProperties = [];

    private readonly DecoratedProperty<LayoutParameters> layout = new(x => x.Layout, (x, v) => x.Layout = v, new());
    private readonly DecoratedProperty<string> name = new(x => x.Name, (x, v) => x.Name = v, "");
    private readonly DecoratedProperty<bool> pointerEventsEnabled =
        new(x => x.PointerEventsEnabled, (x, v) => x.PointerEventsEnabled = v, true);
    private readonly DecoratedProperty<Orientation?> scrollWithChildren =
        new(x => x.ScrollWithChildren, (x, v) => x.ScrollWithChildren = v, null);
    private readonly DecoratedProperty<string> tooltip = new(x => x.Tooltip, (x, v) => x.Tooltip = v, "");
    private readonly DecoratedProperty<Visibility> visibility =
        new(x => x.Visibility, (x, v) => x.Visibility = v, Visibility.Visible);
    private readonly DecoratedProperty<int> zIndex = new(x => x.ZIndex, (x, v) => x.ZIndex = v, 0);

    private T? view;

    /// <summary>
    /// Initializes a new <see cref="DecoratorView{T}"/> instance.
    /// </summary>
    public DecoratorView()
    {
        this.RegisterDecoratedProperty(this.layout);
        this.RegisterDecoratedProperty(this.name);
        this.RegisterDecoratedProperty(this.pointerEventsEnabled);
        this.RegisterDecoratedProperty(this.scrollWithChildren);
        this.RegisterDecoratedProperty(this.tooltip);
        this.RegisterDecoratedProperty(this.visibility);
        this.RegisterDecoratedProperty(this.zIndex);
    }

    /// <inheritdoc />
    public virtual bool ContainsPoint(Vector2 point)
    {
        return this.view?.ContainsPoint(point) ?? false;
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        this.DetachHandlers();
        if (this.view is IDisposable viewDisposable)
        {
            viewDisposable.Dispose();
        }

        this.view = null;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual void Draw(ISpriteBatch b)
    {
        this.view?.Draw(b);
    }

    /// <inheritdoc />
    public virtual FocusSearchResult? FocusSearch(Vector2 position, Direction direction)
    {
        return this.view?.FocusSearch(position, direction);
    }

    /// <inheritdoc />
    public virtual ViewChild? GetChildAt(Vector2 position)
    {
        return this.view?.ContainsPoint(position) == true ? new(this.view, Vector2.Zero) : null;
    }

    /// <inheritdoc />
    public virtual Vector2? GetChildPosition(IView childView)
    {
        return childView == this.view ? Vector2.Zero : null;
    }

    /// <inheritdoc />
    public virtual IEnumerable<ViewChild> GetChildren()
    {
        return this.view is not null ? [new(this.view, Vector2.Zero)] : [];
    }

    /// <inheritdoc />
    public virtual IEnumerable<ViewChild> GetChildrenAt(Vector2 position)
    {
        return this.view?.ContainsPoint(position) == true ? [new(this.view, Vector2.Zero)] : [];
    }

    /// <inheritdoc />
    public virtual ViewChild? GetDefaultFocusChild()
    {
        return this.view?.GetDefaultFocusChild() ?? (this.view?.IsFocusable == true ? new(this.view, Vector2.Zero) : null);
    }

    /// <inheritdoc />
    public virtual bool HasOutOfBoundsContent()
    {
        return this.view?.HasOutOfBoundsContent() ?? false;
    }

    /// <inheritdoc />
    public virtual bool IsDirty()
    {
        return this.view?.IsDirty() ?? false;
    }

    /// <inheritdoc />
    public virtual bool Measure(Vector2 availableSize)
    {
        bool wasDirty = this.view?.Measure(availableSize) ?? false;
        if (wasDirty)
        {
            this.OnLayout();
        }
        return wasDirty;
    }

    /// <inheritdoc />
    public virtual void OnButtonPress(ButtonEventArgs e)
    {
        this.view?.OnButtonPress(e);
    }

    /// <inheritdoc />
    public virtual void OnClick(ClickEventArgs e)
    {
        this.view?.OnClick(e);
    }

    /// <inheritdoc />
    public virtual void OnDrag(PointerEventArgs e)
    {
        this.view?.OnDrag(e);
    }

    /// <inheritdoc />
    public virtual void OnDrop(PointerEventArgs e)
    {
        this.view?.OnDrop(e);
    }

    /// <inheritdoc />
    public virtual void OnPointerMove(PointerMoveEventArgs e)
    {
        this.view?.OnPointerMove(e);
    }

    /// <inheritdoc />
    public virtual void OnUpdate(TimeSpan elapsed)
    {
        this.view?.OnUpdate(elapsed);
    }

    /// <inheritdoc />
    public virtual void OnWheel(WheelEventArgs e)
    {
        this.view?.OnWheel(e);
    }

    /// <inheritdoc />
    public virtual bool ScrollIntoView(IEnumerable<ViewChild> path, out Vector2 distance)
    {
        distance = default;
        return this.view?.ScrollIntoView(path, out distance) ?? false;
    }

    /// <summary>
    /// Runs whenever layout occurs as a result of the UI elements changing.
    /// </summary>
    protected virtual void OnLayout() { }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        this.PropertyChanged?.Invoke(this, args);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new(propertyName));
    }

    /// <summary>
    /// Registers a <see cref="DecoratedProperty{TValue}"/>.
    /// </summary>
    /// <remarks>
    /// All <see cref="DecoratedProperty{TValue}"/> fields must be registered in the constructor or before they are read
    /// from or written to.
    /// </remarks>
    /// <typeparam name="TValue">The property's value type.</typeparam>
    /// <param name="property">The property to register.</param>
    protected void RegisterDecoratedProperty<TValue>(DecoratedProperty<TValue> property)
    {
        ((IDecoratedProperty)property).Init(this);
        this.decoratedProperties.Add(property);
    }

    private void AttachHandlers()
    {
        if (this.view is null)
        {
            return;
        }

        this.view.ButtonPress += this.View_ButtonPress;
        this.view.Click += this.View_Click;
        this.view.Drag += this.View_Drag;
        this.view.DragEnd += this.View_DragEnd;
        this.view.DragStart += this.View_DragStart;
        this.view.LeftClick += this.View_LeftClick;
        this.view.PointerEnter += this.View_PointerEnter;
        this.view.PointerLeave += this.View_PointerLeave;
        this.view.PropertyChanged += this.View_PropertyChanged;
        this.view.RightClick += this.View_RightClick;
        this.view.Wheel += this.View_Wheel;
    }

    private void DetachHandlers()
    {
        if (this.view is null)
        {
            return;
        }

        this.view.ButtonPress -= this.View_ButtonPress;
        this.view.Click -= this.View_Click;
        this.view.Drag -= this.View_Drag;
        this.view.DragEnd -= this.View_DragEnd;
        this.view.DragStart -= this.View_DragStart;
        this.view.LeftClick -= this.View_LeftClick;
        this.view.PointerEnter -= this.View_PointerEnter;
        this.view.PointerLeave -= this.View_PointerLeave;
        this.view.PropertyChanged -= this.View_PropertyChanged;
        this.view.RightClick -= this.View_RightClick;
        this.view.Wheel -= this.View_Wheel;
    }

    private void SetInnerView(T? innerView)
    {
        if (innerView == this.view)
        {
            return;
        }

        this.DetachHandlers();
        this.view = innerView;
        foreach (var property in this.decoratedProperties)
        {
            property.Update();
        }

        this.AttachHandlers();
    }

    private void View_ButtonPress(object? _, ButtonEventArgs e) => this.ButtonPress?.Invoke(this, e);

    private void View_Click(object? _, ClickEventArgs e) => this.Click?.Invoke(this, e);

    private void View_Drag(object? _, PointerEventArgs e) => this.Drag?.Invoke(this, e);

    private void View_DragEnd(object? _, PointerEventArgs e) => this.DragEnd?.Invoke(this, e);

    private void View_DragStart(object? _, PointerEventArgs e) => this.DragStart?.Invoke(this, e);

    private void View_LeftClick(object? _, ClickEventArgs e) => this.LeftClick?.Invoke(this, e);

    private void View_PointerEnter(object? _, PointerEventArgs e) => this.PointerEnter?.Invoke(this, e);

    private void View_PointerLeave(object? _, PointerEventArgs e) => this.PointerLeave?.Invoke(this, e);

    private void View_PropertyChanged(object? _, PropertyChangedEventArgs e) => this.PropertyChanged?.Invoke(this, e);

    private void View_RightClick(object? _, ClickEventArgs e) => this.RightClick?.Invoke(this, e);

    private void View_Wheel(object? _, WheelEventArgs e) => this.Wheel?.Invoke(this, e);
}
