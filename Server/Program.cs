
using RestSharp;
using Server;
using Server.Socket;

var socketManager = new SocketManager();
var manager = new Manager(socketManager);

var taskCycle = Task.Run(async () =>
{
    while (true)
    {
        try
        {
            manager.Cycle();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Ошибка в Cycle: {ex}");
        }

        await Task.Delay(TimeSpan.FromSeconds(30));
    }
});


var cliTask = Task.Run(() =>
{
    while (true)
    {
        var input = Console.ReadLine();
        if (input == null) continue;

        if (input.Equals("test", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("TEST");
        }
        else if (input.Equals("stop", StringComparison.OrdinalIgnoreCase))
        {
            socketManager.Stop();
            Environment.Exit(0);
        }
    }
});

// Ждём завершения обеих (на практике задачи вечные, пока не Ctrl+C)
await Task.WhenAll(taskCycle, cliTask);