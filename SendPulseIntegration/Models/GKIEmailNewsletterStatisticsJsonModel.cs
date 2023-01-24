namespace Terrasoft.Configuration.SendPulse.Models
{
    public class EmailNewsletterStatisticsJsonModel
    {
        public int sent { get; set; }
        public int delivered { get; set; }
        public int opening { get; set; }
        public int link_redirected { get; set; }
        public int unsubscribe { get; set; }
        public int error { get; set; }
    }
}