using ProductMonitoringService.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramAPI.Abstract;
using TelegramAPI.Conversations;
using TelegramAPI.Enums;

namespace TelegramAPI.Services;

public sealed class ConversationService
{
    private readonly Dictionary<long, ConversationState> chatStates = new();
    private readonly Dictionary<ConversationState, IConversationStrategy> strategies;

    public ConversationService(ProductService productService)
    {
        ArgumentNullException.ThrowIfNull(productService);

        strategies = new Dictionary<ConversationState, IConversationStrategy>
        {
            { ConversationState.WaitingForProductUrl, new WaitingForProductUrlStrategy(productService) },
            { ConversationState.WaitingForRemoveProductName, new WaitingForRemoveProductNameStrategy(productService) },
            { ConversationState.WaitingForImportFile, new WaitingForImportFileStrategy(productService) }
        };
    }

    public async Task HandleConversationStateAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (!chatStates.TryGetValue(message.Chat.Id, out var state))
        {
            chatStates[message.Chat.Id] = ConversationState.Normal;
            return;
        }

        if (strategies.TryGetValue(state, out var strategy))
        {
            await strategy.HandleAsync(botClient, message, SetState, cancellationToken);
        }
    }

    public bool IsInConversation(long chatId)
    {
        return chatStates.ContainsKey(chatId) && chatStates[chatId] != ConversationState.Normal;
    }

    public void SetState(long chatId, ConversationState newState)
    {
        chatStates[chatId] = newState;
    }
}