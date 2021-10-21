using Gmtl.MailerAPI.WebAPI.Domain;
using Gmtl.MailerAPI.WebAPI.Events;
using Gmtl.MailerAPI.WebAPI.Persistance;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gmtl.MailerAPI.WebAPI.Services
{
    public class MailDataService
    {
        private readonly MailForSendingQueue _mailForSendingQueue;
        private readonly MailerDbContext _dbContext;

        public MailDataService(MailForSendingQueue mailForSendingQueue, MailerDbContext dbContext)
        {
            _mailForSendingQueue = mailForSendingQueue;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<MailMessage>> Find(MailMessageFilter filter)
        {
            //TODO2 apply filter
            return _dbContext.Mails.Where(m => true);
        }

        public async Task<MailMessage> Get(int mailId)
        {
            return await _dbContext.Mails.FirstOrDefaultAsync(m => m.Id == mailId);
        }

        public async Task<List<int>> SendPendingEmails()
        {
            var mailIds = new List<int>();
            var mailsToSend = _dbContext.Mails.Where(m => m.MailStatus == MailStatus.Pending && m.Processing == false);

            foreach (var mail in mailsToSend)
            {
                _mailForSendingQueue.Enqueue(MailForSendingEvent.Create(mail));
                mailIds.Add(mail.Id);
                mail.Processing = true;
            }

            await _dbContext.SaveChangesAsync();

            return mailIds;
        }
    }
}
