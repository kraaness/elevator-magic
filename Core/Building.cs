using Microsoft.Extensions.Logging;

namespace ElevatorMagic.Core;

public class Building : IBuilding
{
    private readonly ILogger? logger;
    private readonly IElevator[] elevators;
    private readonly IFloor[] floors;

    public int TopFloor { get; }
    public int BottomFloor { get; }

    public IFloor? GetFloor(int floor) => floors.FirstOrDefault(w => w.FloorNumber == floor);
    public IElevator? GetElevator(int elevatorId) => elevators.FirstOrDefault(w => w.Id == elevatorId);

    public Building(int elevators, int bottomFloor, int topFloor, ILogger logger)
    {
        this.logger = logger;

        TopFloor = topFloor;
        BottomFloor = bottomFloor;

        this.elevators = Enumerable.Range(1, elevators)
            .Select(index => new Elevator(this, index, logger))
            .ToArray();

        this.floors = Enumerable.Range(bottomFloor, topFloor - bottomFloor)
            .Select(floorNumber => new Floor(this, floorNumber, logger))
            .ToArray();
    }


    public async Task DispatchElevatorAsync(int floor)
    {
        var closestElevator = elevators
            .Where(w => !w.IsSuspended)
            .OrderBy(o => o.GetTimeToFloor(floor))
            .FirstOrDefault();

        if (closestElevator == null)
        {
            logger?.LogError("No elevators available.");
            return;
        }

        logger?.LogInformation($"Elevator requested for floor {floor} - dispatching EL #{closestElevator.Id}");

        await closestElevator.CallAsync(floor);
    }

    public async Task EmergencyStopAsync()
    {
        logger?.LogInformation($"Emergency stop requested.");

        var tasks = elevators
            .Where(w => !w.IsSuspended)
            .Select(s => s.StopAsync());

        await Task.WhenAll(tasks);
    }
}