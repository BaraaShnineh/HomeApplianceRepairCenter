namespace HomeApplianceRepairCenter.Models
{
    /// <summary>
    /// The lifecycle of an appliance inside the repair center.
    /// The only allowed path is: Received -> UnderRepair -> Repaired -> Delivered.
    /// </summary>
    public enum RepairStatus
    {
        Received,       // registered at the reception desk, waiting for a technician
        UnderRepair,    // a technician is working on it
        Repaired,       // repair finished, waiting for the customer to pick it up
        Delivered       // handed back to the customer
    }
}
