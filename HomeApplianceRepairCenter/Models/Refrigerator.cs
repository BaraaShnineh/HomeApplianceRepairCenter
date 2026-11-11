using System;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models
{
    /// <summary>
    /// A refrigerator. Cost depends on gas recharge and on the size of the unit
    /// (large refrigerators need two technicians to move and open).
    /// </summary>
    public class Refrigerator : Appliance
    {
        public const string SerialPrefix = "REF";

        private const decimal HourlyRate = 12m;
        private const decimal GasRechargeCost = 35m;
        private const decimal LargeUnitSurcharge = 15m;
        private const int LargeUnitLiters = 400;

        private readonly int capacityLiters;
        private bool needsGasRecharge;

        public Refrigerator(string serialNumber, string brand, string ownerName, string problemDescription,
                            DateTime receivedDate, DateTime expectedDeliveryDate,
                            int capacityLiters, bool needsGasRecharge)
            : base(serialNumber, SerialPrefix, brand, ownerName, problemDescription, receivedDate, expectedDeliveryDate)
        {
            if (!Validator.IsPositive(capacityLiters))
                throw new ArgumentException("Capacity must be greater than zero.", nameof(capacityLiters));

            this.capacityLiters = capacityLiters;
            this.needsGasRecharge = needsGasRecharge;
        }

        public override string ApplianceType => "Refrigerator";

        public int CapacityLiters => capacityLiters;    // physical property: read-only

        public bool NeedsGasRecharge
        {
            get => needsGasRecharge;
            set => needsGasRecharge = value;              // can change after diagnosis
        }

        public override decimal CalculateRepairCost()
        {
            decimal cost = CalculateBaseCost(HourlyRate);
            if (needsGasRecharge)
                cost += GasRechargeCost;
            if (capacityLiters > LargeUnitLiters)
                cost += LargeUnitSurcharge;
            return cost;
        }

        protected override string GetSpecificDetails()
        {
            return $"Capacity: {capacityLiters} L | Gas recharge: {(needsGasRecharge ? "Yes" : "No")}";
        }
    }
}
