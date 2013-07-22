using System;
using RestSharp;
using RestSharp.Extensions;
using RestSharp.Validation;

namespace BrightVision.ServerSide.SMS
{
    public partial class TwilioSMS
	{
		/// <summary>
		/// Retrieve the details for a specific SMS message instance.
		/// Makes a GET request to an SMSMessage Instance resource.
		/// </summary>
		/// <param name="smsMessageSid">The Sid of the message to retrieve</param>
		public SMSMessage GetSmsMessage(string smsMessageSid)
		{
			var request = new RestRequest();
			request.Resource = "Accounts/{AccountSid}/SMS/Messages/{SMSMessageSid}.json";
			request.AddUrlSegment("SMSMessageSid", smsMessageSid);

			return Execute<SMSMessage>(request);
		}

		/// <summary>
		/// Returns a list of SMS messages. 
		/// The list includes paging information.
		/// Makes a GET request to the SMSMessage List resource.
		/// </summary>
		public SmsMessageResult ListSmsMessages()
		{
			return ListSmsMessages(null, null, null, null, null);
		}

		/// <summary>
		/// Returns a filtered list of SMS messages. The list includes paging information.
		/// Makes a GET request to the SMSMessages List resource.
		/// </summary>
		/// <param name="to">(Optional) The phone number of the message recipient</param>
		/// <param name="from">(Optional) The phone number of the message sender</param>
		/// <param name="dateSent">(Optional) The date the message was sent (GMT)</param>
		/// <param name="pageNumber">(Optional) The page to start retrieving results from</param>
		/// <param name="count">(Optional) The number of results to retrieve</param>
		public SmsMessageResult ListSmsMessages(string to, string from, DateTime? dateSent, int? pageNumber, int? count)
		{
			var request = new RestRequest();
			request.Resource = "Accounts/{AccountSid}/SMS/Messages.json";

			if (to.HasValue()) request.AddParameter("To", to);
			if (from.HasValue()) request.AddParameter("From", from);
			if (dateSent.HasValue) request.AddParameter("DateSent", dateSent.Value.ToString("yyyy-MM-dd"));
			if (pageNumber.HasValue) request.AddParameter("Page", pageNumber.Value);
			if (count.HasValue) request.AddParameter("PageSize", count.Value);

			return Execute<SmsMessageResult>(request);
		}

        /// <summary>
        /// Send a new SMS message to the specified recipients.
        /// Makes a POST request to the SMSMessages List resource.
        /// </summary>
        /// <param name="from">The phone number to send the message from. Must be a Twilio-provided or ported local (not toll-free) number. Validated outgoing caller IDs cannot be used.</param>
        /// <param name="to">The phone number to send the message to. If using the Sandbox, this number must be a validated outgoing caller ID</param>
        /// <param name="body">The message to send. Must be 160 characters or less.</param>
        public SMSMessage SendSmsMessage(string to, string body)
        {
            string from = "+19142155010";
            return SendSmsMessage(from, to, body, string.Empty);
        }

		/// <summary>
		/// Send a new SMS message to the specified recipients.
		/// Makes a POST request to the SMSMessages List resource.
		/// </summary>
		/// <param name="from">The phone number to send the message from. Must be a Twilio-provided or ported local (not toll-free) number. Validated outgoing caller IDs cannot be used.</param>
		/// <param name="to">The phone number to send the message to. If using the Sandbox, this number must be a validated outgoing caller ID</param>
		/// <param name="body">The message to send. Must be 160 characters or less.</param>
		public SMSMessage SendSmsMessage(string from, string to, string body)
		{
			return SendSmsMessage(from, to, body, string.Empty);
		}

		/// <summary>
		/// Send a new SMS message to the specified recipients
		/// Makes a POST request to the SMSMessages List resource.
		/// </summary>
		/// <param name="from">The phone number to send the message from. Must be a Twilio-provided or ported local (not toll-free) number. Validated outgoing caller IDs cannot be used.</param>
		/// <param name="to">The phone number to send the message to. If using the Sandbox, this number must be a validated outgoing caller ID</param>
		/// <param name="body">The message to send. Must be 160 characters or less.</param>
		/// <param name="statusCallback">A URL that Twilio will POST to when your message is processed. Twilio will POST the SmsSid as well as SmsStatus=sent or SmsStatus=failed</param>
		public SMSMessage SendSmsMessage(string from, string to, string body, string statusCallback)
		{
            return SendSmsMessage(from, to, body, statusCallback, string.Empty);
        }

        /// <summary>
        /// Send a new SMS message to the specified recipients
        /// Makes a POST request to the SMSMessages List resource.
        /// </summary>
        /// <param name="from">The phone number to send the message from. Must be a Twilio-provided or ported local (not toll-free) number. Validated outgoing caller IDs cannot be used.</param>
        /// <param name="to">The phone number to send the message to. If using the Sandbox, this number must be a validated outgoing caller ID</param>
        /// <param name="body">The message to send. Must be 160 characters or less.</param>
        /// <param name="statusCallback">A URL that Twilio will POST to when your message is processed. Twilio will POST the SmsSid as well as SmsStatus=sent or SmsStatus=failed</param>
        /// <param name="applicationSid">Twilio will POST SmsSid as well as SmsStatus=sent or SmsStatus=failed to the URL in the SmsStatusCallback property of this Application. If the StatusCallback parameter above is also passed, the Application's SmsStatusCallback parameter will take precedence.</param>
        public SMSMessage SendSmsMessage(string from, string to, string body, string statusCallback, string applicationSid)
        {
            Require.Argument("from", from);
            Require.Argument("to", to);
            Require.Argument("body", body);

            var request = new RestRequest(Method.POST);
            request.Resource = "Accounts/{AccountSid}/SMS/Messages.json";
            request.AddParameter("From", from);
            request.AddParameter("To", to);
            request.AddParameter("Body", body);
            if (statusCallback.HasValue()) request.AddParameter("StatusCallback", statusCallback);
            if (applicationSid.HasValue()) request.AddParameter("ApplicationSid", statusCallback);

            return Execute<SMSMessage>(request);
        }
	}
}
