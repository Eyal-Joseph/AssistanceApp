using AssistanceHandler;
using AssistanceService.Protos;
using Grpc.Core;

namespace AssistanceService.Services;

public class AssistanceGrpcService : AssistanceGrpc.AssistanceGrpcBase
{
    private readonly IAssistanceHandler _assistanceHandler;

    public AssistanceGrpcService(IAssistanceHandler assistanceHandler)
    {
        _assistanceHandler = assistanceHandler;
    }

    public override async Task<grpcResponse> GetReply(grpcRequest request, ServerCallContext context)
    {
        var res = await _assistanceHandler.GetReplyAsync(request.Message);

        return new grpcResponse
        {
            Reply = res
        };
    }
}