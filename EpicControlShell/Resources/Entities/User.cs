using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EpicControlShell.Resources.Entities
{
    public class User
    {
        public int Id { get; set; }


        [MaxLength(50), Index(IsUnique = true)]
        public string NickName { get; set; }


        [MaxLength(300)]
        public string FullName { get; set; }


        [MaxLength(100), Index(IsUnique = true)]
        public string Email { get; set; }


        [MaxLength(64), MinLength(6)]
        public string Password { get; set; }

        public int GroupId { get; set; }

        public string PersonalApplicationSettings { get; set; }

        public string Avatar { get; set; }


        public virtual PermissionGroup Group { get; set; }
        public virtual ICollection<UserInProject> UserProjects { get; set; }
        public virtual ICollection<Executor> UserCard { get; set; }
    }
}
