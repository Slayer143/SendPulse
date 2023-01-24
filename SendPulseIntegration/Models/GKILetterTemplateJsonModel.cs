namespace Terrasoft.Configuration.SendPulse.Models
{
    public class LetterTemplateJsonModel<A, B>
    {
        public string id { get; set; }
        public int real_id { get; set; }
        public string lang { get; set; }
        public string name { get; set; }
        public string name_slug { get; set; }
        public string created { get; set; }
        public string full_description { get; set; }
        public bool is_structure { get; set; }
        public string category { get; set; }
        public A category_info { get; set; }
        public B tags { get; set; }
        public string owner { get; set; }
        public string preview { get; set; }
    }
}