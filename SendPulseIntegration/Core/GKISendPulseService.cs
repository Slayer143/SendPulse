using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using Terrasoft.Configuration.SendPulse.Constants;
using Terrasoft.Configuration.SendPulse.Core;
using Terrasoft.Configuration.SendPulse.Models;
using Terrasoft.Configuration.SendPulse.Models.Answers;
using Terrasoft.Configuration.SendPulse.Models.Queries;
using Terrasoft.Core;
using Terrasoft.Core.Entities;
using Terrasoft.Web.Common;

namespace Terrasoft.Configuration.SendPulse
{
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class SendPulseService : BaseService
    {
        private SystemUserConnection _systemUserConnection;
        private SystemUserConnection SystemUserConnection
        {
            get
            {
                return _systemUserConnection ?? (_systemUserConnection = (SystemUserConnection)AppConnection.SystemUserConnection);
            }
        }

        private string _sendPulseServiceSecretKey;
        private string SendPulseServiceSecretKey
        {
            get
            {
                { return _sendPulseServiceSecretKey ?? (_sendPulseServiceSecretKey = Terrasoft.Core.Configuration.SysSettings.GetValue(SystemUserConnection, "SendPulseAPISecret").ToString()); }
            }
        }

        private string _sendPulseAPIID;
        private string SendPulseAPIID
        {
            get { return _sendPulseAPIID ?? (_sendPulseAPIID = Terrasoft.Core.Configuration.SysSettings.GetValue(SystemUserConnection, "SendPulseAPIID").ToString()); }
        }

        private string _sendPulseAuthToken;
        private string SendPulseAuthToken
        {
            get
            {
                try
                {
                    return _sendPulseAuthToken ?? (_sendPulseAuthToken = Terrasoft.Core.Configuration.SysSettings.GetValue(SystemUserConnection, "SendPulseAuthToken").ToString());
                }
                catch (Exception)
                {
                    return _sendPulseAuthToken = string.Empty;
                }
            }
        }

        private DateTime _sendPulseAuthTokenExpireDate;
        private DateTime SendPulseAuthTokenExpireDate
        {
            get
            {
                _sendPulseAuthTokenExpireDate = Convert.ToDateTime(Terrasoft.Core.Configuration.SysSettings.GetValue(SystemUserConnection, "SendPulseAuthTokenExpireDate", DateTime.MinValue));
                return _sendPulseAuthTokenExpireDate;
            }
        }

        private SendPulseRouter _sendPulseRouter;

        public SendPulseService()
        {
            _sendPulseRouter = new SendPulseRouter(string.Empty, SendPulseAPIID, SendPulseServiceSecretKey, SendPulseAuthToken);

            if (SendPulseAuthTokenExpireDate == DateTime.MinValue
                || DateTime.Now >= SendPulseAuthTokenExpireDate
                || SendPulseAuthToken == string.Empty)
                GetSendPulseToken();
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public void GetAddressBooks()
        {
            _sendPulseRouter.ChangeRoute(SendPulseConstantsCs.URIs.AddressBooksRequestURI);

            JsonConvert.DeserializeObject<List<AddressBookJsonModel>>(_sendPulseRouter.GetRequestResult("GET"))
                .ForEach(addressBook =>
            {
                ProcessAddressBook(addressBook);
                GetAddressBookEmails(addressBook.id);
                GetEmailNewslettersByAddressBook(addressBook.id);
            });
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public void GetAddressBook(int bookId)
        {
            _sendPulseRouter.ChangeRoute(string.Format(SendPulseConstantsCs.URIs.AddressBookRequestURI, bookId));

            var addressBook = JsonConvert.DeserializeObject<List<AddressBookJsonModel>>(_sendPulseRouter.GetRequestResult("GET")).First();
            ProcessAddressBook(addressBook);
            GetAddressBookEmails(bookId);
            GetEmailNewslettersByAddressBook(bookId);
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public void GetAddressBookEmails(int bookId)
        {
            _sendPulseRouter.ChangeRoute(string.Format(SendPulseConstantsCs.URIs.AddressBookEmailsRequestURI, bookId));

            JsonConvert.DeserializeObject<List<AddressBookEmailJsonModel>>(_sendPulseRouter.GetRequestResult("GET"))
                .ForEach(email =>
            {
                ProcessAddressBookEmail(bookId, email);
            });
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public void GetLetterTemplates()
        {
            _sendPulseRouter.ChangeRoute(SendPulseConstantsCs.URIs.LetterTemplatesRequestURI);

            var requestResult = _sendPulseRouter.GetRequestResult("GET");

            ProcessLetterTemplateRequestResult(requestResult);
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public void GetLetterTemplatesByOwner(string owner)
        {
            _sendPulseRouter.ChangeRoute(string.Format(SendPulseConstantsCs.URIs.LetterTemplatesWithParamsRequestURI, owner));

            var requestResult = _sendPulseRouter.GetRequestResult("GET");

            ProcessLetterTemplateRequestResult(requestResult);
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public void GetLetterTemplate(int letterTemplateId)
        {
            _sendPulseRouter.ChangeRoute(string.Format(SendPulseConstantsCs.URIs.LetterTemplateRequestURI, letterTemplateId));

            var requestResult = _sendPulseRouter.GetRequestResult("GET");

            ProcessLetterTemplateRequestResult(requestResult);
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public void CreateNewAddressBook(Guid addressBookId, string addressBookName)
        {
            _sendPulseRouter.ChangeRoute(SendPulseConstantsCs.URIs.CreateAddressBookRequestURI);

            var createResult = JsonConvert.DeserializeObject<IntAnswer>(_sendPulseRouter.GetRequestResult(
                "POST",
                JsonConvert.SerializeObject(new CreateNewAddressBookQuery(addressBookName)))).id;

            var addressBook = new AddressBook(SystemUserConnection);

            if (addressBook.FetchFromDB(addressBookId))
            {
                addressBook.SetColumnValue("SendPulseId", createResult);
                addressBook.Save();
            }

            GetAddressBook(createResult);
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public bool UpdateAddressBook(int bookId, string addressBookName)
        {
            _sendPulseRouter.ChangeRoute(string.Format(SendPulseConstantsCs.URIs.UpdateAddressBookRequestURI, bookId));

            var updateResult = JsonConvert.DeserializeObject<BooleanAnswer>(_sendPulseRouter.GetRequestResult(
                "PUT",
                JsonConvert.SerializeObject(new UpdateAddressBookQuery(addressBookName)))).result;

            GetAddressBook(bookId);

            return updateResult;
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public bool CreateNewAddressBookEmails(int bookId, string[] emails)
        {
            _sendPulseRouter.ChangeRoute(string.Format(SendPulseConstantsCs.URIs.CreateAddressBookEmailsRequestURI, bookId));

            var result = JsonConvert.DeserializeObject<BooleanAnswer>(_sendPulseRouter.GetRequestResult(
                "POST",
                JsonConvert.SerializeObject(new CreateNewAddressBookEmailsQuery(emails)))).result;

            GetAddressBook(bookId);

            return result;
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public void GetEmailNewsletters()
        {
            _sendPulseRouter.ChangeRoute(SendPulseConstantsCs.URIs.EmailNewslettersRequestURI);

            JsonConvert.DeserializeObject<List<EmailNewsletterJsonModel>>(_sendPulseRouter.GetRequestResult("GET"))
                .ForEach(emailNewsletter =>
                {
                    ProcessEmailNewsletter(emailNewsletter);
                });
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public void GetEmailNewslettersByAddressBook(int bookId)
        {
            _sendPulseRouter.ChangeRoute(string.Format(SendPulseConstantsCs.URIs.EmailNewslettersByAddressBookRequestURI, bookId));

            JsonConvert.DeserializeObject<List<EmailNewsletterByAddressBookAnswer>>(_sendPulseRouter.GetRequestResult("GET"))
                .ForEach(emailNewsletter =>
                {
                    ProcessEmailNewsletter(emailNewsletter, bookId);
                });
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public void CreateEmailNewsletter(
            Guid emailNewsletterId,
            string emailNewsletterName, 
            string senderName, 
            string senderEmail, 
            string subject, 
            int templateId, 
            int addressBookId)
        {
            _sendPulseRouter.ChangeRoute(SendPulseConstantsCs.URIs.CreateEmailNewsletterRequestURI);

            var newsletterCreationResult = JsonConvert.DeserializeObject<CreateEmailNewsletterAnswer>(_sendPulseRouter.GetRequestResult(
                "POST",
                JsonConvert.SerializeObject(new CreateEmailNewsletterQuery<int, int>(
                    emailNewsletterName, 
                    senderName, 
                    senderEmail, 
                    subject, 
                    templateId, 
                    addressBookId))));

            ProcessEmailNewsletterCreationResult(emailNewsletterId, newsletterCreationResult);
        }

        private void ProcessEmailNewsletterCreationResult(
            Guid emailNewsletterId, 
            CreateEmailNewsletterAnswer newsletterCreationResult)
        {
            var emailNewsletterInSystem = new EmailNewsletter(SystemUserConnection);

            if (emailNewsletterInSystem.FetchFromDB(emailNewsletterId))
            {
                var emailNewsletterStatusExistCheck = CheckDoesEntityExist("EmailNewsletterStatus", "SendPulseId", newsletterCreationResult.status);

                if ((bool)emailNewsletterStatusExistCheck[0])
                    emailNewsletterInSystem.SetColumnValue("EmailNewsletterStatusId", (emailNewsletterStatusExistCheck[1] as Entity).PrimaryColumnValue);

                emailNewsletterInSystem.SetColumnValue("SendPulseId", newsletterCreationResult.id);
                emailNewsletterInSystem.SetColumnValue("AllEmailsQuantity", newsletterCreationResult.count);
                emailNewsletterInSystem.SetColumnValue("TariffEmailsQuantity", newsletterCreationResult.tariff_email_qty);
                emailNewsletterInSystem.SetColumnValue("OverdraftPrice", newsletterCreationResult.overdraft_price);
                emailNewsletterInSystem.SetColumnValue("OverdraftCurrency", newsletterCreationResult.overdraft_currency);

                emailNewsletterInSystem.Save();
            }
        }

        private void ProcessEmailNewsletter(EmailNewsletterByAddressBookAnswer emailNewsletter, int bookId)
        {
            var emailNewsletterInSystem = new EmailNewsletter(SystemUserConnection);

            if (!emailNewsletterInSystem.FetchFromDB("SendPulseId", emailNewsletter.task_id))
            {
                emailNewsletterInSystem.SetDefColumnValues();
                emailNewsletterInSystem.SetColumnValue("SendPulseId", emailNewsletter.task_id);
            }

            var addressBookExistCheck = CheckDoesEntityExist("AddressBook", "SendPulseId", bookId);
            var emailNewsletterStatusExistCheck = CheckDoesEntityExist("EmailNewsletterStatus", "SendPulseId", emailNewsletter.task_status);

            if ((bool)addressBookExistCheck[0])
                emailNewsletterInSystem.SetColumnValue("AddressBookId", (addressBookExistCheck[1] as Entity).PrimaryColumnValue);

            if ((bool)emailNewsletterStatusExistCheck[0])
                emailNewsletterInSystem.SetColumnValue("EmailNewsletterStatusId", (emailNewsletterStatusExistCheck[1] as Entity).PrimaryColumnValue);

            emailNewsletterInSystem.SetColumnValue("Name", emailNewsletter.task_name);

            emailNewsletterInSystem.Save();
        }

        private void ProcessEmailNewsletter(EmailNewsletterJsonModel emailNewsletter)
        {
            var emailNewsletterInSystem = new EmailNewsletter(SystemUserConnection);

            if (!emailNewsletterInSystem.FetchFromDB("SendPulseId", emailNewsletter.id))
            {
                emailNewsletterInSystem.SetDefColumnValues();
                emailNewsletterInSystem.SetColumnValue("SendPulseId", emailNewsletter.id);
            }

            var addressBookExistCheck = CheckDoesEntityExist("AddressBook", "SendPulseId", emailNewsletter.message.list_id);
            var emailNewsletterStatusExistCheck = CheckDoesEntityExist("EmailNewsletterStatus", "SendPulseId", emailNewsletter.status);

            if ((bool)addressBookExistCheck[0])
                emailNewsletterInSystem.SetColumnValue("AddressBookId", (addressBookExistCheck[1] as Entity).PrimaryColumnValue);

            if ((bool)emailNewsletterStatusExistCheck[0])
                emailNewsletterInSystem.SetColumnValue("EmailNewsletterStatusId", (emailNewsletterStatusExistCheck[1] as Entity).PrimaryColumnValue);

            emailNewsletterInSystem.SetColumnValue("Name", emailNewsletter.name);
            emailNewsletterInSystem.SetColumnValue("IsSMS", emailNewsletter.is_sms);
            emailNewsletterInSystem.SetColumnValue("IsViber", emailNewsletter.is_viber);
            emailNewsletterInSystem.SetColumnValue("SendDate", Convert.ToDateTime(emailNewsletter.send_date));
            emailNewsletterInSystem.SetColumnValue("AllEmailsQuantity", emailNewsletter.all_email_qty);
            emailNewsletterInSystem.SetColumnValue("TariffEmailsQuantity", emailNewsletter.tariff_email_qty);
            emailNewsletterInSystem.SetColumnValue("PaidEmailsQuantity", emailNewsletter.paid_email_qty);
            emailNewsletterInSystem.SetColumnValue("OverdraftPrice", emailNewsletter.overdraft_price);
            emailNewsletterInSystem.SetColumnValue("CompanyPrice", emailNewsletter.company_price);
            emailNewsletterInSystem.SetColumnValue("OverdraftCurrency", emailNewsletter.overdraft_currency);
            emailNewsletterInSystem.SetColumnValue("SenderName", emailNewsletter.message.sender_name);
            emailNewsletterInSystem.SetColumnValue("SenderEmail", emailNewsletter.message.sender_email);
            emailNewsletterInSystem.SetColumnValue("Subject", emailNewsletter.message.subject);
            emailNewsletterInSystem.SetColumnValue("Attachments", emailNewsletter.message.attachments);
            emailNewsletterInSystem.SetColumnValue("SentTimesQuantity", emailNewsletter.statistics.sent);
            emailNewsletterInSystem.SetColumnValue("DeliveredTimesQuantity", emailNewsletter.statistics.delivered);
            emailNewsletterInSystem.SetColumnValue("OpeningTimesQuantity", emailNewsletter.statistics.opening);
            emailNewsletterInSystem.SetColumnValue("LinkRedirectsQuantity", emailNewsletter.statistics.link_redirected);
            emailNewsletterInSystem.SetColumnValue("UnsubscribedQuantity", emailNewsletter.statistics.unsubscribe);
            emailNewsletterInSystem.SetColumnValue("ErrorsQuantity", emailNewsletter.statistics.error);

            emailNewsletterInSystem.Save();
        }

        private object[] CheckDoesEntityExist<T>(string entitySchemaName, string searchColumn, T searchValue)
        {
            var entity = SystemUserConnection
                .EntitySchemaManager
                .GetInstanceByName(entitySchemaName)
                .CreateEntity(SystemUserConnection);

            return new object[] { entity.FetchFromDB(searchColumn, searchValue), entity };
        }

        private void ProcessLetterTemplateRequestResult(string requestResult)
        {
            switch (0)
            {
                case 0 when requestResult.IndexOf("\"category_info\":[]") != -1 &&
                requestResult.IndexOf("\"tags\":[]") != -1:
                    JsonConvert
                        .DeserializeObject<List<LetterTemplateJsonModel<List<object>, List<object>>>>(
                        requestResult)
                        .ForEach(letterTemplate =>
                        {
                            ProcessLetterTemplate(letterTemplate);
                        });
                    break;

                case 0 when requestResult.IndexOf("\"category_info\":[]") != -1 &&
                requestResult.IndexOf("\"tags\":[]") == -1:
                    JsonConvert
                        .DeserializeObject<List<LetterTemplateJsonModel<CategoryInfoJsonModel, List<object>>>>(
                        requestResult)
                        .ForEach(letterTemplate =>
                        {
                            ProcessLetterTemplate(letterTemplate);
                        });
                    break;

                case 0 when requestResult.IndexOf("\"category_info\":[]") == -1 &&
                requestResult.IndexOf("\"tags\":[]") != -1:
                    JsonConvert
                        .DeserializeObject<List<LetterTemplateJsonModel<string[], LetterTemplateTagsJsonModel>>>(
                        requestResult)
                        .ForEach(letterTemplate =>
                        {
                            ProcessLetterTemplate(letterTemplate);
                        });
                    break;

                case 0 when requestResult.IndexOf("\"category_info\":[]") == -1 &&
                requestResult.IndexOf("\"tags\":[]") == -1:
                    JsonConvert
                        .DeserializeObject<List<LetterTemplateJsonModel<CategoryInfoJsonModel, LetterTemplateTagsJsonModel>>>(
                        requestResult)
                        .ForEach(letterTemplate =>
                        {
                            ProcessLetterTemplate(letterTemplate);
                        });
                    break;
            }
        }

        private void ProcessLetterTemplate<A, B>(LetterTemplateJsonModel<A, B> letterTemplate)
        {
            var letterTemplateInSystem = new LetterTemplate(SystemUserConnection);

            if (!letterTemplateInSystem.FetchFromDB("RealSendPulseId", letterTemplate.real_id))
            {
                letterTemplateInSystem.SetDefColumnValues();
                letterTemplateInSystem.SetColumnValue("RealSendPulseId", letterTemplate.real_id);
                letterTemplateInSystem.SetColumnValue("SendPulseId", letterTemplate.id);
            }

            letterTemplateInSystem.SetColumnValue("Name", letterTemplate.name);
            letterTemplateInSystem.SetColumnValue("Language", letterTemplate.lang);
            letterTemplateInSystem.SetColumnValue("NameSlug", letterTemplate.name_slug);
            letterTemplateInSystem.SetColumnValue("CreationDate", Convert.ToDateTime(letterTemplate.created));
            letterTemplateInSystem.SetColumnValue("FullDescription", letterTemplate.full_description);
            letterTemplateInSystem.SetColumnValue("sStructure", letterTemplate.is_structure);
            letterTemplateInSystem.SetColumnValue("Owner", letterTemplate.owner);
            letterTemplateInSystem.SetColumnValue("Preview", letterTemplate.preview);

            if (typeof(A) == typeof(CategoryInfoJsonModel))
                letterTemplateInSystem.SetColumnValue(
                    "LetterTemplateCategoryId",
                    GetLetterTemplateCategory(letterTemplate.category_info as CategoryInfoJsonModel));

            letterTemplateInSystem.Save();
        }

        private Guid GetLetterTemplateCategory(CategoryInfoJsonModel categoryId)
        {
            var letterTemplateCategory = new LetterTemplateCategory(SystemUserConnection);

            if (!letterTemplateCategory.FetchFromDB("SendPulseId", categoryId.id))
            {
                letterTemplateCategory.SetDefColumnValues();

                letterTemplateCategory.SetColumnValue("Code", categoryId.code);
                letterTemplateCategory.SetColumnValue("FullDescription", categoryId.full_description);
                letterTemplateCategory.SetColumnValue("MetaDescription", categoryId.meta_description);

                letterTemplateCategory.Save();
            }

            return letterTemplateCategory.PrimaryColumnValue;
        }

        private void ProcessAddressBookEmail(int bookId, AddressBookEmailJsonModel email)
        {
            var addressBookInSystem = new AddressBook(SystemUserConnection);
            if (addressBookInSystem.FetchFromDB("SendPulseId", bookId))
            {
                var addressBookEmailInSystem = new AddressBookEmail(SystemUserConnection);
                if (!addressBookEmailInSystem.FetchFromDB("Email", email.email))
                {
                    addressBookEmailInSystem.SetDefColumnValues();
                    addressBookEmailInSystem.SetColumnValue("Email", email.email);
                }

                addressBookEmailInSystem.SetColumnValue("Phone", email.phone);
                addressBookEmailInSystem.SetColumnValue("CreationDate", Convert.ToDateTime(email.add_date));
                addressBookEmailInSystem.SetColumnValue("StatusCode", email.status);
                addressBookEmailInSystem.SetColumnValue("StatusCodeExplain", email.status_explain);
                addressBookEmailInSystem.SetColumnValue("AddressBookId", addressBookInSystem.PrimaryColumnValue);

                addressBookEmailInSystem.Save();
            }
        }

        private void ProcessAddressBook(AddressBookJsonModel addressBook)
        {
            var addressBookInSystem = new AddressBook(SystemUserConnection);

            if (!addressBookInSystem.FetchFromDB("SendPulseId", addressBook.id))
            {
                addressBookInSystem.SetDefColumnValues();
                addressBookInSystem.SetColumnValue("SendPulseId", addressBook.id);
            }

            addressBookInSystem.SetColumnValue("Name", addressBook.name);
            addressBookInSystem.SetColumnValue("CreationDate", Convert.ToDateTime(addressBook.creationdate));
            addressBookInSystem.SetColumnValue("StatusCode", addressBook.status);
            addressBookInSystem.SetColumnValue("StatusCodeExplain", addressBook.status_explain);
            addressBookInSystem.SetColumnValue("EmailsQuantity", addressBook.all_email_qty);
            addressBookInSystem.SetColumnValue("InactiveEmailsQuantity", addressBook.inactive_email_qty);
            addressBookInSystem.SetColumnValue("ActiveEmailsQuantity", addressBook.active_email_qty);
            addressBookInSystem.SetColumnValue("NewPhonesQuantity", addressBook.new_phones_quantity);
            addressBookInSystem.SetColumnValue("InctivePhonesQuantity", addressBook.exc_phones_quantity);
            addressBookInSystem.SetColumnValue("ActivePhonesQuantity", addressBook.active_phones_quantity);

            addressBookInSystem.Save();
        }

        private void GetSendPulseToken()
        {
            _sendPulseRouter.ChangeRoute(SendPulseConstantsCs.URIs.AuthTokenRequestURI);
            Terrasoft.Core.Configuration.SysSettings.SetValue(
                _systemUserConnection,
                "SendPulseAuthToken",
                _sendPulseRouter.GetAuthRequestResult());

            Terrasoft.Core.Configuration.SysSettings.SetValue(
                _systemUserConnection,
                "SendPulseAuthTokenExpireDate",
                DateTime.Now.AddHours(1));
        }
    }
}