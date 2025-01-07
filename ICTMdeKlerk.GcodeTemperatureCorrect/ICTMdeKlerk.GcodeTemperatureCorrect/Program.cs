using CommandLine;
using System.Text.RegularExpressions;

namespace ICTMdeKlerk.GcodeTemperatureCorrect
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("The file will be read and temperatures will be adjusted accordingly.");            
            
            var options = GetCommandLineOptions(args);

            var gcodes = File.ReadAllLines(options.File);

            var temperatureSetForExtruder = new Dictionary<Extruder, bool>
            {
                { Extruder.First, false },
                { Extruder.Second, false },
            };

            Extruder globalExtruder = Extruder.Undetermined;

            var resultingGcode = new List<string>();
            for (var i = 0; i < gcodes.Length; i++)
            {
                var skipLine = false;
                var gcode = gcodes[i].Trim();

                DetermineGlobalExtruder(gcode, ref globalExtruder);

                if (IsTemperatureLine(gcode))
                {
                    var extruder = GetApplicableExtruder(globalExtruder, gcode);
                    if (options.SkipWaitingForTemperatureAfterInitialTemperature == true && temperatureSetForExtruder[extruder] && !gcode.Contains(" S0"))
                    {
                        skipLine = true;
                    }

                    if (gcode.Contains(Constants.Temperature.SetAndWait))
                    {
                        temperatureSetForExtruder[extruder] = true;                        
                    }

                    var desiredTemperature = GetDesiredTemperature(globalExtruder, gcode, options);
                    if (!gcode.Contains($" S{desiredTemperature}") && !gcode.Contains(" S0"))
                    {
                        var correctedGcode = SetTemperature(gcode, desiredTemperature);
                        Console.WriteLine($"Replacing temperature for line {(i + 1)}:'{gcode}' to {desiredTemperature} resulting in '{correctedGcode}'.");
                        gcode = correctedGcode;
                    }
                }

                if (!skipLine)
                {
                    resultingGcode.Add(gcode);
                }
            }

            string fullPath = WriteResultingGcodeToFile(options, resultingGcode);
            Console.WriteLine($"All done! File written to '{fullPath}'.");
        }

        private static Options GetCommandLineOptions(string[] args)
        {
            Options? options = null;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => options = opts)
                .WithNotParsed(errs => HandleErrors(errs));

            if (options == null)
            {
                options = new Options();
            }

            while (string.IsNullOrWhiteSpace(options.File))
            {
                Console.WriteLine("Specify the path to the gcode file:");
                options.File = Console.ReadLine();
            }

            while (options.TemperatureFirstExtruder == default)
            {
                Console.WriteLine("Specify the temperature to use for the first extruder (whole numbers, integers)");
                var input = Console.ReadLine();
                int.TryParse(input, out int temperature);
                options.TemperatureFirstExtruder = temperature;
            }

            while (options.TemperatureSecondExtruder == default)
            {
                Console.WriteLine("Specify the temperature to use for the second extruder (whole numbers, integers)");
                var input = Console.ReadLine();
                int.TryParse(input, out int temperature);
                options.TemperatureSecondExtruder = temperature;
            }

            while (!options.SkipWaitingForTemperatureAfterInitialTemperature.HasValue)
            {
                Console.WriteLine("Speed up printing by only awaiting exact temperature for nozzles once at the beginning? y/n");
                var answer = "";
                while (answer != "y" && answer != "n")
                {
                    answer = Console.ReadKey().Key.ToString().ToLower();
                    options.SkipWaitingForTemperatureAfterInitialTemperature = answer == "y";
                }
            }
            
            return options;
        }

        private static void DetermineGlobalExtruder(string gcode, ref Extruder extruder)
        {
            if (gcode.StartsWith(Constants.Extruders.First))
            {
                extruder = Extruder.First;
            }
            else if (gcode.StartsWith(Constants.Extruders.Second))
            {
                extruder = Extruder.Second;
            }
        }

        public static bool IsTemperatureLine(string gcode)
        {
            return gcode.StartsWith(Constants.Temperature.SetAndContinue) || gcode.StartsWith(Constants.Temperature.SetAndWait);
        }

        public static string SetTemperature(string gcode, int temperature)
        {
            return Regex.Replace(gcode, @"\sS[0-9]{3}", $" S{temperature}");
        }

        public static Extruder GetApplicableExtruder(Extruder globalExtruder, string gcode)
        {
            var localExtruder = Extruder.Undetermined;

            if (gcode.Contains($" {Constants.Extruders.First} "))
            {
                localExtruder = Extruder.First;
            }
            else if (gcode.Contains($" {Constants.Extruders.Second} "))
            {
                localExtruder = Extruder.Second;
            }
            else
            {
                localExtruder = globalExtruder;
            }

            if (localExtruder == Extruder.Undetermined)
            {
                throw new InvalidOperationException("The extruder is not determined yet, cannot determine temperature.");
            }

            return localExtruder;
        }

        public static int GetDesiredTemperature(Extruder globalExtruder, string gcode, Options options)
        {
            var applicableExtruder = GetApplicableExtruder(globalExtruder, gcode);

            if (applicableExtruder == Extruder.Undetermined)
            {
                throw new InvalidOperationException("The extruder is not determined yet, cannot determine temperature.");
            }

            if (applicableExtruder == Extruder.First)
            {
                return options.TemperatureFirstExtruder;
            }

            return options.TemperatureSecondExtruder;
        }

        private static string WriteResultingGcodeToFile(Options? options, List<string> resultingGcode)
        {
            var extension = Path.GetExtension(options.File);
            var newFileName = $"{Path.GetFileNameWithoutExtension(options.File)}_temperature-corrected{extension}";
            var fullPath = Path.Combine(Path.GetDirectoryName(options.File)!, newFileName);
            File.WriteAllLines(fullPath, resultingGcode);
            return fullPath;
        }

        static void HandleErrors(IEnumerable<Error> errs)
        {
            Console.WriteLine("Error parsing arguments.");
        }
    }
}
