using Unity.Burst;
using Unity.Entities;
using UnityEngine;
partial struct MazeGenerationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (mazeData, entity) in SystemAPI.Query<MazeData>().WithEntityAccess())
        {
            var entityManager = state.EntityManager;
            // Instancia una nueva pared
            var wall = entityManager.Instantiate(mazeData.WallEntity);

            // Una vez generado el laberinto, elimina la entidad para evitar re-generación
            entityManager.DestroyEntity(entity);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}