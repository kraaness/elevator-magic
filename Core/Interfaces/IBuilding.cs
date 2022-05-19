namespace ElevatorMagic.Core;

public interface IBuilding
{
    int TopFloor { get; }
    int BottomFloor { get; }
    Task DispatchElevatorAsync(int floor);
    Task EmergencyStopAsync();
    IFloor? GetFloor(int floor);
    IElevator? GetElevator(int elevatorId);
}
