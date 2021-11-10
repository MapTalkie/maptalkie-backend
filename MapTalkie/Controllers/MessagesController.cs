using System;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Services.MessageService;
using MapTalkie.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        public MessagesController(UserManager<User> userManager, IMessageService messageService) : base(userManager)
        {
            _messageService = messageService;
        }

        [HttpPost("pm/conversation/{conversationId:int}")]
        public async Task<ActionResult<PrivateMessageView>> SendPmToConversation([FromRoute] int conversationId,
            [FromBody] NewMessage body)
        {
            var privateConv = await _messageService.GetPrivateConversationOrNull(conversationId);
            if (privateConv == null)
                return NotFound();
            var sender = await RequireUser();
            var message = await _messageService.SendPrivateMessage(privateConv, sender, body.Text);
            return new PrivateMessageView
            {
                Id = message.Id,
                SenderId = message.SenderId,
                Text = message.Text,
                Read = message.Read,
                SenderName = sender.UserName
            };
        }

        [HttpPost("pm/user/{userId}")]
        public async Task<ActionResult<PrivateMessage>> SendPmToUser([FromRoute] string userId,
            [FromBody] NewMessage body)
        {
            var user = await RequireUser();
            var recipient = await UserManager.FindByIdAsync(userId);
            if (recipient == null)
                return NotFound();
            var message = await _messageService.SendPrivateMessage(recipient, user, body.Text);
            return message;
        }

        [HttpGet("pm")]
        public async Task<ListResponse<PrivateConversationView>> GetConversations()
        {
            var userId = RequireUserId();
            var conversations = await _messageService.QueryPrivateConversationViews(userId).ToListAsync();
            return new ListResponse<PrivateConversationView>(conversations);
        }

        [HttpGet("pm/conversation/{conversationId:int}")]
        public async Task<ActionResult<ListResponse<PrivateMessageView>>> GetConversationMessages(
            [FromRoute] int conversationId)
        {
            var userId = RequireUserId();
            var conversation = await _messageService.GetPrivateConversationOrNull(conversationId);
            if (conversation == null || (conversation.UserHigherId != userId && conversation.UserLowerId != userId))
                return Unauthorized();
            var messages = await _messageService.QueryPrivateMessageViews(conversationId).ToListAsync();
            return new ListResponse<PrivateMessageView>(messages);
        }

        public class NewMessage
        {
            public string Text { get; set; } = string.Empty;
        }

        public class ConversationView
        {
            public int Id { get; set; }
            public string UserId { get; set; }
            public string LastMessage { get; set; }
            public DateTime LastActive { get; set; }
        }
    }
}