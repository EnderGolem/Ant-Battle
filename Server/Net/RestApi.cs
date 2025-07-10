using RestSharp;
using Server.Net.Models;

namespace Server.Net;

internal class RestApi
{
    
    private const string _baseUrl = "https://jsonplaceholder.typicode.com/";
    private readonly RestClient _client;

    public RestApi()
    {
        var options = new RestClientOptions(_baseUrl)
        {
            ThrowOnAnyError = true,
            Timeout = TimeSpan.FromMilliseconds(10000)
        };
        _client = new RestClient(options);
    }

    public async Task<T> ExecuteAsync<T>(
        string resource,
        Method method,
        IDictionary<string, string>? queryParams = null,
        IDictionary<string, string>? headers = null,
        IDictionary<string, object>? bodyParams = null)
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

        if (bodyParams != null &&
            (method == Method.Post ||
             method == Method.Put ||
             method == Method.Patch))
        {
            request.AddJsonBody(bodyParams);
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

    public async Task<List<Post>> GetPostsAsync()
    {
        return  await ExecuteAsync<List<Post>>("posts", Method.Get);       
    }

}
