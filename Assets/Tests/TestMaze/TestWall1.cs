using UnityEngine;

public class TestWall1 : MonoBehaviour
{
    [SerializeField] private Maze maze;
    [SerializeField] private Material[] wallMaterials;

    private void Start()
    {
        maze.rows = maze.columns = 16;
        maze.AddFloor();
        maze.AddWallV(0, 0, 3);
    }
}
