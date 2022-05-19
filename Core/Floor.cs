using Microsoft.Extensions.Logging;

namespace ElevatorMagic.Core;

public class Floor : IFloor
{
    public int FloorNumber { get; }
    public bool IsTopFloor => FloorNumber == building.TopFloor;
    public bool IsBottomFloor => FloorNumber == building.BottomFloor;

    private readonly IBuilding building;
    private readonly ILogger? logger;

    public Floor(IBuilding building, int floorNumber)
    {
        FloorNumber = floorNumber;
        this.building = building;
    }

    public Floor(IBuilding building, int floorNumber, ILogger logger)
        : this(building, floorNumber)
    {
        this.logger = logger;
    }

    public async Task CallElevatorAsync() 
    {
        logger?.LogInformation($"(FL #{FloorNumber}) Called an elevator");
        await building.DispatchElevatorAsync(FloorNumber);
    }
}