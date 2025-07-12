
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


//var cliTask = Task.Run(() =>
//{
//    // Выводим справку при запуске
//    Console.WriteLine("=== Консольные команды ===");
//    Console.WriteLine("  test  - тестовая команда");
//    Console.WriteLine("  right - все боты делают 2 шага вправо");
//    Console.WriteLine("  left  - все боты делают 2 шага влево");
//    Console.WriteLine("  stop  - остановить сервер");
//    Console.WriteLine("  help  - показать эту справку");
//    Console.WriteLine("=========================");
    
//    while (true)
//    {
//        var input = Console.ReadLine();
//        if (input == null) continue;

//        if (input.Equals("test", StringComparison.OrdinalIgnoreCase))
//        {
//            Console.WriteLine("TEST");
//        }
//        else if (input.Equals("stop", StringComparison.OrdinalIgnoreCase))
//        {
//            socketManager.Stop();
//            Environment.Exit(0);
//        }
//        else if (input.Equals("right", StringComparison.OrdinalIgnoreCase))
//        {
//            try
//            {
//                if (manager.Combat != null)
//                {
//                    Console.WriteLine("Команда: все боты делают 2 шага вправо");
//                    var forcedMoves = manager.Combat.ForceMoveBots(true);
//                    manager.SetForcedMoves(forcedMoves);
//                }
//                else
//                {
//                    Console.WriteLine("Combat не инициализирован. Подождите начала игры.");
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Ошибка при выполнении команды 'right': {ex.Message}");
//            }
//        }
//        else if (input.Equals("left", StringComparison.OrdinalIgnoreCase))
//        {
//            try
//            {
//                if (manager.Combat != null)
//                {
//                    Console.WriteLine("Команда: все боты делают 2 шага влево");
//                    var forcedMoves = manager.Combat.ForceMoveBots(false);
//                    manager.SetForcedMoves(forcedMoves);
//                }
//                else
//                {
//                    Console.WriteLine("Combat не инициализирован. Подождите начала игры.");
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Ошибка при выполнении команды 'left': {ex.Message}");
//            }
//        }
//        else if (input.Equals("help", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(input))
//        {
//            Console.WriteLine("=== Доступные команды ===");
//            Console.WriteLine("  test  - тестовая команда");
//            Console.WriteLine("  right - все боты делают 2 шага вправо");
//            Console.WriteLine("  left  - все боты делают 2 шага влево");
//            Console.WriteLine("  stop  - остановить сервер");
//            Console.WriteLine("  help  - показать эту справку");
//            Console.WriteLine("========================");
//        }
//    }
//});

var cliTask = Task.Run(() =>
{
    // Выводим справку при запуске
    Console.WriteLine("=== Консольные команды ===");
    Console.WriteLine("  test  - тестовая команда");
    Console.WriteLine("  right - все боты делают 2 шага вправо");
    Console.WriteLine("  left  - все боты делают 2 шага влево");
    Console.WriteLine("  r [N] - движение вправо (по умолч. 2 шага)");
    Console.WriteLine("  l [N] - движение влево (по умолч. 2 шага)");
    Console.WriteLine("  ru [N] - движение вправо-вверх");
    Console.WriteLine("  rd [N] - движение вправо-вниз");
    Console.WriteLine("  lu [N] - движение влево-вверх");
    Console.WriteLine("  ld [N] - движение влево-вниз");
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
        else if (input.StartsWith("r ", StringComparison.OrdinalIgnoreCase) ||
                 input.Equals("r", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (manager.Combat != null)
                {
                    var parts = input.Split(' ');
                    var steps = parts.Length > 1 && int.TryParse(parts[1], out var s) ? Math.Clamp(s, 1, 10) : 2;
                    Console.WriteLine($"Команда: все боты делают {steps} шагов вправо");
                    var forcedMoves = manager.Combat.ForceMoveBotsDirection("r", steps);
                    manager.SetForcedMoves(forcedMoves);
                }
                else
                {
                    Console.WriteLine("Combat не инициализирован. Подождите начала игры.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении команды 'r': {ex.Message}");
            }
        }
        else if (input.StartsWith("l ", StringComparison.OrdinalIgnoreCase) ||
                 input.Equals("l", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (manager.Combat != null)
                {
                    var parts = input.Split(' ');
                    var steps = parts.Length > 1 && int.TryParse(parts[1], out var s) ? Math.Clamp(s, 1, 10) : 2;
                    Console.WriteLine($"Команда: все боты делают {steps} шагов влево");
                    var forcedMoves = manager.Combat.ForceMoveBotsDirection("l", steps);
                    manager.SetForcedMoves(forcedMoves);
                }
                else
                {
                    Console.WriteLine("Combat не инициализирован. Подождите начала игры.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении команды 'l': {ex.Message}");
            }
        }
        else if (input.StartsWith("ru ", StringComparison.OrdinalIgnoreCase) ||
                 input.Equals("ru", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (manager.Combat != null)
                {
                    var parts = input.Split(' ');
                    var steps = parts.Length > 1 && int.TryParse(parts[1], out var s) ? Math.Clamp(s, 1, 10) : 2;
                    Console.WriteLine($"Команда: все боты делают {steps} шагов вправо-вверх");
                    var forcedMoves = manager.Combat.ForceMoveBotsDirection("ru", steps);
                    manager.SetForcedMoves(forcedMoves);
                }
                else
                {
                    Console.WriteLine("Combat не инициализирован. Подождите начала игры.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении команды 'ru': {ex.Message}");
            }
        }
        else if (input.StartsWith("rd ", StringComparison.OrdinalIgnoreCase) ||
                 input.Equals("rd", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (manager.Combat != null)
                {
                    var parts = input.Split(' ');
                    var steps = parts.Length > 1 && int.TryParse(parts[1], out var s) ? Math.Clamp(s, 1, 10) : 2;
                    Console.WriteLine($"Команда: все боты делают {steps} шагов вправо-вниз");
                    var forcedMoves = manager.Combat.ForceMoveBotsDirection("rd", steps);
                    manager.SetForcedMoves(forcedMoves);
                }
                else
                {
                    Console.WriteLine("Combat не инициализирован. Подождите начала игры.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении команды 'rd': {ex.Message}");
            }
        }
        else if (input.StartsWith("lu ", StringComparison.OrdinalIgnoreCase) ||
                 input.Equals("lu", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (manager.Combat != null)
                {
                    var parts = input.Split(' ');
                    var steps = parts.Length > 1 && int.TryParse(parts[1], out var s) ? Math.Clamp(s, 1, 10) : 2;
                    Console.WriteLine($"Команда: все боты делают {steps} шагов влево-вверх");
                    var forcedMoves = manager.Combat.ForceMoveBotsDirection("lu", steps);
                    manager.SetForcedMoves(forcedMoves);
                }
                else
                {
                    Console.WriteLine("Combat не инициализирован. Подождите начала игры.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении команды 'lu': {ex.Message}");
            }
        }
        else if (input.StartsWith("ld ", StringComparison.OrdinalIgnoreCase) ||
                 input.Equals("ld", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                if (manager.Combat != null)
                {
                    var parts = input.Split(' ');
                    var steps = parts.Length > 1 && int.TryParse(parts[1], out var s) ? Math.Clamp(s, 1, 10) : 2;
                    Console.WriteLine($"Команда: все боты делают {steps} шагов влево-вниз");
                    var forcedMoves = manager.Combat.ForceMoveBotsDirection("ld", steps);
                    manager.SetForcedMoves(forcedMoves);
                }
                else
                {
                    Console.WriteLine("Combat не инициализирован. Подождите начала игры.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении команды 'ld': {ex.Message}");
            }
        }
        else if (input.Equals("help", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("=== Доступные команды ===");
            Console.WriteLine("  test  - тестовая команда");
            Console.WriteLine("  right - все боты делают 2 шага вправо");
            Console.WriteLine("  left  - все боты делают 2 шага влево");
            Console.WriteLine("  r [N] - движение вправо (по умолч. 2 шага)");
            Console.WriteLine("  l [N] - движение влево (по умолч. 2 шага)");
            Console.WriteLine("  ru [N] - движение вправо-вверх");
            Console.WriteLine("  rd [N] - движение вправо-вниз");
            Console.WriteLine("  lu [N] - движение влево-вверх");
            Console.WriteLine("  ld [N] - движение влево-вниз");
            Console.WriteLine("  stop  - остановить сервер");
            Console.WriteLine("  help  - показать эту справку");
            Console.WriteLine("========================");
        }
    }
});

// Ждём завершения обеих (на практике задачи вечные, пока не Ctrl+C)
await Task.WhenAll(taskCycle, cliTask);