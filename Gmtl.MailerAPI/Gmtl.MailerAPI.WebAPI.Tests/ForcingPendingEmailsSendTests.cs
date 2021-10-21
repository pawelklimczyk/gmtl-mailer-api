using Gmtl.MailerAPI.WebAPI.Domain;
using Gmtl.MailerAPI.WebAPI.Events;
using Gmtl.MailerAPI.WebAPI.Persistance;
using Gmtl.MailerAPI.WebAPI.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Gmtl.MailerAPI.WebAPI.Tests
{
    public class ForcingPendingEmailsSendTests
    {
        private MailDataService _sut;
        private MailerDbContext _database;
        private MailForSendingQueue _queue;
        private int _mailReceivedForSending = 0;
        private int _dbInstanceId = 0;

        public ForcingPendingEmailsSendTests()
        {
            _database = GetDatabase();
            _queue = new MailForSendingQueue();
            _queue.Enqueued += _queue_Enqueued;
            _sut = new MailDataService(_queue, _database);
        }

        private void _queue_Enqueued(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _mailReceivedForSending);
        }

        [Fact]
        public async Task ShouldForceSendEmail()
        {
            //Arrange
            _database.Mails.Add(MailMessage.Create("test1@test.com", "test2@test.com", "test subject 2", "test body", DateTime.Now.AddDays(2), MailStatus.Pending));
            await _database.SaveChangesAsync();

            //Act
            await _sut.SendPendingEmails();

            //Assert
            Assert.True(_mailReceivedForSending == 1, "Mail should be put into queue");
        }

        [Fact]
        public async Task ShouldForceSendEmailWithPendingStatusOnly()
        {
            //Arrange
            _database.Mails.Add(MailMessage.Create("test1@test.com", "test2@test.com", "test subject 1", "test body", DateTime.Now.AddDays(2), MailStatus.Draft));
            var pendingEmail = MailMessage.Create("test1@test.com", "test2@test.com", "test subject 2", "test body", DateTime.Now.AddDays(2), MailStatus.Pending);
            _database.Mails.Add(pendingEmail);
            _database.Mails.Add(MailMessage.Create("test1@test.com", "test2@test.com", "test subject 3", "test body", DateTime.Now.AddDays(2), MailStatus.Error));
            await _database.SaveChangesAsync();

            //Act
            await _sut.SendPendingEmails();

            //Assert
            Assert.True(_mailReceivedForSending == 1, "Only mail with status Pending should be forced");

            var mail = await _database.Mails.FirstAsync(m => m.Id == pendingEmail.Id);
            Assert.True(mail.Processing);

        }

        [Fact]
        public async Task ShouldMailBeMarkedAsProcessingAfterForcingSend()
        {
            //Arrange
            MailMessage mail = MailMessage.Create("test1@test.com", "test2@test.com", "test subject", "test body", DateTime.Now.AddDays(2), MailStatus.Pending);
            _database.Mails.Add(mail);
            await _database.SaveChangesAsync();

            //Act
            await _sut.SendPendingEmails();

            //Assert
            mail = await _database.Mails.FirstAsync(m => m.Id == mail.Id);
            Assert.True(mail.Processing);
        }

        [Fact]
        public async Task ForcingSendShouldNotSendEmailTwice()
        {
            /*
             This situation may occur when user presses 'send' button multiple times
             */

            //Arrange
            _database.Mails.Add(MailMessage.Create("test1@test.com", "test2@test.com", "test subject 1", "test body", DateTime.Now.AddDays(2), MailStatus.Pending));
            _database.Mails.Add(MailMessage.Create("test1@test.com", "test2@test.com", "test subject 2", "test body", DateTime.Now.AddDays(2), MailStatus.Pending));
            _database.Mails.Add(MailMessage.Create("test1@test.com", "test2@test.com", "test subject 3", "test body", DateTime.Now.AddDays(2), MailStatus.Pending));
            await _database.SaveChangesAsync();

            //Act
            await _sut.SendPendingEmails();
            await _sut.SendPendingEmails();
            await _sut.SendPendingEmails();
            await _sut.SendPendingEmails();

            //Assert
            Assert.True(_mailReceivedForSending == 3, "3 mails should be put into queue");
        }

        private MailerDbContext GetDatabase()
        {
            int id = Interlocked.Increment(ref _dbInstanceId);
            DbContextOptionsBuilder<MailerDbContext> optionsBuilder = new DbContextOptionsBuilder<MailerDbContext>();
            optionsBuilder.UseInMemoryDatabase("unitTestsDb-" + id);

            return new MailerDbContext(optionsBuilder.Options);
        }
    }
}
