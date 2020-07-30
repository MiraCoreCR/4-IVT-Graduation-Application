using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EpicControlShell.Resources.Entities
{
    public class Epic
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }


        [MaxLength(100)]
        public string Title { get; set; }


        [MaxLength(500)]
        public string Description { get; set; }

        public int? MasterId { get; set; }

        public DateTime StartDay { get; set; }

        public DateTime DeadLine { get; set; }

        public string ColorMarker { get; set; }

        public int? NextEpicId { get; set; }

        public virtual Project Project { get; set; }
        public virtual User Master { get; set; }
        public virtual ICollection<Card> Cards { get; set; }
        public virtual ICollection<EpicComment> EpicComments { get; set; }
    }
}
