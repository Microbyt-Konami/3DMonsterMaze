using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Random = Unity.Mathematics.Random;


/// <summary>
/// 
/// </summary>
public struct EllerJob : IJob
{
    public uint seed;
    public int rows, columns;
    public NativeArray<int> setCells;

    /*
     *      1 2 3 4 5
     * desp 0 0 0 0 0
     * set  0 1 2 3 4
     *
     *      1 2 3 1 5
     * desp 3 0 0 0 0
     * set  0 1 2   4
     *
     *      1  1  3  1  5
     * desp 3  0  0  -2 0
     * set  0     2     4
     */

    private NativeArray<int> despSetCells;
    private NativeHashMap<int, int> setIniCells;
    private Random random;

    public void Execute()
    {
        int i, j, row, lastSet;

        random.InitState(seed);
        // La primera fila cada celda pertenece a un conjunto único, columna 1 conjunto 1, columna 2 conjunto 2, etc.
        setCells = new NativeArray<int>(rows * columns, Allocator.TempJob);
        despSetCells = new NativeArray<int>(columns, Allocator.TempJob);
        setIniCells = new NativeHashMap<int, int>(columns, Allocator.TempJob);

        for (i = 0; i < columns; i++)
        {
            setCells[i] = i + 1;
            despSetCells[i] = 0;
            setIniCells.Add(i + 1, i);
        }

        row = 0;
        // primera celda de la fila
        i = 0;
        // ultima celda de la fila
        j = columns - 1;
        lastSet = columns;

        // por cada fila menos la ultima
        for (; row < rows - 1; row++, i += columns, j += columns)
        {
            // en la fila actual decidimos aleatoriamente por cada par de celda adjacentes si se unen o no
            for (int k = 0; k < columns - 1; k++)
                if (random.NextBool())
                    setCells[i + k + 1] = setCells[i + k];

            /*
             * Ahora determinamos aleatoriamente las conexiones verticales, al menos una por conjunto.
             * Las celdas de la siguiente fila a las que nos conectamos deben asignarse al conjunto de la celda que está encima de ellas
             */
        }

        //se conectan todas las celdas que no pertenezcan al mismo conjunto entre sí
        lastSet++;
        for (; i <= j; i++)
        {
            if (setCells[i] == 0)
                setCells[i] = lastSet;
        }
    }
}

/*
public struct PruebaJob : IJob
{
    public uint seed;
    public NativeArray<float> values;

    private Random random;

    public void Execute()
    {
        random.InitState(seed);
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = random.NextFloat(0, 10);
        }
    }
}
*/

public class MazeGenerator : MonoBehaviour
{
    public int rows = 10, columns = 10;
    public NativeArray<int> setCells;

    void Start()
    {
        var job = new EllerJob
        {
            seed = (uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue),
            rows = rows,
            columns = columns
        };

        job.Execute();
        setCells = new NativeArray<int>(job.setCells, Allocator.Persistent);
        int idx = 0;
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
                sb.Append($"{setCells[idx++]} ");
            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
    /*
    private PruebaJob _job;

    void Start()
    {
        var values = new NativeArray<float>(10, Allocator.TempJob);

        _job = new PruebaJob()
        {
            seed = (uint)UnityEngine.Random.Range(0, int.MaxValue),
            //prueba = new Prueba { x = 1, y = 2, z = 3 },
            values = values
        };

        //_job.Run();
        // Schedule() puts the job instance on the job queue.
        JobHandle findHandle = _job.Schedule();
        findHandle.Complete();

        foreach (var value in _job.values)
        {
            Debug.Log(value);
        }
    }*/
}