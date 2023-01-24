namespace Terrasoft.Configuration.SendPulse.Models
{
    public class EmailNewsletterMessageJsonModel
    {
        public string sender_name { get; set; }
        public string sender_email { get; set; }
        public string subject { get; set; }
        public string attachments { get; set; }
        public int list_id { get; set; }
    }
}