using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.Utils;
using MapTalkie.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Controllers
{
    [Authorize, ApiController, Route("api/[controller]")]
    public class MessagesController : AuthorizedController
    {
        public MessagesController(AppDbContext context) : base(context)
        {
        }

        [HttpGet("pm")]
        public async Task<ListResponse<ConversationView>> GetConversations()
        {
            var userId = RequireUserId();
            var conversationsQuery = from cp in _context.PrivateConversationParticipants
                where cp.IsActive
                let lastMessage = _context.PrivateMessageReceipts
                    .LastOrDefault(r => r.UserIdA == userId && r.UserIdB == cp.RecipientId)
                select new ConversationView
                {
                    CanSend = true, // TODO
                    Recipient = new UserInMessageView
                    {
                        Id = cp.RecipientId,
                        UserName = cp.Recipient.UserName
                    },
                    LastUpdate = lastMessage == null ? cp.CreatedAt : lastMessage.Message.CreatedAt,
                    LastMessage = lastMessage == null
                        ? null
                        : new MessageView
                        {
                            SenderId = lastMessage.Message.SenderId,
                            Sender = lastMessage.Message.Sender.UserName,
                            Id = lastMessage.MessageId,
                            Read = lastMessage.Message.Read,
                            Text = lastMessage.Message.Text
                        }
                };
            var conversations = await conversationsQuery.Take(50).ToListAsync();
            return new ListResponse<ConversationView>(conversations);
        }

        [HttpPost("pm/{userId}")]
        public async Task<ActionResult<PrivateMessage>> SendPmToUser(
            [FromRoute] string userId,
            [FromBody] NewMessage body)
        {
            var user = await RequireUser();
            var recipient = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (recipient == null)
                return NotFound();
            if (await _context.BlacklistedUsers.IsBlacklisted(userId, user.Id))
                return Forbid();
            var message = new PrivateMessage
            {
                Text = body.Text,
                SenderId = user.Id,
                Receipts =
                {
                    new PrivateMessageReceipt
                    {
                        UserIdA = user.Id,
                        UserIdB = userId,
                        OutFlag = true
                    },
                    new PrivateMessageReceipt
                    {
                        UserIdA = userId,
                        UserIdB = user.Id,
                        OutFlag = false
                    }
                }
            };
            _context.Add(message);
            await _context.SaveChangesAsync();
            ;
            return message;
        }

        [HttpGet("pm/{userId}")]
        public async Task<ActionResult<List<MessageView>>> GetMessages(string userId, DateTime? before = null)
        {
            var currentUserId = RequireUserId();
            if (!await _context.Users.AnyAsync(u => u.Id == userId))
                return NotFound();
            var messages = await _context.PrivateMessageReceipts
                .Where(r => r.UserIdA == currentUserId && r.UserIdB == userId &&
                            (before == null || r.Message.CreatedAt < before))
                .OrderByDescending(r => r.Message.CreatedAt)
                .Select(r => new MessageView
                {
                    Id = r.MessageId,
                    Read = r.Message.Read,
                    Sender = r.Message.Sender.UserName,
                    SenderId = r.Message.SenderId,
                    Text = r.Message.Text,
                    CreatedAt = r.Message.CreatedAt
                }).ToListAsync();
            return messages;
        }

        [HttpDelete("pm/{userId}/{messageId}")]
        public async Task<IActionResult> DeleteMessage([FromRoute] string userId, [FromRoute] long messageId)
        {
            var currentUserId = RequireUserId();
            var receipt = await _context.PrivateMessageReceipts
                .FirstOrDefaultAsync(r => r.UserIdA == currentUserId && r.UserIdB == userId &&
                                          r.MessageId == messageId);
            if (receipt == null) return NotFound($"Message {messageId} not found");

            _context.Remove(receipt);
            await _context.SaveChangesAsync();
            return Ok();
        }

        public class NewMessage
        {
            public string Text { get; set; } = string.Empty;
        }
    }
}