using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EpicControlShell.Resources.Entities
{
    public class Card
    {
        public int Id { get; set; }

        public int EpicId { get; set; }


        [MaxLength(100)]
        public string Title { get; set; }


        [MaxLength(500)]
        public string Description { get; set; }

        public int Status { get; set; }

        public DateTime? DeadLine { get; set; }

        public bool Archival { get; set; }


        [MaxLength(500)]
        public string PathFile { get; set; }


        public virtual Epic Epic { get; set; }
        public virtual ICollection<Executor> Executors { get; set; }
        public virtual ICollection<CardComment> CardComments { get; set; }
        public virtual ICollection<ExecutorChange> ExecutorsChanges { get; set; }
    }
}
