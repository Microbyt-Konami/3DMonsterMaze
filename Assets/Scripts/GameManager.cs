using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private MazeGenerator _mazeGenerator;

    private void Awake()
    {
        _mazeGenerator = FindAnyObjectByType<MazeGenerator>();
    }

    void Start()
    {
        // Set target frame rate
        Application.targetFrameRate = 60;
        _mazeGenerator.GenerateMaze();
    }
}