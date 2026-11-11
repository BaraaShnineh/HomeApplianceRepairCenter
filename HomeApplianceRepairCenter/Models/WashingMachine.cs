using System;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models
{
    /// <summary>
    /// A washing machine. Automatic machines need an electronic-board test,
    /// and a damaged drum adds a replacement cost.
    /// </summary>
    public class WashingMachine : Appliance
    {
        public const string SerialPrefix = "WSH";

        private const decimal HourlyRate = 10m;
        private const decimal DrumReplacementCost = 40m;
        private const decimal ElectronicBoardTestCost = 8m;

        private readonly double loadCapacityKg;
        private readonly bool isAutomatic;
        private bool needsDrumReplacement;

        public WashingMachine(string serialNumber, string brand, string ownerName, string problemDescription,
                              DateTime receivedDate, DateTime expectedDeliveryDate,
                              double loadCapacityKg, bool isAutomatic, bool needsDrumReplacement)
            : base(serialNumber, SerialPrefix, brand, ownerName, problemDescription, receivedDate, expectedDeliveryDate)
        {
            if (!Validator.IsPositive(loadCapacityKg))
                throw new ArgumentException("Load capacity must be greater than zero.", nameof(loadCapacityKg));

            this.loadCapacityKg = loadCapacityKg;
            this.isAutomatic = isAutomatic;
            this.needsDrumReplacement = needsDrumReplacement;
        }

        public override string ApplianceType => "Washing Machine";

        public double LoadCapacityKg => loadCapacityKg;
        public bool IsAutomatic => isAutomatic;

        public bool NeedsDrumReplacement
        {
            get => needsDrumReplacement;
            set => needsDrumReplacement = value;
        }

        public override decimal CalculateRepairCost()
        {
            decimal cost = CalculateBaseCost(HourlyRate);
            if (isAutomatic)
                cost += ElectronicBoardTestCost;
            if (needsDrumReplacement)
                cost += DrumReplacementCost;
            return cost;
        }

        protected override string GetSpecificDetails()
        {
            return $"Load: {loadCapacityKg} kg | {(isAutomatic ? "Automatic" : "Manual")} | " +
                   $"Drum replacement: {(needsDrumReplacement ? "Yes" : "No")}";
        }
    }
}
