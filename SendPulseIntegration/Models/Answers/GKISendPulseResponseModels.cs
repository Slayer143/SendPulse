 namespace Terrasoft.Configuration.SendPulse.Models.Answers
 {
 	public class BooleanAnswer
    {
		public bool result { get; set; }
    }

    public class IntAnswer
    {
        public int id { get; set; }
    }

    public class EmailNewsletterByAddressBookAnswer
    {
        public int task_id { get; set; }
        public string task_name { get; set; }
        public int task_status { get; set; }
    }

    public class CreateEmailNewsletterAnswer
    {
        public int id { get; set; }
        public int status { get; set; }
        public int count { get; set; }
        public int tariff_email_qty { get; set; }
        public double overdraft_price { get; set; }
        public string overdraft_currency { get; set; }
    }
}