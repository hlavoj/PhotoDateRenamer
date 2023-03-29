using System;
using CommandLine;
using CommandLine.Text;
using log4net.Config;

namespace FotoDateRanamer
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var options = new Options();
            if (Parser.Default.ParseArguments(args, options))
            {
                // Values are available here
                Renamer renamer = new Renamer(Boolean.Parse(options.Debug), options.Lever, options.interval);
                renamer.ReadDirectories(options.Folder);


            }


            Console.ReadLine();
        }
    }

    class Options
    {
        [Option('f', "folder", Required = true, HelpText = "Folder for checking")]
        public string Folder { get; set; }

        [Option('l', "level", DefaultValue = 0, HelpText = "\n\t 0 - Rename only same avg and Directory date. \n\t 1 - Rename directory which hasn't name. \n\t 2 - avg and directory date is different, take avg. \n\t 3 - avg and directory date is different, take directory. \n ")]
        public int Lever { get; set; }

        [Option('i', "interval", DefaultValue = 10, HelpText = "Deferrence between avg and directory date (in days)")]
        public int interval { get; set; }

        [Option('d', "debug", DefaultValue = "true", HelpText = "Debuging only print renaming, but no change")]
        public string Debug { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
