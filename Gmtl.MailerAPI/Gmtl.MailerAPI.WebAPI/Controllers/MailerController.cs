using Gmtl.MailerAPI.WebAPI.Domain;
using Gmtl.MailerAPI.WebAPI.Events;
using Gmtl.MailerAPI.WebAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gmtl.MailerAPI.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailerController : ControllerBase
    {
        private readonly MailDataService _mailDataService;
        private readonly IncomingMailQueue _incomingMailQueue;
        private readonly ILogger<MailerController> _logger;

        public MailerController(MailDataService mailDataService, IncomingMailQueue incomingMailQueue, ILogger<MailerController> logger)
        {
            _mailDataService = mailDataService;
            _incomingMailQueue = incomingMailQueue;
            _logger = logger;
        }

        [HttpPost("send")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SendMailResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(SendMailResponse))]
        public async Task<IActionResult> Send([FromBody] SendMailRequest request)
        {
            try
            {
                foreach (var e in request.ToEvents())
                    _incomingMailQueue.Enqueue(e);

                return Ok(new SendMailResponse());
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);

                return BadRequest();
            }
        }

        [HttpPost("send-pending")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SendPendingEmailsResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(SendPendingEmailsResponse))]
        public async Task<IActionResult> SendPendingEmails()
        {
            try
            {
                List<int> emailIds = await _mailDataService.SendPendingEmails();

                return Ok(new SendPendingEmailsResponse(emailIds));
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);

                return BadRequest();
            }
        }


        [HttpGet("get/{MailId}")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MailDetailsResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MailDetailsResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(MailDetailsResponse))]
        public async Task<IActionResult> Details([FromRoute] MailDetailsRequest request)
        {
            try
            {
                var mail = await _mailDataService.Get(request.MailId);

                return mail != null ?
                 Ok(MailDetailsResponse.Create(mail)) : NotFound();
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);

                return BadRequest();
            }
        }

        [HttpGet("get-all")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(MailListResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(MailListResponse))]
        public async Task<IActionResult> Find([FromQuery] MailListRequest request)
        {
            try
            {
                var mails = await _mailDataService.Find(MailMessageFilter.Empty);

                return Ok(MailListResponse.Create(mails));
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, exc.Message);

                return BadRequest();
            }
        }
    }
}
