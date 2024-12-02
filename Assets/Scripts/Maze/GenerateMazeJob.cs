using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

/*
 La generación del laberinto se realiza mediante la ejecución del código en BASIC 
después de activar el modo rápido del ZX81 mediante el comando FAST. Por lo 
general, este proceso tarda aproximadamente 30 segundos en completarse, 
creando un laberinto de dimensiones 16x18 celdas.
El algoritmo de generación del laberinto es teóricamente simple, como se explica 
en el artículo de Soft Tango UK [23]. Inicialmente, se llenan todas las celdas con 
paredes. A continuación, se trazan pasillos de forma aleatoria, escogiendo una 
dirección (norte, sur, este u oeste) y una longitud (de 1 a 6 celdas), ambos 
componentes aleatorios para cada pasillo. El primer pasillo siempre se inicia en 
la posición de inicio del jugador, ubicada en la esquina sureste del mapa. 
Después, cada nuevo pasillo se inicia al final del anterior. El código también 
asegura que no haya pasillos con un ancho mayor de una celda, y que no se 
tracen pasillos en las paredes limítrofes del norte, sur, este u oeste.
El código continúa creando pasillos hasta que se han intentado convertir 800 
celdas en pasillos. Aunque esto supone crear pasillos en más celdas de las que 
hay en el laberinto (240 celdas), se asume que se recorrerán algunos pasillos 
varias veces o se abandonarán algunos trazos debido a las restricciones 
anteriores. Una vez generado el laberinto, el código elige aleatoriamente una 
posición para la salida, buscando una celda vacía en las siete filas más al norte 
y, si es necesario, rellenando las celdas adyacentes para asegurarse de que la 
salida se encuentra al final de un callejón sin salida. No obstante, debido a la 
lógica utilizada para colocar la salida, es posible que la salida se encuentre en 
un túnel cerrado, lo que impide ganar el juego. No se ha implementado código 
para prevenir esta situación, aunque es poco común.
Por último, el código posiciona a Rex de forma aleatoria en las cinco filas más 
al norte del laberinto. Cabe destacar que, al final del juego, si Rex ha comido al 
jugador, el monstruo se posiciona aleatoriamente en las trece filas más al norte 
del laberinto, lo
 */

public struct GenerateMazeJob
{

}
