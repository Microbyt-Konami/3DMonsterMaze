using System.CodeDom;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

/*
 *  El laberinto siempre consta de 18 filas por 16 columnas.
 *  La generación del laberinto se produce durante las "nieblas del tiempo" con la máquina en modo FAST y tarda unos 30 segundos en el ZX81. 
 *  El laberinto se mapea en la memoria y aparece como se muestra a continuación. 
 *  
 *  La lógica de generación del laberinto es simple pero efectiva y da como resultado cada vez un laberinto que es diferente pero similar en apariencia al anterior, con muchos pasajes y aberturas entrecruzados. 
 *  Esto tiene el efecto de hacer que sea más fácil para Rex encontrarte y mantener el laberinto difícil de resolver y que parezca más grande de lo que es.
 *  
 *  La generación del laberinto comienza llenando todas las casillas con una pared sólida. Luego, corta pasajes al azar eligiendo una dirección (norte, sur, este u oeste) y una longitud (1 a 6). 
 *  Cada pasaje que se corta comienza al final del último. El primer pasaje siempre comienza en la posición inicial del jugador (que se muestra en el mapa anterior como "P"). 
 *  La posición inicial del jugador siempre está en el mismo lugar en el extremo sureste del mapa, mirando hacia el oeste.
 *  
 *  La lógica se asegura de que el laberinto nunca contenga un pasaje de más de una casilla de ancho. Para ello, nunca permite un bloque de 4 casillas vacías juntas
 *  
 *  La única otra regla que el cortador de pasajes observa es que no puede cortar en las paredes más al oeste, norte o sur. La pared que está más al este del laberinto sí se corta, pero debido a la naturaleza 
 *  envolvente de la forma en que el mapa se almacena en la memoria, la pared más al oeste también es la pared más al este, por lo que, en efecto, el laberinto siempre está rodeado por una pared sólida e 
 *  ininterrumpida por todos los lados.
 *  
 *  El código corta pasajes de forma repetitiva hasta que intenta cortar 800 fichas, y luego considera que el laberinto está completo. No hay ningún otro código que garantice que el laberinto se complete 
 *  satisfactoriamente, simplemente se supone que después de 800 fichas debería estarlo. El laberinto solo contiene 240 fichas cortables, por lo que la cifra de 800 supone que se deben volver a cortar pasajes existentes 
 *  o cortes de pasajes abandonados debido a golpes en el borde o la regla de "no se permiten bloques de 4" descrita anteriormente.
 *  
 *  Una vez que se completa la generación del laberinto, el juego elige una ubicación para la salida. Lo hace utilizando un enfoque un tanto torpe y caótico. Dentro de las 7 filas más al norte, elige una ficha al azar. 
 *  Si esta ficha es un trozo de pared, elige otra. Esto continúa hasta que encuentra una ficha vacía y allí coloca la salida (mostrada como "H" en el laberinto anterior). Para asegurarse de que la salida esté siempre 
 *  al final de un callejón sin salida, luego llena todos los espacios vacíos alrededor de la ficha elegida hasta que solo quede 1. Lo hace en el orden de prioridad norte, este, oeste y sur.
 *  
 *  Lamentablemente, debido a la lógica rudimentaria que se utiliza para colocar la salida, a veces ocurre que la salida termina en un túnel cerrado y no se puede ganar el juego. No existe ningún código que impida 
 *  esto y, aunque es poco frecuente, sucede.
 *  
 *  Por último, el Rex se coloca aleatoriamente en las 5 filas más al norte. Ten en cuenta que cuando el juego termina después de que Rex se come al jugador, Rex se colocará aleatoriamente en las 13 filas más al norte y, 
 *  por lo tanto, puede comenzar mucho más cerca del jugador. Debido al error mencionado anteriormente que a veces crea un túnel sellado, es posible que Rex termine dentro de él y nunca pueda atraparte.
 *  
 *  La lógica se asegura de que el laberinto nunca contenga un pasaje de más de una casilla de ancho. Para ello, nunca permite un bloque de 4 casillas vacías juntas, como se muestra a continuación: *
 */

public struct GenerateMazeJob : IJob
{
    [ReadOnly] public readonly uint Seed;
    [ReadOnly] public readonly bool Debug;
    /// <summary>
    /// No se cuenta La pared que esta más al norte y la de mas al sur. Son las filas que pueden hacer camino
    /// </summary>
    [ReadOnly] public readonly int Rows;
    /// <summary>
    /// No se cuenta La pared que esta más al este y la de mas al oeste. Son las columnas que pueden hacer camino.
    /// </summary>
    [ReadOnly] public readonly int Columns;
    // En el original de 18 rows x 16 columns, el valor son 800. El laberinto solo contiene 240 fichas cortables, 
    /// por lo que la cifra de 800 supone que se deben volver a cortar pasajes existentes o cortes de pasajes abandonados debido a golpes en el borde o la regla de "no se permiten bloques de 4" descrita anteriormente.
    [ReadOnly] public int CellsToCut;
    public NativeBitArray Walls;
    /// <summary>
    /// Default 1
    /// </summary>
    public int LongMinToCut;
    // Default 6
    public int LongMaxToCut;
    public int CellExit;

    private Random random;

    public GenerateMazeJob(uint seed, int rows, int columns, int celdasACorta, Allocator allocator = Allocator.TempJob)
    {
        Seed = seed;
        Rows = rows;
        Columns = columns;
        CellsToCut = celdasACorta;
        Walls = new NativeBitArray(rows * columns + 1, allocator);
        CellExit = 0;
        LongMinToCut = 1;
        LongMaxToCut = 6;
        Debug = false;
        random = new Random(Seed);
    }

    // 16x15 => 800
    // RxC => X
    // X=800*rows*columns/16*15
    public GenerateMazeJob(uint seed, int rows, int columns, Allocator allocator = Allocator.TempJob) : this(seed, rows, columns, (int)(800L * rows * columns / (16L * 15L))) { }

    public void Execute()
    {
        // La generación del laberinto comienza llenando todas las casillas con una pared sólida.
        Walls.SetBits(0, true, Walls.Length);

        // La posición inicial del jugador siempre está en el mismo lugar en el extremo sureste del mapa, mirando hacia el oeste.
        int rowCurrent = Rows - 1;
        int colCurrent = Columns - 1;

        int longitud = 0, newX, newY;

        for (int count = CellsToCut; count > 0; count -= longitud)
        {
            newX = rowCurrent;
            newY = colCurrent;
            // corta pasajes al azar eligiendo una dirección (norte, sur, este u oeste) y una longitud (1 a 6). 
            (Direction direction, int deltaX, int deltaY) = GetRandomDirection();

            longitud = random.NextInt(LongMinToCut, LongMaxToCut + 1);
            for (int i = 0; i < longitud; i++)
            {
                // La lógica se asegura de que el laberinto nunca contenga un pasaje de más de una casilla de ancho. Para ello, nunca permite un bloque de 4 casillas vacías juntas
                if (Has4CellCut(newY, newX))
                    break;

                rowCurrent = newY;
                colCurrent = newX;
                SetWall(rowCurrent, colCurrent);
                newX = colCurrent + deltaX;
                newY = rowCurrent + deltaY;
                if (newX < 0 || newX >= Columns)
                    break;
                if (newY < 0 || newY >= Rows)
                    break;
            }
        }
    }

    private (Direction direction, int deltaX, int deltaY) GetRandomDirection()
    {
        int deltaX, deltaY;
        var direction = (Direction)random.NextInt((int)Direction.End);

        if (rowCurrent == 0 && direction == Direction.North)
            direction = Direction.South;
        else if (rowCurrent == Rows - 1 && direction == Direction.South)
            direction = Direction.North;
        else if (colCurrent == 0 && direction == Direction.West)
            direction = Direction.East;
        else if (colCurrent == Columns - 1 && direction == Direction.East)
            direction = Direction.West;

        switch (direction)
        {
            case Direction.North:
                deltaX = 0;
                deltaY = -1;
                break;
            case Direction.South:
                deltaX = 0;
                deltaY = 1;
                break;
            case Direction.East:
                deltaX = 1;
                deltaY = 0;
                break;
            case Direction.West:
                deltaX = -1;
                deltaY = 0;
                break;
            default:
                deltaX = deltaY = 0;
                break;
        }

        return (direction, deltaX, deltaY);
    }

    private int GetIdx(int row, int col) => row * Columns + col;

    private void SetWall(int row, int col, bool value = true) => Walls.Set(GetIdx(row, col), value);

    private bool HasWall(int row, int col) => (row < 0 || row > Rows) || (col < 0 || col > Columns) || Walls.IsSet(GetIdx(row, col));

    private int CountHWalls(int row, int col, int n)
        => ((row < 0 || row > Rows) || (col < 0 || col > Columns))
            ? 0
            : Walls.CountBits(GetIdx(row, col), math.min(n, Columns - col));

    private bool Has4CellCut(int row, int col)
        /*
         *    0       1       2       3       4
            W W W   . . W   W . .   W W W   W W W
            W * W   . * W   W * .   . * W   W * .
            W W W   W W W   W W W   . . W   W . .
        */


        =>
            CountHWalls(row - 1, col - 1, 2) == 2 && HasWall(row, col - 1)   // Caso 1
            || CountHWalls(row - 1, col, 2) == 2 && HasWall(row, col + 1)   // Caso 2
            || CountHWalls(row + 1, col - 1, 2) == 2 && HasWall(row, col - 1)   // Caso 3
            || CountHWalls(row + 1, col, 2) == 2 && HasWall(row, col + 1)   // Caso 4
            ;

    private enum Direction { North, South, East, West, End = West }
}
