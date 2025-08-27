using AssistanceAPI.BL;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatBL _assistanceBL;

    public ChatController(IChatBL assistanceBL)
    {
        _assistanceBL = assistanceBL;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request)
    {
        string aiReply = await _assistanceBL.GetReplyAsync(request.Message);

        // Replace this with your actual AI agent logic
        //string aiReply = $"You said: {request.Message}";

        return Ok(new ChatResponse { Reply = aiReply });
    }
}