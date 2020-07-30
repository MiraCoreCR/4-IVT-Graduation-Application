namespace EpicControlShell.Resources
{
    public class InformationAboutCurrnetUser
    {
        public static int? UserId { get; set; }
        public static int? GroupID { get; set; }
        public static bool CanViewAllProjects { get; set; }
        public static bool CanManageProject { get; set; }
        public static bool CanManageEpic { get; set; }
        public static bool CanCommentEpic { get; set; }
        public static bool CanManageGroup { get; set; }
        public static bool CanManageCard { get; set; }
        public static bool CanMarkAndCommentCard { get; set; }

        public static void NullAllSettings()
        {
            UserId = null;
            GroupID = null;
            CanViewAllProjects = false;
            CanManageProject = false;
            CanManageEpic = false;
            CanCommentEpic = false;
            CanManageGroup = false;
            CanManageCard = false;
            CanMarkAndCommentCard = false;
        }
    }
}
