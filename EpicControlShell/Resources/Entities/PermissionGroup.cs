using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EpicControlShell.Resources.Entities
{
    public class PermissionGroup
    {
        public int Id { get; set; }


        [MaxLength(100), Index(IsUnique = true)]
        public string Title { get; set; }

        public bool CanViewAllProjects { get; set; }

        public bool CanManageProject { get; set; }

        public bool CanManageEpic { get; set; }

        public bool CanCommentEpic { get; set; }

        public bool CanManageGroup { get; set; }

        public bool CanManageCard { get; set; }

        public bool CanMarkAndCommentCard { get; set; }



        public virtual ICollection<User> Users { get; set; }
    }
}
