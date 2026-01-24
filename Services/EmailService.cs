namespace MentorEval.Services
{
    public sealed class EmailService
    {

        private static EmailService? _instance;

        private EmailService()
        {
            Console.WriteLine("[EmailService] Kreirana emailservice instanca");
        }

        public static EmailService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EmailService();
                }
                return _instance;
            }
        }

        // fix S2325
        public static bool SendVerificationEmail(string toEmail, string token)
        {
            Console.WriteLine($"════════════════════════════════════════");
            Console.WriteLine($"[EMAIL] Saljem VERIFIKACIJSKI email");
            Console.WriteLine($"[EMAIL] Prima: {toEmail}");
            Console.WriteLine($"[EMAIL] Link: https://mentoreval.com/verify?token={token}");
            Console.WriteLine($"════════════════════════════════════════");
            return true;
        }

        // fix S2325
        public static bool SendPasswordResetEmail(string toEmail, string token)
        {
            Console.WriteLine($"════════════════════════════════════════");
            Console.WriteLine($"[EMAIL] Šaljem RESET LOZINKE email");
            Console.WriteLine($"[EMAIL] Prima: {toEmail}");
            Console.WriteLine($"[EMAIL] Link: https://mentoreval.com/reset?token={token}");
            Console.WriteLine($"════════════════════════════════════════");
            return true;
        }
    }
}