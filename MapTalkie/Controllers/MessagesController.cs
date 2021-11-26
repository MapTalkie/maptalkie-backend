using System.Linq;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.Services.MessageService;
using MapTalkie.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : AuthorizedController
    {
        private readonly IMessageService _messageService;

        public MessagesController(AppDbContext dbContext, IMessageService messageService) : base(dbContext)
        {
            _messageService = messageService;
        }

        [HttpPost("pm/{userId}")]
        public async Task<ActionResult<PrivateMessage>> SendPmToUser(
            [FromRoute] string userId,
            [FromBody] NewMessage body)
        {
            var user = await RequireUser();
            var recipient = await _dbContext.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (recipient == null)
                return NotFound();
            var message = await _messageService.CreateMessage(recipient.Id, user.Id, body.Text);
            return message;
        }

        [HttpGet("pm")]
        public async Task<ListResponse<ConversationView>> GetConversations()
        {
            var userId = RequireUserId();
            var conversations = await _messageService.GetUserConversations(userId);
            return new ListResponse<ConversationView>(conversations);
        }

        [HttpDelete("pm/{userId}/{messageId}")]
        public async Task<IActionResult> DeleteMessage([FromRoute] string userId, [FromRoute] long messageId)
        {
            var currentUserId = RequireUserId();
            if (await _messageService.DeleteMessageForUser(messageId, currentUserId, userId)) return Ok();

            return NotFound($"Message {messageId} not found");
        }

        public class NewMessage
        {
            public string Text { get; set; } = string.Empty;
        }
    }
}