using StarterAssets;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    private GameObject _player;
    private FirstPersonController _playerController;
    private CharacterController _characterController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _playerController = _player.GetComponent<FirstPersonController>();
        _characterController = _player.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Rex siempre se mueve a un ritmo más lento que el jugador. Cuando el jugador se mueve, Rex se mueve a la mitad de la velocidad del jugador. Cuando el jugador está parado, Rex se mueve a un cuarto de la velocidad de movimiento del jugado
        //navMeshAgent.speed = _characterController.velocity.x == 0 ? _playerController.MoveSpeed / 4 : _playerController.MoveSpeed / 2;
        //navMeshAgent.SetDestination(_player.transform.position);
    }
}
