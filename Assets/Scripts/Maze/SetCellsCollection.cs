using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public delegate IEnumerator<bool> RandomUsedCellsCoRoutineHandler(SetCells set);

public class SetCellsCollection : IDisposable
{
    private int _lastSetId;
    private Dictionary<int, SetCells> _sets = new Dictionary<int, SetCells>();
    private Dictionary<int, SetCells> _setRemoves = new Dictionary<int, SetCells>();
    private Queue<int> _setCellsQuery = new Queue<int>();
    private bool disposedValue;

    public int AddNewSet()
    {
        int setid = NextSetId();
        var set = new SetCells { NCells = 1 };

        _setRemoves.Remove(setid);
        _sets[setid] = set;

        return setid;
    }

    public void RemoveSet(int setId)
    {
        if (!_sets.TryGetValue(setId, out var set))
            _setCellsQuery.Enqueue(setId);
        else
        {
            if (--set.NCells == 0)
            {
                _setRemoves.Remove(setId);
                _setCellsQuery.Enqueue(setId);
                set.Dispose();
            }
            else
                _setRemoves[setId] = set;

            _sets.Remove(setId);
        }
    }

    public void AddCellToSet(int setId)
    {
        if (_sets.TryGetValue(setId, out var set))
            set.NCells++;
    }

    public void RemoveCellToSet(int setId)
    {
        if (_sets.TryGetValue(setId, out var set))
            set.NCells--;
    }

    public void AsignSetsRandomUsed(RandomUsedCellsCoRoutineHandler randomUsedCellsCoRoutine)
    {
        foreach (var set in _sets.Values)
            set.RandomUsedCellsEnumerator = randomUsedCellsCoRoutine(set);
    }

    public bool RandomUsed(int setId)
        => (!_sets.TryGetValue(setId, out var set) || set.RandomUsedCellsEnumerator == null)
            ? Random.value >= 0.5f
            : set.RandomUsedCellsEnumerator.MoveNext() && set.RandomUsedCellsEnumerator.Current;

    public void FreeSetsRandomUsed()
    {
        foreach (var set in _sets.Values)
        {
            if (set.RandomUsedCellsEnumerator != null)
            {
                set.RandomUsedCellsEnumerator.Dispose();
                set.RandomUsedCellsEnumerator = null;
            }
        }
    }

    private int NextSetId() => (_setCellsQuery.TryDequeue(out var setCells)) ? setCells : ++_lastSetId;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: eliminar el estado administrado (objetos administrados)
                _sets?.Clear();
                _setCellsQuery?.Clear();
                _setRemoves?.Clear();
            }

            // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
            // TODO: establecer los campos grandes como NULL

            _sets = null;
            _setCellsQuery = null;
            _setRemoves = null;

            disposedValue = true;
        }
    }

    // // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene c�digo para liberar los recursos no administrados
    // ~SetCellsCollection()
    // {
    //     // No cambie este c�digo. Coloque el c�digo de limpieza en el m�todo "Dispose(bool disposing)".
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // No cambie este c�digo. Coloque el c�digo de limpieza en el m�todo "Dispose(bool disposing)".
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}