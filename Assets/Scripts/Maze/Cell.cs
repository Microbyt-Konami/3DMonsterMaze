using UnityEngine;

public class Cell : MonoBehaviour
{
    public int row, column;
    public GameObject root;
    public GameObject thisWay;
    public Floor floor;
    public Transform wallEastPoint;
    public Transform wallWestPoint;
    public Transform wallNorthPoint;
    public Transform wallSouthPoint;
    public Transform walls;

    public void ShowWalls(CellWall walls)
    {
        if (walls.HasFlag(CellWall.East)) wallEastPoint.gameObject.SetActive(true);
        if (walls.HasFlag(CellWall.West)) wallWestPoint.gameObject.SetActive(true);
        if (walls.HasFlag(CellWall.North)) wallNorthPoint.gameObject.SetActive(true);
        if (walls.HasFlag(CellWall.South)) wallSouthPoint.gameObject.SetActive(true);
    }

    public void HideWalls(CellWall walls)
    {
        if (walls.HasFlag(CellWall.East)) wallEastPoint.gameObject.SetActive(false);
        if (walls.HasFlag(CellWall.West)) wallWestPoint.gameObject.SetActive(false);
        if (walls.HasFlag(CellWall.North)) wallNorthPoint.gameObject.SetActive(false);
        if (walls.HasFlag(CellWall.South)) wallSouthPoint.gameObject.SetActive(false);
    }
}