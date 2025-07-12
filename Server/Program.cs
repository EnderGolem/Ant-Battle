
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
            await manager.Cycle();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Ошибка в Cycle: {ex}");
        }

#if DEBUG
        await Task.Delay(TimeSpan.FromSeconds(60));
#endif
#if RELEASE
        await Task.Delay(TimeSpan.FromSeconds(2));
#endif

    }
});


var cliTask = Task.Run(() =>
{
    // Выводим справку при запуске
    Console.WriteLine("=== Консольные команды ===");
    Console.WriteLine("  test  - тестовая команда");
    Console.WriteLine("  right - все боты делают 2 шага вправо");
    Console.WriteLine("  left  - все боты делают 2 шага влево");
    Console.WriteLine("  stop  - остановить сервер");
    Console.WriteLine("  help  - показать эту справку");
    Console.WriteLine("=========================");
    
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
        else if (input.Equals("right", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (manager.Combat != null)
                {
                    Console.WriteLine("Команда: все боты делают 2 шага вправо");
                    var forcedMoves = manager.Combat.ForceMoveBots(true);
                    manager.SetForcedMoves(forcedMoves);
                }
                else
                {
                    Console.WriteLine("Combat не инициализирован. Подождите начала игры.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении команды 'right': {ex.Message}");
            }
        }
        else if (input.Equals("left", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (manager.Combat != null)
                {
                    Console.WriteLine("Команда: все боты делают 2 шага влево");
                    var forcedMoves = manager.Combat.ForceMoveBots(false);
                    manager.SetForcedMoves(forcedMoves);
                }
                else
                {
                    Console.WriteLine("Combat не инициализирован. Подождите начала игры.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении команды 'left': {ex.Message}");
            }
        }
        else if (input.Equals("help", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("=== Доступные команды ===");
            Console.WriteLine("  test  - тестовая команда");
            Console.WriteLine("  right - все боты делают 2 шага вправо");
            Console.WriteLine("  left  - все боты делают 2 шага влево");
            Console.WriteLine("  stop  - остановить сервер");
            Console.WriteLine("  help  - показать эту справку");
            Console.WriteLine("========================");
        }
    }
});

// Ждём завершения обеих (на практике задачи вечные, пока не Ctrl+C)
await Task.WhenAll(taskCycle, cliTask);