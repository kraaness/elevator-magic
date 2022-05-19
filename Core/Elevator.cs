using Microsoft.Extensions.Logging;

namespace ElevatorMagic.Core;

public class Elevator : IElevator
{
    private CancellationTokenSource cancellationSource;
    private ILogger? logger;
    private IBuilding building;
    
    private IElevatorDoors doors;
    private IElevatorMotor motor;
    private SortedSet<int> upwardsQueue;
    private SortedSet<int> downwardsQueue;

    public int Id { get; }
    public int CurrentFloor { get; private set; }
    public ElevatorDirection Direction => motor.Direction;
    public bool IsSuspended => motor.State == ElevatorState.SUSPENDED;
    public IElevatorPanel Panel { get; }

    public Elevator(IBuilding building, int id, ILogger? logger)
    {
        Id = id;
        CurrentFloor = building.BottomFloor;
        Panel = new ElevatorPanel(this, logger);

        this.building = building;
        this.logger = logger;

        doors = new ElevatorDoors(this, logger);
        motor = new ElevatorMotor(this, logger);

        upwardsQueue = new SortedSet<int>();
        downwardsQueue = new SortedSet<int>(new DescendingComparer<int>());
        cancellationSource = new CancellationTokenSource();
        cancellationSource.Token.Register(() => logger?.LogInformation($"(EL #{Id}) Emergency Stopped"));
    }

    private SortedSet<int> GetQueue(bool reverse = false)
    {
        bool elevatorGoingDown = Direction == ElevatorDirection.DOWN;
        if (reverse)
        {
            elevatorGoingDown = !elevatorGoingDown;
        }
        return elevatorGoingDown ? downwardsQueue : upwardsQueue;
    }

    private async Task MoveAsync()
    {
        if (IsSuspended) return;

        bool goingDown = Direction == ElevatorDirection.DOWN;

        var queue = GetQueue();
        if (queue.Count == 0)
        {
            // No items in directional queue
            queue = GetQueue(reverse: true);
            if (queue.Count == 0)
            {
                // No items in reverse queue either - elevator should idle
                motor.Idle();
                return;
            }
            else
            {
                // reverse direction
                goingDown = !goingDown;
            }
        }

        if (goingDown)
        {
            motor.MoveDown();
        }
        else
        {
            motor.MoveUp();
        }

        int nextFloor = CurrentFloor + (goingDown ? -1 : 1);

        logger?.LogInformation($"(EL #{Id}) On floor {CurrentFloor} - Moving {Direction} to {nextFloor}");
        await Task.Delay(Constants.FLOOR_TRAVEL_TIME, cancellationSource.Token);

        if (IsSuspended) return;

        CurrentFloor = nextFloor;

        if (queue.Contains(CurrentFloor))
        {
            motor.Stop();
            GetQueue().Remove(CurrentFloor);
            await doors.OpenAsync(cancellationSource.Token);
        }

        await MoveAsync();
    }

    /// <returns><see cref="int"/> - time to arrival in milliseconds</returns>
    public int GetTimeToFloor(int destinationFloor)
    {
        int maxFloor = Math.Max(CurrentFloor, destinationFloor);
        int minFloor = Math.Min(CurrentFloor, destinationFloor);
        int floorsToTravel = 0;
        int numberOfStops = 0;

        var queue = GetQueue();

        bool elevatorGoingTheSameWay =
            Direction == ElevatorDirection.NONE ||
            (Direction == ElevatorDirection.UP && destinationFloor > CurrentFloor) ||
            (Direction == ElevatorDirection.DOWN && destinationFloor < CurrentFloor);

        if (!elevatorGoingTheSameWay)
        {
            // Calculate number of floors to last stop of direction and then back to CurrentFloor
            if (Direction == ElevatorDirection.DOWN)
            {
                minFloor = downwardsQueue.Last();
                floorsToTravel += CurrentFloor - minFloor;
            }
            else
            {
                maxFloor = upwardsQueue.Last();
                floorsToTravel += maxFloor - CurrentFloor;
            }

            // Multiply by 2 to calculate the number of floors to return to CurrentFloor
            floorsToTravel *= 2;

            numberOfStops += queue.Count;

            queue = GetQueue(reverse: true);
        }

        floorsToTravel += Math.Abs(CurrentFloor - destinationFloor);
        numberOfStops += queue.Count(floor => floor != destinationFloor && floor >= minFloor && floor <= maxFloor);
        // don't calculate stop delay for destinationfloor, add the door opening time later instead

        return
            floorsToTravel * Constants.FLOOR_TRAVEL_TIME +
            numberOfStops * Constants.ELEVATOR_DOOR_TIME + 
            Constants.ELEVATOR_DOOR_OPENING_TIME;
    }

    public Task StopAsync()
    {
        motor.Suspend();
        cancellationSource.Cancel();

        return Task.FromResult(0);
    }

    /// <returns>
    /// <see langword="true"/> if the element is added to the Queue; 
    /// <see langword="false"/> if the element was not added (outside accepted bounds).
    /// </returns>
    private bool AssignFloorToQueue(int floor)
    {
        if (floor < building.BottomFloor || floor > building.TopFloor)
        {
            logger?.LogError($"{floor} is outside accepted floor range {building.BottomFloor} - {building.TopFloor}");
            return false;
        }

        ICollection<int> set;

        if (floor == CurrentFloor)
        {
            //Elevator just passed the floor, assign to opposite queue
            set = GetQueue(reverse: true);
        }
        else
        {
            set = floor > CurrentFloor ? upwardsQueue : downwardsQueue;
        }

        set.Add(floor);
        return true;
    }

    public async Task CallAsync(int floor)
    {
        if (floor == CurrentFloor && motor.State == ElevatorState.IDLE && doors.State == ElevatorDoorsState.CLOSED)
        {
            await doors.OpenAsync(cancellationSource.Token);
            return;
        }

        if (AssignFloorToQueue(floor))
        {
            logger?.LogInformation($"(EL #{Id}) Queued floor {floor} - ETA: {GetTimeToFloor(floor) / 1000} seconds");

            if (motor.State == ElevatorState.IDLE && doors.State == ElevatorDoorsState.CLOSED)
            {
                await MoveAsync();
            }
        }
    }
}