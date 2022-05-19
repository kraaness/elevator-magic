using Microsoft.Extensions.Logging;

namespace ElevatorMagic.Core;

public class ElevatorMotor : IElevatorMotor
{
    private readonly ILogger? logger;
    private readonly IElevator elevator;

    public ElevatorState State { get; private set; }
    public ElevatorDirection Direction { get; private set; }

    public ElevatorMotor(IElevator elevator)
    {
        State = ElevatorState.IDLE;
        Direction = ElevatorDirection.NONE;

        this.elevator = elevator ?? throw new ArgumentNullException(nameof(elevator));
    }

    public ElevatorMotor(IElevator elevator, ILogger? logger) : this(elevator)
    {
        this.logger = logger;
    }

    private void Move(ElevatorDirection direction)
    {
        Direction = direction;
        State = ElevatorState.MOVING;
    }

    public void MoveUp() => Move(ElevatorDirection.UP);
    public void MoveDown() => Move(ElevatorDirection.DOWN);

    public void Idle()
    {
        Stop();
        Direction = ElevatorDirection.NONE;
    }

    public void Stop()
    {
        State = ElevatorState.IDLE;
    }

    public void Suspend()
    {
        State = ElevatorState.SUSPENDED;
    }
}
