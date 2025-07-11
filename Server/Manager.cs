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

    public async void Cycle()
    {
        try
        {
            var register = await _api.PostRegisterAsync();

            if (register.LobbyEndsIn < 0)
            {
                _combat = new Combat.Combat();
            }

            var game = await _api.GetGameStateAsync();

            var input = _combat.Tick(game);
            
            var jsonGameState = JsonConvert.SerializeObject(game);
        
            // Отправляем JSON данные игры через WebSocket
            _socketManager.BroadcastGameState(jsonGameState);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during registration: {ex.Message}");
            return;
        }
        
    }
}
