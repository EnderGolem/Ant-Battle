
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
    Console.WriteLine("  r     - все боты делают N шагов вправо");
    Console.WriteLine("  l     - все боты делают N шагов влево");
    Console.WriteLine("  ru    - все боты делают N шагов вправо-вверх");
    Console.WriteLine("  rd    - все боты делают N шагов вправо-вниз");
    Console.WriteLine("  lu    - все боты делают N шагов влево-вверх");
    Console.WriteLine("  ld    - все боты делают N шагов влево-вниз");
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
        else if (input.StartsWith("r ", StringComparison.OrdinalIgnoreCase) ||
                 input.StartsWith("l ", StringComparison.OrdinalIgnoreCase) ||
                 input.StartsWith("ru ", StringComparison.OrdinalIgnoreCase) ||
                 input.StartsWith("rd ", StringComparison.OrdinalIgnoreCase) ||
                 input.StartsWith("lu ", StringComparison.OrdinalIgnoreCase) ||
                 input.StartsWith("ld ", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (manager.Combat != null)
                {
                    var parts = input.Split(' ');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int steps) && steps > 0)
                    {
                        Console.WriteLine($"Команда: все боты делают {steps} шагов в направлении {parts[0]}");
                        var forcedMoves = manager.Combat.ForceMoveBots(parts[0].ToLower(), steps);
                        manager.SetForcedMoves(forcedMoves);
                    }
                    else
                    {
                        Console.WriteLine("Некорректный формат команды. Используйте: [направление] [количество шагов]");
                    }
                }
                else
                {
                    Console.WriteLine("Combat не инициализирован. Подождите начала игры.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении команды '{input}': {ex.Message}");
            }
        }
        else if (input.Equals("help", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("=== Доступные команды ===");
            Console.WriteLine("  test  - тестовая команда");
            Console.WriteLine("  r N   - все боты делают N шагов вправо");
            Console.WriteLine("  l N   - все боты делают N шагов влево");
            Console.WriteLine("  ru N  - все боты делают N шагов вправо-вверх");
            Console.WriteLine("  rd N  - все боты делают N шагов вправо-вниз");
            Console.WriteLine("  lu N  - все боты делают N шагов влево-вверх");
            Console.WriteLine("  ld N  - все боты делают N шагов влево-вниз");
            Console.WriteLine("  stop  - остановить сервер");
            Console.WriteLine("  help  - показать эту справку");
            Console.WriteLine("========================");
        }
    }
});

// Ждём завершения обеих (на практике задачи вечные, пока не Ctrl+C)
await Task.WhenAll(taskCycle, cliTask);