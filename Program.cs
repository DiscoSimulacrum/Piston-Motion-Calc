using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

                    if (!ValidateInputs(arguments))
                    {
                        Console.WriteLine("Invalid input values detected. Please check your inputs and try again.");

                        if (isCommandLineMode)
                        {
                            Environment.Exit(1);
                        }

                        continue;
                    }

                    var results = new Results();
                    var csvResults = Calculate(arguments, results);

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

        private static bool ValidateInputs(Arguments arguments)
        {
            var validationErrors = new List<string>();

            if (arguments.Stroke <= 0) validationErrors.Add("Stroke must be positive");
            if (arguments.Bore <= 0) validationErrors.Add("Bore must be positive");
            if (arguments.RodLength <= 0) validationErrors.Add("Rod length must be positive");
            if (arguments.DeckHeight < 0) validationErrors.Add("Deck height cannot be negative");
            if (arguments.CompHeight <= 0) validationErrors.Add("Compression height must be positive");
            if (arguments.ChamberVolume <= 0) validationErrors.Add("Chamber volume must be positive");
            if (arguments.GasketHeight < 0) validationErrors.Add("Gasket height cannot be negative");
            if (arguments.RPM <= 0) validationErrors.Add("RPM must be positive");
            if (arguments.CylinderCount <= 0) validationErrors.Add("Cylinder count must be positive");

            // Rod length should be greater than stroke
            if (arguments.RodLength < arguments.Stroke)
            {
                validationErrors.Add("Warning: Rod length is shorter than stroke - this may produce unrealistic results");
            }

            // Check for potential compression ratio issues
            double clearanceVolume = arguments.ChamberVolume - arguments.PistonVolume;
            if (clearanceVolume <= 0)
            {
                validationErrors.Add("Chamber volume minus piston volume must be positive");
            }

            if (validationErrors.Count > 0)
            {
                Console.WriteLine("Validation errors:");
                foreach (var error in validationErrors)
                {
                    Console.WriteLine($"  - {error}");
                }
                return false;
            }

            return true;
        }

        public static List<PistonResult> Calculate(Arguments arguments, Results results)
        {
            var csvResults = new List<PistonResult>();

            double angVelocity = 2 * Math.PI * (arguments.RPM / 60.0);
            double radius = arguments.Stroke / 2.0;
            double totalDeckHeight = arguments.DeckHeight + arguments.GasketHeight;

            // Calculate static results
            CalculateStaticResults(arguments, results);

            // Calculate piston motion for each degree from 0 to 180
            for (int angle = 0; angle <= 180; angle++)
            {
                double radAngle = (angle / 180.0) * Math.PI;
                double sinAngle = Math.Sin(radAngle);
                double cosAngle = Math.Cos(radAngle);

                // Calculate piston velocity
                double velocity = CalculateVelocity(angVelocity, radius, sinAngle, cosAngle, arguments.RodLength);

                // Calculate piston position
                double x = radius * cosAngle + Math.Sqrt(Math.Pow(arguments.RodLength, 2) - Math.Pow(radius * sinAngle, 2));
                double pistonPosition = -(totalDeckHeight - (x + arguments.CompHeight));

                var result = new PistonResult(angle, pistonPosition, velocity);
                csvResults.Add(result);

                // Track peak velocity
                if (results.MaxVelocity < velocity)
                {
                    results.MaxVelocity = velocity;
                    results.MaxVelocityDeg = angle;
                }
            }

            return csvResults;
        }

        private static void CalculateStaticResults(Arguments arguments, Results results)
        {
            // Displacement per cylinder (total for all cylinders)
            double cylinderVolume = Math.PI * Math.Pow(arguments.Bore / 2.0, 2) * arguments.Stroke;
            results.Displacement = cylinderVolume * arguments.CylinderCount;

            // Bore to stroke ratio
            results.BoreRatio = arguments.Bore / arguments.Stroke;

            // Rod ratio
            results.RodRatio = arguments.RodLength / arguments.Stroke;

            // Piston to deck
            results.Piston2deck = (arguments.DeckHeight + arguments.GasketHeight) -
                                 (arguments.RodLength + arguments.CompHeight + arguments.Stroke / 2.0);

            // Compression ratio - fixed calculation
            results.CompressionRatio = CalculateCompressionRatio(arguments);
        }

        private static double CalculateCompressionRatio(Arguments arguments)
        {
            // Swept volume (cylinder displacement)
            double sweptVolume = Math.PI * Math.Pow(arguments.Bore / 2.0, 2) * arguments.Stroke;

            // Gasket volume 
            double gasketVolume = Math.PI * Math.Pow(arguments.Bore / 2.0, 2) * arguments.GasketHeight;

            // Clearance volume = chamber volume + gasket volume - piston dome volume
            double clearanceVolume = arguments.ChamberVolume + gasketVolume - arguments.PistonVolume;

            // Unit conversion handling
            if (arguments.IsMetric)
            {
                // Metric: bore/stroke in mm creates volume in mm³
                // Chamber/piston volumes typically entered in cc (cm³)
                // Convert cc to mm³: 1 cc = 1000 mm³
                double chamberAndPistonInMM3 = (arguments.ChamberVolume - arguments.PistonVolume) * 1000;
                clearanceVolume = chamberAndPistonInMM3 + gasketVolume;
            }
            else
            {
                // Imperial: bore/stroke in inches creates volume in in³
                // Chamber/piston volumes should be in in³ (cubic inches)
                clearanceVolume = (arguments.ChamberVolume - arguments.PistonVolume) + gasketVolume;
            }

            if (clearanceVolume <= 0)
            {
                throw new InvalidOperationException("Clearance volume must be positive. Check chamber volume and piston volume values.");
            }

            return (sweptVolume + clearanceVolume) / clearanceVolume;
        }

        private static double CalculateVelocity(double angVelocity, double radius, double sinAngle, double cosAngle, double rodLength)
        {
            double term1 = radius * sinAngle;
            double term2 = (Math.Pow(radius, 2) * sinAngle * cosAngle) /
                          Math.Sqrt(Math.Pow(rodLength, 2) - Math.Pow(radius * sinAngle, 2));

            return Math.Abs(angVelocity * (term1 + term2));
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