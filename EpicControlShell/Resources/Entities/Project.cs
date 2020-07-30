using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EpicControlShell.Resources.Entities
{
    public class Project
    {
        public int Id { get; set; }

        public int? MasterId { get; set; }

        [MaxLength(100), Index(IsUnique = true)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(11)]
        public string ColorMarker { get; set; }



        public virtual User Master { get; set; }
        public virtual ICollection<Epic> Epics { get; set; }
        public virtual ICollection<UserInProject> UserInProjects { get; set; }
        public virtual ICollection<Link> Links { get; set; }
    }
}
