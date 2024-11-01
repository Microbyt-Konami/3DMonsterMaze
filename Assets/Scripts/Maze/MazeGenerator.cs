using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

/*
    https://www.redalyc.org/articulo.oa?id=81629470011

    https://oa.upm.es/76050/1/TFG_JOSE_ANTONIO_MARTINEZ_MARTINEZ.pdf

Laberinto “Perfecto” o conectado simplemente: Un laberinto perfecto, es aquel laberinto
que no presenta ciclos en su estructura, es decir, aquel para el que solo existe un único
camino entre dos puntos cualesquiera. Además, un laberinto perfecto no debe poseer
ninguna región inaccesible.
•
Laberinto “Unicursal” o de camino único: Un laberinto unicursal es aquel que solo tiene
un único camino, es decir, aquel que no tiene ninguna bifurcación y en el que, en todo
momento, solo se puede avanzar o retroceder. Un ejemplo de laberinto unicursal sería
una espiral.
•
Algoritmo “Uniforme”: De un algoritmo de generación procedural de laberintos se dice
que es uniforme si los laberintos que genera son generados de manera equiprobable
entre todos los posibles laberintos de las mismas dimensiones. Por el contrario, un
algoritmo es no uniforme si de entre todos los posibles laberintos que podrían generarse
para unas dimensiones hay alternativas que se generan con más o menos probabilidad
que el resto o si directamente hay laberintos que nunca se generarán.
•
Bias: De un laberinto se dice que presenta un bias si la estructura del laberinto posee
alguna tendencia en las direcciones de sus caminos. Por ejemplo, un laberinto que tenga
un bias horizontal tendrá pasillos horizontales más largos y frecuentes, conectados entre
sí por cortos pasillos verticales. Un laberinto perfecto puede tener bias sin dejar de ser
perfecto por ello.

En función de si añaden o eliminan muros:
•
Algoritmos de eliminación de muros o de tallado. Estos algoritmos parten de un laberinto
en el que todas las celdas están aisladas entre sí por paredes y se basan en decidir si
conectar o no las celdas eliminando dichas paredes.
•
Algoritmos de adición de muros. Estos algoritmos parten de una única área vacía y van
añadiendo paredes segmentando dicha área sin llegar a dividirla completamente.
Cabe destacar que, aunque no es el caso de todos los algoritmos, existen algunos que pueden
funcionar tanto añadiendo como eliminando muros, aunque puedan varias sus características
en función de una u otra estrategia.

----------------------------------------------------------------------------------------------------------------

Árbol binario
Se trata del algoritmo más sencillo de entre todos. Debido a su trivialidad no se le atribuye autor
concreto.
Ha sido elegido para servir como punto de referencia al resto de algoritmos. Es el más sencillo
de implementar y produce los laberintos más limitados, por lo que el resto de los algoritmos
supondrán mejoras respecto a este.
El funcionamiento de este algoritmo se basa en la toma azarosa de decisiones. Para cada celda
se elige al azar si esta se conecta con su vecina derecha o superior.
Si no hay vecinas superiores, como es el caso de la última fila, se conectan todas las celdas
con su vecina derecha.
Si no hay vecinas derechas, como es el caso de la última columna, se conectan todas las celdas
con su vecina superior.
La otra única excepción es la celda de la esquina superior derecha. Esta celda se ignora al no
tener vecinos válidos.
Usualmente, se emplea un recorrido en orden, empezando por la esquina inferior izquierda y
acabando en la superior derecha, pero, como cada celda se trata de forma independiente, se
puede realizar en el orden que se desee siempre y cuando se visiten todas las celdas.
Cabe destacar que este algoritmo también se puede usar añadiendo muros en vez de
eliminándolos, en cuyo caso la única diferencia sería elegir si no se añade el muro superior o
el derecho.
Se trata de un algoritmo no uniforme basado en sets en el que hay posibles laberintos que
nunca se generarán. No puede generar intersecciones cuádruples debido a su forma
excluyente de conectar las celdas, la última fila y columna siempre son pasillos y los laberintos
que crea tienen un notable bias diagonal desde la esquina inferior izquierda a la superior
derecha.
A su favor, tiene que, al ser tan sencillo, es el algoritmo más fácil de implementar y el más
rápido en ejecución y, además, no consume memoria extra.
18En contra tiene las limitaciones y bias que introduce en los laberintos que genera y su no
uniformidad.
Su complejidad temporal es de O(N 2 ), ya que solo debe visitar cada celda una vez.
Su complejidad espacial es O(1), puesto que, al tratar de manera independiente cada celda, no
debe mantener en memoria ningún dato.
A continuación, se mostrarán unas capturas de la herramienta de visualizado generadas
empleando este algoritmo para destacar las características mencionadas.

Para cada Celda del laberinto:
    If Celda no tiene vecina superior ni derecha:
        Continue
    Else If Celda no tiene vecina superior:
        Conectar Celda y vecina derecha
    Else If Celda no tiene vecina derecha:
        Conectar Celda y vecina superior
    Else:
        Conectar Celda y vecina superior o derecha al azar

----------------------------------------------------------------------------------------------------------------

Bactracker recursivo
Se trata del algoritmo más común de entre los algoritmos seleccionados y, al igual que el
algoritmo de árbol binario, carece de autor concreto. Genera unos laberintos menos limitados
a costa de un pequeño incremento en la complejidad del algoritmo. Gracias a ser uno de los
algoritmos con mejor relación complejidad resultados suele emplearse como algoritmo por
defecto a la hora de abordar la generación de laberintos.
Ha sido elegido por ser el algoritmo más popular y por su relación entre complejidad y
resultados.
El algoritmo se basa en el uso de la recursividad, aunque es más frecuente la implementación
utilizando una estructura de pila.
En la implementación de pila, se comienza añadiendo a la pila una celda aleatoria.
En cada iteración, se escoge una vecina no visitada aleatoria de la cabeza actual de la pila, se
conecta con la misma y se añade a la pila como nueva cabeza.
21Cuando ya no quedan vecinas sin visitar en la cabeza actual, esta es eliminada de la pila y
comienza la marcha atrás. Se van extrayendo a la inversa las celdas de la pila en orden inverso
y se comprueba si alguna tiene vecinas sin visitar, en cuyo caso se continúa con el proceso
normal.
Este proceso de adición marcha atrás, adición se repite hasta que la pila se vacía por completo,
momento en el que el algoritmo se da por concluido.
Cabe destacar que este algoritmo solo funciona correctamente eliminando paredes y no
añadiéndolas.
Se trata de un algoritmo no uniforme basado en árboles en el que hay posibles laberintos que
nunca se generarán. Parte de su no uniformidad viene derivada de la tendencia que presenta
este algoritmo a generar caminos largos pero escasos. Como cada camino es extendido hasta
que la última celda no tenga vecinos, en vez de varios caminos cortos, se obtendrán caminos
largos siempre que sea posible imposibilitando que se den ciertas formas.
Además de no ser uniforme, este algoritmo no tiene bias.
A su favor, tiene el ser sencillo de implementar y de ejecución rápida, aunque no tanto como el
algoritmo de árbol binario, y que presenta una muy buena relación entre complejidad de
implementación y laberintos obtenidos, siendo esta última cualidad la que hace tan popular a
este algoritmo.
Su principal desventaja es que no uniforme con laberintos que no se dan y que tiene un
consumo de memoria notable, pero, a excepción de esto, se trata de un algoritmo bastante
completo y con muy buena relación entre complejidad y resultados.
Su complejidad temporal es de O(N 2 ), ya que este algoritmo se ve obligado a visitar, como
mínimo, dos veces cada celda. Una la primera vez que dicha celda es añadida a la pila y otra
cuando se comprueba por última vez si tiene vecinos no visitados.
Su complejidad espacial es O(N 2 ), puesto que, en el peor de los casos, debe guardar en
memoria la totalidad del laberinto. Esta situación solo podría darse en caso de que el laberinto
formado fuese un laberinto unicursal.

PilaCeldas = nueva pila
    Insertar celda aleatoria en PilaCeldas
Mientras PilaCeldas no esté vacía:
    CeldaActual = Peek cabeza de PilaCeldas (sin extraer la cabeza)
    If CeldaActual tiene vecinos no visitados:
        VecinoElegido = vecino aleatorio de CeldaActual
        Conectar CeldaActual y VecinoElegido
        Insertar VecinoElegido en PilaCeldas
    If CeldaActual no tiene vecinos no visitados
        Extraer cabeza de PilaCeldas

----------------------------------------------------------------------------------------------------------------

Aldous-Broder
Este algoritmo se trata de un caso especial, fue inventado de manera independiente por David
Aldous y Andrei Broder como un algoritmo de generación de árboles generadores uniformes.
De los algoritmos seleccionados, este es el único completamente uniforme y sin ningún tipo de
limitación en los laberintos que genera. Además, se trata de un algoritmo muy sencillo de
implementar y que no requiere almacenar información extra. Sin embargo, este algoritmo
carece de complejidad temporal al no tener un número fijo de iteraciones antes de terminar.
Se ha elegido por generar laberintos completamente uniformes de una manera sencilla.
El algoritmo se basa única y exclusivamente en la toma de decisiones aleatorias. Mientras
queden celdas sin visitar, empezando en una celda aleatoria, se va eligiendo una celda vecina
al azar de la celda actual y se viaja a ella. Si dicha celda no había sido visitada nunca, se
conecta con la celda de la que se procede.
25Este proceso se repite una cantidad indeterminada de veces al no realizar el algoritmo ningún
esfuerzo por incentivar la elección de celdas no visitadas. Esto provoca que el algoritmo pueda
estar repitiendo una serie de movimientos sobre las mismas celdas hasta que se elija
aleatoriamente otra celda a la que viajar.
Cabe destacar que este algoritmo funciona tanto con adición como con eliminación de paredes.
Se trata de un algoritmo completamente uniforme y sin bias, basado en árboles que genera, de
forma equiprobable, cualquier laberinto posible para unas dimensiones dadas.
Las principales ventajas de este algoritmo son su uniformidad, su ausencia de bias, que no
necesita memoria extra para ejecutarse y la sencillez de su implementación.
Por otro lado, su desventaja principal es la velocidad de ejecución que presenta.
Su complejidad temporal es indeterminada, puesto que el número de iteraciones depende del
azar. Como mínimo tiene que visitar cada celda una vez, pero, en el peor de los casos, puede
quedarse atrapado hasta que las elecciones aleatorias le lleven a la última celda.
Su complejidad espacial es O(1) al no necesitar almacenar ninguna información.

CeldaActual = celda aleatoria del laberinto
    Mientras haya celdas sin visitar:
        Marcar CeldaActual como visitada
        VecinoAleatorio = vecino aleatorio de CeldaActual
        If VecinoAleatorio no ha sido visitado aún:
            Conectar CeldaActual y VecinoAleatorio
        CeldaActual = VecinoAleatorio

----------------------------------------------------------------------------------------------------------------

Árbol creciente
Se trata de un algoritmo especialmente adaptable. Fue inventado por Walter Pullen.
Lo que hace tan polivalente a este algoritmo es que, en función de cómo se elijan las celdas a
visitar en cada iteración, puede simular a otros algoritmos como por ejemplo el de Backtracker
recursivo.
El motivo de su elección es la polivalencia que presenta.
El funcionamiento de este algoritmo es similar al de Backtracker recursivo, pero empleando una
lista en vez de una pila.
Se comienza con una lista de celdas activas en la cual se ha introducido una celda aleatoria.
En cada iteración del algoritmo, utilizando una función de elección, se elige qué celda de las
presentes en dicha lista será la elegida para expandir el laberinto.
28De esa celda elegida se escoge al azar una celda vecina no visitada para conectar ambas y
añadir dicha vecina a la lista de celdas activas. Si se da el caso de que una celda no tiene
vecinas no visitadas, se elimina de la lista.
La principal diferencia respecto al algoritmo de backtracker recursivo y la que le proporciona
esta capacidad de imitar otros algoritmos es ese uso de una función de elección en vez de
escoger siempre la última celda añadida. Cabe destacar que este algoritmo puede funcionar
tanto añadiendo como eliminando paredes, aunque la implementación más común y sencilla
es eliminando.
El algoritmo se basa en árboles, no es uniforme debido a que algunas estructuras tienen más
probabilidades de generarse y no tiene bias.
Como características positivas tiene el ser altamente personalizable y poderse adaptar a las
necesidades específicas que se tengan.
Como punto negativo tiene que no es completamente uniforme y que, según qué función de
elección se utilice, puede ser complejo de implementar.
Su complejidad temporal depende en gran medida de la función de elección. Si asumimos una
complejidad temporal de la función de elección de O(1), como es el caso del backtracker
recursivo, este algoritmo tendría una complejidad de O(N 2 ), ya que, de media, visita cada celda
un mínimo de 2 veces.
Su complejidad espacial es O(N 2 ), puesto que, en el caso peor, debe almacenar todas las
celdas en memoria.

CeldasActivas = nueva lista
Insertar celda aleatoria en CeldasActivas

Mientras CeldasActivas tenga celdas:
    CeldaActual = celda de CeldasActivas elegida mediante una función
    If CeldaActual tiene vecinos no visitados:
        VecinoElegido = vecino no visitado aleatorio de CeldaActual
        Conectar CeldaActual y VecinoElegido
        Añadir VecinoElegido a CeldasActivas
    If CeldaActual no tiene vecinos no visitados:
        Eliminar CeldaActual de CeldasActivas

La función que elige CeldaActual en cada iteración puede ser elegida con total libertad.
Algunas funciones populares son: la última celda de la lista siempre (comportamiento de
backtracker recursivo), la primera siempre, aleatorio entre todas.

----------------------------------------------------------------------------------------------------------------

Eller
Este algoritmo es el más complejo de implementar de entre todos los elegidos. Fue inventado
por Marlin Eller.
De los algoritmos vistos hasta ahora, este es el único que utiliza conjuntos de celdas en el
proceso de creación de los laberintos.
Además, al igual que el algoritmo de árbol binario, es capaz de generar laberintos de infinita
longitud, ya que trabaja fila a fila.
Ha sido elegido por ser excepcionalmente diferente al resto de algoritmos vistos y por sus
capacidades de generación alternativas.
32Este algoritmo basa su funcionamiento en el uso de conjuntos y la operación fila a fila.
Empezando con la primera fila, cada una de las celdas se asigna a un conjunto independiente.
Después, para cada par de conjuntos independientes adyacentes, se decide aleatoriamente si
se unen o no, en cuyo caso se conectan las celdas de los conjuntos y se agrupan en un único
superconjunto.
Una vez se ha decidido para cada conjunto si se une o no y se ha llegado al final de la fila,
comienza la segunda parte del algoritmo.
Se elige, de manera aleatoria, un número de celdas de cada conjunto, debiendo elegirse como
mínimo una o todas como máximo.
Las celdas seleccionadas de cada conjunto son conectadas con sus vecinas de la siguiente
fila.
Finalmente, las celdas de la siguiente fila que hayan sido conectadas con la anterior se asignan
a los conjuntos de las celdas con las que se hayan conectado y se asignan nuevos conjuntos
a aquellas que no hayan sido conectadas.
De esta manera se logra que la información relativa a la pertenencia de cada celda a un
conjunto permee entre filas para evitar la formación de ciclos que podrían darse al unir
accidentalmente dos celdas conectadas en filas anteriores.
Todo este proceso termina en la última fila donde se conectan todas las celdas que no
pertenezcan al mismo conjunto entre sí para terminar de unir todo el laberinto en un único
laberinto perfecto.
Cabe destacar que este algoritmo funciona tanto con eliminación como con adición de muros.
Se trata de un algoritmo basado en sets no uniforme, puesto que algunos de los laberintos que
puede generar se dan con mayor probabilidad que otros, y con bias.
Su falta de uniformidad se debe a que, al ejecutarse fila a fila, las uniones de conjuntos son
más probables en las filas superiores que en las inferiores porque, conforme se avance de fila,
habrá habido más tiradas de azar para unir los conjuntos.
Sus principales ventajas son la poca memoria que necesita, al solo almacenar una fila en
memoria, y que puede emplearse para generar laberintos de infinita longitud.
Como desventajas tiene ser muy complejo de implementar y que no es plenamente uniforme.
Su complejidad temporal es O(N 2 ), ya que, en el caso peor, debe visitar cada celda 2 veces,
una para decidir si une o no cada par de celdas de una fila y otra para unir cada conjunto de
celdas con la fila siguiente.
Su complejidad espacial es O(N) puesto que en todo momento debe almacenar la información
de una fila en memoria.

----------------------------------------------------------------------------------------------------------------
ConjuntosCeldas = lista de conjuntos con tantos conjuntos como columnas haya

Para cada fila menos la última:
    Para cada par de celdas adyacentes en esa fila:
        Si el par pertenece a distinto conjunto:
            Unir los conjuntos y conectar las celdas al azar
    Para cada par de conjuntos:
        Elegir entre 1 y longitud de conjunto celdas de ese conjunto (al azar):
        Conectar esas celdas con las celdas de la siguiente fila
    Reiniciar los conjuntos conservando en el mismo conjunto aquellas celdas que compartan conjunto superior
 */

public struct EllerJob : IJob
{
    public int rows, columns;

    [ReadOnly] public NativeHashMap<int, int> neighborcells;

    // Conjuntos: { {2},{3}.{2,4},{5} }
    // setCellsRefs: { 2,3,2,4,5 }
    // setsNumber:{ 1,2,3,3,4 }
    [ReadOnly] public NativeList<int> setCellsRefs;
    [ReadOnly] public NativeList<int> setsNumber;
    [ReadOnly] public int rowCurrent;

    public void Execute()
    {
        InitCells();
    }

    private void InitCells()
    {
        neighborcells = new NativeHashMap<int, int>();
        setCellsRefs = new NativeList<int>();
        setsNumber = new NativeList<int>();
        rowCurrent = rows - 1;
    }
}

public struct Prueba
{
    public float x, y, z;
}

public struct PruebaJob : IJob
{
    public Prueba prueba;
    public NativeArray<float> values;

    public void Execute()
    {
        InitPrueba();
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = prueba.x + 0.2f;
        }
    }

    private void InitPrueba()
    {
        prueba = new Prueba { x = 1, y = 2, z = 3 };
    }
}

public class MazeGenerator : MonoBehaviour
{
    private PruebaJob _job;

    void Start()
    {
        var values = new NativeArray<float>(10, Allocator.TempJob);

        _job = new PruebaJob()
        {
            //prueba = new Prueba { x = 1, y = 2, z = 3 },
            values = values
        };

        // Schedule() puts the job instance on the job queue.
        JobHandle findHandle = _job.Schedule();

        findHandle.Complete();

        Debug.Log($"x: {_job.prueba.x}, y: {_job.prueba.y}, z: {_job.prueba.z}");
        foreach (var value in _job.values)
        {
            Debug.Log(value);
        }
    }
}