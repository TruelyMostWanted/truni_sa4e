using System.Collections.Generic;
using Godot;

namespace SA4E.scripts.fireflies;

public static class FireflyNeighbour
{
    public enum DirectionEnum
    {
        East = 0,
        South = 1,
        West = 2,
        North = 3,
            
        SouthEast = 4,
        SouthWest = 5,
        NorthWest = 6,
        NorthEast = 8
    }

    
    public static readonly Dictionary<DirectionEnum, Vector2I> Directions = new()
    {
        { DirectionEnum.East, new Vector2I(1, 0) },
        { DirectionEnum.South, new Vector2I(0, 1) },
        { DirectionEnum.West, new Vector2I(-1, 0) },
        { DirectionEnum.North, new Vector2I(0, -1) },
        { DirectionEnum.SouthEast, new Vector2I(1, 1) },
        { DirectionEnum.SouthWest, new Vector2I(-1, 1) },
        { DirectionEnum.NorthWest, new Vector2I(-1, -1) },
        { DirectionEnum.NorthEast, new Vector2I(1, -1) }
    };
    public static readonly Dictionary<DirectionEnum, Vector2I> InversedDirections = new()
    {
        { DirectionEnum.East, new Vector2I(-1, 0) },
        { DirectionEnum.South, new Vector2I(0, -1) },
        { DirectionEnum.West, new Vector2I(1, 0) },
        { DirectionEnum.North, new Vector2I(0, 1) },
        { DirectionEnum.SouthEast, new Vector2I(-1, -1) },
        { DirectionEnum.SouthWest, new Vector2I(1, -1) },
        { DirectionEnum.NorthWest, new Vector2I(1, 1) },
        { DirectionEnum.NorthEast, new Vector2I(-1, 1) }
    };

    
    public static Vector2I GetNeighbourIndex(Vector2I index, DirectionEnum direction, Vector2I mapSize, bool isTorus)
    {
        var neighbourIndex = index + Directions[direction];

        if (isTorus)
        {
            neighbourIndex.X = (neighbourIndex.X + mapSize.X) % mapSize.X;
            neighbourIndex.Y = (neighbourIndex.Y + mapSize.Y) % mapSize.Y;
            return neighbourIndex;
        }

        if (neighbourIndex.X < 0 || neighbourIndex.X >= mapSize.X || 
            neighbourIndex.Y < 0 || neighbourIndex.Y >= mapSize.Y)
            return new Vector2I(-1, -1);
        return neighbourIndex;
    }
    public static Vector2I GetInversedNeighbourIndex(Vector2I index, DirectionEnum direction, Vector2I mapSize, bool isTorus)
    {
        var neighbourIndex = index + InversedDirections[direction];

        if (isTorus)
        {
            neighbourIndex.X = (neighbourIndex.X + mapSize.X) % mapSize.X;
            neighbourIndex.Y = (neighbourIndex.Y + mapSize.Y) % mapSize.Y;
            return neighbourIndex;
        }

        if (neighbourIndex.X < 0 || neighbourIndex.X >= mapSize.X || 
            neighbourIndex.Y < 0 || neighbourIndex.Y >= mapSize.Y)
            return new Vector2I(-1, -1);
        return neighbourIndex;
    }
}