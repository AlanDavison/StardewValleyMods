using Microsoft.Xna.Framework;

namespace PlayerCoordinates
{
    public struct Coordinates
    {
        public int x, y;

        public Coordinates(int newX, int newY)
        {
            this.x = newX;
            this.y = newY;
        }

        public static implicit operator Coordinates(Vector2 v)
        {
            return new Coordinates((int)v.X, (int)v.Y);
        }

        public override string ToString()
        {
            return $"X: {this.x}, Y: {this.y}";
        }
    }
}
