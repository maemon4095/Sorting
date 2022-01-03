namespace Sorting.Services;

public struct Dimension2D
{
    public int Width { get; }
    public int Height { get; }

    public Dimension2D(int width, int height)
    {
        this.Width = width;
        this.Height = height;
    }

    public override string ToString() => $"({this.Width}, {this.Height})";
}
