namespace PCGameRate.Interfaces
{
    public interface ICaptchaService
    {
        Task<bool> IsCaptchaValid(string captchaResponse);
    }
}
