using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

/*
 La generaci�n del laberinto se realiza mediante la ejecuci�n del c�digo en BASIC 
despu�s de activar el modo r�pido del ZX81 mediante el comando FAST. Por lo 
general, este proceso tarda aproximadamente 30 segundos en completarse, 
creando un laberinto de dimensiones 16x18 celdas.
El algoritmo de generaci�n del laberinto es te�ricamente simple, como se explica 
en el art�culo de Soft Tango UK [23]. Inicialmente, se llenan todas las celdas con 
paredes. A continuaci�n, se trazan pasillos de forma aleatoria, escogiendo una 
direcci�n (norte, sur, este u oeste) y una longitud (de 1 a 6 celdas), ambos 
componentes aleatorios para cada pasillo. El primer pasillo siempre se inicia en 
la posici�n de inicio del jugador, ubicada en la esquina sureste del mapa. 
Despu�s, cada nuevo pasillo se inicia al final del anterior. El c�digo tambi�n 
asegura que no haya pasillos con un ancho mayor de una celda, y que no se 
tracen pasillos en las paredes lim�trofes del norte, sur, este u oeste.
El c�digo contin�a creando pasillos hasta que se han intentado convertir 800 
celdas en pasillos. Aunque esto supone crear pasillos en m�s celdas de las que 
hay en el laberinto (240 celdas), se asume que se recorrer�n algunos pasillos 
varias veces o se abandonar�n algunos trazos debido a las restricciones 
anteriores. Una vez generado el laberinto, el c�digo elige aleatoriamente una 
posici�n para la salida, buscando una celda vac�a en las siete filas m�s al norte 
y, si es necesario, rellenando las celdas adyacentes para asegurarse de que la 
salida se encuentra al final de un callej�n sin salida. No obstante, debido a la 
l�gica utilizada para colocar la salida, es posible que la salida se encuentre en 
un t�nel cerrado, lo que impide ganar el juego. No se ha implementado c�digo 
para prevenir esta situaci�n, aunque es poco com�n.
Por �ltimo, el c�digo posiciona a Rex de forma aleatoria en las cinco filas m�s 
al norte del laberinto. Cabe destacar que, al final del juego, si Rex ha comido al 
jugador, el monstruo se posiciona aleatoriamente en las trece filas m�s al norte 
del laberinto, lo
 */

public struct GenerateMazeJob
{

}
