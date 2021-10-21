using System.Collections.Generic;

namespace Gmtl.MailerAPI.WebAPI.Controllers
{
    public class SendPendingEmailsResponse : AbstractApiResponse
    {
        public SendPendingEmailsResponse(List<int> emailIds)
        {
            EmailIds = emailIds;
        }

        public List<int> EmailIds { get; }
    }
}