using System;
using System.ComponentModel.DataAnnotations;

namespace EpicControlShell.Resources.Entities
{
    public class ExecutorChange
    {
        public int Id { get; set; }

        public int CardId { get; set; }

        public int? OldExecutorId { get; set; }

        public int? NewExecutorId { get; set; }

        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string Reason { get; set; }

        

        public virtual Card Card { get; set; }
        public virtual User OldExecutor { get; set; }
        public virtual User NewExecutor { get; set; }
    }
}
