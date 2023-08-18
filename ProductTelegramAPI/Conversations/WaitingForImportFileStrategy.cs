using System.Text.Json;
using ProductMonitoringService.ConcreteClasses;
using ProductMonitoringService.Services;
using ProductTelegramAPI.Abstract;
using ProductTelegramAPI.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace ProductTelegramAPI.Conversations;

public sealed class WaitingForImportFileStrategy : IConversationStrategy
{
    private readonly ProductService productService;

    public WaitingForImportFileStrategy(ProductService productService)
    {
        this.productService = productService;
    }

    public async Task HandleAsync(ITelegramBotClient botClient, Message message, Action<long, ConversationState> setStateCallback, CancellationToken cancellationToken)
    {
        if (message.Document == null)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Please send me the file containing the products.",
                cancellationToken: cancellationToken);
            return;
        }

        // Download the file sent by the user
        var file = await botClient.GetFileAsync(message.Document.FileId, cancellationToken: cancellationToken);
        using var stream = new MemoryStream();
        await botClient.DownloadFileAsync(file.FilePath!, stream, cancellationToken);

        // Move the stream position to the beginning
        stream.Seek(0, SeekOrigin.Begin);

        try
        {
            // Deserialize the content of the file into product objects
            var jsonString = await new StreamReader(stream).ReadToEndAsync(cancellationToken);
            var products = JsonSerializer.Deserialize<IEnumerable<Product>>(jsonString);

            var successfullyImported = await productService.ImportProducts(products!);

            if (!successfullyImported)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Something went wrong importing the products.",
                    cancellationToken: cancellationToken);
                return;
            }

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Products have been imported successfully.",
                cancellationToken: cancellationToken);
        }
        catch (JsonException)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "The file format is incorrect. Please send a valid products JSON file.",
                cancellationToken: cancellationToken);
        }

        setStateCallback(message.Chat.Id, ConversationState.Normal);
    }
}