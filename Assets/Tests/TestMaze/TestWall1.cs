using UnityEngine;

public class TestWall1 : MonoBehaviour
{
    [SerializeField] private Maze maze;
    [SerializeField] private Quaternion wallRotationNorth = Quaternion.Euler(0, 90, 90);
    [SerializeField] private Quaternion wallRotationEast = Quaternion.Euler(0, 0, 90);
    [SerializeField] private Material[] wallMaterials;

    private void Start()
    {
        maze.rows = maze.columns = 16;
        maze.AddFloor();
        for (var i = 0; i < maze.columns; i++)
            maze.AddWall(0, i, wallRotationNorth);
        for (var i = 0; i < maze.columns; i++)
            maze.AddWall(maze.rows - 1, i, wallRotationNorth);
        for (var i = 0; i < maze.rows; i++)
            maze.AddWall(i, 0, wallRotationEast);
        for (var i = 0; i < maze.rows; i++)
            maze.AddWall(i, maze.columns - 1, wallRotationEast);
    }
}
