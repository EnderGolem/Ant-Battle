using Server.Net;
using Newtonsoft.Json;
using Server.Socket;

namespace Server;

internal class Manager
{
    private readonly RestApi _api;
    private readonly SocketManager _socketManager;
    
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during registration: {ex.Message}");
            return;
        }

        var game = await _api.GetGameStateAsync();
        var jsonGameState = JsonConvert.SerializeObject(game);
        
        // Отправляем JSON данные игры через WebSocket
        _socketManager.BroadcastGameState(jsonGameState);
    }
}
