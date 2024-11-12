using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewUI.Graphics;
using StardewUI.Input;
using StardewUI.Layout;
using StardewValley;

namespace StardewUI.Widgets.Keybinding;

/// <summary>
/// Display widget for a single <see cref="Keybind"/> showing all required keys.
/// </summary>
public class KeybindView : ComponentView<Lane>
{
    /// <summary>
    /// Default setting for <see cref="ButtonHeight"/>.
    /// </summary>
    public const int DEFAULT_BUTTON_HEIGHT = 64;

    /// <summary>
    /// The height for button images/sprites. Images are scaled uniformly, preserving source aspect ratio.
    /// </summary>
    public int ButtonHeight
    {
        get => this.buttonHeight;
        set
        {
            if (value == this.buttonHeight)
            {
                return;
            }

            this.buttonHeight = value;
            this.UpdateContent();
            this.OnPropertyChanged(nameof(this.ButtonHeight));
        }
    }

    /// <summary>
    /// Minimum width for button images/sprites, used if the layout width would be less than that implied by the
    /// <see cref="ButtonHeight"/> and placeholder content (if any).
    /// </summary>
    public int? ButtonMinWidth
    {
        get => this.buttonMinWidth;
        set
        {
            if (value == this.buttonMinWidth)
            {
                return;
            }

            this.buttonMinWidth = value;
            this.UpdateContent();
            this.OnPropertyChanged(nameof(this.ButtonMinWidth));
        }
    }

    /// <summary>
    /// Placeholder text to display when the current keybind is empty.
    /// </summary>
    public string EmptyText
    {
        get => this.emptyText;
        set
        {
            if (value == this.emptyText)
            {
                return;
            }

            this.emptyText = value;
            if (this.View?.Children.Count == 1 && this.View.Children[0] is Label label)
            {
                label.Text = value;
            }

            this.OnPropertyChanged(nameof(this.EmptyText));
        }
    }

    /// <summary>
    /// Font used to display text in button/key placeholders.
    /// </summary>
    /// <remarks>
    /// Only applies for buttons that use a placeholder sprite (i.e. set the <c>isPlaceholder</c> output of
    /// <see cref="ISpriteMap{T}.Get(T, out bool)"/> to <c>true</c>). In these cases, the actual button text drawn
    /// inside the sprite will be drawn using the specified font.
    /// </remarks>
    public SpriteFont Font
    {
        get => this.font;
        set
        {
            if (value == this.font)
            {
                return;
            }

            this.font = value;
            this.UpdateContent();
            this.OnPropertyChanged(nameof(this.Font));
        }
    }

    /// <summary>
    /// The current keybind.
    /// </summary>
    public Keybind Keybind
    {
        get => this.keybind;
        set
        {
            if (value.Buttons.SequenceEqual(this.keybind.Buttons))
            {
                return;
            }

            this.keybind = value;
            this.UpdateContent();
            this.OnPropertyChanged(nameof(this.Keybind));
        }
    }

    /// <inheritdoc cref="View.Margin" />
    public Edges Margin
    {
        get => this.View.Margin;
        set => this.View.Margin = value;
    }

    /// <summary>
    /// Extra spacing between displayed button sprites, if the sprites do not have implicit wide margins.
    /// </summary>
    public float Spacing
    {
        get => this.spacing;
        set
        {
            if (value == this.spacing)
            {
                return;
            }

            this.spacing = value;
            this.UpdateContent();
            this.OnPropertyChanged(nameof(this.Spacing));
        }
    }

    /// <summary>
    /// Map of bindable buttons to sprite representations.
    /// </summary>
    public ISpriteMap<SButton>? SpriteMap
    {
        get => this.spriteMap;
        set
        {
            if (value == this.spriteMap)
            {
                return;
            }

            this.spriteMap = value;
            this.UpdateContent();
            this.OnPropertyChanged(nameof(this.SpriteMap));
        }
    }

    /// <summary>
    /// Text color for the button text inside any placeholder sprites.
    /// </summary>
    public Color TextColor
    {
        get => this.textColor;
        set
        {
            if (value == this.textColor)
            {
                return;
            }

            this.textColor = value;
            if (this.View?.Children.Count == 1 && this.View.Children[0] is Label label)
            {
                label.Color = value;
            }

            this.OnPropertyChanged(nameof(this.TextColor));
        }
    }

    /// <summary>
    /// Color to tint the background/sprite of each key.
    /// </summary>
    public Color TintColor
    {
        get => this.tintColor;
        set
        {
            if (value == this.tintColor)
            {
                return;
            }

            this.tintColor = value;
            this.UpdateTint();
            this.OnPropertyChanged(nameof(this.TintColor));
        }
    }

    private int buttonHeight = DEFAULT_BUTTON_HEIGHT;
    private int? buttonMinWidth = null;
    private string emptyText = "";
    private SpriteFont font = Game1.smallFont;
    private Keybind keybind = new();
    private float spacing;
    private ISpriteMap<SButton>? spriteMap;
    private Color textColor = Game1.textColor;
    private Color tintColor = Color.White;

    /// <inheritdoc />
    protected override Lane CreateView()
    {
        var lane = new Lane() { Layout = LayoutParameters.FitContent(), VerticalContentAlignment = Alignment.Middle };
        this.UpdateContent(lane);
        return lane;
    }

    private IView CreateButtonImage(SButton button)
    {
        bool isPlaceholder = false;
        var sprite = this.SpriteMap?.Get(button, out isPlaceholder);
        var image = new Image()
        {
            Layout = new()
            {
                Width = Length.Content(),
                Height = Length.Px(this.buttonHeight),
                MinWidth = this.buttonMinWidth,
            },
            Fit = isPlaceholder ? ImageFit.Stretch : ImageFit.Contain,
            Sprite = sprite,
            Tint = this.tintColor,
        };
        return isPlaceholder ? this.FillPlaceholder(image, button) : image;
    }

    private IView FillPlaceholder(Image image, SButton button)
    {
        var label = Label.Simple(ButtonName.ForButton(button), this.font, this.textColor);
        label.Margin = (image.Sprite!.FixedEdges ?? Edges.NONE) * (image.Sprite!.SliceSettings?.Scale ?? 1);
        image.Layout = new()
        {
            Width = Length.Stretch(),
            Height = image.Layout.Height,
            MinWidth = this.buttonMinWidth,
        };
        return new Panel()
        {
            Layout = LayoutParameters.FitContent(),
            HorizontalContentAlignment = Alignment.Middle,
            VerticalContentAlignment = Alignment.Middle,
            Children = [image, label],
        };
    }

    private void UpdateContent(Lane? lane = null)
    {
        lane ??= this.View;
        lane.Children = this.keybind.IsBound
            ? this.keybind
                .Buttons.SelectMany(button => new IView[] { Label.Simple("+", this.font), this.CreateButtonImage(button) })
                .Skip(1)
                .ToList()
            : [Label.Simple(this.EmptyText, this.font)];
        this.UpdateTint(lane);
    }

    private void UpdateTint()
    {
        if (this.View is not null)
        {
            this.UpdateTint(this.View);
        }
    }

    private void UpdateTint(IView view)
    {
        if (view is Image image)
        {
            image.Tint = this.tintColor;
            return;
        }
        foreach (var child in view.GetChildren())
        {
            this.UpdateTint(child.View);
        }
    }
}
