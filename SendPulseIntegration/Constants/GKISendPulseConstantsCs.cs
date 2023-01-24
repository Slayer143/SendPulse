using System;
using System.Collections.Generic;
using Terrasoft.Core;

namespace Terrasoft.Configuration.SendPulse.Constants
{
	public static class SendPulseConstantsCs
	{
		public static class URIs
		{
			#region GET

			public static readonly string AddressBooksRequestURI = "https://api.sendpulse.com/addressbooks";
			public static readonly string AddressBookRequestURI = "https://api.sendpulse.com/addressbooks/{0}";
			public static readonly string AddressBookEmailsRequestURI = "https://api.sendpulse.com/addressbooks/{0}/emails";
			public static readonly string AddressBookEmailsWithParamsRequestURI = "https://api.sendpulse.com/addressbooks/{0}/emails?limit={1}&offset={2}";
			public static readonly string LetterTemplateRequestURI = "https://api.sendpulse.com/template/{0}";
			public static readonly string LetterTemplatesRequestURI = "https://api.sendpulse.com/templates";
			public static readonly string LetterTemplatesWithParamsRequestURI = "https://api.sendpulse.com/templates/?owner={0}";
			public static readonly string EmailNewslettersRequestURI = "https://api.sendpulse.com/campaigns";
			public static readonly string EmailNewslettersByAddressBookRequestURI = "https://api.sendpulse.com/addressbooks/{0}/campaigns";

			#endregion

			#region POST

			public static readonly string AuthTokenRequestURI = "https://api.sendpulse.com/oauth/access_token";
			public static readonly string CreateAddressBookRequestURI = "https://api.sendpulse.com/addressbooks";
			public static readonly string CreateAddressBookEmailsRequestURI = "https://api.sendpulse.com/addressbooks/{0}/emails";
			public static readonly string CreateEmailNewsletterRequestURI = "https://api.sendpulse.com/campaigns";

			#endregion

			#region PUT

			public static readonly string UpdateAddressBookRequestURI = "https://api.sendpulse.com/addressbooks/{0}";

            #endregion
        }
    }
}