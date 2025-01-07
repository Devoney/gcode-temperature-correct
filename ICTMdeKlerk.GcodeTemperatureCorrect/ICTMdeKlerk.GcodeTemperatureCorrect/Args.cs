using CommandLine;

namespace ICTMdeKlerk.GcodeTemperatureCorrect
{
    public class Options
    {
        [Option('f', "file", Required = false, HelpText = "The path to the gcode file")]
        public string File { get; set; }

        [Option('0', "t0", Default = 0, HelpText = "The temperature for the first extruder")]
        public int TemperatureFirstExtruder { get; set; }

        [Option('1', "t1", Default = 0, HelpText = "The temperature for the second extruder")]
        public int TemperatureSecondExtruder { get; set; }

        [Option('s', "skip", Default = 0, HelpText = "Skip waiting for the nozzle to reach the exact temperature after it reached the temperature initially.")]
        public bool? SkipWaitingForTemperatureAfterInitialTemperature { get; set; }
    }
}
