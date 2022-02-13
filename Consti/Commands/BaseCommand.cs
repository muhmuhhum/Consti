using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace Consti.Commands;

public abstract class BaseCommand
{
    public CommandOption<string?> UserOption { get; set; }
    public CommandOption<string?> PasswordOption { get; set; }
    public CommandOption<string?> AddressOption { get; set; }
    public CommandOption<string?> PortOption { get; set; }
    public CommandOption<string?> DatabaseOption { get; set; }
    public CommandOption<bool> FillUserSecretsOption { get; set; }
    public CommandOption<string?> ConnectionNameOption { get; set; }
    public CommandOption<ConnectionStringLocations> ConnectionStringLocationOption { get; set; }

    protected BaseCommand(string commandName, CommandLineApplication application)
    {
        application.Command(commandName, Configuration);
    }

    protected virtual void Configuration(CommandLineApplication app)
    {
        app.HelpOption();
        app.OnExecute(OnExecute);
        UserOption = app.Option<string?>("-u|--user", "User that accesses databse", CommandOptionType.SingleValue);
        PasswordOption = app.Option<string?>("-p|--password", "Password for the user", CommandOptionType.SingleValue);
        AddressOption = app.Option<string?>("-a|--address", "Address where the db server is",
            CommandOptionType.SingleValue);
        PortOption = app.Option<string?>("--port", "Port of the db server", CommandOptionType.SingleValue);
        DatabaseOption = app.Option<string?>("-d|--database", "Name of the database", CommandOptionType.SingleValue);
        FillUserSecretsOption = app.Option<bool>("--user-secrets", "Fill connection string into User secrets",
            CommandOptionType.NoValue);
        ConnectionNameOption = app.Option<string?>("--connectionString", "Set the name of the connection string",
            CommandOptionType.SingleValue);
        ConnectionStringLocationOption = app.Option<ConnectionStringLocations>("--connectionStringLocation",
            "Where should the connection string be saved", CommandOptionType.SingleValue);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ConnectionStringLocationOption.DefaultValue = ConnectionStringLocations.USERSECRETS;
        }
        else
        {
            ConnectionStringLocationOption.DefaultValue = ConnectionStringLocations.APPSETTINGS;
            ConnectionStringLocationOption.ShowInHelpText = false;
        }
        
    }

    protected void OnExecute()
    {
        if (!FillUserSecretsOption.ParsedValue)
        {
            Console.WriteLine(GenerateConnectionString());
            return;
        }
            
        if (ConnectionNameOption.ParsedValue is null)
        {
            Console.WriteLine($"Set --{ConnectionNameOption.LongName} to specify name of Connection string");
            return;
        }

        switch (ConnectionStringLocationOption.ParsedValue)
        {
            case ConnectionStringLocations.APPSETTINGS:
                AddConStringToAppSettings();
                break;
            case ConnectionStringLocations.USERSECRETS:
                AddConStringToUserSecrets();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AddConStringToUserSecrets()
    {
        throw new NotImplementedException();
    }

    private void AddConStringToAppSettings()
    {
        var currentFolder = Environment.CurrentDirectory;
        var appSettings = Directory.GetFiles(currentFolder, "appsettings.Development.json");
        if (appSettings.Length != 1)
        {
            Console.WriteLine("Run in Directory with appsettings.Development.json");
            return;
        }
        AddConnectionStringToFile(appSettings.First());
    }

    public void AddConnectionStringToFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var jsonNode = JsonObject.Parse(json);
        var conStringNode = jsonNode["ConnectionStrings"];
        if (conStringNode is null)
        {
            jsonNode["ConnectionStrings"] = new JsonObject();
        }

        if (conStringNode[ConnectionNameOption.ParsedValue] != null)
        {
            Console.WriteLine("ConnectionString with same name already exists");
        }

        conStringNode[ConnectionNameOption.ParsedValue] = GenerateConnectionString();
        File.WriteAllText(filePath, jsonNode.ToJsonString());
    }

    public abstract string GenerateConnectionString();
}