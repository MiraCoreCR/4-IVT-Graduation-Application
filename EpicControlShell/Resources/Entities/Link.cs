using System.ComponentModel.DataAnnotations;

namespace EpicControlShell.Resources.Entities
{
    public class Link
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public int UserId { get; set; }

        public string LinkToResources { get; set; }


        [MaxLength(200)]
        public string Comment { get; set; }

        public virtual Project Project { get; set; }
        public virtual User User { get; set; }
    }
}
