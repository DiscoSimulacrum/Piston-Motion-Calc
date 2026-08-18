namespace PistonMotion.Core
{
    public class Arguments
    {
        public string FileLocation { get; set; } = "C:\\Windows\\Temp\\Piston-Motion-Calc\\";
        public string Filename { get; set; } = "";
        public bool IsMetric { get; set; } = true;
        public double Stroke { get; set; }
        public double Bore { get; set; }
        public double RodLength { get; set; }
        public double DeckHeight { get; set; }
        public double CompHeight { get; set; }
        public double PistonVolume { get; set; }
        public double ChamberVolume { get; set; }
        public double GasketHeight { get; set; }
        public int RPM { get; set; }
        public int CylinderCount { get; set; } = 1;

        /// <summary>
        /// Creates a copy of the current Arguments object
        /// </summary>
        public Arguments Clone()
        {
            return new Arguments
            {
                FileLocation = this.FileLocation,
                Filename = this.Filename,
                IsMetric = this.IsMetric,
                Stroke = this.Stroke,
                Bore = this.Bore,
                RodLength = this.RodLength,
                DeckHeight = this.DeckHeight,
                CompHeight = this.CompHeight,
                PistonVolume = this.PistonVolume,
                ChamberVolume = this.ChamberVolume,
                GasketHeight = this.GasketHeight,
                RPM = this.RPM,
                CylinderCount = this.CylinderCount
            };
        }

        /// <summary>
        /// Returns a string representation of the engine configuration
        /// </summary>
        public override string ToString()
        {
            string units = IsMetric ? "mm" : "in";
            string volumeUnits = IsMetric ? "cc" : "ci";

            return $"Engine: {Bore:F1}x{Stroke:F1}{units}, {CylinderCount} cyl, {RPM} RPM, Rod: {RodLength:F1}{units}";
        }
    }
}
