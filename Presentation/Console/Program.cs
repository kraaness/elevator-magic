using System.Globalization;
using ElevatorMagic.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;


ILogger logger = CreateLogger();
IBuilding building = new Building(elevators: 4, bottomFloor: 1, topFloor: 10, logger);

while (true)
{
    Console.WriteLine("Hello! Input command: [!simulate, !call, !stop]");

    string? input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    if (input == "!simulate")
    {
        logger?.LogInformation("Running simulations.");

        var simulationTasks = new List<Task>() {
            SimTask(1, building.GetFloor(6)?.CallElevatorAsync()),
            SimTask(3, building.DispatchElevatorAsync(4)),
            SimTask(3, building.GetElevator(1)?.Panel.NavigateToFloorAsync(3)),
            SimTask(5, building.GetFloor(5)?.CallElevatorAsync()),
            SimTask(8, building.GetElevator(4)?.StopAsync()),
            SimTask(10, building.DispatchElevatorAsync(3)),
            SimTask(12, building.GetFloor(2)?.CallElevatorAsync()),
            SimTask(15, building.DispatchElevatorAsync(10)),
            SimTask(17, building.EmergencyStopAsync())
        };

        try
        {
            await Task.WhenAll(simulationTasks);
        }
        catch
        {
            // no worries
        }

        logger?.LogInformation("Simulations finished.");
    }

    if (input == "!stop")
    {
        await building.EmergencyStopAsync();
    }

    var splits = input.Split(" ");
    if (splits[0] == "!call")
    {
        int floorNumber = 0;
        bool floorDefined = false;

        if (splits.Length > 3)
        {
            Console.WriteLine("Wrong syntax: !call {floor} {elevator}");
            return;
        }

        if (splits.Length == 3)
        {
            floorDefined = int.TryParse(splits[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out floorNumber);
            bool elevatorDefined = int.TryParse(splits[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int elevatorId);

            if (floorDefined && elevatorDefined)
            {
                var elevatorObj = building.GetElevator(elevatorId);
                if (elevatorObj != null)
                {
                    await (elevatorObj as IElevator).CallAsync(floorNumber);
                }
            }
            else
            {
                Console.WriteLine("Wrong syntax: !call {floor} {elevator}");
            }

            return;
        }

        if (splits.Length == 2)
        {
            floorDefined = int.TryParse(splits[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out floorNumber);
        }

        while (!floorDefined)
        {
            Console.WriteLine("Which floor are you calling from ?");
            floorDefined = int.TryParse(Console.ReadLine(), NumberStyles.Integer, CultureInfo.InvariantCulture, out floorNumber);
        }

        var obj = building.GetFloor(floorNumber);
        if (obj != null)
        {
            await (obj as IFloor).CallElevatorAsync();
        }
    }
}
return;


Task SimTask(int delay, Task task)
{
    Task.Delay(delay * 1000).Wait();
    return task;
}

ILogger CreateLogger()
{
    using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                .AddSimpleConsole(o =>
                {
                    o.SingleLine = true;
                    o.TimestampFormat = "mm:ss ";
                    o.IncludeScopes = false;
                });
        });
    return loggerFactory.CreateLogger<Program>();
}