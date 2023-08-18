using System.Reflection;
using System.Text;
using Scraper.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Text.Json;
using Scraper.Contracts;
using SharedServices.Extensions;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Attributes;
using TelegramBot.Enums;

namespace TelegramBot.Services;

public sealed class CommandService
{
    private readonly ProductService productService;
    private readonly ConversationService conversationService;
    private readonly Lazy<Dictionary<string, Func<ITelegramBotClient, Message, CancellationToken, Task<Message>>>> commandHandlers;

    public CommandService(ProductService productService, ConversationService conversationService)
    {
        this.productService = productService;
        this.conversationService = conversationService;

        commandHandlers = new Lazy<Dictionary<string, Func<ITelegramBotClient, Message, CancellationToken, Task<Message>>>>(() => {
            return GetCommandMethodsAndAttributes()
                .ToDictionary(
                    tuple => tuple.Command.CommandName,
                    tuple =>
                        (Func<ITelegramBotClient, Message, CancellationToken, Task<Message>>)Delegate.CreateDelegate(
                            typeof(Func<ITelegramBotClient, Message, CancellationToken, Task<Message>>),
                            this,
                            tuple.Method)
                );
        });
    }

    public async Task HandleCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (conversationService.IsInConversation(message.Chat.Id))
        {
            await conversationService.HandleConversationStateAsync(botClient, message, cancellationToken);
            return;
        }

        var commandName = (message.Text ?? string.Empty).Split(' ')[0];
        if (commandHandlers.Value.TryGetValue(commandName, out var commandAction))
        {
            await commandAction(botClient, message, cancellationToken);
        }
        else
        {
            await Usage(botClient, message, cancellationToken);
        }
    }

    [Command("/list", "display products")]
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

    [Command("/add", "add a product from a URL")]
    private async Task<Message> Add(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        // Mark the chat state as waiting for product URL
        conversationService.SetState(message.Chat.Id, ConversationState.WaitingForProductUrl);

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Send me the url of the product you want to add",
            cancellationToken: cancellationToken);
    }

    [Command("/remove", "remove a product by name")]
    private async Task<Message> Remove(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        // Mark the chat state as waiting for product name
        conversationService.SetState(message.Chat.Id, ConversationState.WaitingForRemoveProductName);

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Send me the name of the product you want to remove",
            cancellationToken: cancellationToken);
    }

    [Command("/export", "exports all products to a file")]
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

    [Command("/import", "import products from a file")]
    private async Task<Message> InitiateImport(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        // Set the chat state as waiting for the import file
        conversationService.SetState(message.Chat.Id, ConversationState.WaitingForImportFile);

        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Send me the file containing the products you want to import.",
            cancellationToken: cancellationToken);
    }

    private async Task Usage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var commands = GetCommandMethodsAndAttributes().Select(tuple => tuple.Command).ToList();

        var usageText = new StringBuilder("Usage:\n");

        foreach (var command in commands)
        {
            usageText.AppendLine($"{command!.CommandName} - {command.Description}");
        }

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: usageText.ToString(),
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken);
    }

    private static string FormatProductsByStore(IEnumerable<IProduct> products)
    {
        var productGroups = products
            .GroupBy(p => p.StoreName)
            .OrderBy(g => g.Key);

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

    private static IEnumerable<(MethodInfo Method, CommandAttribute Command)> GetCommandMethodsAndAttributes() =>
        typeof(CommandService)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => Attribute.IsDefined(m, typeof(CommandAttribute)))
            .Select(m => (Method: m, Command: Attribute.GetCustomAttribute(m, typeof(CommandAttribute)) as CommandAttribute))
            .Where(tuple => tuple.Command != null)!;
}