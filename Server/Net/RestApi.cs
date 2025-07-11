using Newtonsoft.Json;
using RestSharp;
using Server.Net.Models;

namespace Server.Net;

internal class RestApi
{

    private const string _baseUrl = "https://games-test.datsteam.dev/";
    private string _token = "";
    private readonly RestClient _client;

    public RestApi(string token = "")
    {
        var options = new RestClientOptions(_baseUrl)
        {
            ThrowOnAnyError = true,
            Timeout = TimeSpan.FromMilliseconds(10000)
        };
        _client = new RestClient(options);
        _token = token;
    }

    public async Task<T> ExecuteAsync<T>(
        string resource,
        Method method,
        IDictionary<string, string>? queryParams = null,
        IDictionary<string, string>? headers = null,
        string? jsonPayload = null)
    {
        var request = new RestRequest(resource, method);

        if (queryParams != null)
        {
            foreach (var (name, value) in queryParams)
                request.AddQueryParameter(name, value);
        }

        if (headers != null)
        {
            foreach (var (name, value) in headers)
                request.AddHeader(name, value);
        }

        if (jsonPayload != null &&
            (method == Method.Post ||
             method == Method.Put ||
             method == Method.Patch))
        {
            request.AddStringBody(jsonPayload, DataFormat.Json);
        }

        var response = await _client.ExecuteAsync<T>(request);

        if (response.IsSuccessful && response.Data != null)
        {
            return response.Data;
        }
        else
        {
            throw new Exception($"REST Ошибка: {response.StatusCode} — {response.ErrorMessage}");
        }
    }

    public async Task<GameState> GetGameStateAsync()
    {
        return await ExecuteAsync<GameState>("api/arena", Method.Get);
    }

    public async Task<List<Message>> GetMessagesAsync()
    {
        return await ExecuteAsync<List<Message>>("api/logs", Method.Get);
    }

    public async Task<GameState> PostMoveAsync(MovesRequest movesRequest)
    {
        var json = JsonConvert.SerializeObject(movesRequest);
        return await ExecuteAsync<GameState>("api/move", Method.Post, jsonPayload: json);
    }

    public async Task<GameState> PostRegisterAsync()
    {
        Dictionary<string, string> headers = new();
        headers["X-Auth-Token"] = _token;
        return await ExecuteAsync<GameState>("api/register", Method.Post, headers: headers);
    }

}
