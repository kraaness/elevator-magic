namespace ElevatorMagic.Core;

public interface IFloor
{
    int FloorNumber { get; }
    bool IsTopFloor { get; }
    bool IsBottomFloor { get; }

    Task CallElevatorAsync();
}