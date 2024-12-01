using StarterAssets;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool noGenerateMaze = false;
    [SerializeField] private GameObject monsterPrefab;
    private MazeGenerator _mazeGenerator;

    IEnumerator Start()
    {
        _mazeGenerator = FindAnyObjectByType<MazeGenerator>();

        // Set target frame rate
        Application.targetFrameRate = 60;

        var player = GameObject.FindGameObjectWithTag("Player");

        player.gameObject.SetActive(false);

        if (noGenerateMaze)
            yield break;

        yield return _mazeGenerator.GenerateMaze();

        player.gameObject.SetActive(true);


        if (_mazeGenerator.CellEntryGO != null)
        {
            var cell = _mazeGenerator.CellEntryGO.GetComponent<Cell>();

            if (cell != null)
                player.transform.position = cell.floor.transform.position;

            //if (_mazeGenerator.CellExitGO != null && _mazeGenerator.CellExitGO.TryGetComponent<Cell>(out var cell))
            //    cell.HideWalls(CellWall.South);

            if (_mazeGenerator.CellExitGO != null)
                Instantiate(monsterPrefab, _mazeGenerator.CellExitGO.transform.position, Quaternion.identity);
        }
    }
}