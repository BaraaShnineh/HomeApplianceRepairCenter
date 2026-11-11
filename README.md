# Home Appliance Repair Center Management System

Object-oriented C# console application for the **IPG203 – Object-Oriented Programming and Design** assignment (F25, Syrian Virtual University).

The system manages a repair center for home appliances (refrigerators, washing machines and air conditioners):
it registers appliances, follows each one through the repair workflow, calculates the repair cost of every
appliance type in its own way, alerts the manager about late deliveries and notifies customers when a repair is completed.

## Project structure

```
HomeApplianceRepairCenter/
├── HomeApplianceRepairCenter.sln
└── HomeApplianceRepairCenter/
    ├── Program.cs                     entry point + sample data
    ├── Interfaces/
    │   └── IRepairable.cs             operations shared by all appliances
    ├── Models/
    │   ├── RepairStatus.cs            Received → UnderRepair → Repaired → Delivered
    │   ├── Appliance.cs               abstract base class (implements IRepairable)
    │   ├── Refrigerator.cs
    │   ├── WashingMachine.cs
    │   └── AirConditioner.cs
    ├── Events/
    │   ├── Delegates.cs               ApplianceStatusChangedHandler, DeliveryDelayedHandler
    │   └── StatusChangedEventArgs.cs
    ├── Services/
    │   ├── RepairCenter.cs            main class: List<Appliance> + events
    │   └── CustomerNotifier.cs        event subscriber (SMS / log / delay alert)
    ├── Utilities/
    │   └── Validator.cs               static validation class
    └── UI/
        └── ConsoleMenu.cs             text menu
```

## OOP principles

| Principle | Where |
|---|---|
| Abstraction | `IRepairable` interface, abstract class `Appliance` with abstract `CalculateRepairCost()` |
| Inheritance & polymorphism | `Refrigerator`, `WashingMachine`, `AirConditioner` override `CalculateRepairCost()`; `RepairCenter` processes a `List<Appliance>` |
| Encapsulation | all fields private; read-only `SerialNumber`, `ReceivedDate`, `Brand`, `OwnerName`; `Status` changes only through the workflow methods; validated setters |
| Delegates & events | `StatusChanged`, `RepairCompleted`, `DeliveryDelayed` |
| Static class & members | `Validator` static class; `Appliance.TotalRegistered`, `Appliance.UnderRepairCount`, `Appliance.ReadyForPickupCount` |

## How to run

Requires the .NET 8 SDK.

```
dotnet run --project HomeApplianceRepairCenter
```

Or open `HomeApplianceRepairCenter.sln` in Visual Studio 2022 and press F5.
