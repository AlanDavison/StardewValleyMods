using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewUI.Events;
using StardewUI.Graphics;
using StardewUI.Layout;
using StardewUI.Overlays;
using StardewValley;

namespace StardewUI.Widgets;

/// <summary>
/// Editor widget for a <see cref="NineGridPlacement"/>, which brings up a <see cref="PositioningOverlay"/> on click.
/// </summary>
/// <remarks>
/// Appears as a grid with 9 squares, with the "selected" cell according to <see cref="Placement"/> drawn with a
/// different background color. The grid is uniform and the size of each cell is determined by the widget's actual size
/// (by way of its <see cref="IView.Layout"/>) and the current <see cref="LineWidth"/>.
/// </remarks>
public class NineGridPlacementEditor : View
{
    /// <summary>
    /// Map of buttons to button prompt sprites.
    /// </summary>
    public ISpriteMap<SButton>? ButtonSpriteMap { get; set; }

    /// <summary>
    /// The default color to draw grid cells (inside the gridlines).
    /// </summary>
    public Color CellColor
    {
        get => this.cellColor;
        set
        {
            if (value != this.cellColor)
            {
                this.cellColor = value;
                this.OnPropertyChanged(nameof(this.CellColor));
            }
        }
    }

    /// <summary>
    /// The content to display in the <see cref="PositioningOverlay"/> when editing. Not shown in the editor itself.
    /// </summary>
    public IView? Content
    {
        get => this.content;
        set
        {
            if (value != this.content)
            {
                this.content = value;
                this.OnPropertyChanged(nameof(this.Content));
            }
        }
    }

    /// <summary>
    /// Map of directions to directional arrow sprites; used to indicate dragging.
    /// </summary>
    public ISpriteMap<Direction>? DirectionSpriteMap { get; set; }

    /// <summary>
    /// The color to draw gridlines.
    /// </summary>
    public Color GridColor
    {
        get => this.gridColor;
        set
        {
            if (value != this.gridColor)
            {
                this.gridColor = value;
                this.OnPropertyChanged(nameof(this.GridColor));
            }
        }
    }

    /// <summary>
    /// Color to tint cells and gridlines while the mouse is hovering over the editor.
    /// </summary>
    public Color HoverTintColor
    {
        get => this.hoverTintColor;
        set
        {
            if (value != this.hoverTintColor)
            {
                this.hoverTintColor = value;
                this.OnPropertyChanged(nameof(this.HoverTintColor));
            }
        }
    }

    /// <summary>
    /// Thickness of gridlines.
    /// </summary>
    public int LineWidth
    {
        get => this.lineWidth.Value;
        set
        {
            if (this.lineWidth.SetIfChanged(value))
            {
                this.OnPropertyChanged(nameof(this.LineWidth));
            }
        }
    }

    /// <summary>
    /// The current content placement.
    /// </summary>
    public NineGridPlacement Placement
    {
        get => this.placement;
        set
        {
            if (value != this.placement)
            {
                this.placement = value;
                this.OnPropertyChanged(nameof(this.Placement));
            }
        }
    }

    /// <summary>
    /// Color for the grid cell that is is selected according to <see cref="Placement"/>.
    /// </summary>
    public Color SelectionBackgroundColor
    {
        get => this.selectionBackgroundColor;
        set
        {
            if (value != this.selectionBackgroundColor)
            {
                this.selectionBackgroundColor = value;
                this.OnPropertyChanged(nameof(this.SelectionBackgroundColor));
            }
        }
    }

    private readonly Rectangle[,] cells = new Rectangle[3, 3];
    private readonly Rectangle[] gridlines = new Rectangle[8];
    private readonly DirtyTracker<int> lineWidth = new(4);

    private Color cellColor = Color.Transparent;
    private IView? content;
    private Color gridColor = new(64, 64, 64);
    private Color hoverTintColor = Color.White;
    private bool isHovering;
    private PositioningOverlay? overlay;
    private NineGridPlacement placement = new(Alignment.Start, Alignment.Start);
    private Color selectionBackgroundColor = new(80, 150, 220);

    /// <inheritdoc />
    public override void OnClick(ClickEventArgs e)
    {
        if (e.IsPrimaryButton())
        {
            Game1.playSound("bigSelect");
            this.overlay = new PositioningOverlay(this.ButtonSpriteMap, this.DirectionSpriteMap)
            {
                Content = this.Content,
                ContentPlacement = this.Placement,
                DimmingAmount = 0.93f,
            };
            this.overlay.Close += this.Overlay_Close;
            Overlay.Push(this.overlay);
            e.Handled = true;
        }
        base.OnClick(e);
    }

    /// <inheritdoc />
    public override void OnPointerMove(PointerMoveEventArgs e)
    {
        this.isHovering = this.ContainsPoint(e.Position);
        base.OnPointerMove(e);
    }

    /// <inheritdoc />
    protected override bool IsContentDirty()
    {
        return this.lineWidth.IsDirty;
    }

    /// <inheritdoc />
    protected override void OnDrawContent(ISpriteBatch b)
    {
        var tintedSelectedCellColor = this.Tint(this.SelectionBackgroundColor);
        var tintedCellColor = this.Tint(this.CellColor);
        for (int row = 0; row < this.cells.GetLength(0); row++)
        {
            for (int col = 0; col < this.cells.GetLength(1); col++)
            {
                var color = this.IsSelectedCell(col, row) ? tintedSelectedCellColor : tintedCellColor;
                if (color != Color.Transparent)
                {
                    b.Draw(Game1.staminaRect, this.cells[row, col], null, color);
                }
            }
        }
        if (this.GridColor == Color.Transparent)
        {
            return;
        }
        var tintedGridColor = this.Tint(this.GridColor);
        foreach (var gridline in this.gridlines)
        {
            b.Draw(Game1.staminaRect, gridline, null, this.Tint(tintedGridColor));
        }
    }

    /// <inheritdoc />
    protected override void OnMeasure(Vector2 availableSize)
    {
        var limits = this.Layout.GetLimits(availableSize);
        this.ContentSize = this.Layout.Resolve(availableSize, () => Vector2.Zero);

        int columnWidth = (int)MathF.Round((this.ContentSize.X - this.LineWidth * 4) / 3);
        int rowHeight = (int)MathF.Round((this.ContentSize.Y - this.LineWidth * 4) / 3);
        // Cells
        int y = 0;
        for (int row = 0; row < 3; row++)
        {
            y += this.LineWidth;
            int x = 0;
            for (int col = 0; col < 3; col++)
            {
                x += this.LineWidth;
                this.cells[row, col] = new(x, y, columnWidth, rowHeight);
                x += columnWidth;
            }
            y += rowHeight;
        }
        // Gridlines
        int gridWidth = columnWidth * 3 + this.LineWidth * 4;
        int gridHeight = rowHeight * 3 + this.LineWidth * 4;
        int gridX = 0;
        for (int col = 0; col < 4; col++)
        {
            this.gridlines[col] = new(gridX, 0, this.LineWidth, gridHeight);
            gridX += this.LineWidth + columnWidth;
        }
        int gridY = 0;
        for (int row = 0; row < 4; row++)
        {
            this.gridlines[row + 4] = new(0, gridY, gridWidth, this.LineWidth);
            gridY += this.LineWidth + rowHeight;
        }
    }

    /// <inheritdoc />
    protected override void ResetDirty()
    {
        this.lineWidth.ResetDirty();
    }

    private static int GetCellIndex(Alignment alignment)
    {
        return alignment switch
        {
            Alignment.Start => 0,
            Alignment.Middle => 1,
            Alignment.End => 2,
            _ => throw new ArgumentException($"Invalid alignment: {alignment}", nameof(alignment)),
        };
    }

    private bool IsSelectedCell(int col, int row)
    {
        return col == GetCellIndex(this.Placement.HorizontalAlignment) && row == GetCellIndex(this.Placement.VerticalAlignment);
    }

    private void Overlay_Close(object? sender, EventArgs e)
    {
        if (this.overlay is not null)
        {
            this.Placement = this.overlay.ContentPlacement;
        }

        this.overlay = null;
        this.isHovering = false;
    }

    private Color Tint(Color color)
    {
        if (!this.isHovering || this.HoverTintColor.A == 0 || this.HoverTintColor == Color.White)
        {
            return color;
        }
        const float tintAmount = 0.25f;
        float tintAlpha = 255 / this.HoverTintColor.A;
        int r = (int)(color.R * (1 - tintAmount) + this.HoverTintColor.R * tintAmount * tintAlpha);
        int g = (int)(color.G * (1 - tintAmount) + this.HoverTintColor.G * tintAmount * tintAlpha);
        int b = (int)(color.B * (1 - tintAmount) + this.HoverTintColor.B * tintAmount * tintAlpha);
        int a = (int)(color.A * (1 - tintAmount) + this.HoverTintColor.A * tintAmount);
        return new(r, g, b, a);
    }
}
