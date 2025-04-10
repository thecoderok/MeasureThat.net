namespace MeasureThat.Net.Services
{
    public class AuthMessageSenderOptions
    {
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

        public string MailjetPublicKey
        {
            get; set;
        }

        public string MailjetPrivateKey
        {
            get; set;
        }

        public string MailjetSenderEmail
        {
            get; set;
        }
    }
}
