using Server.Net;
using Newtonsoft.Json;
using Server.Socket;

namespace Server;

internal class Manager
{
    private readonly RestApi _api;
    private readonly SocketManager _socketManager;

    private Combat.Combat _combat;
    public Manager(SocketManager socketManager)
    {
        _api = new RestApi();
        _socketManager = socketManager;
    }

    public async Task Cycle()
    {
        try
        {
            var register = await _api.PostRegisterAsync();

            if (register.LobbyEndsIn < 0)
            {
                _combat = new Combat.Combat();
            }

            var game = await _api.GetGameStateAsync();

            var jsonGameState = JsonConvert.SerializeObject(game);

            // Отправляем JSON данные игры через WebSocket
            _socketManager.BroadcastGameState(jsonGameState);



            var input = _combat.Tick(game);

            var json = JsonConvert.SerializeObject(input);
            string truncated = json.Length <= 20
                ? json
                : json.Substring(0, 20);

            Console.WriteLine($"Moves {JsonConvert.SerializeObject(truncated)} ");

            var resultMove = await _api.PostMoveAsync(input);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during registration: {ex.Message} {ex.StackTrace}");
            return;
        }

    }
}
