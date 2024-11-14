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
        [SerializeField] private bool collectInputString;

        private class DotsInputBaker : Baker<DotsInputAuthoring>
        {
            public override void Bake(DotsInputAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                if (authoring.asset_s != null)
                {
                    AddComponent(e, new DotsInputAsset { asset = authoring.asset_s });
                    AddBuffer<DotsInputPrimitiveElement>(e);
                    AddBuffer<DotsInputAxisElement>(e);
                    AddComponent<DotsInputData>(e);
                    AddComponent<DotsInputUnregisteredTag>(e);
                }

                if (authoring.collectInputString)
                    AddComponent(e, new DotsInputString { });
            }
        }
    }
}
#endif