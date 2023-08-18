using ProductMonitoringService.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ProductTelegramAPI.Services;

public sealed class CallbackService
{
    private readonly ProductService productService;

    public CallbackService(ProductService productService)
    {
        this.productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    public async Task HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        if (callbackQuery.Data!.StartsWith("remove_"))
        {
            // Remove options from message
            var updatedKeyboard = new InlineKeyboardMarkup(Array.Empty<InlineKeyboardButton>());
            await botClient.EditMessageReplyMarkupAsync(
                chatId: callbackQuery.Message!.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                replyMarkup: updatedKeyboard,
                cancellationToken: cancellationToken);

            var parts = callbackQuery.Data.Split('_', 3);
            if (parts.Length < 3) return; // Invalid callback data

            var productId = parts[1];
            var storeName = parts[2];

            var removedProduct = await productService.RemoveProduct(productId, storeName);

            var resultMessage = removedProduct != null
                ? $"Product '{removedProduct}' has been successfully removed from {storeName}"
                : $"Failed to remove product with ID {productId} from {storeName}. It may not exist in the database";

            await botClient.SendTextMessageAsync(chatId: callbackQuery.Message!.Chat.Id,
                text: resultMessage,
                cancellationToken: cancellationToken);
        }
    }
}