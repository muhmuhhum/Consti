using System.Text.Json;
using McMaster.Extensions.CommandLineUtils;

namespace Consti.Commands;

public class PostgresCommand : BaseCommand
{
    public CommandOption<bool> PollingOption { get; set; }
    public PostgresCommand(CommandLineApplication commandLineApplication) : base("postgres", commandLineApplication)
    {
    }

    protected override void Configuration(CommandLineApplication app)
    {
        base.Configuration(app);
        UserOption.IsRequired();
        PasswordOption.IsRequired();
        DatabaseOption.IsRequired();
        AddressOption.DefaultValue = "localhost";
        PortOption.DefaultValue = "5432";
        PollingOption = app.Option<bool>("--polling", "Enable polling", CommandOptionType.NoValue);
    }

    public override string GenerateConnectionString()
    {
        return $"User Id={UserOption.ParsedValue};" +
               $"Password={PasswordOption.ParsedValue};" +
               $"Host={AddressOption.ParsedValue};" +
               $"Port={PortOption.ParsedValue};" +
               $"Database={DatabaseOption.ParsedValue};" +
               $"Polling={PollingOption.ParsedValue}";
    }
}