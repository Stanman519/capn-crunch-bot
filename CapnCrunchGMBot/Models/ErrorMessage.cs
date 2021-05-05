namespace CapnCrunchGMBot.Models
{
    public class ErrorMessage
    {
        public string Message { get; set; }

        public ErrorMessage(string msg)
        {
            Message = msg;
        }
    }
}