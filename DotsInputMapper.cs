using UnityEngine.InputSystem;

namespace DotsInput
{
    public static class DotsInputMapper
    {
        public enum DotsInputType
        {
            Bool,
            Int,
            Float,
            Float2,
            Float3
        }

        public static bool TryGetPrimitiveDotsInputType(InputAction c, out DotsInputType type)
        {
            switch (c.expectedControlType)
            {
                case "Button":
                    type = DotsInputType.Bool;
                    break;
                default:
                    type = DotsInputType.Int;
                    return false;
            }

            return true;
        } 
        public static bool TryGetAxisDotsInputType(InputAction c, out DotsInputType type)
        {
            switch (c.expectedControlType)
            {
                case "Vector2":
                    type = DotsInputType.Float2;
                    break;
                case "Vector3":
                    type = DotsInputType.Float3;
                    break;
                default:
                    type = DotsInputType.Int;
                    return false;
            }

            return true;
        }
    }
}