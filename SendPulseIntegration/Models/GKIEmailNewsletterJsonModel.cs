namespace Terrasoft.Configuration.SendPulse.Models
{
    public class EmailNewsletterJsonModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public int status { get; set; }
        public bool is_sms { get; set; }
        public bool is_viber { get; set; }
        public string send_date { get; set; }
        public int all_email_qty { get; set; }
        public int tariff_email_qty { get; set; }
        public int paid_email_qty { get; set; }
        public int overdraft_price { get; set; }
        public string company_price { get; set; }
        public string overdraft_currency { get; set; }
        public EmailNewsletterMessageJsonModel message { get; set; }
        public EmailNewsletterStatisticsJsonModel statistics { get; set; }
    }
}