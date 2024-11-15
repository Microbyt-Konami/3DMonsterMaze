using UnityEngine;

public class Cell : MonoBehaviour
{
    public int row, column;
    public GameObject root;
    public Floor floor;
    public Transform wallEastPoint;
    public Transform wallWestPoint;
    public Transform wallNorthPoint;
    public Transform wallSouthPoint;
    public Transform walls;
}