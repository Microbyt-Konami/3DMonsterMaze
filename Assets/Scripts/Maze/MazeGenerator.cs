using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int rows = 10, columns = 10;
    [field: SerializeField] public int[] SetCells { get; private set; }

    private int[] _setCellsRowWork;
    private HashSet<int> _usedCells;
    private int _lastSetId = 0;
    private int _idxRowCurrent;

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

        ClearVarsTemp();
    }

    /// <summary>
    /// La primera fila cada celda pertenece a un conjunto único, columna 1 conjunto 1, columna 2 conjunto 2, etc.
    /// </summary>
    private void InitCells()
    {
        SetCells = new int[rows * columns];
        _setCellsRowWork = new int[columns];
        _usedCells = new HashSet<int>();
        _idxRowCurrent = 0;
        for (var i = columns; i < SetCells.Length; i++)
            SetCells[i] = 0;
        for (var col = 0; col < columns; col++)
            SetCells[col] = _setCellsRowWork[col = ++_lastSetId;
    }

    private void ClearVarsTemp()
    {
        _setCellsRowWork = null;
        _usedCells.Clear();
    }

    private IEnumerator ProcessRowCoRoutine(int row)
    {
        // en la fila actual decidimos aleatoriamente por cada par de celda adjacentes si se unen o no
        yield return JoinRowHCellsRandom();

        /* Ahora determinamos aleatoriamente las conexiones verticales, al menos una por conjunto.
         * Las celdas de la siguiente fila a las que nos conectamos deben asignarse al conjunto de la celda que está encima de ellas
         */
        yield return JoinRowVCellsRandom();

        _idxRowCurrent += columns;
    }

    private IEnumerator JoinRowHCellsRandom()
    {
        var lastCol = columns - 1;

        // en la fila actual decidimos aleatoriamente por cada par de celda adjacentes si se unen o no
        for (var col = 0; col < lastCol; col++)
        {
            if (Random.value > 0.5f)
                JoinRowCells(col, col + 1);
        }

        yield return null;
    }

    private IEnumerator JoinRowVCellsRandom()
    {
        /* Ahora determinamos aleatoriamente las conexiones verticales, al menos una por conjunto.
         * Las celdas de la siguiente fila a las que nos conectamos deben asignarse al conjunto de la celda que está encima de ellas
         */
        bool done;
        int nextIdx = _idxRowCurrent + columns;

        // Reseteamos valores used
        _usedCells.Clear();

        do
        {
            done = true;
            // Primero hacemos las conexiones horizontales aleatorias
            for (var i = 0; i < columns; i++)
            {
                var set = _setCellsRowWork[i];

                if (_usedCells.Contains(set))
                    continue;

                done = false;
                if (Random.value > .5f)
                {
                    _usedCells.Add(set);
                    SetCells[nextIdx + i] = _setCellsRowWork[i + _idxRowCurrent];
                }
            }

            yield return null;
        } while (!done);

        // Las celdas de la siguiente filas no conectadas se asignar cada una a un nuevo conjunto
        for (var i = 0; i < columns; i++)
        {
            if (SetCells[nextIdx + i] == 0)
            {
                SetCells[nextIdx + i] = _setCellsRowWork[i + _idxRowCurrent] = ++_lastSetId;
            }
        }
    }

    private void JoinRowCells(int col1, int col2)
    {
        var set = _setCellsRowWork[col1];

        if (set == _setCellsRowWork[col2])
            return;

        SetCells[col1 + _idxRowCurrent] = _setCellsRowWork[col2] = _setCellsRowWork[col1];
    }
}