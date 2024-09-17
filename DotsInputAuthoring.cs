#if UNITY_EDITOR

using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DotsInput
{


    [DisallowMultipleComponent]
    public class DotsInputAuthoring : MonoBehaviour
    {
        [SerializeField] private InputActionAsset asset_s;

        private class DotsInputBaker : Baker<DotsInputAuthoring>
        {
            public override void Bake(DotsInputAuthoring authoring)
            {
                if (authoring.asset_s == null) return;
                var e = GetEntity(TransformUsageFlags.None);
                AddComponent(e, new DotsInputAsset { asset = authoring.asset_s });
                AddBuffer<DotsInputPrimitiveElement>(e);
                AddBuffer<DotsInputAxisElement>(e);
                AddComponent<DotsInputData>(e);
            }
        }
    }
}
#endif