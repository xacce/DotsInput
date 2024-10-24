
using Unity.Burst;
using Unity.Entities;

namespace DotsInput
{
    
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
#if DOTS_INPUT_NETCODE
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
#endif
[BurstCompile]
public partial struct DotsInputFixedTickSystem : ISystem
{
    public struct Singleton : IComponentData
    {
        public uint tick;
    }

    public void OnCreate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<Singleton>())
        { 
            Entity singletonEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(singletonEntity, new Singleton());
        }
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ref Singleton singleton = ref SystemAPI.GetSingletonRW<Singleton>().ValueRW;
        singleton.tick++;
    }
}
}
