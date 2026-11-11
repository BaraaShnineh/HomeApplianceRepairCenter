using System;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models
{
    /// <summary>
    /// An air conditioner. Gas refill cost grows with the cooling capacity (BTU),
    /// and wall-mounted units need an on-site visit by the technician.
    /// </summary>
    public class AirConditioner : Appliance
    {
        public const string SerialPrefix = "AIR";

        private const decimal HourlyRate = 15m;
        private const decimal GasRefillCostPer12000Btu = 25m;
        private const decimal OnSiteVisitFee = 20m;

        private readonly int coolingCapacityBtu;
        private readonly bool isWallMounted;
        private bool needsGasRefill;

        public AirConditioner(string serialNumber, string brand, string ownerName, string problemDescription,
                              DateTime receivedDate, DateTime expectedDeliveryDate,
                              int coolingCapacityBtu, bool isWallMounted, bool needsGasRefill)
            : base(serialNumber, SerialPrefix, brand, ownerName, problemDescription, receivedDate, expectedDeliveryDate)
        {
            if (!Validator.IsPositive(coolingCapacityBtu))
                throw new ArgumentException("Cooling capacity must be greater than zero.", nameof(coolingCapacityBtu));

            this.coolingCapacityBtu = coolingCapacityBtu;
            this.isWallMounted = isWallMounted;
            this.needsGasRefill = needsGasRefill;
        }

        public override string ApplianceType => "Air Conditioner";

        public int CoolingCapacityBtu => coolingCapacityBtu;
        public bool IsWallMounted => isWallMounted;

        public bool NeedsGasRefill
        {
            get => needsGasRefill;
            set => needsGasRefill = value;
        }

        public override decimal CalculateRepairCost()
        {
            decimal cost = CalculateBaseCost(HourlyRate);
            if (needsGasRefill)
                cost += GasRefillCostPer12000Btu * coolingCapacityBtu / 12000m;
            if (isWallMounted)
                cost += OnSiteVisitFee;
            return cost;
        }

        protected override string GetSpecificDetails()
        {
            return $"Capacity: {coolingCapacityBtu} BTU | {(isWallMounted ? "Wall-mounted" : "Portable")} | " +
                   $"Gas refill: {(needsGasRefill ? "Yes" : "No")}";
        }
    }
}
