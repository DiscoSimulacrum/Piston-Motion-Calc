using System;
using System.Collections.Generic;

namespace PistonMotion.Core
{
    /// <summary>
    /// Pure calculation logic for piston kinematics, shared between the console and web front ends.
    /// </summary>
    public static class PistonCalculator
    {
        public static List<string> ValidateInputs(Arguments arguments)
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

            return validationErrors;
        }

        public static List<PistonResult> Calculate(Arguments arguments, Results results)
        {
            var pistonResults = new List<PistonResult>();

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
                pistonResults.Add(result);

                // Track peak velocity
                if (results.MaxVelocity < velocity)
                {
                    results.MaxVelocity = velocity;
                    results.MaxVelocityDeg = angle;
                }
            }

            return pistonResults;
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

            double clearanceVolume;

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
    }
}
