using System;
using HomeApplianceRepairCenter.Models;

namespace HomeApplianceRepairCenter.Events
{
    /// <summary>
    /// Data sent with every status-change notification.
    /// All values are read-only: an event must not be able to rewrite history.
    /// </summary>
    public class StatusChangedEventArgs : EventArgs
    {
        public RepairStatus OldStatus { get; }
        public RepairStatus NewStatus { get; }
        public DateTime ChangedAt { get; }

        public StatusChangedEventArgs(RepairStatus oldStatus, RepairStatus newStatus, DateTime changedAt)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
            ChangedAt = changedAt;
        }
    }
}
