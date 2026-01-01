namespace MentorEval.Services
{

    public sealed class EmailService
    {
        private static EmailService? _instance;

        private static readonly object _lock = new object();

        private EmailService()
        {
            Console.WriteLine("[EmailService] Kreirana emailservice instanca");
        }

        public static EmailService Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new EmailService();
                    }
                    return _instance;
                }
            }
        }

        public bool SendVerificationEmail(string toEmail, string token)
        {
            Console.WriteLine($"════════════════════════════════════════");
            Console.WriteLine($"[EMAIL] Saljem VERIFIKACIJSKI email");
            Console.WriteLine($"[EMAIL] Prima: {toEmail}");
            Console.WriteLine($"[EMAIL] Link: https://mentoreval.com/verify?token={token}");
            Console.WriteLine($"════════════════════════════════════════");
            return true;
        }

        public bool SendPasswordResetEmail(string toEmail, string token)
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