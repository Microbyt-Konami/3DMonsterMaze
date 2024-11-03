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
public class Cell : IEquatable<Cell>, IComparable<Cell>
{
    public int row;
    public int column;
    public SetCells set;
    public bool used;
    
    public bool IsAssigned() => set != null;

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

    public int CompareTo(Cell other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        
        var rowComparison = row.CompareTo(other.row);
        
        if (rowComparison != 0) return rowComparison;
        
        return column.CompareTo(other.column);
    }
    
    public void AssignSet(SetCells set)
    {
        this.set = set;
        set.NCells++;
    }

    public void DeAssingSet()
    {
        set.NCells--;
        set = null;
    }
}