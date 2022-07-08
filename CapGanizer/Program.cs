using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CapGanizer
{
    class Program
    {
        public class Options
        {
            [Option("target-directory", Required = true, Default = new String[] { }, HelpText = "Set target directories.")]
            public IEnumerable<string> TargetDirectory { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       CaptureProcessor.ProcessDirectories(o.TargetDirectory);
                   });
        }

    }
}
