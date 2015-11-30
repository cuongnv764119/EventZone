namespace EventZone.Models
{
    public class ViewThumbUserModel
    {
        public long UserID { get; set; }
        public long? Avatar { get; set; }
        public string Name { get; set; }
        public int NumberOwnedEvent { get; set; }
        public int NumberFollower { get; set; }
    }
}