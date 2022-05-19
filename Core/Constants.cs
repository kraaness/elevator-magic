namespace ElevatorMagic.Core;

public static class Constants
{
    /// <summary>
    /// Time spent from stopping at a floor, includes opening and closing delay
    /// </summary>
    public static readonly int ELEVATOR_DOOR_TIME = 10_000;

    /// <summary>
    /// Elevator opening and closing delay
    /// </summary>
    public static readonly int ELEVATOR_DOOR_OPENING_TIME = 1_500;

    /// <summary>
    /// Time spent moving from one floor to the next
    /// </summary>
    public static readonly int FLOOR_TRAVEL_TIME = 3_000;
}