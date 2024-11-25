using System;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    private MazeGenerator _mazeGenerator;

    void Start()
    {
        _mazeGenerator = FindAnyObjectByType<MazeGenerator>();        
        // Set target frame rate
        Application.targetFrameRate = 60;
        _mazeGenerator.GenerateMaze();
    }
}