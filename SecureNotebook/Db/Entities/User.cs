using System.Collections.Generic;

namespace SecureNotebook.Db.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }

        public List<UserData> UserDatas { get; set; }

        public User()
        {
            UserDatas = new List<UserData>();
        }
    }
}
