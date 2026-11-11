using System;
using HomeApplianceRepairCenter.Models;
using HomeApplianceRepairCenter.Services;
using HomeApplianceRepairCenter.UI;

namespace HomeApplianceRepairCenter
{
    /// <summary>
    /// Home Appliance Repair Center Management System - entry point.
    /// Builds the center, loads sample data, connects the event subscribers and starts the menu.
    /// </summary>
    public static class Program
    {
        public static void Main()
        {
            RepairCenter center = new RepairCenter("Home Appliance Repair Center");
            LoadSampleData(center);

            CustomerNotifier notifier = new CustomerNotifier();
            notifier.Subscribe(center);     // from now on, every event is shown on the screen

            new ConsoleMenu(center).Run();
        }

        /// <summary>Sample appliances (dates are relative to today so the delay check always has results).</summary>
        private static void LoadSampleData(RepairCenter center)
        {
            DateTime today = DateTime.Today;

            Refrigerator fridge = new Refrigerator("REF-1001", "Samsung", "Ahmad Khalil", "Not cooling",
                today.AddDays(-6), today.AddDays(-1), capacityLiters: 450, needsGasRecharge: true);
            fridge.LaborHours = 3;
            fridge.SparePartsCost = 20m;

            WashingMachine washer = new WashingMachine("WSH-2001", "LG", "Sara Haddad", "Drum does not spin",
                today.AddDays(-4), today.AddDays(2), loadCapacityKg: 7, isAutomatic: true, needsDrumReplacement: true);
            washer.LaborHours = 2.5;

            AirConditioner splitUnit = new AirConditioner("AIR-3001", "Gree", "Omar Nasser", "Leaking water",
                today.AddDays(-3), today.AddDays(1), coolingCapacityBtu: 18000, isWallMounted: true, needsGasRefill: true);
            splitUnit.LaborHours = 2;
            splitUnit.SparePartsCost = 15m;

            Refrigerator smallFridge = new Refrigerator("REF-1002", "Beko", "Lina Saleh", "Noisy compressor",
                today.AddDays(-10), today.AddDays(-5), capacityLiters: 300, needsGasRecharge: false);
            smallFridge.LaborHours = 4;
            smallFridge.SparePartsCost = 60m;

            AirConditioner portableUnit = new AirConditioner("AIR-3002", "Midea", "Khaled Youssef", "Remote not working",
                today.AddDays(-8), today.AddDays(-3), coolingCapacityBtu: 12000, isWallMounted: false, needsGasRefill: false);
            portableUnit.LaborHours = 1;
            portableUnit.SparePartsCost = 5m;

            Appliance[] all = { fridge, washer, splitUnit, smallFridge, portableUnit };
            foreach (Appliance appliance in all)
                center.Register(appliance);

            // Move some appliances forward in the workflow.
            fridge.StartRepair();
            splitUnit.StartRepair();
            smallFridge.StartRepair();
            smallFridge.CompleteRepair();
            smallFridge.Deliver();
        }
    }
}
