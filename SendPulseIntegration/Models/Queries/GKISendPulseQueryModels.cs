using System.Collections.Generic;

namespace Terrasoft.Configuration.SendPulse.Models.Queries
{
    public class CreateNewAddressBookEmailsQuery
    {
        public string[] emails { get; set; }

        public CreateNewAddressBookEmailsQuery(string[] emailsListParam)
        {
            emails = emailsListParam;
        }
    }

    public class UpdateAddressBookQuery
    {
        public string name { get; set; }

        public UpdateAddressBookQuery(string nameParam)
        {
            name = nameParam;
        }
    }

    public class CreateNewAddressBookQuery
    {
        public string bookName { get; set; }

        public CreateNewAddressBookQuery(string bookNameParam)
        {
            bookName = bookNameParam;
        }
    }

    public class CreateEmailNewsletterQuery<A, B>
    {
        public string name { get; set; }
        public string sender_name { get; set; }
        public string sender_email { get; set; }
        public string subject { get; set; }
        public A template_id { get; set; }
        public B list_id { get; set; }

        public CreateEmailNewsletterQuery(
            string nameParam,
            string senderNameParam,
            string senderEmailParam,
            string subjectParam,
            A templateId,
            B listId)
        {
            name = nameParam;
            sender_name = senderNameParam;
            sender_email = senderEmailParam;
            subject = subjectParam;
            template_id = templateId;
            list_id = listId;
        }
    }
}