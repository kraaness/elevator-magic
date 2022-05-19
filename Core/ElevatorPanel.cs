using Microsoft.Extensions.Logging;

namespace ElevatorMagic.Core;

public class ElevatorPanel : IElevatorPanel
{
    private readonly IElevator elevator;
    private readonly ILogger? logger;

    public ElevatorPanel(IElevator elevator)
    {
        this.elevator = elevator ?? throw new ArgumentNullException(nameof(elevator));
    }

    public ElevatorPanel(IElevator elevator, ILogger? logger)
        : this(elevator)
    {
        this.logger = logger;
    }

    public async Task NavigateToFloorAsync(int floorNumber)
    {
        logger?.LogInformation($"Passenger clicked on floor {floorNumber} inside elevator {elevator.Id}");
        await this.elevator.CallAsync(floorNumber);
    }

    public async Task EmergencyStopAsync()
    {
        logger?.LogInformation($"Passenger clicked emergency stop from inside elevator {elevator.Id}");
        await this.elevator.StopAsync();
    }
}