using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Events
{
    /// <summary>
    /// Signature of methods that want to be notified when an appliance changes status.
    /// Used by Appliance.StatusChanged, RepairCenter.StatusChanged and RepairCenter.RepairCompleted.
    /// </summary>
    public delegate void ApplianceStatusChangedHandler(Appliance appliance, StatusChangedEventArgs e);

    /// <summary>
    /// Signature of methods that want to be notified when an appliance is late
    /// (it passed its expected delivery date and is still not repaired).
    /// </summary>
    public delegate void DeliveryDelayedHandler(Appliance appliance, int daysLate);
}
