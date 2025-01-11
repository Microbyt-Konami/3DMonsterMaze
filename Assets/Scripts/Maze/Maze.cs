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

    public Vector3 GetPosition(int row, int col) => new Vector3(-cellSize.x * col, 0, cellSize.y * row);

    public void AddFloor()
    {
        var position = new Vector3(-cellSize.x * columns / 2.0f + cellSize.x / 2.0f, -cellSize.y / 2.0f - thickness / 2.0f, cellSize.z * rows / 2.0f);

        _floorGO = Instantiate(floorPrefab, position, Quaternion.identity, transform);
        _floorGO.name = $"Floor_{rows}_{columns}";
        _floorGO.transform.localScale = new Vector3(cellSize.x * columns, thickness, cellSize.z * rows);

        var floorRenderer = _floorGO.GetComponent<MeshRenderer>();
        var material = new Material(floorRenderer.material);

        material.mainTextureScale = new Vector2(columns / 4.0f, rows / 4.0f);
        floorRenderer.material = material;
    }

    public void AddWall(int row, int col, Quaternion rotation, Material material = null)
    {
        var position = GetPosition(row, col);
        var wallGO = Instantiate(wallPrefab, position, rotation, transform);
        var wall = wallGO.GetComponent<Wall>();

        if (material != null && wall != null)
            wall.SetWallMaterial(material);

        wallGO.name = $"Wall_{row}_{col}";
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
