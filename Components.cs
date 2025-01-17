﻿using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DotsInput
{
    public partial struct DotsInputData : IComponentData
    {
        public int2 primitiveRange;
        public int2 axisRange;
    }

    public partial struct DotsInputUnregisteredTag : IComponentData
    {
    }

    public partial struct InputPointerLtwPresentation : IComponentData
    {
        public float3 viewPort;
        public float3 screen;
        public Matrix4x4 cameraMatrix;
        public Matrix4x4 projectionMatrix;
        public float screenWidth;
        public float screenHeight;
        public float farClip;
    }

    public partial struct DotsInputAsset : IComponentData
    {
        public UnityObjectRef<InputActionAsset> asset;
    }

    public partial struct DotsInputString : IComponentData
    {
        public FixedString128Bytes value;
    }

    [InternalBufferCapacity(0)]
    public partial struct DotsInputAxisElement : IBufferElementData
    {
        public float3 value;
        public float2 GetFloat2() => new float2(value.x, value.y);
        public float3 GetFloat3() => value;
    }

    [InternalBufferCapacity(0)]
    [StructLayout(LayoutKind.Explicit)]
    public partial struct DotsInputPrimitiveElement : IBufferElementData
    {
        [FieldOffset(0)] public int iValue;
        [FieldOffset(0)] public float fValue;
        [FieldOffset(0)] public bool bValue;
        [FieldOffset(4)] public uint atTick;

        public int GetInt() => iValue;
        public float GetFloat() => fValue;
        public bool GetBool() => bValue;
        public bool IsPressedAt(uint tick) => bValue && atTick == tick;
    }
}