using System;
using HomeApplianceRepairCenter.Events;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Services
{
    /// <summary>
    /// Subscriber to the repair center events. In a real center these methods would send
    /// an SMS to the customer or alert the manager; here they print the message.
    /// The center does not know this class: it only raises events (loose coupling).
    /// </summary>
    public class CustomerNotifier
    {
        public void Subscribe(RepairCenter center)
        {
            center.StatusChanged += LogStatusChange;
            center.RepairCompleted += NotifyCustomer;
            center.DeliveryDelayed += AlertManager;
        }

        private void LogStatusChange(Appliance appliance, StatusChangedEventArgs e)
        {
            WriteColored(ConsoleColor.DarkCyan,
                $"  [LOG {e.ChangedAt:HH:mm:ss}] {appliance.SerialNumber}: {e.OldStatus} -> {e.NewStatus}");
        }

        private void NotifyCustomer(Appliance appliance, StatusChangedEventArgs e)
        {
            WriteColored(ConsoleColor.Green,
                $"  [SMS to {appliance.OwnerName}] Your {appliance.ApplianceType.ToLower()} ({appliance.SerialNumber}) " +
                $"is repaired. Amount due: {appliance.CalculateRepairCost():N2} USD.");
        }

        private void AlertManager(Appliance appliance, int daysLate)
        {
            WriteColored(ConsoleColor.Red,
                $"  [DELAY ALERT] {appliance.SerialNumber} ({appliance.ApplianceType}, owner {appliance.OwnerName}) " +
                $"is {daysLate} day(s) late. Status: {appliance.Status}.");
        }

        private static void WriteColored(ConsoleColor color, string message)
        {
            ConsoleColor previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = previous;
        }
    }
}
