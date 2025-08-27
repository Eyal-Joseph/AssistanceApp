namespace AssistanceHandler;

public interface IAssistanceHandler
{
    Task<string> GetReplyAsync(string request);
}