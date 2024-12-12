using Unity.Entities;
using UnityEngine;

namespace DotsInput
{
    public class InputPointerLtwPresentationAuthoring:MonoBehaviour
    {
        private class InputPointerLtwPresentationBaker : Baker<InputPointerLtwPresentationAuthoring>
        {
            public override void Bake(InputPointerLtwPresentationAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.WorldSpace);
                AddComponent<InputPointerLtwPresentation>(e);
            }
        }
    }
}