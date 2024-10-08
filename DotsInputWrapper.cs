﻿using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DotsInput
{
    public partial struct DotsInputWrapper<TPrimitive, TAxis> where TPrimitive : unmanaged, Enum where TAxis : unmanaged, Enum
    {
        [ReadOnly] public NativeArray<DotsInputPrimitiveElement> primitives;
        [ReadOnly] public NativeArray<DotsInputAxisElement> axises;
        public uint tick;


        public static DotsInputWrapper<TPrimitive, TAxis> FromJob(uint tick, DynamicBuffer<DotsInputPrimitiveElement> p, DynamicBuffer<DotsInputAxisElement> a)
        {
            return new DotsInputWrapper<TPrimitive, TAxis>()
            {
                axises = a.AsNativeArray(),
                primitives = p.AsNativeArray(),
                tick = tick,
            };
        }

        public bool isReady => primitives.Length > 0 || axises.Length > 0;


        public unsafe int GetInt(TPrimitive index) => primitives[*(int*)(&index)].GetInt();
        public unsafe float GetFloat(TPrimitive index) => primitives[*(int*)(&index)].GetFloat();
        public unsafe float2 GetFloat2(TAxis index) => axises[*(int*)(&index)].GetFloat2();
        public unsafe float3 GetFloat3(TAxis index) => axises[*(int*)(&index)].GetFloat3();
        public unsafe bool GetBool(TPrimitive index) => primitives[*(int*)(&index)].GetBool();
        public unsafe bool IsPressedNow(TPrimitive index) => primitives[*(int*)(&index)].IsPressedAt(tick);

        public int GetInt(int index) => primitives[(index)].GetInt();
        public float GetFloat(int index) => primitives[(index)].GetFloat();
        public float2 GetFloat2(int index) => axises[(index)].GetFloat2();
        public float3 GetFloat3(int index) => axises[(index)].GetFloat3();
        public bool GetBool(int index) => primitives[(index)].GetBool();
        public bool IsPressedNow(int index) => primitives[(index)].IsPressedAt(tick);
    }
}