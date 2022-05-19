using Microsoft.Extensions.Logging;

namespace ElevatorMagic.Core;

public class ElevatorDoors : IElevatorDoors
{
    private readonly ILogger? logger;
    private readonly IElevator elevator;

    public ElevatorDoorsState State { get; private set; }

    public ElevatorDoors(IElevator elevator)
    {
        State = ElevatorDoorsState.CLOSED;
        this.elevator = elevator ?? throw new ArgumentNullException(nameof(elevator));
    }

    public ElevatorDoors(IElevator elevator, ILogger? logger) : this(elevator)
    {
        this.logger = logger;
    }

    public async Task OpenAsync(CancellationToken token)
    {
        logger?.LogInformation($"(EL #{elevator.Id}) Opening doors on floor {elevator.CurrentFloor}");

        State = ElevatorDoorsState.OPENING;
        await Task.Delay(Constants.ELEVATOR_DOOR_OPENING_TIME, token);

        State = ElevatorDoorsState.OPEN;
        await Task.Delay(Constants.ELEVATOR_DOOR_TIME - (Constants.ELEVATOR_DOOR_OPENING_TIME * 2), token);

        if (!token.IsCancellationRequested)
        {
            await CloseAsync(token);
        }
    }

    public async Task CloseAsync(CancellationToken token)
    {
        logger?.LogInformation($"(EL #{elevator.Id}) Closing doors.");

        State = ElevatorDoorsState.CLOSING;
        await Task.Delay(Constants.ELEVATOR_DOOR_OPENING_TIME, token);

        State = ElevatorDoorsState.CLOSED;
    }
}