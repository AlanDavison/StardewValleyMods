using Microsoft.Xna.Framework.Graphics;

namespace DecidedlyShared.Ui;

public class MenuPage
{
    public UiElement page;
    public TextElement pageText;

    public int TotalHeight
    {
        get
        {
            int height = this.page.Height;
            if (this.pageText != null)
                height += this.pageText.Height;

            return height;
        }
    }

    public int TotalWidth
    {
        get
        {
            int width = this.page.Width;
            if (this.pageText != null)
                width += this.pageText.Width;

            return width;
        }
    }

    public void Draw(SpriteBatch sb)
    {
        if (this.page != null)
            this.page.Draw(sb);

        if (this.pageText != null)
            this.pageText.Draw(sb);
    }
}
