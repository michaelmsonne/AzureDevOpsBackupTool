using System;
using CommandLine;

namespace AzureDevOpsBackup.Class
{
    public class CommandLineParser
    {
        public static ArgumentOptions Parse(string[] args)
        {
            ArgumentOptions options = null;

            var parser = new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
                settings.HelpWriter = null;
            });

            var result = parser.ParseArguments<ArgumentOptions>(args)
                .WithParsed(parsedOptions =>
                {
                    options = parsedOptions;
                })
                .WithNotParsed(errors =>
                {
                    Console.WriteLine("Failed to parse arguments");

                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Error: {error.Tag}");
                    }

                    // Display help text when parsing fails
                    //DisplayHelpToConsole.DisplayGuide();
                });

            return options;
        }
    }
}