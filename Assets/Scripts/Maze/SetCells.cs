using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SetCells : IEquatable<SetCells>
{
    public int setId;
    public List<Cell> cells;

    public bool Equals(SetCells other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return setId == other.setId;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((SetCells)obj);
    }

    public override int GetHashCode() => setId.GetHashCode();

    public void AddCell(Cell cell)
    {
        if (!cells.Contains(cell))
            cells.Add(cell);
    }

    public void AddCell(int row, int column) => AddCell(new Cell { row = row, column = column });

    public bool RemoveCell(Cell cell) => cells.Remove(cell);

    public bool RemoveCell(int row, int column)
    {
        var cell = FindCell(row, column);

        return cell != null && RemoveCell(cell);
    }

    public Cell FindCell(int row, int column)
    {
        var cell = cells.Find(c => c.row == row && c.column == column);

        return cell;
    }
}