namespace Terrasoft.Configuration.SendPulse.Models
{
    public class AddressBookJsonModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public int all_email_qty { get; set; }
        public int active_email_qty { get; set; }
        public int inactive_email_qty { get; set; }
        public int new_phones_quantity { get; set; }
        public int active_phones_quantity { get; set; }
        public int exc_phones_quantity { get; set; }
        public string creationdate { get; set; }
        public int status { get; set; }
        public string status_explain { get; set; }
    }
}