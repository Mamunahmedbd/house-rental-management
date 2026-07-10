using Housing_rental.Models;

namespace Housing_rental.BLL
{
    public static class CurrentSession
    {
        public static User User { get; private set; }

        public static bool IsAuthenticated
        {
            get { return User != null; }
        }

        public static bool IsAdmin
        {
            get { return User != null && User.RoleName == "Admin"; }
        }

        public static void Start(User user)
        {
            User = user;
        }

        public static void End()
        {
            User = null;
        }
    }
}
