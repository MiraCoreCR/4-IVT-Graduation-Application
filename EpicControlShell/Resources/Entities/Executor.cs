namespace EpicControlShell.Resources.Entities
{
    public class Executor
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int CardId { get; set; }



        public virtual User User { get; set; }
        public virtual Card Card { get; set; }
    }
}
