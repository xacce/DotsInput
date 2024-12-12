using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DotsInput
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(DotsInputFixedTickSystem))]
#if DOTS_INPUT_NETCODE
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
#endif
    public partial class DotsInputSystem : SystemBase
    {
        private NativeList<DotsInputPrimitiveElement> _inputs;
        private NativeList<DotsInputAxisElement> _axis;
        private Dictionary<int, (int, DotsInputMapper.DotsInputType)> _outPaths;
        private uint _tick;
        private List<InputAction> _cleanupActions;
        private List<InputActionAsset> registered;

        private void RegisterPrimitives(ref DotsInputData dotsInput, InputActionAsset asset, DynamicBuffer<DotsInputPrimitiveElement> primitiveBuffer)
        {
            NativeList<DotsInputPrimitiveElement> inputs = new NativeList<DotsInputPrimitiveElement>(Allocator.Temp);
            using var ie = asset.GetEnumerator();
            int i = 0;
            while (ie.MoveNext())
            {
                var c = ie.Current;
                if (!DotsInputMapper.TryGetPrimitiveDotsInputType(c, out var type)) continue;

                _outPaths[c.GetHashCode()] = (_inputs.Length + i, type);
                inputs.Add(new DotsInputPrimitiveElement() { });
                primitiveBuffer.Add(new DotsInputPrimitiveElement() { });
                c.started += OnEvt;
                c.performed += OnEvt;
                c.canceled += OnEvt;
                _cleanupActions.Add(c);
                i++;
            }

            dotsInput.primitiveRange = new int2(_inputs.Length, _inputs.Length + inputs.Length);
            _inputs.AddRange(inputs);
        }

        private void RegisterAxises(ref DotsInputData dotsInput, InputActionAsset asset, DynamicBuffer<DotsInputAxisElement> axisBuffer)
        {
            NativeList<DotsInputAxisElement> inputs = new NativeList<DotsInputAxisElement>(Allocator.Temp);
            using var ie = asset.GetEnumerator();
            int i = 0;
            while (ie.MoveNext())
            {
                var c = ie.Current;
                if (!DotsInputMapper.TryGetAxisDotsInputType(c, out var type)) continue;
                switch (type)
                {
                    case DotsInputMapper.DotsInputType.Float2 or DotsInputMapper.DotsInputType.Float3:
                        break;
                    default: continue;
                }

                axisBuffer.Add(new DotsInputAxisElement() { });
                inputs.Add(new DotsInputAxisElement() { });
                _outPaths[c.GetHashCode()] = (_axis.Length + i, type);
                Debug.Log($"Bind: {c.name} : {c.GetHashCode()}");
                c.started += OnEvt;
                c.performed += OnEvt;
                c.canceled += OnEvt;
                _cleanupActions.Add(c);
                i++;
            }

            dotsInput.axisRange = new int2(_axis.Length, _axis.Length + inputs.Length);
            _axis.AddRange(inputs);
        }

        protected override void OnStartRunning()
        {
            if (_outPaths != null) return;
            registered = new List<InputActionAsset>();
            _cleanupActions = new List<InputAction>();
            _outPaths = new Dictionary<int, (int, DotsInputMapper.DotsInputType)>();
        }

        protected override void OnCreate()
        {
            _inputs = new NativeList<DotsInputPrimitiveElement>(Allocator.Persistent);
            _axis = new NativeList<DotsInputAxisElement>(Allocator.Persistent);
            _outPaths = null;
        }


        protected override void OnDestroy()
        {
            foreach (var asset in registered)
            {
                asset.Disable(); //hotenter
            }

            foreach (var cleanupAction in _cleanupActions)
            {
                cleanupAction.performed -= OnEvt;
                cleanupAction.started -= OnEvt;
                cleanupAction.canceled -= OnEvt;
            }

            if (_inputs.IsCreated) _inputs.Dispose();
            if (_axis.IsCreated) _axis.Dispose();
        }

        private void OnEvt(InputAction.CallbackContext obj)
        {
            //Action guid cant be used cause its only unique per actionasset
            if (!_outPaths.TryGetValue(obj.action.GetHashCode(), out var o) && _inputs.IsCreated) return;
            switch (o.Item2)
            {
                case DotsInputMapper.DotsInputType.Float:
                {
                    var v = new DotsInputPrimitiveElement();
                    v.fValue = obj.ReadValue<float>();
                    v.atTick = _tick;
                    _inputs[o.Item1] = v;
                    break;
                }
                case DotsInputMapper.DotsInputType.Int:
                {
                    var v = new DotsInputPrimitiveElement();
                    v.iValue = obj.ReadValue<int>();
                    v.atTick = _tick;
                    _inputs[o.Item1] = v;
                    break;
                }
                case DotsInputMapper.DotsInputType.Bool:
                {
                    var v = new DotsInputPrimitiveElement();
                    v.bValue = obj.ReadValueAsButton();
                    v.atTick = _tick;
                    _inputs[o.Item1] = v;
                    break;
                }
                case DotsInputMapper.DotsInputType.Float2:
                {
                    var ov = new DotsInputAxisElement();
                    var v = obj.ReadValue<Vector2>();
                    ov.value = new float3(v.x, v.y, 0f);
                    _axis[o.Item1] = ov;
                    break;
                }
                case DotsInputMapper.DotsInputType.Float3:
                {
                    var ov = new DotsInputAxisElement();
                    ov.value = obj.ReadValue<Vector3>();
                    _axis[o.Item1] = ov;
                    break;
                }
            }
        }

        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
            var ecbM = ecb.CreateCommandBuffer(EntityManager.WorldUnmanaged);
            foreach (var (dotsInputRw, inputAssetRo, primitiveBuffer, axisBuffer, entity) in SystemAPI.Query<
                         RefRW<DotsInputData>,
                         DotsInputAsset,
                         DynamicBuffer<DotsInputPrimitiveElement>,
                         DynamicBuffer<DotsInputAxisElement>
                     >().WithAll<DotsInputUnregisteredTag>().WithEntityAccess())
            {
                var asset = inputAssetRo.asset.Value;
                asset.Enable();
                ref var dotsInput = ref dotsInputRw.ValueRW;
                RegisterPrimitives(ref dotsInput, inputAssetRo.asset, primitiveBuffer);
                RegisterAxises(ref dotsInput, inputAssetRo.asset, axisBuffer);
                Debug.Log($"Asset was added to dots input {asset.name}");
                Debug.Log($"Registered primitives: {_inputs.Length}");
                Debug.Log($"Registered axis: {_axis.Length}");
                ecbM.RemoveComponent<DotsInputUnregisteredTag>(entity);
                registered.Add(asset);
            }

            foreach (var inputString in SystemAPI.Query<RefRW<DotsInputString>>())
            {
                inputString.ValueRW.value = Input.inputString;
            }


            _tick = SystemAPI.GetSingleton<DotsInputFixedTickSystem.Singleton>().tick;
            _tick += 2; // Looks like input loop works after update cycle.
            new SyncInput()
            {
                axis = _axis,
                inputs = _inputs,
            }.ScheduleParallel(Dependency).Complete();

            if (SystemAPI.HasSingleton<InputPointerLtwPresentation>() && Camera.main)
            {
                var pointer = Pointer.current.position;
                ref var pointerLtw = ref SystemAPI.GetComponentRW<LocalToWorld>(SystemAPI.GetSingletonEntity<InputPointerLtwPresentation>()).ValueRW;
                var camRay = Camera.main.ScreenPointToRay(new Vector3(pointer.x.value, pointer.y.value, 0f));
                pointerLtw.Value = Matrix4x4.TRS(camRay.origin, Quaternion.LookRotation(camRay.direction), Vector3.one);
            }
        }

        [BurstCompile]
        internal partial struct SyncInput : IJobEntity
        {
            [ReadOnly] public NativeList<DotsInputPrimitiveElement> inputs;
            [ReadOnly] public NativeList<DotsInputAxisElement> axis;

            [BurstCompile]
            public void Execute(in DotsInputData input, DynamicBuffer<DotsInputPrimitiveElement> oPritimitive, DynamicBuffer<DotsInputAxisElement> oAxis)
            {
                for (int i = input.primitiveRange.x, oi = 0; i < input.primitiveRange.y; i++, oi++)
                {
                    oPritimitive[oi] = inputs[i];
                }

                for (int i = input.axisRange.x, oi = 0; i < input.axisRange.y; i++, oi++)
                {
                    oAxis[oi] = axis[i];
                }
            }
        }
    }
}