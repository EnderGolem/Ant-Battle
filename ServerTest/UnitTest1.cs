using Server.Net;
using Server.Net.Models;

namespace ServerTest;

public class Tests
{
    private readonly RestApi _restApi = new("1cf58591-1677-40ce-9bf5-b7a8cd9e9502");
    

    [Test]
    public void RegisterTest()
    {
        var result = _restApi.PostRegisterAsync().GetAwaiter().GetResult();
    }

    [Test]
    public void GetGameStateTest()
    {
        var result = _restApi.GetGameStateAsync().GetAwaiter().GetResult();
    }


    [Test]
    public void GetMessagesTest()
    {
        var result = _restApi.GetMessagesAsync().GetAwaiter().GetResult();
    }


    [Test]
    public void PostMoveAsyncTest()
    {

        var moveRequest = new MovesRequest();
        var result = _restApi.PostMoveAsync(moveRequest).GetAwaiter().GetResult();
    }
}