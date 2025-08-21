public class SceneElementPuzzle : SceneElement
{
    public int index;

    public SceneElementPuzzle(
        int index, int x = 0, int y = 0,
        int distance = 0, int xRotationOffset = 0)
        : base(x, y, distance, xRotationOffset)
    {
        this.index = index;
    }

    public override string ToString()
    {
        return $"Puzzle {index} at x:{x} y:{y}, a distance of {distance}";
    }
}