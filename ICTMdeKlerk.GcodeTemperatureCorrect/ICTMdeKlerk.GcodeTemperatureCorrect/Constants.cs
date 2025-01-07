namespace ICTMdeKlerk.GcodeTemperatureCorrect
{
    public static class Constants
    {
        public static class Extruders
        {
            public const string First = "T0";
            public const string Second = "T1";
        }

        public static class Temperature
        {
            public const string SetAndContinue = "M104";
            public const string SetAndWait = "M109";
        }
    }
}
