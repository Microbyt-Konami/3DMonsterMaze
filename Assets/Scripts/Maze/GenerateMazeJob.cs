using System.CodeDom;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

/*
 *  El laberinto siempre consta de 18 filas por 16 columnas.
 *  La generaci�n del laberinto se produce durante las "nieblas del tiempo" con la m�quina en modo FAST y tarda unos 30 segundos en el ZX81. 
 *  El laberinto se mapea en la memoria y aparece como se muestra a continuaci�n. 
 *  
 *  La l�gica de generaci�n del laberinto es simple pero efectiva y da como resultado cada vez un laberinto que es diferente pero similar en apariencia al anterior, con muchos pasajes y aberturas entrecruzados. 
 *  Esto tiene el efecto de hacer que sea m�s f�cil para Rex encontrarte y mantener el laberinto dif�cil de resolver y que parezca m�s grande de lo que es.
 *  
 *  La generaci�n del laberinto comienza llenando todas las casillas con una pared s�lida. Luego, corta pasajes al azar eligiendo una direcci�n (norte, sur, este u oeste) y una longitud (1 a 6). 
 *  Cada pasaje que se corta comienza al final del �ltimo. El primer pasaje siempre comienza en la posici�n inicial del jugador (que se muestra en el mapa anterior como "P"). 
 *  La posici�n inicial del jugador siempre est� en el mismo lugar en el extremo sureste del mapa, mirando hacia el oeste.
 *  
 *  La l�gica se asegura de que el laberinto nunca contenga un pasaje de m�s de una casilla de ancho. Para ello, nunca permite un bloque de 4 casillas vac�as juntas
 *  
 *  La �nica otra regla que el cortador de pasajes observa es que no puede cortar en las paredes m�s al oeste, norte o sur. La pared que est� m�s al este del laberinto s� se corta, pero debido a la naturaleza 
 *  envolvente de la forma en que el mapa se almacena en la memoria, la pared m�s al oeste tambi�n es la pared m�s al este, por lo que, en efecto, el laberinto siempre est� rodeado por una pared s�lida e 
 *  ininterrumpida por todos los lados.
 *  
 *  El c�digo corta pasajes de forma repetitiva hasta que intenta cortar 800 fichas, y luego considera que el laberinto est� completo. No hay ning�n otro c�digo que garantice que el laberinto se complete 
 *  satisfactoriamente, simplemente se supone que despu�s de 800 fichas deber�a estarlo. El laberinto solo contiene 240 fichas cortables, por lo que la cifra de 800 supone que se deben volver a cortar pasajes existentes 
 *  o cortes de pasajes abandonados debido a golpes en el borde o la regla de "no se permiten bloques de 4" descrita anteriormente.
 *  
 *  Una vez que se completa la generaci�n del laberinto, el juego elige una ubicaci�n para la salida. Lo hace utilizando un enfoque un tanto torpe y ca�tico. Dentro de las 7 filas m�s al norte, elige una ficha al azar. 
 *  Si esta ficha es un trozo de pared, elige otra. Esto contin�a hasta que encuentra una ficha vac�a y all� coloca la salida (mostrada como "H" en el laberinto anterior). Para asegurarse de que la salida est� siempre 
 *  al final de un callej�n sin salida, luego llena todos los espacios vac�os alrededor de la ficha elegida hasta que solo quede 1. Lo hace en el orden de prioridad norte, este, oeste y sur.
 *  
 *  Lamentablemente, debido a la l�gica rudimentaria que se utiliza para colocar la salida, a veces ocurre que la salida termina en un t�nel cerrado y no se puede ganar el juego. No existe ning�n c�digo que impida 
 *  esto y, aunque es poco frecuente, sucede.
 *  
 *  Por �ltimo, el Rex se coloca aleatoriamente en las 5 filas m�s al norte. Ten en cuenta que cuando el juego termina despu�s de que Rex se come al jugador, Rex se colocar� aleatoriamente en las 13 filas m�s al norte y, 
 *  por lo tanto, puede comenzar mucho m�s cerca del jugador. Debido al error mencionado anteriormente que a veces crea un t�nel sellado, es posible que Rex termine dentro de �l y nunca pueda atraparte.
 *  
 *  La l�gica se asegura de que el laberinto nunca contenga un pasaje de m�s de una casilla de ancho. Para ello, nunca permite un bloque de 4 casillas vac�as juntas, como se muestra a continuaci�n: *
 */

public struct GenerateMazeJob : IJob
{
    [ReadOnly] public uint Seed;
    /// <summary>
    /// No se cuenta La pared que esta m�s al norte y la de mas al sur. Son las filas que pueden hacer camino
    /// </summary>
    [ReadOnly] public int Rows;
    /// <summary>
    /// No se cuenta La pared que esta m�s al este y la de mas al oeste. Son las columnas que pueden hacer camino.
    /// </summary>
    [ReadOnly] public int Columns;
    // En el original de 18 rows x 16 columns, el valor son 800. El laberinto solo contiene 240 fichas cortables, 
    /// por lo que la cifra de 800 supone que se deben volver a cortar pasajes existentes o cortes de pasajes abandonados debido a golpes en el borde o la regla de "no se permiten bloques de 4" descrita anteriormente.
    [ReadOnly] public int CellsToCut;
    public NativeArray<CellWall> Walls;
    /// <summary>
    /// Default 1
    /// </summary>
    public int LongMinToCut;
    // Default 6
    public int LongMaxToCut;
    public int CellExit;

    public GenerateMazeJob(uint seed, int rows, int columns, int celdasACorta, Allocator allocator = Allocator.TempJob)
    {
        Seed = seed;
        Rows = rows;
        Columns = columns;
        CellsToCut = celdasACorta;
        Walls = new NativeArray<CellWall>(rows * columns + 1, allocator);
        CellExit = 0;
        LongMinToCut = 1;
        LongMaxToCut = 6;
    }

    // 16x15 => 800
    // RxC => X
    // X=800*rows*columns/16*15
    public GenerateMazeJob(uint seed, int rows, int columns, Allocator allocator = Allocator.TempJob) : this(seed, rows, columns, (int)(800L * rows * columns / (16L * 15L))) { }

    public void Execute()
    {
        var random = new Random(Seed);

        // La generaci�n del laberinto comienza llenando todas las casillas con una pared s�lida.
        for (int i = 0; i < Walls.Length; i++)
            Walls[i] = CellWall.AllWalls;

        // La posici�n inicial del jugador siempre est� en el mismo lugar en el extremo sureste del mapa, mirando hacia el oeste.
        int row = Rows - 1, col = Columns - 1;
        int longitud = 0, deltaX, deltaY, newX, newY;
        Direction direction;
        CellWall wall;

        for (int count = CellsToCut; count > 0;)
        {
            // corta pasajes al azar eligiendo una direcci�n (norte, sur, este u oeste) y una longitud (1 a 6). 
            direction = (Direction)random.NextInt((int)Direction.End);
            switch (direction)
            {
                case Direction.North:
                    deltaX = 0;
                    deltaY = -1;
                    wall = CellWall.North;
                    break;
                case Direction.South:
                    deltaX = 0;
                    deltaY = 1;
                    wall = CellWall.South;
                    break;
                case Direction.East:
                    deltaX = 1;
                    deltaY = 0;
                    wall = CellWall.East;
                    break;
                case Direction.West:
                    deltaX = -1;
                    deltaY = 0;
                    wall = CellWall.West;
                    break;
                default:
                    deltaX = deltaY = 0;
                    break;
            }
            longitud = random.NextInt(LongMinToCut, LongMaxToCut + 1);
            for (int i = 0; i < longitud; i++)
            {
                // La l�gica se asegura de que el laberinto nunca contenga un pasaje de m�s de una casilla de ancho. Para ello, nunca permite un bloque de 4 casillas vac�as juntas
                /*
                        W W W   . W W
                        W W W   W W W
                        W W W   W W W
                 */
                newX = col + deltaX;
                newY = row + deltaY;
                if (newX < 0 || newX >= Columns)
                    break;
                if (newY < 0 || newY >= Rows)
                    break;

            }
        }

    }

    private enum Direction { North, South, East, West, End = West }
}