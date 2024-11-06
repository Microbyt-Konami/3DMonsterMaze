using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int rows = 10, columns = 10;
    [field: SerializeField] public Cell[] SetCells { get; private set; }
    [field: SerializeField] public bool MazeGenerated { get; private set; }

    private int _idxRowCurrent;
    private SetCellsCollection _setworks;

    void Start()
    {
        GenerateMaze();
    }

    private void LogSetCells()
    {
        int idx = 0;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
                sb.Append($"{SetCells[idx++]:00} ");
            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }

    private void GenerateMaze()
    {
        StartCoroutine(GenerateMazeCoRoutine());
    }

    IEnumerator GenerateMazeCoRoutine()
    {
        MazeGenerated = false;
        // La primera fila cada celda pertenece a un conjunto único, columna 1 conjunto 1, columna 2 conjunto 2, etc.
        InitVars();

        // por cada fila menos la ultima
        var lastIdxRow = SetCells.Length - columns;

        _idxRowCurrent = 0;
        for (; _idxRowCurrent < lastIdxRow; _idxRowCurrent += columns)
            yield return ProcessRowCoRoutine();

        // Reiniciar los conjuntos conservando en el mismo conjunto aquellas celdas que compartan conjunto superior
        yield return ProcessLastRowCoRoutine();

        FreeVarsTemps();
        LogSetCells();
        MazeGenerated = true;
    }

    /// <summary>
    /// La primera fila cada celda pertenece a un conjunto único, columna 1 conjunto 1, columna 2 conjunto 2, etc.
    /// </summary>
    private void InitVars()
    {
        _setworks = new SetCellsCollection();
        SetCells = new Cell[rows * columns];
        _idxRowCurrent = 0;

        // Inicializo las primera file
        for (var col = 0; col < columns; col++)
            SetCells[col] = new Cell { SetId = _setworks.AddNewSet(), Walls = CellWall.None };

        // Inicializo las otras filas
        for (var i = columns; i < SetCells.Length; i++)
            SetCells[i] = new Cell { SetId = 0, Walls = CellWall.None };
    }

    private void FreeVarsTemps()
    {
        _setworks.Dispose();
        _setworks = null;
    }

    private IEnumerator<bool> RandomUsedCellsCoRoutine(SetCells set)
    {
        var random = Random.Range(1, set.NCells);
        int i = 0, count = 0;

        for (; i < set.NCells; i++)
        {
            if (i - count + 1 >= set.NCells)
                break;

            if (count == random)
                yield break;

            var value = Random.value >= 0.5f;

            yield return value;

            if (value)
                ++count;
        }

        for (; i < set.NCells && count < random; i++)
            yield return true;
    }

    private IEnumerator ProcessRowCoRoutine()
    {
        // en la fila actual decidimos aleatoriamente por cada par de celda adjacentes si se unen o no
        yield return JoinRowHCellsRandomCoRoutine();

        /* Ahora determinamos aleatoriamente las conexiones verticales, al menos una por conjunto.
         * Las celdas de la siguiente fila a las que nos conectamos deben asignarse al conjunto de la celda que está encima de ellas
         */
        yield return JoinRowVCellsRandomCoRoutine();
    }

    private IEnumerator JoinRowHCellsRandomCoRoutine()
    {
        var lastCol = columns - 1;

        // en la fila actual decidimos aleatoriamente por cada par de celda adjacentes si se unen o no
        for (var col = 0; col < lastCol; col++)
        {
            if (Random.value > 0.5f)
                JoinAdjacentCells(col);
        }

        yield return null;
    }

    private IEnumerator JoinRowVCellsRandomCoRoutine()
    {
        /* Ahora determinamos aleatoriamente las conexiones verticales, al menos una por conjunto.
         * Las celdas de la siguiente fila a las que nos conectamos deben asignarse al conjunto de la celda que está encima de ellas
         */
        int nextIdx = _idxRowCurrent + columns;

        // Reseteamos valores used
        _setworks.AsignSetsRandomUsed(RandomUsedCellsCoRoutine);

        // Primero hacemos las conexiones horizontales aleatorias, de cada conjunto al menos una se tiene que conectar
        for (var i = 0; i < columns; i++)
        {
            var setId = SetCells[_idxRowCurrent + i].SetId;

            if (_setworks.RandomUsed(setId))
                JoinBottomCell(i);
        }

        // Las celdas de la siguiente filas no conectadas se asignar cada una a un nuevo conjunto
        for (var i = 0; i < columns; i++)
        {
            if (SetCells[nextIdx + i].SetId == 0)
            {
                _setworks.RemoveCellToSet(SetCells[_idxRowCurrent + i].SetId);
                SetCells[nextIdx + i].SetId = _setworks.AddNewSet();
            }
        }

        _setworks.FreeSetsRandomUsed();

        yield return null;
    }

    private IEnumerator ProcessLastRowCoRoutine()
    {
        // Reiniciar los conjuntos conservando en el mismo conjunto aquellas celdas que compartan conjunto superior 

        int idxRowPrev = _idxRowCurrent - 2 * columns;
        int lastSetId = SetCells[_idxRowCurrent - columns].SetId;

        for (var i = 0; i < columns; i++)
        {
            var cellPrevWalls = idxRowPrev >= 0 ? SetCells[idxRowPrev + i].Walls : CellWall.None;
            var cell = SetCells[_idxRowCurrent + i];

            cell.SetId = lastSetId;
            cell.Walls = CellWall.Right;
            if (cellPrevWalls.HasFlag(CellWall.Bottom))
                cell.Walls |= CellWall.Bottom;
        }

        yield return null;
    }

    private void JoinAdjacentCells(int col)
    {
        var set1 = SetCells[col + _idxRowCurrent];
        var set2 = SetCells[col + 1 + _idxRowCurrent];

        set1.Walls |= CellWall.Right;
        if (set1.SetId == set2.SetId)
            return;

        _setworks.AddCellToSet(set1.SetId);
        _setworks.RemoveSet(set2.SetId);
        set2.SetId = set1.SetId;
    }

    private void JoinBottomCell(int col)
    {
        var set1 = SetCells[col + _idxRowCurrent];
        var set2 = SetCells[col + _idxRowCurrent + columns];

        set1.Walls |= CellWall.Bottom;
        set2.SetId = set1.SetId;
    }
}