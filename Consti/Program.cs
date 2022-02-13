using Consti.Commands;
using McMaster.Extensions.CommandLineUtils;

var app = new CommandLineApplication();

new PostgresCommand(app);

app.HelpOption();
try
{
    app.Execute(args);
}
catch (CommandParsingException exception)
{
    Console.WriteLine(exception.Message);
}