using System;

namespace SecureNotebook.Db.Entities
{
    public class UserData
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string SessionKey { get; set; }
        public long IV { get; set; }
        public bool IsValid { get; set; }
        public DateTime ExpirationDate { get; set; }

        public User User { get; set; }
    }
}
