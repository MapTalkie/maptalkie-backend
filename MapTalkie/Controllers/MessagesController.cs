using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.Views;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class MessagesController : AuthorizedController
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MessagesController(AppDbContext context, IPublishEndpoint publishEndpoint) : base(context)
        {
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet("pm")]
        public async Task<List<ConversationView>> GetConversations()
        {
            var userId = RequireUserId();
            var conversationsQuery = from cp in _context.PrivateConversationParticipants
                where cp.IsActive
                let lastMessage = _context.PrivateMessageReceipts
                    .OrderByDescending(r => r.Message.CreatedAt)
                    .FirstOrDefault(r => r.UserIdA == userId && r.UserIdB == cp.RecipientId)
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
            return conversations;
        }

        [HttpPost("pm/{userId}")]
        public async Task<ActionResult<PrivateMessage>> SendPmToUser(
            [FromRoute] string userId,
            [FromBody] NewMessage body)
        {
            var user = await RequireUser();
            var recipientUsername = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();
            if (recipientUsername == null)
                return NotFound();
            if (await _context.BlacklistedUsers.IsBlacklisted(userId, user.Id))
                return Forbid();
            if (!await _context.PrivateConversationParticipants.AnyAsync(p =>
                    p.SenderId == user.Id && p.RecipientId == userId))
                _context.Add(new PrivateConversationParticipant
                {
                    SenderId = user.Id,
                    RecipientId = userId
                });
            var message = new PrivateMessage
            {
                Text = body.Text,
                SenderId = user.Id,
                Receipts = new List<PrivateMessageReceipt>
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
            await _publishEndpoint.Publish(
                new Domain.Messages.PrivateMessages.PrivateMessage(
                    user.Id, user.UserName,
                    userId, recipientUsername,
                    message.Id, message.Text));
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