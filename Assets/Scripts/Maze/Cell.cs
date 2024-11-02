using System;

[Flags]
public enum CellNeighbor
{
    North = 0x1,
    East = 0x2,
    South = 0x4,
    West = 0x8
}

[Serializable]
public class Cell : IEquatable<Cell>
{
    public int row;
    public int column;

    public bool Equals(Cell other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return row == other.row && column == other.column;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Cell)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(row, column);
    }
}