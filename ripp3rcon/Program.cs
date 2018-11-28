using System;
using System.Threading;
using CommandLine;
using Ripp3r;
using Ripp3r.Iso9660;

namespace ripp3rcon
{
    static class Program
    {
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            var config = new Options();
            ConsoleWriter consoleWriter = new ConsoleWriter();
            Console.CancelKeyPress += OnCancel;
            Console.CursorVisible = false;

            Interaction.SetInteraction(new ConsoleInteraction(config, consoleWriter));

            var parser = new Parser(new ParserSettings(Console.Error));
            string invokedCommand = null;

            if (!parser.ParseArguments(args, config, (v, s) => { invokedCommand = v; }))
            {
                Environment.Exit(Parser.DefaultExitCodeFail);
            }

            switch (invokedCommand)
            {
                case "createiso":
                    IsoBuilder isoBuilder = new IsoBuilder();
                    isoBuilder.CreateIso(cancellationTokenSource.Token).Wait();
                    break;
                case "crypt":
                    IsoCryptoClass cryptoClass = new IsoCryptoClass();
                    cryptoClass.Process(cancellationTokenSource.Token).Wait();
                    break;
                case "createird":
                    IrdCreator creator = IrdCreator.Create();
                    creator.CreateIRD(cancellationTokenSource.Token).Wait();
                    break;
            }
        }

        private static void OnCancel(object sender, ConsoleCancelEventArgs e)
        {
            cancellationTokenSource.Cancel();
        }
    }
}
