using Unity.Entities;
using UnityEngine;

class MazeGeneratorAuthoring : MonoBehaviour
{
    public Entity WallEntity { get; private set; }
    public GameObject wallPrefab;

    private void Start()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //WallEntity = entityManager.get.CreateEntity(WallEntity);
        //entityManager.AddComponentData(WallEntity, new MazeData { WallEntity = WallEntity });
    }
}

//class MazeGeneratorAuthoringBaker : Baker<MazeGeneratorAuthoring>
//{
//    public override void Bake(MazeGeneratorAuthoring authoring)
//    {
//        // The entity will be moved
//        var entity = GetEntity(TransformUsageFlags.Renderable);

//        AddComponent(entity, new MazeData { WallEntity = GetEntity(authoring.wallPrefab, TransformUsageFlags.Dynamic) });
//    }
//}
