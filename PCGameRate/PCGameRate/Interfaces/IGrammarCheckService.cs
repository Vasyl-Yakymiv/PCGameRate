namespace PCGameRate.Interfaces
{
    public interface IGrammarCheckService
    {
        Task<List<string>> CheckGrammarAsync(string text);
    }
}
