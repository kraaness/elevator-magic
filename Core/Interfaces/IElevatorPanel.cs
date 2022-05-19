namespace ElevatorMagic.Core;

public interface IElevatorPanel
{
    Task NavigateToFloorAsync(int floorNumber);
    Task EmergencyStopAsync();
}
