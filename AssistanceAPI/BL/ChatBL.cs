using Grpc.Net.Client;
using AssistanceAPI.Protos;

namespace AssistanceAPI.BL;


public interface IChatBL
{
    Task<string> GetReplyAsync(string request);
}

public class ChatBL : IChatBL
{
    private readonly AssistanceGrpc.AssistanceGrpcClient _grpcClient;


    public ChatBL()
    {
        var grpcChannel = GrpcChannel.ForAddress("http://localhost:6005");
        _grpcClient = new AssistanceGrpc.AssistanceGrpcClient(grpcChannel);
    }

    public async Task<string> GetReplyAsync(string request)
    {
        var grpcRequest = new grpcRequest { Message = request };

        var reply = await _grpcClient.GetReplyAsync(grpcRequest);
       
        return reply.Reply;
    }
}