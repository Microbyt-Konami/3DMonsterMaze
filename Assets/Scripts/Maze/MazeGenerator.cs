using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int rows = 10, columns = 10;
    [field: SerializeField] public SetCells[] SetCells { get; private set; }

    private HashSet<SetCells> _setCells;
    private SetCells[] _setCellsRowWork;
    private int _lastSetId = -1;
    private int _rowCurrent;

    void Start()
    {
        GenerateMaze();
        /*
        int idx = 0;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
                sb.Append($"{setCells[idx++]} ");
            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
        */
    }

    private void GenerateMaze()
    {
        StartCoroutine(GenerateMazeCoRoutine());
    }

    IEnumerator GenerateMazeCoRoutine()
    {
        // La primera fila cada celda pertenece a un conjunto único, columna 1 conjunto 1, columna 2 conjunto 2, etc.
        InitCells();

        // por cada fila menos la ultima
        var lastRow = rows - 1;

        for (var row = 0; row < lastRow; row++)
        {
            yield return ProcessRowCoRoutine(row);
        }

        SetCells = _setCells.ToArray();
        yield return null;
    }

    /// <summary>
    /// La primera fila cada celda pertenece a un conjunto único, columna 1 conjunto 1, columna 2 conjunto 2, etc.
    /// </summary>
    private void InitCells()
    {
        _setCells = new HashSet<SetCells>();
        _setCellsRowWork = new SetCells[columns];
        _rowCurrent = 0;
        for (var col = 0; col < columns; col++)
        {
            var setcells = NewSetCell();
            ;
            _setCellsRowWork[col] = setcells;
            setcells.AddCell(_rowCurrent, col);
        }
    }

    private SetCells NewSetCell()
    {
        var setCell = new SetCells
        {
            setId = ++_lastSetId,
            cells = new List<Cell>()
        };

        _setCells.Add(setCell);

        return setCell;
    }

    private IEnumerator ProcessRowCoRoutine(int row)
    {
        _rowCurrent = row;
        // en la fila actual decidimos aleatoriamente por cada par de celda adjacentes si se unen o no
        yield return JoinRowCellsRandom();
    }

    private IEnumerator JoinRowCellsRandom()
    {
        var lastCol = columns - 1;

        for (var col = 0; col < lastCol; col++)
        {
            if (Random.value > 0.5f)
                JoinRowCells(col, col + 1);
            else
            {
                // crear un nuevo conjunto que tenga esa columna
            }
        }

        yield return null;
    }

    private void JoinRowCells(int col1, int col2)
    {
        int set1 = GetSetId(col1);
        int set2 = GetSetId(col2);

        // si son del mismo conjunto no se unen
        if (set1 == set2)
            return;

        var cell2 = RemoveCell(col2);

        if (cell2 == null)
            return;

        AddCell(col1, cell2);
    }

    private int GetSetId(int col) => _setCellsRowWork[col].setId;

    private Cell GetCell(int col) => _setCellsRowWork[col]?.FindCell(_rowCurrent, col);

    private void AddCell(int col1, Cell cell)
    {
        cell.row = _rowCurrent;
        cell.column = col1;
        _setCellsRowWork[col1]?.AddCell(cell);
    }

    private Cell RemoveCell(int col)
    {
        var setcells = _setCellsRowWork[col];
        var cell = GetCell(col);

        if (setcells == null || cell == null)
            return null;

        setcells.RemoveCell(cell);
        _setCellsRowWork[col] = null;
        if (setcells.cells.Count == 0)
            _setCells.Remove(setcells);

        return cell;
    }
}