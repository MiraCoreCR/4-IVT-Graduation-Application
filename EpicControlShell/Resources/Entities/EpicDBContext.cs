using System.Data.Entity;


namespace EpicControlShell.Resources.Entities
{
    class EpicDbContext : DbContext
    {
        public EpicDbContext() : base("DbConnectionString")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)            // Настройка каскадного удаления записей
        {
            modelBuilder.Entity<Project>()                                              // Если удаляется проект удалить принадлежащие ему...
                .HasMany(p => p.Epics)                                                  // Эпики
                .WithRequired(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.UserInProjects)                                         // Записи о пользователях, поключённых к этому проекту
                .WithRequired(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Links)                                                  // Ссылки на ресурсы
                .WithRequired(l => l.Project)
                .HasForeignKey(l => l.ProjectId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<User>()                                                 // При удалении пользователя...
                .HasMany(u => u.UserProjects)                                           // Удалить все его подключения к другим проектам
                .WithRequired(up => up.User)
                .HasForeignKey(up => up.UserId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Epic>()                                                 // При удалении эпика удалить все принадлежащие ему...
                .HasMany(e => e.Cards)                                                  // Карточки
                .WithRequired(c => c.Epic)
                .HasForeignKey(c => c.EpicId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Epic>()
                .HasMany(e => e.EpicComments)                                           // Комментарии
                .WithRequired(ec => ec.Epic)
                .HasForeignKey(ec => ec.EpicId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Card>()                                                 // При удалении карточки удалить все принадлежащие ей...
                .HasMany(c => c.Executors)                                              // Записи об исполнителях этой карточки
                .WithRequired(e => e.Card)
                .HasForeignKey(e => e.CardId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Card>()
                .HasMany(c => c.CardComments)                                           // Комментарии
                .WithRequired(cc => cc.Card)
                .HasForeignKey(cc => cc.CardId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Card>()                                                 // Записи о смене исполнителей карточек
                .HasMany(c => c.ExecutorsChanges)
                .WithRequired(ec => ec.Card)
                .HasForeignKey(ec => ec.CardId)
                .WillCascadeOnDelete(true);


            base.OnModelCreating(modelBuilder);
        }



        #region ProjectsSet
        public DbSet<Project> Projects { get; set; }                            // Таблица проектов
        public DbSet<Epic> Epics { get; set; }                                  // Таблица эпиков
        public DbSet<Card> Cards { get; set; }                                  // Таблица карточек
        #endregion

        #region TableConnections
        public DbSet<UserInProject> UserInProjects { get; set; }                // Таблица доступа пользователей к проектов
        public DbSet<Executor> Executors { get; set; }                          // Таблица исполнителей карточек
        #endregion

        #region GroupsAndUsers
        public DbSet<PermissionGroup> PermissionGroups { get; set; }            // Таблица групп и прав доступа
        public DbSet<User> Users { get; set; }                                  // Таблица пользователей
        #endregion

        #region LinksAndComments
        public DbSet<Link> Links { get; set; }                                  // Таблица ссылок в проектах
        public DbSet<EpicComment> EpicComments { get; set; }                    // Таблица комментариев к эпикам
        public DbSet<CardComment> CardComments { get; set; }                    // Таблица комментариев к карточкам
        #endregion

        #region Statistic
        public DbSet<ExecutorChange> ExecutorsChanges { get; set; }
        #endregion
    }
}
