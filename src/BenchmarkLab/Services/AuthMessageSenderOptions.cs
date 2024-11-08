namespace MeasureThat.Net.Services
{
    public class AuthMessageSenderOptions
    {
        public string SendGridUser
        {
            get; set;
        }

        public string SendGridKey
        {
            get; set;
        }

        public string SenderEmail
        {
            get; set;
        }

        public string SenderName
        {
            get; set;
        }

        public string SendGridApiKey
        {
            get; set;
        }

        public bool RequireEmailConfirmation
        {
            get; set;
        }
    }
}
