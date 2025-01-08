using UnityEngine;

public class TestWall1 : MonoBehaviour
{
    [SerializeField] private Maze maze;

    private void Start()
    {
        maze.rows = maze.columns = 16;
        maze.UpdateFloor();
    }
}
