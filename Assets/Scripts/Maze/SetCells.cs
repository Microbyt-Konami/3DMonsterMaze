using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[DebuggerDisplay("NCells = {NCells}, NCellsRandom = {NCellsRandom}, NCellsUsed = {NCellsUsed}")]
public class SetCells : IDisposable
{
    private bool disposedValue;

    public int NCells { get; set; }
    public IEnumerator<bool> RandomUsedCellsEnumerator { get; set; }
    public bool Used { get; set; }
    public bool UsedTmp { get; set; }
    public int NCellsRandom { get; set; }
    public int NCellsUsed { get; set; }
    public int IdxCellsRandom { get; set; }

    public float RandomUsed() => NCellsUsed > 0 ? 0.5f : (NCells - NCellsRandom) / NCells;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: eliminar el estado administrado (objetos administrados)
                RandomUsedCellsEnumerator?.Dispose();
            }

            // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
            // TODO: establecer los campos grandes como NULL

            RandomUsedCellsEnumerator = null;

            disposedValue = true;
        }
    }

    // // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
    // ~SetCells()
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