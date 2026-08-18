using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PistonMotion.Core;

namespace PistonMotion
{
    /// <summary>
    /// Calculates piston velocity based on crank stroke, rod length, and max RPM.
    /// </summary>
    class Program
    {
        public static void Main(string[] args)
        {
            DisplayTitle();

            bool isCommandLineMode = args.Length > 0;

            while (true)
            {
                try
                {
                    var arguments = GetArguments(args);
                    var validationErrors = PistonCalculator.ValidateInputs(arguments);

                    if (validationErrors.Count > 0)
                    {
                        Console.WriteLine("Validation errors:");
                        foreach (var error in validationErrors)
                        {
                            Console.WriteLine($"  - {error}");
                        }
                        Console.WriteLine("Invalid input values detected. Please check your inputs and try again.");

                        if (isCommandLineMode)
                        {
                            Environment.Exit(1);
                        }

                        continue;
                    }

                    var results = new Results();
                    var csvResults = PistonCalculator.Calculate(arguments, results);

                    DisplayResults(arguments, results);
                    SaveResults(arguments.FileLocation, arguments.Filename, csvResults);

                    if (isCommandLineMode)
                    {
                        return;
                    }

                    Console.WriteLine("\n\nPress any key to continue or Ctrl+C to exit...");
                    Console.ReadKey();
                    Console.Clear();
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid number format. Please enter valid numeric values.");

                    if (isCommandLineMode)
                    {
                        Environment.Exit(1);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    Console.WriteLine("Please try again.");

                    if (isCommandLineMode)
                    {
                        Environment.Exit(1);
                    }
                }
            }
        }

        private static void DisplayTitle()
        {
            Console.WriteLine(" _____________       _____                   ______  ___     __________              ");
            Console.WriteLine(" ___  __ \\__(_)________  /_____________      ___   |/  /_______  /___(_)____________ ");
            Console.WriteLine(" __  /_/ /_  /__  ___/  __/  __ \\_  __ \\     __  /|_/ /_  __ \\  __/_  /_  __ \\_  __ \\");
            Console.WriteLine(" _  ____/_  / _(__  )/ /_ / /_/ /  / / /     _  /  / / / /_/ / /_ _  / / /_/ /  / / /");
            Console.WriteLine(" /_/     /_/  /____/ \\__/ \\____//_/ /_/      /_/  /_/  \\____/\\__/ /_/  \\____//_/ /_/ ");
            Console.WriteLine("\n\t\t\t Piston Motion and Velocity Calc v0.4 \n \t Enter stroke, rod length, and max RPM - Outputs velocity to CSV\n");
        }

        private static Arguments GetArguments(string[] args)
        {
            var arguments = new Arguments();

            if (args.Length == 0)
            {
                GetArgumentsFromConsole(arguments);
            }
            else
            {
                GetArgumentsFromCommandLine(args, arguments);
            }

            return arguments;
        }

        private static void GetArgumentsFromConsole(Arguments arguments)
        {
            Console.Write("File location (Press Enter for default: C:\\Windows\\Temp\\Piston-Motion-Calc\\): ");
            string fileLocation = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(fileLocation))
            {
                arguments.FileLocation = "C:\\Windows\\Temp\\Piston-Motion-Calc\\";
            }
            else
            {
                arguments.FileLocation = fileLocation.EndsWith("\\") ? fileLocation : fileLocation + "\\";
            }

            Console.Write("File name: ");
            string filename = Console.ReadLine();
            arguments.Filename = Path.Combine(arguments.FileLocation, filename + ".csv");

            Console.Write("Using metric units (mm/cc), y/n? [y]: ");
            string checkIfMetric = Console.ReadLine()?.ToLower();
            arguments.IsMetric = string.IsNullOrWhiteSpace(checkIfMetric) || checkIfMetric == "y";

            string units = arguments.IsMetric ? "mm" : "inches";
            string volumeUnits = arguments.IsMetric ? "cc" : "cubic inches";

            Console.Write($"Bore ({units}): ");
            arguments.Bore = double.Parse(Console.ReadLine());

            Console.Write($"Stroke ({units}): ");
            arguments.Stroke = double.Parse(Console.ReadLine());

            Console.Write($"Rod Length ({units}): ");
            arguments.RodLength = double.Parse(Console.ReadLine());

            Console.Write($"Block Deck Height ({units}): ");
            arguments.DeckHeight = double.Parse(Console.ReadLine());

            Console.Write($"Piston Compression Height ({units}): ");
            arguments.CompHeight = double.Parse(Console.ReadLine());

            Console.Write($"Piston Dome Volume ({volumeUnits}) (Negative for dish): ");
            arguments.PistonVolume = double.Parse(Console.ReadLine());

            Console.Write($"Combustion Chamber Volume ({volumeUnits}): ");
            arguments.ChamberVolume = double.Parse(Console.ReadLine());

            Console.Write($"Head Gasket Compressed Thickness ({units}): ");
            arguments.GasketHeight = double.Parse(Console.ReadLine());

            Console.Write("Max RPM: ");
            arguments.RPM = int.Parse(Console.ReadLine());

            Console.Write("Cylinder Count: ");
            arguments.CylinderCount = int.Parse(Console.ReadLine());
        }

        private static void GetArgumentsFromCommandLine(string[] args, Arguments arguments)
        {
            if (args.Length < 13)
            {
                throw new ArgumentException($"Expected 13 command line arguments, got {args.Length}");
            }

            Console.WriteLine($"Arguments: {string.Join(",", args)}");

            arguments.FileLocation = args[0];
            arguments.Filename = args[1];
            arguments.IsMetric = bool.Parse(args[2]);
            arguments.Bore = double.Parse(args[3]);
            arguments.Stroke = double.Parse(args[4]);
            arguments.RodLength = double.Parse(args[5]);
            arguments.DeckHeight = double.Parse(args[6]);
            arguments.CompHeight = double.Parse(args[7]);
            arguments.PistonVolume = double.Parse(args[8]);
            arguments.ChamberVolume = double.Parse(args[9]);
            arguments.GasketHeight = double.Parse(args[10]);
            arguments.RPM = int.Parse(args[11]);
            arguments.CylinderCount = int.Parse(args[12]);
        }

        public static void DisplayResults(Arguments arguments, Results results)
        {
            string units = arguments.IsMetric ? "mm" : "inches";
            string volumeUnits = arguments.IsMetric ? "cc" : "cubic inches";
            string velocityUnits = arguments.IsMetric ? "mm/s" : "inches/s";

            // Convert displacement for display
            double displayDisplacement = results.Displacement;
            if (arguments.IsMetric)
            {
                displayDisplacement = displayDisplacement / 1000; // Convert cubic mm to cc
            }

            Console.WriteLine("\n=== CALCULATION RESULTS ===");
            Console.WriteLine($"Total swept displacement: \t\t{displayDisplacement:F2} {volumeUnits}");
            Console.WriteLine($"Bore to Stroke Ratio: \t\t\t{results.BoreRatio:F3}");
            Console.WriteLine($"Rod Ratio: \t\t\t\t{results.RodRatio:F3}");
            Console.WriteLine($"Piston to deck (including gasket): \t{results.Piston2deck:F3} {units}");
            Console.WriteLine($"  (Negative value indicates 'out of the hole')");
            Console.WriteLine($"Static Compression Ratio: \t\t{results.CompressionRatio:F2}:1");
            Console.WriteLine($"Peak piston velocity: \t\t\t{results.MaxVelocity:F2} {velocityUnits} at {results.MaxVelocityDeg}°");
        }

        public static void SaveResults(string fileLocation, string fileName, List<PistonResult> csvResults)
        {
            try
            {
                if (!Directory.Exists(fileLocation))
                {
                    Directory.CreateDirectory(fileLocation);
                }

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Angle (degrees),Position (units),Velocity (units/s)");

                foreach (var result in csvResults)
                {
                    stringBuilder.AppendLine($"{result.Angle},{result.PistonPosition:F6},{result.PistonVelocity:F6}");
                }

                File.WriteAllText(fileName, stringBuilder.ToString());
                Console.WriteLine($"\nResults saved to: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }
    }
}