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
    //public bool occluder;

    public void Occlude(bool argIsOccluder)
    {        
        gameObject.SetActive(!argIsOccluder);
    }

    public bool HasWall(CellWall walls)
    {
        var wallGO = GetWallGO(walls);

        return wallGO != null && wallGO.activeSelf && wallGO.transform.childCount > 0;
    }

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

    private GameObject GetWallGO(CellWall walls)
        => walls.HasFlag(CellWall.East)
            ? wallEastPoint.gameObject
            : walls.HasFlag(CellWall.West)
                ? wallWestPoint.gameObject
                : walls.HasFlag(CellWall.North)
                    ? wallNorthPoint.gameObject
                    : walls.HasFlag(CellWall.South)
                        ? wallSouthPoint.gameObject
                        : null;
}