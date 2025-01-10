using NUnit.Framework.Constraints;
using UnityEngine;

public class Maze : MonoBehaviour
{
    //[SerializeField] private MeshRenderer floorRenderer;
    [SerializeField] private Vector3 cellSize = new Vector3(4, 4, 4);
    [SerializeField] private float thickness = 0.1f;
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;

    public int rows, columns;
    public Cell[,] cells;

    private GameObject _floorGO;

    public void AddFloor()
    {
        _floorGO = Instantiate(floorPrefab, Vector3.zero, Quaternion.identity, transform);
        _floorGO.transform.localScale = new Vector3(cellSize.x * columns, thickness, cellSize.z * rows);

        var floorRenderer = _floorGO.GetComponent<MeshRenderer>();
        var material = new Material(floorRenderer.material);

        material.mainTextureScale = new Vector2(columns / 4.0f, rows / 4.0f);
        floorRenderer.material = material;
    }

    public void AddWallV(int row, int column, int ncell = 1, Material material = null)
    {
        var position = new Vector3((row / rows - (rows / 2)) * cellSize.x, cellSize.x / 2, (column / columns - (columns / 2)) * cellSize.y);
        var wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);

        wall.transform.localScale = new Vector3(1, 1, cellSize.x * ncell);
    }

    public void UpdateFloor()
    {
        //var material = new Material(floorRenderer.material);

        //material.mainTextureScale = new Vector2(columns, rows);
        //floorRenderer.material = material;
        //floorRenderer.transform.localScale = new Vector3(4 * columns, 0.1f, 4 * rows);
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
