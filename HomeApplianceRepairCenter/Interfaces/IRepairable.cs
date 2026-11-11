using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Interfaces
{
    /// <summary>
    /// The basic operations shared by every appliance that enters the repair center.
    /// </summary>
    public interface IRepairable
    {
        string SerialNumber { get; }
        RepairStatus Status { get; }

        void StartRepair();
        void CompleteRepair();
        void Deliver();

        decimal CalculateRepairCost();
        string GetDetails();
    }
}
