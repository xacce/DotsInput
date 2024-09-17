Representation for Unity New input system in Dots.

Does not use a finite structure for the view, but uses a fixed and immutable(conditionally) dynamicbuffer.

Add `DotsInputAuthoring`. This will add a trace:
`DotsInputData` - stores system information about global buffer mapping
`DotsInputAxisElement` - holds input data for vector2/vector3
`DotsInputPrimitiveElement` - contains input data for bool/float/int.

Also, the system will generate an Enum in the Assets/_GENERATED folder (create it, by the way xd). This ENum is your way of associating the indexes from DynamicBuffer and the desired input in an understandable way


Inside the Job, you can do things like this

```csharp
public void Execute(
DynamicBuffer<DotsInputPrimitiveElement> p,
DynamicBuffer<DotsInputAxisElement> a,
in EveCarrier carrier,
Entity entity)
{
        var input = DotsInputWrapper<UrPrimitiveEnum, UrAxisEnum>.FromJob(tick, p, a);

        input.IsPressedNow(UrInputENum.PLAYER_FIRE)
    }
```
