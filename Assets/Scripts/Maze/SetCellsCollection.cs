using System;
using System.Collections.Generic;

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
        var set = new SetCells();

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

    public void SetSetsToNotUsed()
    {
        foreach (var set in _sets.Values)
            set.Used = set.UsedTmp = false;
    }

    public void UpdateSetsUseds()
    {
        foreach (var set in _sets.Values)
            if (!set.Used && set.UsedTmp)
                set.Used = set.UsedTmp;
    }

    public void SetSetUsed(int setId)
    {
        if (_sets.TryGetValue(setId, out var set))
        {
            set.UsedTmp = true;
            set.Used = false;
        }
    }

    public bool IsSetUsed(int setId) => _sets.TryGetValue(setId, out var set) && set.Used;
    public bool IsSetOneCell(int setId) => _sets.TryGetValue(setId, out var set) && set.NCells == 1;

    private int NextSetId() => (_setCellsQuery.TryDequeue(out var setCells)) ? setCells : ++_lastSetId;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: eliminar el estado administrado (objetos administrados)
                _sets.Clear();
                _setCellsQuery.Clear();
                _setRemoves.Clear();
            }

            // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
            // TODO: establecer los campos grandes como NULL

            _sets = null;
            _setCellsQuery = null;
            _setRemoves = null;
            disposedValue = true;
        }
    }

    // // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
    // ~SetCellsCollection()
    // {
    //     // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
