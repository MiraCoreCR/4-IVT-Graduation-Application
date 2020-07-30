using EpicControlShell.Resources.Entities;

namespace EpicControlShell.Resources
{
    class DatabaseProvider
    {
        private static EpicDbContext _instance;

        public static EpicDbContext GetInstance()
        {
            if (_instance == null)
            {
                _instance = new EpicDbContext();
            }

            return _instance;
        }
    }
}
