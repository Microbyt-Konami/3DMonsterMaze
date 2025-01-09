using UnityEngine;

public class Maze : MonoBehaviour
{
    [SerializeField] private MeshRenderer floorRenderer;
    [SerializeField] private GameObject wallPrefab;

    public int rows, columns;
    public Cell[,] cells;

    public void UpdateFloor()
    {
        //var material = new Material(floorRenderer.material);

        //material.mainTextureScale = new Vector2(columns, rows);
        //floorRenderer.material = material;
        floorRenderer.transform.localScale = new Vector3(4 * columns, 0.1f, 4 * rows);
    }

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
