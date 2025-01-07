using UnityEngine;

public class Maze : MonoBehaviour
{
    public Cell[,] cells;

    public Cell GetCell(int row, int column)
    {
        if (row < 0 || row >= cells.GetLength(0) || column < 0 || column >= cells.GetLength(1))
        {
            return null;
        }

        return cells[row, column];
    }

    public Cell GetCell(Cell cell, CellWall direction)
        => direction switch
        {
            CellWall.North => GetCell(cell.row - 1, cell.column),
            CellWall.East => GetCell(cell.row, cell.column + 1),
            CellWall.South => GetCell(cell.row + 1, cell.column),
            CellWall.West => GetCell(cell.row, cell.column - 1),
            _ => null
        };
}
