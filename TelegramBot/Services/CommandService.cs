using Scraper.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text.Json;
using Scraper.Contracts;
using SharedServices.Extensions;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Enums;

namespace TelegramBot.Services;

public sealed class CommandService
{
    private readonly ProductService productService;
    private readonly ConversationService conversationService;

    public CommandService(ProductService productService, ConversationService conversationService)
    {
        this.productService = productService;
        this.conversationService = conversationService;
    }

    public async Task HandleCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (conversationService.IsInConversation(message.Chat.Id))
        {
            await conversationService.HandleConversationStateAsync(botClient, message, cancellationToken);
            return;
        }

        // Parse the command from the message text
        var action = (message.Text ?? string.Empty).Split(' ')[0] switch
        {
            "/list"   => ListProducts(botClient, message, cancellationToken),
            "/add"    => Add(botClient, message, cancellationToken),
            "/remove" => Remove(botClient, message, cancellationToken),
            "/export" => ExportProducts(botClient, message, cancellationToken),
            _         => Usage(botClient, message, cancellationToken)
        };

        await action;
    }

    private async Task<Message> ListProducts(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var products = productService.GetAllProducts();
        var formattedProducts = FormatProductsByStore(products);

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: string.IsNullOrEmpty(formattedProducts) ? "You have no products tracked yet" : formattedProducts,
            parseMode: ParseMode.MarkdownV2,
            cancellationToken: cancellationToken);
    }

    private async Task<Message> Add(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        // Mark the chat state as waiting for product URL
        conversationService.SetState(message.Chat.Id, ConversationState.WaitingForProductUrl);

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Send me the url of the product you want to add",
            cancellationToken: cancellationToken);
    }

    private async Task<Message> Remove(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        // Mark the chat state as waiting for product name
        conversationService.SetState(message.Chat.Id, ConversationState.WaitingForRemoveProductName);

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Send me the name of the product you want to remove",
            cancellationToken: cancellationToken);
    }

    private async Task<Message> ExportProducts(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var products = productService.GetAllProducts().ToList();
        if (!products.Any())
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "No products to export.",
                cancellationToken: cancellationToken);
        }

        var filePath = GenerateJsonFile(products);

        try
        {
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var fileName = $"Products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";
            return await botClient.SendDocumentAsync(
                chatId: message.Chat.Id,
                document: InputFile.FromStream(stream, fileName),
                cancellationToken: cancellationToken);
        }
        finally
        {
            System.IO.File.Delete(filePath);
        }
    }

    private static async Task<Message> Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        const string usage = "Usage:\n" +
                             "/list - display products\n" +
                             "/add - add a product from a URL\n" +
                             "/remove - remove a product by its name or a part of its name\n" +
                             "/export - exports all products to a file";

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: usage,
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }
        
    private static string FormatProductsByStore(IEnumerable<IProduct> products)
    {
        var productGroups = products.GroupBy(p => p.StoreName);
        var storeProductStrings = new List<string>();
        foreach (var group in productGroups)
        {
            var productNames = group.Select(p => p.ToString()!.EscapeMarkdown());
            var storeProductsString = $"*{group.Key}*\n{string.Join("\n", productNames)}";
            storeProductStrings.Add(storeProductsString);
        }

        return string.Join("\n\n", storeProductStrings);
    }
        
    private static string GenerateJsonFile(IEnumerable<IProduct> products)
    {
        var jsonString = JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = true });
    
        var filePath = Path.Combine(Path.GetTempPath(), $"Products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
        System.IO.File.WriteAllText(filePath, jsonString);

        return filePath;
    }
}