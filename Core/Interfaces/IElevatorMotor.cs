namespace ElevatorMagic.Core;

public interface IElevatorMotor
{
    ElevatorState State { get; }
    ElevatorDirection Direction { get; }

    void Idle();
    void MoveUp();
    void MoveDown();
    void Stop();
    void Suspend();
}
