﻿namespace TelegramBot.Configuration.Validations;

using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Configuration;

public sealed class BotConfigurationValidation : IValidateOptions<BotConfiguration>
{
    public ValidateOptionsResult Validate(string name, BotConfiguration options)
    {
        const string tokenFormat = @"^\d+:[A-Za-z0-9-_]{35}$";
        var match = Regex.Match(options.BotToken, tokenFormat, RegexOptions.IgnoreCase);

        return !match.Success ? ValidateOptionsResult.Fail($"Invalid bot token format: {options.BotToken}") : ValidateOptionsResult.Success;
    }
}
