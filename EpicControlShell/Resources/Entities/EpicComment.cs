using System.ComponentModel.DataAnnotations;

namespace EpicControlShell.Resources.Entities
{
    public class EpicComment
    {
        public int Id { get; set; }

        public int EpicId { get; set; }

        public int UserId { get; set; }


        [MaxLength(1000)]
        public string Text { get; set; }

        public virtual Epic Epic { get; set; }
        public virtual User User { get; set; }
    }
}
