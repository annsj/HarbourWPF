using System;
using System.Collections.Generic;

namespace HarbourWPF
{
    class Utils
    {
        public static Random random = new Random();

        public static double ConvertKnotToKmPerHour(double knot)
        {
            return knot * 1.852;
        }

        public static double ConvertFeetToMeter(int feet)
        {
            return feet * 0.305;
        }

        public static string PrintTextFromFile(IEnumerable<string> text)
        {
            string s = "Text från fil\n";

            foreach (var line in text)
            {
                s += $"{line}\n";
            }
            s += "\n";
            return s;
        }

    }
}
