using System;
using System.ComponentModel.DataAnnotations;

namespace EpicControlShell.Resources.Entities
{
    public class CardComment
    {
        public int Id { get; set; }

        public int CardId { get; set; }

        public int UserId { get; set; }

        public DateTime TimeToDelete { get; set; }


        [MaxLength(1000)]
        public string Text { get; set; }


        public virtual Card Card { get; set; }
        public virtual User User { get; set; }
    }
}
