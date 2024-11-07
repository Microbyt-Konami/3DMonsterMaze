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

        for (int j = 0 - 1; j < columns; j++)
            sb.Append("---");
        sb.AppendLine();
        for (int i = 0; i < rows; i++)
        {
            sb.Append("|");
            bool wasWallRight = false;

            for (int j = 0; j < columns; j++)
            {
                var cell = SetCells[idx + j];
                wasWallRight = cell.isWallRight;

                sb.Append($"{cell.SetId:00}{(wasWallRight ? "|" : " ")}");
            }

            sb.AppendLine((!wasWallRight) ? "|" : "");

            if (i < rows - 1)
            {
                sb.Append("|");
                for (int j = 0; j < columns; j++)
                    sb.Append(SetCells[idx++].isWallBottom ? "---" : "   ");
                sb.AppendLine("|");
            }
        }

        for (int j = 0 - 2; j < columns; j++)
            sb.Append("---");
        sb.AppendLine();

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
            SetCells[col] = new Cell { SetId = _setworks.AddNewSet(), Connects = CellConnect.None };

        // Inicializo las otras filas
        for (var i = columns; i < SetCells.Length; i++)
            SetCells[i] = new Cell { SetId = 0, Connects = CellConnect.None };
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

        int idxRowPrevPrev = _idxRowCurrent - 2 * columns;
        int idxRowPrev = idxRowPrevPrev + columns;
        int lastSetId = SetCells[idxRowPrev].SetId;

        if (idxRowPrevPrev >= 0)
        {
            for (var i = 0; i < columns; i++)
                if (SetCells[idxRowPrevPrev + i].Connects.HasFlag(CellConnect.Bottom))
                {
                    lastSetId = SetCells[idxRowPrev + i].SetId;
                    break;
                }
        }

        for (var i = 0; i < columns; i++)
        {
            var cellPrevWalls = idxRowPrevPrev >= 0 ? SetCells[idxRowPrevPrev + i].Connects : CellConnect.None;
            var cell = SetCells[_idxRowCurrent + i];

            cell.SetId = lastSetId;
            cell.Connects = CellConnect.Right;
            if (cellPrevWalls.HasFlag(CellConnect.Bottom))
                SetCells[idxRowPrev + i].Connects |= CellConnect.Bottom;
        }

        yield return null;
    }

    private void JoinAdjacentCells(int col)
    {
        var set1 = SetCells[col + _idxRowCurrent];
        var set2 = SetCells[col + 1 + _idxRowCurrent];

        set1.Connects |= CellConnect.Right;
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

        set1.Connects |= CellConnect.Bottom;
        set2.SetId = set1.SetId;
    }
}