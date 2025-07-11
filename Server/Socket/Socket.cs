using WebSocketSharp.Server;
using WebSocketSharp;

namespace Server.Socket;

public class SocketManager
{
    private WebSocketServer _server;
    private EchoBehavior _echoHost;
    
    public SocketManager()
    {
        _server = new WebSocketServer("ws://0.0.0.0:8080");
        _server.AddWebSocketService<EchoBehavior>("/echo");
        _server.Start();

        // Берём менеджер сессий для рассылки
        var host = _server.WebSocketServices["/echo"];
        EchoBehavior.SessionManager = host.Sessions;

        Console.WriteLine("WebSocket server started at ws://localhost:8080/echo");
    }

    public void BroadcastGameState(string jsonGameState)
    {
        Console.WriteLine(jsonGameState.Take(50));
        foreach (var id in EchoBehavior.Subscribers.ToList())
        {
            EchoBehavior.SessionManager.SendTo(jsonGameState, id);
        }
        Console.WriteLine($"[Server] Broadcasted game state to {EchoBehavior.Subscribers.Count} subscriber(s)");
    }

    public void Stop()
    {
        _server?.Stop();
        Console.WriteLine("WebSocket server stopped.");
    }
}


public class EchoBehavior : WebSocketBehavior
{
    public static HashSet<string> Subscribers { get; } = new HashSet<string>();

    public static WebSocketSessionManager SessionManager { get; set; }

    protected override void OnOpen()
    {
        Console.WriteLine($"[Server] Client connected: {ID}");
    }

    protected override void OnClose(CloseEventArgs e)
    {
        // На всякий случай убираем из подписчиков
        Subscribers.Remove(ID);
        Console.WriteLine($"[Server] Client disconnected: {ID}");
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        switch (e.Data)
        {
            case "subscribe":
                Subscribers.Add(ID);
                Send("Subscribed");
                Console.WriteLine($"[Server] {ID} subscribed");
                break;

            case "unsubscribe":
                Subscribers.Remove(ID);
                Send("Unsubscribed");
                Console.WriteLine($"[Server] {ID} unsubscribed");
                break;

            default:
                // Всё остальное эхо
                Console.WriteLine($"[Server] Received from {ID}: {e.Data}");
                Send(e.Data);
                break;
        }
    }
}
