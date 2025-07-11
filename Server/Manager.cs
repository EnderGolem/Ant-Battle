using Server.Net;

namespace Server;

internal class Manager
{
    private readonly RestApi _api;
    public Manager()
    {
        _api = new RestApi();
    }

    public  async void Cycle()
    {
      //var posts =  await _api.GetPostsAsync();
    }

}
