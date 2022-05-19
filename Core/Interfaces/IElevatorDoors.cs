namespace ElevatorMagic.Core;

public interface IElevatorDoors
{
    ElevatorDoorsState State { get; }

    Task OpenAsync(CancellationToken token);
    Task CloseAsync(CancellationToken token);
}
