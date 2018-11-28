using CommandLine;
using CommandLine.Text;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace ripp3rcon
{
    internal class ConfigBase
    {
        [Option("cores", HelpText = "The amount of cores to use.")]
        public int? Cores { get; set; }
    }

    internal class CreateIrdConfig : ConfigBase
    {
        [Option('i', "iso", Required = true, HelpText = "Location of the iso file for which to create the IRD.")]
        public string IsoFile { get; set; }

        [Option('o', "output", HelpText = "Location of the output IRD file. If omitted, defaults to <inputfile>.ird")]
        public string IrdFile { get; set; }
    }

    internal class CreateIsoConfig : ConfigBase
    {
        [Option('d', "dir", Required = true, HelpText = "Directory where the JB files are located.")]
        public string InputDirectory { get; set; }

        [Option('i', "ird", Required = true, HelpText = "Location of the IRD file.")]
        public string IrdFile { get; set; }

        [Option('o', "output", Required = true, HelpText = "Location of the output file.")]
        public string Output { get; set; }

        [Option("update", DefaultValue = true, HelpText = "Download the PS3 Update file if it's either missing or incorrect")]
        public bool DownloadUpdate { get; set; }
    }

    internal class CryptConfig : ConfigBase
    {
        [Option('i', null, Required = true, HelpText = "The input file to process.")]
        public string InputFile { get; set; }

        [Option('o', null, Required = false, HelpText = "The output file, defaults to <inputfile>.dec.")]
        public string OutputFile { get; set; }

        [Option('z', "zip", Required = false, HelpText = "Compress the output file. Only valid when decrypting.")]
        public bool Compress { get; set; }

        [Option('p', "partsize", Required = false, HelpText = "Split the zipfiles when reaching this amount of bytes.")]
        public long? PartSize { get; set; }
    }

    class Options
    {
        public ConfigBase ConfigBase
        {
            get { return CreateIsoVerb ?? (ConfigBase) CreateIrdVerb ?? CryptConfigVerb; }
        }

        [VerbOption("createiso", HelpText = "Creates an iso")]
        public CreateIsoConfig CreateIsoVerb { get; set; }

        [VerbOption("crypt", HelpText = "Encrypt/decrypt an iso")]
        public CryptConfig CryptConfigVerb { get; set; }

        [VerbOption("createird", HelpText = "Creates an IRD file")]
        public CreateIrdConfig CreateIrdVerb { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }
}
// ReSharper restore UnusedAutoPropertyAccessor.Global
// ReSharper restore ClassNeverInstantiated.Global
