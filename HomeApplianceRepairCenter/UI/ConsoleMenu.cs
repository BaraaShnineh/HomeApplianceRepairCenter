using System;
using System.Collections.Generic;
using System.Globalization;
using HomeApplianceRepairCenter.Models;
using HomeApplianceRepairCenter.Services;
using HomeApplianceRepairCenter.Utilities;

namespace HomeApplianceRepairCenter.UI
{
    /// <summary>
    /// Text menu that lets the user work with the repair center.
    /// All input is checked with the static Validator class before it is used.
    /// </summary>
    public class ConsoleMenu
    {
        private readonly RepairCenter center;
        private bool inputEnded;    // becomes true if the input stream closes (EOF)

        public ConsoleMenu(RepairCenter center)
        {
            this.center = center ?? throw new ArgumentNullException(nameof(center));
        }

        public void Run()
        {
            bool running = true;
            while (running && !inputEnded)
            {
                PrintMenu();
                int choice = ReadInt("Your choice: ", 0, 10);
                Console.WriteLine();

                switch (choice)
                {
                    case 1: ShowAppliances(center.Appliances, "All appliances"); break;
                    case 2: ShowByStatus(); break;
                    case 3: RegisterAppliance(); break;
                    case 4: ChangeStatus("start repair", a => a.StartRepair()); break;
                    case 5: ChangeStatus("complete repair", a => a.CompleteRepair()); break;
                    case 6: ChangeStatus("deliver", a => a.Deliver()); break;
                    case 7: UpdateRepairDetails(); break;
                    case 8: ShowCostReport(); break;
                    case 9: CheckDelays(); break;
                    case 10: ShowStatistics(); break;
                    case 0: running = false; break;
                }
            }
            Console.WriteLine("Goodbye.");
        }

        private void PrintMenu()
        {
            Console.WriteLine();
            Console.WriteLine($"========== {center.Name} ==========");
            Console.WriteLine(" 1) Show all appliances         6) Deliver to customer");
            Console.WriteLine(" 2) Show appliances by status   7) Update labor hours / spare parts");
            Console.WriteLine(" 3) Register a new appliance    8) Cost report");
            Console.WriteLine(" 4) Start repair                9) Check delayed deliveries");
            Console.WriteLine(" 5) Complete repair            10) Statistics");
            Console.WriteLine(" 0) Exit");
        }

        // ---------- menu actions ----------
        private static void ShowAppliances(IReadOnlyList<Appliance> list, string title)
        {
            Console.WriteLine($"--- {title} ({list.Count}) ---");
            if (list.Count == 0)
            {
                Console.WriteLine("  (none)");
                return;
            }
            foreach (Appliance appliance in list)
                Console.WriteLine(appliance.GetDetails());      // polymorphic: each type adds its own details
        }

        private void ShowByStatus()
        {
            RepairStatus status = ReadStatus();
            ShowAppliances(center.GetByStatus(status), $"Status: {status}");
        }

        private void RegisterAppliance()
        {
            Console.WriteLine("Type: 1) Refrigerator  2) Washing machine  3) Air conditioner");
            int type = ReadInt("Choose type: ", 1, 3);
            string prefix = type == 1 ? Refrigerator.SerialPrefix
                          : type == 2 ? WashingMachine.SerialPrefix
                          : AirConditioner.SerialPrefix;

            string serial = ReadSerial(prefix);
            string brand = ReadName("Brand: ");
            string owner = ReadName("Owner name: ");
            string problem = ReadText("Problem description: ");
            DateTime received = ReadDate($"Received date ({Validator.DateFormat}, empty = today): ", DateTime.Today);
            DateTime expected = ReadDate($"Expected delivery date ({Validator.DateFormat}): ", null);

            try
            {
                Appliance appliance;
                if (type == 1)
                {
                    int liters = ReadInt("Capacity (liters): ", 1, 2000);
                    bool gas = ReadYesNo("Needs gas recharge? (y/n): ");
                    appliance = new Refrigerator(serial, brand, owner, problem, received, expected, liters, gas);
                }
                else if (type == 2)
                {
                    double kg = ReadDouble("Load capacity (kg): ", 1, 30);
                    bool automatic = ReadYesNo("Automatic? (y/n): ");
                    bool drum = ReadYesNo("Needs drum replacement? (y/n): ");
                    appliance = new WashingMachine(serial, brand, owner, problem, received, expected, kg, automatic, drum);
                }
                else
                {
                    int btu = ReadInt("Cooling capacity (BTU): ", 1000, 60000);
                    bool wall = ReadYesNo("Wall-mounted? (y/n): ");
                    bool gas = ReadYesNo("Needs gas refill? (y/n): ");
                    appliance = new AirConditioner(serial, brand, owner, problem, received, expected, btu, wall, gas);
                }

                center.Register(appliance);
                Console.WriteLine($"Registered: {appliance.SerialNumber}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
            }
        }

        // One method for the three workflow actions (start / complete / deliver).
        private void ChangeStatus(string actionName, Action<Appliance> action)
        {
            Appliance appliance = ReadExistingAppliance();
            if (appliance == null)
                return;

            try
            {
                action(appliance);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"  Cannot {actionName}: {ex.Message}");
            }
        }

        private void UpdateRepairDetails()
        {
            Appliance appliance = ReadExistingAppliance();
            if (appliance == null)
                return;

            try
            {
                appliance.LaborHours = ReadDouble("Labor hours: ", 0, Validator.MaxLaborHours);
                appliance.SparePartsCost = (decimal)ReadDouble("Spare parts cost (USD): ", 0, 100000);
                Console.WriteLine($"  New repair cost: {appliance.CalculateRepairCost():N2} USD");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
            }
        }

        private void ShowCostReport()
        {
            Console.WriteLine("--- Cost report ---");
            foreach (Appliance appliance in center.Appliances)
            {
                Console.WriteLine($"  {appliance.SerialNumber}  {appliance.ApplianceType,-15}  " +
                                  $"{appliance.CalculateRepairCost(),9:N2} USD");
            }
            Console.WriteLine("  -------------------------------------------");
            foreach (RepairStatus status in Enum.GetValues(typeof(RepairStatus)))
                Console.WriteLine($"  Total {status,-12}: {center.CalculateTotalCost(status),9:N2} USD");
            Console.WriteLine($"  GRAND TOTAL       : {center.CalculateTotalCost(),9:N2} USD");
        }

        private void CheckDelays()
        {
            int delayed = center.CheckDelayedDeliveries(DateTime.Today);   // raises DeliveryDelayed events
            Console.WriteLine(delayed == 0 ? "No delayed appliances." : $"{delayed} delayed appliance(s) found.");
        }

        private static void ShowStatistics()
        {
            Console.WriteLine("--- Statistics (static properties of Appliance) ---");
            Console.WriteLine($"  Total registered  : {Appliance.TotalRegistered}");
            Console.WriteLine($"  Under repair      : {Appliance.UnderRepairCount}");
            Console.WriteLine($"  Ready for pickup  : {Appliance.ReadyForPickupCount}");
            foreach (RepairStatus status in Enum.GetValues(typeof(RepairStatus)))
                Console.WriteLine($"  {status,-18}: {Appliance.CountByStatus(status)}");
        }

        // ---------- input helpers (all validated) ----------
        private string ReadLine(string prompt)
        {
            Console.Write(prompt);
            string line = Console.ReadLine();
            if (line == null)
                inputEnded = true;
            return line ?? string.Empty;
        }

        private int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                string input = ReadLine(prompt);
                if (inputEnded) return 0;
                if (int.TryParse(input, out int value) && value >= min && value <= max)
                    return value;
                Console.WriteLine($"  Please enter a whole number between {min} and {max}.");
            }
        }

        private double ReadDouble(string prompt, double min, double max)
        {
            while (true)
            {
                string input = ReadLine(prompt);
                if (inputEnded) return min;
                if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
                    && value >= min && value <= max)
                    return value;
                Console.WriteLine($"  Please enter a number between {min} and {max}.");
            }
        }

        private string ReadText(string prompt)
        {
            while (true)
            {
                string input = ReadLine(prompt).Trim();
                if (inputEnded || input.Length > 0) return input;
                Console.WriteLine("  This field cannot be empty.");
            }
        }

        private string ReadName(string prompt)
        {
            while (true)
            {
                string input = ReadLine(prompt);
                if (inputEnded || Validator.IsValidName(input)) return input.Trim();
                Console.WriteLine("  A name must have 2-50 characters and contain letters.");
            }
        }

        private string ReadSerial(string prefix)
        {
            while (true)
            {
                string input = ReadLine($"Serial number ({prefix}-1234): ").Trim().ToUpperInvariant();
                if (inputEnded) return input;
                if (!Validator.IsValidSerialNumber(input, prefix))
                    Console.WriteLine($"  Format must be {prefix}- followed by 4 digits.");
                else if (center.FindBySerial(input) != null)
                    Console.WriteLine("  This serial number is already registered.");
                else
                    return input;
            }
        }

        private DateTime ReadDate(string prompt, DateTime? defaultValue)
        {
            while (true)
            {
                string input = ReadLine(prompt);
                if (inputEnded) return defaultValue ?? DateTime.Today;
                if (input.Trim().Length == 0 && defaultValue.HasValue)
                    return defaultValue.Value;
                if (Validator.TryParseDate(input, out DateTime date))
                    return date;
                Console.WriteLine($"  Please use the format {Validator.DateFormat}.");
            }
        }

        private bool ReadYesNo(string prompt)
        {
            while (true)
            {
                string input = ReadLine(prompt).Trim().ToLowerInvariant();
                if (inputEnded) return false;
                if (input == "y" || input == "yes") return true;
                if (input == "n" || input == "no") return false;
                Console.WriteLine("  Please answer y or n.");
            }
        }

        private RepairStatus ReadStatus()
        {
            RepairStatus[] statuses = (RepairStatus[])Enum.GetValues(typeof(RepairStatus));
            for (int i = 0; i < statuses.Length; i++)
                Console.WriteLine($"  {i + 1}) {statuses[i]}");
            int index = ReadInt("Choose status: ", 1, statuses.Length);
            return inputEnded ? statuses[0] : statuses[index - 1];
        }

        private Appliance ReadExistingAppliance()
        {
            string serial = ReadLine("Serial number: ").Trim();
            if (inputEnded) return null;
            Appliance appliance = center.FindBySerial(serial);
            if (appliance == null)
                Console.WriteLine("  No appliance with this serial number.");
            return appliance;
        }
    }
}
