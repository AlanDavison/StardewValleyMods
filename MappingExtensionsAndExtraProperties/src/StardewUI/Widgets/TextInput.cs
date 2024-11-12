using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewUI.Animation;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Input;
using StardewUI.Layout;
using StardewValley;
using StardewValley.Menus;

namespace StardewUI.Widgets;

/// <summary>
/// A text input field that allows typing from a physical or virtual keyboard.
/// </summary>
public class TextInput : View
{
    /// <summary>
    /// Event raised when the <see cref="Text"/> changes.
    /// </summary>
    public event EventHandler<EventArgs>? TextChanged;

    /// <inheritdoc cref="Frame.Background"/>
    public Sprite? Background
    {
        get => this.frame.Background;
        set
        {
            if (value != this.frame.Background)
            {
                this.frame.Background = value;
                this.OnPropertyChanged(nameof(this.Background));
            }
        }
    }

    /// <summary>
    /// Gets or sets the thickness of the border edges in the <see cref="Background"/> sprite.
    /// </summary>
    /// <remarks>
    /// This is similar to <see cref="Frame.Border"/> but assumes that the border is part of the background, rather than
    /// a separate sprite. Setting this affects padding of content inside the background.
    /// </remarks>
    public Edges BorderThickness
    {
        get => this.frame.Padding;
        set
        {
            if (value != this.frame.Padding)
            {
                this.frame.Padding = value;
                this.OnPropertyChanged(nameof(this.BorderThickness));
            }
        }
    }

    /// <summary>
    /// Sprite to draw for the cursor showing the current text position.
    /// </summary>
    public Sprite? Caret
    {
        get => this.caret.Sprite;
        set
        {
            if (value != this.caret.Sprite)
            {
                this.caret.Sprite = value;
                this.OnPropertyChanged(nameof(this.Caret));
            }
        }
    }

    /// <summary>
    /// The zero-based position of the caret within the text.
    /// </summary>
    /// <remarks>
    /// This value is the string position; e.g. if the <see cref="Text"/> has a length of 5, and the current caret
    /// position is 2, then the caret is between the 2nd and 3rd characters. The value cannot be greater than the length
    /// of the current text.
    /// </remarks>
    public int CaretPosition
    {
        get => this.labelBeforeCursor.Text.Length;
        set
        {
            if (this.SetCaretPosition(value))
            {
                this.OnPropertyChanged(nameof(this.CaretPosition));
            }
        }
    }

    /// <summary>
    /// The width to draw the <see cref="Caret"/>, if different from the sprite's source width.
    /// </summary>
    public float? CaretWidth
    {
        get => this.caret.Layout.Width.Type == LengthType.Px ? this.caret.Layout.Width.Value : null;
        set
        {
            if (value != this.CaretWidth)
            {
                this.caret.Layout = new()
                {
                    Width = value.HasValue ? Length.Px(value.Value) : Length.Content(),
                    Height = Length.Stretch(),
                };
                this.OnPropertyChanged(nameof(this.CaretWidth));
            }
        }
    }

    /// <summary>
    /// The font with which to render text. Defaults to <see cref="Game1.smallFont"/>.
    /// </summary>
    public SpriteFont Font
    {
        get => this.labelAfterCursor.Font;
        set
        {
            if (value != this.labelAfterCursor.Font || value != this.labelBeforeCursor.Font)
            {
                this.labelAfterCursor.Font = value;
                this.labelBeforeCursor.Font = value;
                this.OnPropertyChanged(nameof(this.Font));
            }
        }
    }

    /// <summary>
    /// The maximum number of characters allowed in this field.
    /// </summary>
    /// <remarks>
    /// The default value is <c>0</c> which does not impose any limit.
    /// </remarks>
    public int MaxLength
    {
        get => this.maxLength;
        set
        {
            if (value != this.maxLength)
            {
                this.maxLength = value;
                if (value > 0 && this.Text.Length > value)
                {
                    this.Text = this.Text[..value];
                }

                this.OnPropertyChanged(nameof(this.MaxLength));
            }
        }
    }

    /// <inheritdoc cref="Frame.ShadowAlpha"/>
    public float ShadowAlpha
    {
        get => this.frame.ShadowAlpha;
        set
        {
            if (value != this.frame.ShadowAlpha)
            {
                this.frame.ShadowAlpha = value;
                this.OnPropertyChanged(nameof(this.ShadowAlpha));
            }
        }
    }

    /// <inheritdoc cref="Frame.ShadowOffset"/>
    public Vector2 ShadowOffset
    {
        get => this.frame.ShadowOffset;
        set
        {
            if (value != this.frame.ShadowOffset)
            {
                this.frame.ShadowOffset = value;
                this.OnPropertyChanged(nameof(this.ShadowOffset));
            }
        }
    }

    /// <summary>
    /// Color of displayed text, as well as the <see cref="Caret"/> tint color.
    /// </summary>
    public Color TextColor
    {
        get => this.labelBeforeCursor.Color;
        set
        {
            if (value != this.labelBeforeCursor.Color)
            {
                this.labelBeforeCursor.Color = value;
                this.labelAfterCursor.Color = value;
                this.caret.Tint = value;
                this.OnPropertyChanged(nameof(this.TextColor));
            }
        }
    }

    /// <summary>
    /// The text currently entered.
    /// </summary>
    /// <remarks>
    /// Setting this to a new value will reset the caret position to the end of the text.
    /// </remarks>
    public string Text
    {
        get => this.labelBeforeCursor.Text + this.labelAfterCursor.Text;
        set => this.SetText(value);
    }

    // A very small positive offset we add to the search position when trying to move the caret to the mouse cursor.
    // In general, the caret should always move BEFORE the character that was clicked on; however, this has a tendency
    // to "overshoot" if the user tries to click between two characters (as many are accustomed to doing). To
    // compensate, we shift the position slightly to the right.
    //
    // Note: We have to be careful not to overdo this in case of a very thin character, like "i" or "l". If the offset
    // is bigger or almost as big as the actual character width, we'll just miss it entirely.
    private const float CARET_SEARCH_OFFSET = 4.0f;

    private readonly Image caret;
    private readonly Animator<Image, Visibility> caretBlinkAnimator;
    private readonly Frame frame;
    private readonly Label labelAfterCursor;
    private readonly Label labelBeforeCursor;
    private readonly TextBoxInterceptor textBoxInterceptor;
    private readonly TextInputSubscriber textInputSubscriber;

    private int maxLength;

    /// <summary>
    /// Initializes a new <see cref="TextInput"/>.
    /// </summary>
    public TextInput()
    {
        this.Focusable = true;

        this.caret = new Image()
        {
            Name = "TextInputCursor",
            Layout = new() { Width = Length.Px(2), Height = Length.Stretch() },
            Margin = new(-2, 0),
            Fit = ImageFit.Stretch,
            Sprite = new(Game1.staminaRect),
            Tint = Game1.textColor,
            Visibility = Visibility.Hidden,
        };
        this.caretBlinkAnimator = Animator.On(this.caret,
            i => i.Visibility,
            (_, _, progress) => progress < 0.5f ? Visibility.Visible : Visibility.Hidden,
            (i, v) => i.Visibility = v
        );
        this.caretBlinkAnimator.Loop = true;
        this.labelBeforeCursor = new()
        {
            Name = "TextInputBeforeCursor",
            Layout = LayoutParameters.FitContent(),
            MaxLines = 1,
        };
        this.labelAfterCursor = new()
        {
            Name = "TextInputAfterCursor",
            Layout = LayoutParameters.FitContent(),
            MaxLines = 1,
        };
        var textLane = new Lane()
        {
            Name = "TextInputContentLane",
            Layout = LayoutParameters.Fill(),
            VerticalContentAlignment = Alignment.Middle,
            Children = [this.labelBeforeCursor, this.caret, this.labelAfterCursor],
        };
        var textBoxSprite = UiSprites.TextBox;
        this.frame = new()
        {
            Layout = LayoutParameters.Fill(),
            Padding = textBoxSprite.FixedEdges ?? new(4),
            Background = textBoxSprite,
            Content = textLane,
        };
        this.textBoxInterceptor = new(this);
        this.textInputSubscriber = new(this, Game1.keyboardFocusInstance.Window);

        this.Font = Game1.smallFont;
        this.TextColor = Game1.textColor;
    }

    /// <inheritdoc />
    protected override IEnumerable<ViewChild> GetLocalChildren()
    {
        return [new(this.frame, Vector2.Zero)];
    }

    /// <inheritdoc />
    protected override bool IsContentDirty()
    {
        return this.frame.IsDirty();
    }

    /// <inheritdoc />
    public override void OnClick(ClickEventArgs e)
    {
        if (e.IsPrimaryButton())
        {
            this.Capture(e.Position);
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnDrawContent(ISpriteBatch b)
    {
        this.frame.Draw(b);
    }

    /// <inheritdoc />
    protected override void OnMeasure(Vector2 availableSize)
    {
        var limits = this.Layout.GetLimits(availableSize);
        this.frame.Measure(limits);
        this.ContentSize = this.Layout.Resolve(availableSize, () => this.frame.OuterSize);
    }

    private void Capture(Vector2 cursorPosition)
    {
        this.Release(); // In case of switch between mouse and controller
        if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse)
        {
            // Vanilla text entry doesn't support moving the caret, so make sure we're at the end.
            this.CaretPosition = this.Text.Length;
            this.textBoxInterceptor.Width = (int)this.OuterSize.X;
            this.textBoxInterceptor.Height = (int)this.OuterSize.Y;
            this.textBoxInterceptor.Selected = true;
            Game1.showTextEntry(this.textBoxInterceptor);
        }
        else
        {
            var searchOrigin = new Vector2(this.BorderThickness.Left - CARET_SEARCH_OFFSET, this.BorderThickness.Top);
            this.MoveCaretToCursor(cursorPosition - searchOrigin);
            this.caretBlinkAnimator.Start(Visibility.Visible, Visibility.Hidden, TimeSpan.FromSeconds(1));
            Game1.keyboardDispatcher.Subscriber = this.textInputSubscriber;
        }
    }

    private void HandleSpecialKey(Keys key)
    {
        switch (key)
        {
            case Keys.Left:
                this.CaretPosition--;
                break;
            case Keys.Right:
                this.CaretPosition++;
                break;
            case Keys.Home:
                this.CaretPosition = 0;
                break;
            case Keys.End:
                this.CaretPosition = this.Text.Length;
                break;
            case Keys.Delete:
                if (this.labelAfterCursor.Text.Length > 0)
                {
                    this.labelAfterCursor.Text = this.labelAfterCursor.Text[1..];
                    this.OnTextChanged();
                }
                break;
        }
    }

    private void Insert(char c)
    {
        switch (c)
        {
            case '\b':
                if (this.labelBeforeCursor.Text.Length > 0)
                {
                    this.labelBeforeCursor.Text = this.labelBeforeCursor.Text[..^1];
                    this.OnTextChanged();
                }
                break;
            case '\t':
            case '\r':
                this.Release();
                break;
            default:
                if (!char.IsControl(c))
                {
                    if (this.MaxLength == 0 || this.Text.Length < this.MaxLength)
                    {
                        this.labelBeforeCursor.Text += c;
                        this.OnTextChanged();
                    }
                }
                break;
        }
    }

    private void Insert(string text)
    {
        if (this.MaxLength > 0)
        {
            int remainingLength = Math.Max(this.MaxLength - this.Text.Length, 0);
            if (text.Length > remainingLength)
            {
                text = text[..remainingLength];
            }
        }
        if (text.Length > 0)
        {
            this.labelBeforeCursor.Text += text;
            this.OnTextChanged();
        }
    }

    private void MoveCaretToCursor(Vector2 position)
    {
        if (position.X < 0 || position.X > this.ContentSize.X || this.Text.Length == 0)
        {
            return;
        }
        // Taking into account proportional widths, bearings, kernings, etc., we know very little about the relationship
        // of pixel positions to character positions and don't want to reimplement the entire font system.
        // A reasonably (?) fast solution should be to actually measure partial strings, using a binary search on the
        // length of the before/after string.
        (int previousCharacterCount, string? labelText, float labelOffset) =
            position.X < this.labelBeforeCursor.OuterSize.X
                ? (0, this.labelBeforeCursor.Text, position.X)
                : (this.labelBeforeCursor.Text.Length, this.labelAfterCursor.Text, position.X - this.labelBeforeCursor.OuterSize.X);
        int searchStart = 0;
        int searchEnd = labelText.Length;
        while (searchStart < searchEnd)
        {
            int searchMid = (int)(MathF.Ceiling((searchStart + searchEnd) / 2.0f));
            string? searchText = labelText[0..searchMid];
            float textWidth = this.Font.MeasureString(searchText).X;
            if (labelOffset < textWidth)
            {
                searchEnd = Math.Min(searchEnd - 1, searchMid);
            }
            else
            {
                searchStart = Math.Max(searchStart + 1, searchMid);
            }
        }
        int finalIndex = searchStart;
        this.CaretPosition = previousCharacterCount + finalIndex;
    }

    private void OnTextChanged()
    {
        this.TextChanged?.Invoke(this, EventArgs.Empty);
        this.OnPropertyChanged(nameof(this.Text));
    }

    private void Release()
    {
        this.textBoxInterceptor.Selected = false;
        this.textInputSubscriber.Selected = false;
        Game1.closeTextEntry();
        this.caretBlinkAnimator.Stop();
        this.caret.Visibility = Visibility.Hidden;
    }

    private bool SetCaretPosition(int position)
    {
        string? fullText = this.Text;
        position = Math.Clamp(position, 0, fullText.Length);
        if (position == this.CaretPosition)
        {
            return false;
        }

        this.labelBeforeCursor.Text = position > 0 ? fullText[0..position] : "";
        this.labelAfterCursor.Text = position < fullText.Length ? fullText[position..] : "";
        return true;
    }

    private void SetText(string text)
    {
        if (text == this.Text)
        {
            return;
        }
        if (this.maxLength > 0 && text.Length > this.maxLength)
        {
            text = text[..this.maxLength];
        }

        this.labelBeforeCursor.Text = text;
        this.labelAfterCursor.Text = "";
        this.OnTextChanged();
    }

    private class TextBoxInterceptor(TextInput owner)
        : TextBox(Game1.staminaRect, Game1.staminaRect, Game1.smallFont, Color.Black),
            ICaptureTarget
    {
        private readonly TextInput owner = owner;

        public IView CapturingView => this.owner;

        public override void Draw(SpriteBatch spriteBatch, bool drawShadow = true)
        {
            var b = new PropagatedSpriteBatch(spriteBatch, Transform.FromTranslation(new(this.X, this.Y)));
            this.owner.Draw(b);
        }

        public override void RecieveCommandInput(char command)
        {
            if (this.Selected)
            {
                this.owner.Insert(command);
            }
        }

        public override void RecieveTextInput(char inputChar)
        {
            if (this.Selected)
            {
                this.owner.Insert(inputChar);
            }
        }

        public override void RecieveTextInput(string text)
        {
            if (this.Selected)
            {
                this.owner.Insert(text);
            }
        }

        public void ReleaseCapture()
        {
            this.owner.Release();
        }
    }

    // Used for when we *don't* have controller input, thus don't use the virtual keyboard, don't want to accidentally
    // incur any side effects of the TextBoxInterceptor.
    private class TextInputSubscriber(TextInput owner, GameWindow window) : ICaptureTarget, IKeyboardSubscriber
    {
        private readonly TextInput owner = owner;
        private readonly GameWindow window = window;

        public bool Selected
        {
            get => this.selected;
            set
            {
                if (value == this.selected)
                {
                    return;
                }

                this.selected = value;
                if (this.selected)
                {
                    Game1.keyboardDispatcher.Subscriber = this;
                    if (PlatformUsesWindowEvents())
                    {
                        this.window.KeyDown += this.Window_KeyDown;
                    }
                    else
                    {
                        KeyboardInput.KeyDown += this.KeyboardInput_KeyDown;
                    }
                }
                else
                {
                    if (PlatformUsesWindowEvents())
                    {
                        this.window.KeyDown -= this.Window_KeyDown;
                    }
                    else
                    {
                        KeyboardInput.KeyDown -= this.KeyboardInput_KeyDown;
                    }
                    if (Game1.keyboardDispatcher.Subscriber == this)
                    {
                        Game1.keyboardDispatcher.Subscriber = null;
                    }
                }
            }
        }

        public IView CapturingView => this.owner;

        private bool selected;

        public void RecieveCommandInput(char command)
        {
            if (this.Selected)
            {
                this.owner.Insert(command);
            }
        }

        public void RecieveSpecialInput(Keys key)
        {
            // KeyboardDispatcher is not consistent about which "special" keys it dispatches, depending on the platform.
            // It's better not to implement this, and instead set up a separate (direct) subscription.
        }

        public void RecieveTextInput(char inputChar)
        {
            if (this.Selected)
            {
                this.owner.Insert(inputChar);
            }
        }

        public void RecieveTextInput(string text)
        {
            if (this.Selected)
            {
                this.owner.Insert(text);
            }
        }

        public void ReleaseCapture()
        {
            this.owner.Release();
        }

        private void KeyboardInput_KeyDown(object sender, KeyEventArgs e)
        {
            this.owner.HandleSpecialKey(e.KeyCode);
        }

        private void Window_KeyDown(object? sender, InputKeyEventArgs e)
        {
            this.owner.HandleSpecialKey(e.Key);
        }

        // Same logic used in KeyboardDispatcher.
        private static bool PlatformUsesWindowEvents()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix
                || Environment.OSVersion.Platform == PlatformID.Win32NT;
        }
    }
}
