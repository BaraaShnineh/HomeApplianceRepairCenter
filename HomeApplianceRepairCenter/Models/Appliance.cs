using System;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Interfaces;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.Models
{
    /// <summary>
    /// Base class of every appliance. It implements IRepairable, holds the data that all
    /// appliances share, controls the repair workflow and leaves the cost calculation
    /// to each appliance type (abstract method CalculateRepairCost).
    /// </summary>
    public abstract class Appliance : IRepairable
    {
        // ---------- static members: shared by all appliances ----------
        private static int totalRegistered;
        private static readonly int[] countByStatus = new int[Enum.GetValues(typeof(RepairStatus)).Length];

        /// <summary>Number of appliances created since the program started.</summary>
        public static int TotalRegistered => totalRegistered;

        /// <summary>Number of appliances currently being repaired.</summary>
        public static int UnderRepairCount => countByStatus[(int)RepairStatus.UnderRepair];

        /// <summary>Number of appliances that are repaired and waiting for pickup.</summary>
        public static int ReadyForPickupCount => countByStatus[(int)RepairStatus.Repaired];

        public static int CountByStatus(RepairStatus status) => countByStatus[(int)status];

        // ---------- common cost values ----------
        protected const decimal InspectionFee = 10m;    // paid for every appliance

        // ---------- private fields (encapsulation) ----------
        private readonly string serialNumber;
        private readonly string brand;
        private readonly string ownerName;
        private readonly DateTime receivedDate;
        private DateTime expectedDeliveryDate;
        private string problemDescription;
        private double laborHours;
        private decimal sparePartsCost;
        private RepairStatus status;

        /// <summary>Raised every time the status of this appliance changes.</summary>
        public event ApplianceStatusChangedHandler StatusChanged;

        protected Appliance(string serialNumber, string serialPrefix, string brand, string ownerName,
                            string problemDescription, DateTime receivedDate, DateTime expectedDeliveryDate)
        {
            if (!Validator.IsValidSerialNumber(serialNumber, serialPrefix))
                throw new ArgumentException($"Serial number must look like {serialPrefix}-1234.", nameof(serialNumber));
            if (!Validator.IsValidName(brand))
                throw new ArgumentException("Brand name is not valid.", nameof(brand));
            if (!Validator.IsValidName(ownerName))
                throw new ArgumentException("Owner name is not valid.", nameof(ownerName));
            if (!Validator.IsValidReceivedDate(receivedDate))
                throw new ArgumentException("Received date cannot be in the future.", nameof(receivedDate));

            this.serialNumber = serialNumber;
            this.brand = brand.Trim();
            this.ownerName = ownerName.Trim();
            this.receivedDate = receivedDate.Date;
            ExpectedDeliveryDate = expectedDeliveryDate;    // validated by the property
            ProblemDescription = problemDescription;        // validated by the property
            status = RepairStatus.Received;

            totalRegistered++;
            countByStatus[(int)RepairStatus.Received]++;
        }

        // ---------- read-only properties: fixed after creation ----------
        public string SerialNumber => serialNumber;
        public string Brand => brand;
        public string OwnerName => ownerName;
        public DateTime ReceivedDate => receivedDate;

        /// <summary>Changes only through StartRepair / CompleteRepair / Deliver.</summary>
        public RepairStatus Status => status;

        /// <summary>Name of the appliance type, defined by each subclass.</summary>
        public abstract string ApplianceType { get; }

        // ---------- properties with validation ----------
        public DateTime ExpectedDeliveryDate
        {
            get => expectedDeliveryDate;
            set
            {
                if (!Validator.IsValidExpectedDeliveryDate(receivedDate, value))
                    throw new ArgumentException("Expected delivery date cannot be before the received date.");
                expectedDeliveryDate = value.Date;
            }
        }

        public string ProblemDescription
        {
            get => problemDescription;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Problem description cannot be empty.");
                problemDescription = value.Trim();
            }
        }

        public double LaborHours
        {
            get => laborHours;
            set
            {
                EnsureNotDelivered();
                if (!Validator.IsValidLaborHours(value))
                    throw new ArgumentException($"Labor hours must be between 0 and {Validator.MaxLaborHours}.");
                laborHours = value;
            }
        }

        public decimal SparePartsCost
        {
            get => sparePartsCost;
            set
            {
                EnsureNotDelivered();
                if (!Validator.IsNonNegative(value))
                    throw new ArgumentException("Spare parts cost cannot be negative.");
                sparePartsCost = value;
            }
        }

        // ---------- repair workflow ----------
        public void StartRepair() => MoveTo(RepairStatus.Received, RepairStatus.UnderRepair);
        public void CompleteRepair() => MoveTo(RepairStatus.UnderRepair, RepairStatus.Repaired);
        public void Deliver() => MoveTo(RepairStatus.Repaired, RepairStatus.Delivered);

        /// <summary>An appliance is late if its date has passed and it is still not repaired.</summary>
        public bool IsDelayed(DateTime today) => DaysLate(today) > 0;

        public int DaysLate(DateTime today)
        {
            bool stillInWorkshop = status == RepairStatus.Received || status == RepairStatus.UnderRepair;
            int days = (today.Date - expectedDeliveryDate).Days;
            return stillInWorkshop && days > 0 ? days : 0;
        }

        // ---------- polymorphism ----------
        /// <summary>Every appliance type calculates its repair cost in its own way.</summary>
        public abstract decimal CalculateRepairCost();

        /// <summary>Extra information that only the subclass knows (capacity, BTU, ...).</summary>
        protected abstract string GetSpecificDetails();

        public string GetDetails()
        {
            return $"[{ApplianceType,-15}] {serialNumber} | {brand,-10} | Owner: {ownerName,-14} | " +
                   $"{status,-11} | Due: {expectedDeliveryDate:yyyy-MM-dd} | Cost: {CalculateRepairCost(),8:N2} USD" +
                   Environment.NewLine + $"      Problem: {problemDescription} | {GetSpecificDetails()}";
        }

        public override string ToString() => GetDetails();

        /// <summary>Shared part of the cost: inspection + labor (rate depends on the type) + spare parts.</summary>
        protected decimal CalculateBaseCost(decimal hourlyRate)
        {
            return InspectionFee + (decimal)laborHours * hourlyRate + sparePartsCost;
        }

        // ---------- private helpers ----------
        private void MoveTo(RepairStatus expectedCurrent, RepairStatus next)
        {
            if (status != expectedCurrent)
                throw new InvalidOperationException(
                    $"{serialNumber} is '{status}'. It must be '{expectedCurrent}' to become '{next}'.");

            RepairStatus old = status;
            status = next;
            countByStatus[(int)old]--;
            countByStatus[(int)next]++;

            OnStatusChanged(new StatusChangedEventArgs(old, next, DateTime.Now));
        }

        protected virtual void OnStatusChanged(StatusChangedEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        private void EnsureNotDelivered()
        {
            if (status == RepairStatus.Delivered)
                throw new InvalidOperationException("A delivered appliance cannot be modified.");
        }
    }
}
