namespace ElevatorMagic.Core;

public interface IElevator
{
    int Id { get; }
    int CurrentFloor { get; }
    bool IsSuspended { get; }
    IElevatorPanel Panel { get; }
    int GetTimeToFloor(int destinationFloor);
    Task StopAsync();
    Task CallAsync(int floor);
}