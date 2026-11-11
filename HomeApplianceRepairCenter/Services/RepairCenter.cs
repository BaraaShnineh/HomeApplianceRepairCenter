using System;
using System.Collections.Generic;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Services
{
    /// <summary>
    /// The main class of the system. It keeps a list of all appliances (List&lt;Appliance&gt;)
    /// and works on them through the base type, so every call to CalculateRepairCost()
    /// runs the version of the real appliance type (polymorphism).
    /// It also publishes the events that the rest of the system listens to.
    /// </summary>
    public class RepairCenter
    {
        private readonly string name;
        private readonly List<Appliance> appliances = new List<Appliance>();

        /// <summary>Raised for every status change of any registered appliance.</summary>
        public event ApplianceStatusChangedHandler StatusChanged;

        /// <summary>Raised only when a repair is finished (status becomes Repaired).</summary>
        public event ApplianceStatusChangedHandler RepairCompleted;

        /// <summary>Raised for every appliance that passed its expected delivery date.</summary>
        public event DeliveryDelayedHandler DeliveryDelayed;

        public RepairCenter(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Center name cannot be empty.", nameof(name));
            this.name = name.Trim();
        }

        public string Name => name;
        public int Count => appliances.Count;

        /// <summary>Read-only view: outside code can read the list but cannot add or remove items directly.</summary>
        public IReadOnlyList<Appliance> Appliances => appliances.AsReadOnly();

        public void Register(Appliance appliance)
        {
            if (appliance == null)
                throw new ArgumentNullException(nameof(appliance));
            if (FindBySerial(appliance.SerialNumber) != null)
                throw new InvalidOperationException($"Serial number {appliance.SerialNumber} is already registered.");

            appliances.Add(appliance);
            appliance.StatusChanged += OnApplianceStatusChanged;   // the center listens to every appliance
        }

        public Appliance FindBySerial(string serialNumber)
        {
            foreach (Appliance appliance in appliances)
            {
                if (string.Equals(appliance.SerialNumber, serialNumber?.Trim(), StringComparison.OrdinalIgnoreCase))
                    return appliance;
            }
            return null;
        }

        public List<Appliance> GetByStatus(RepairStatus status)
        {
            List<Appliance> result = new List<Appliance>();
            foreach (Appliance appliance in appliances)
            {
                if (appliance.Status == status)
                    result.Add(appliance);
            }
            return result;
        }

        /// <summary>Total repair cost of all appliances - one loop, each object calculates its own cost.</summary>
        public decimal CalculateTotalCost()
        {
            decimal total = 0;
            foreach (Appliance appliance in appliances)
                total += appliance.CalculateRepairCost();     // polymorphic call
            return total;
        }

        public decimal CalculateTotalCost(RepairStatus status)
        {
            decimal total = 0;
            foreach (Appliance appliance in GetByStatus(status))
                total += appliance.CalculateRepairCost();
            return total;
        }

        /// <summary>Checks every appliance and raises DeliveryDelayed for the late ones.</summary>
        public int CheckDelayedDeliveries(DateTime today)
        {
            int delayed = 0;
            foreach (Appliance appliance in appliances)
            {
                int daysLate = appliance.DaysLate(today);
                if (daysLate > 0)
                {
                    delayed++;
                    DeliveryDelayed?.Invoke(appliance, daysLate);
                }
            }
            return delayed;
        }

        // Forwards the appliance event and raises RepairCompleted when the condition is met.
        private void OnApplianceStatusChanged(Appliance appliance, StatusChangedEventArgs e)
        {
            StatusChanged?.Invoke(appliance, e);

            if (e.NewStatus == RepairStatus.Repaired)
                RepairCompleted?.Invoke(appliance, e);
        }
    }
}
