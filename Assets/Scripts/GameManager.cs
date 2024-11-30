using StarterAssets;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    private MazeGenerator _mazeGenerator;

    IEnumerator Start()
    {
        _mazeGenerator = FindAnyObjectByType<MazeGenerator>();

        // Set target frame rate
        Application.targetFrameRate = 60;

        yield return _mazeGenerator.GenerateMaze();

        var player = GameObject.FindGameObjectWithTag("Player");

        if (_mazeGenerator.CellEntryGO != null)
        {
            var cell = _mazeGenerator.CellEntryGO.GetComponent<Cell>();

            if (cell != null)
                player.transform.position = cell.floor.transform.position;

            //if (_mazeGenerator.CellExitGO != null && _mazeGenerator.CellExitGO.TryGetComponent<Cell>(out var cell))
            //    cell.HideWalls(CellWall.South);
        }
    }
}