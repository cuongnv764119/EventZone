namespace EventZone.Helpers
{
    public class EventZoneConstants
    {
        public static string DatetimeFormat = "yyyy-MM-dd HH:mm:ss";
        public static string DateFormat = "yyyy-MM-dd";
        public static int UploadedImageMaxWidthPixcel = 800;
        public static int UploadedImageMaxHeightPixcel = 800;
        public static string[] ImageFileExtensions = {".jpg", ".png", ".jpeg"};
        public static int RecordsPerPage = 20;
        public static int User = 0;
        public static int Mod = 1;
        public static int Admin = 2;
        public static int RootAdmin = 3;
        public static int Pending = 0;
        public static int Approved = 1;
        public static int Rejected = 2;
        public static int Closed = 3;

        public static int publicEvent = 0;
        public static int unlistedEvent = 1;
        public static int privateEvent = 2;

        public static int Like = 1;
        public static int NotRate = 0;
        public static int Dislike = -1;

        public static int isMale = 0;
        public static int isFemale = 1;

        public static bool Active = true;
        public static bool Lock = false;
        public static bool ActiveUser = true;
        public static bool LockedUser = false;

        //phan constants cua tracking action
        public static int Event = 4;
        public static int Report = 5;
        public static int Appeal = 6;
        public static int LockEvent = 1;
        public static int UnlockEvent = 2;
        public static int LockUser = 3;
        public static int UnLockUser = 4;
        public static int ChangeUserEmail = 5;
        public static int SetMod = 1005;
        public static int UnSetMod = 1006;
        public static int SetAdmin = 1007;
        public static int UnSetAdmin = 1008;

        //Notification Type
        public static int FollowingUserAddNewEvent = 1;
        public static int CommentNotification = 2;
        public static int ReportNotification = 3;
        public static int NewFollower = 4;
        public static int RequestUploadImage = 5;
        public static int EventHasBeenLocked = 6;
        public static int EventHasBeenUnLocked = 7;
    }
}