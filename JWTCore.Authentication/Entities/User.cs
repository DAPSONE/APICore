using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JWTCore.Authentication.Entities
{
    public class User
    {
        public User()
        {
            UsersModules = new HashSet<UsersModules>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }

        public int Age { get; set; }

        [JsonIgnore]
        public string Password { get; set; }


        public virtual ICollection<UsersModules> UsersModules { get; set; }
    }

    public class Modules
    {
        public Modules()
        {
            UsersModules = new HashSet<UsersModules>();
        }

        public int Id { get; set; }
        public string Module { get; set; }
        public bool Status { get; set; }
        public virtual ICollection<UsersModules> UsersModules { get; set; }
    }

    public class UsersModules
    {
        public int Id { get; set; }

        public int IdUser { get; set; }
        public User User { get; set; }
        public int IdModule { get; set; }
        public Modules Module { get; set; }
    }
}