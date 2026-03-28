using System;

namespace CarApp
{
    enum CarType { Tesla, BMW, Audi, Toyota, Lada }

    interface ICar
    {
        string GetDescription();
    }

    interface IElectric : ICar
    {
    }

    interface IMechanical : ICar
    {
        string FuelType { get; }
    }

    interface IAutomaticTransmission
    {
    }

    interface IMechanicalTransmission
    {
    }

    abstract class ACar : ICar
    {
        public string Brand;
        public int Seats;
        public string InfoSystem;

        public ACar(string brand, int seats, string infoSystem)
        {
            Brand = brand;
            Seats = seats;
            InfoSystem = infoSystem;
        }
        public abstract string GetEngineType();
        public abstract string GetTransmissionType();
        public string GetDescription()
        {
            return Brand + ": " + GetEngineType() + " с " +
                   GetTransmissionType() + " коробкой передач, " +
                   Seats + " местами, " + InfoSystem + " на борту";
        }
    }

    class Tesla : ACar, IElectric, IAutomaticTransmission
    {
        public Tesla() : base("Tesla", 5, "Android") { }
        public override string GetEngineType() { return "электрокар"; }
        public override string GetTransmissionType() { return "автоматической"; }
    }

    class BMW : ACar, IMechanical, IAutomaticTransmission
    {
        public string FuelType { get { return "бензин"; } }
        public BMW() : base("BMW X6", 5, "iDrive") { }
        public override string GetEngineType() { return FuelType; }
        public override string GetTransmissionType() { return "автоматической"; }
    }

    class Toyota : ACar, IMechanical, IAutomaticTransmission
    {
        public string FuelType { get { return "бензин"; } }
        public Toyota() : base("Toyota Land Cruiser", 7, "Toyota Touch") { }
        public override string GetEngineType() { return FuelType; }
        public override string GetTransmissionType() { return "автоматической"; }
    }

    class Audi : ACar, IMechanical, IAutomaticTransmission
    {
        public string FuelType { get { return "дизель"; } }
        public Audi() : base("Audi Q7", 7, "MMI") { }
        public override string GetEngineType() { return FuelType; }
        public override string GetTransmissionType() { return "автоматической"; }
    }

    class Lada : ACar, IMechanical, IMechanicalTransmission
    {
        public string FuelType { get { return "бензин"; } }
        public Lada() : base("Lada", 5, "Enjoy") { }
        public override string GetEngineType() { return FuelType; }
        public override string GetTransmissionType() { return "механической"; }
    }

    static class CarFactory
    {
        public static ICar CreateCar(CarType type)
        {
            switch (type)
            {
                case CarType.Tesla: return new Tesla();
                case CarType.BMW: return new BMW();
                case CarType.Toyota: return new Toyota();
                case CarType.Audi: return new Audi();
                case CarType.Lada: return new Lada();
                default: throw new ArgumentException("Неизвестная машина");
            }
        }
    }

    class Program
    {
        static void Main()
        {
            while (true)
            {
                Console.Write("Введите марку автомобиля или done для остановки (Tesla, BMW, Audi, Toyota, Lada): ");
                string input = Console.ReadLine();
                if (input == "done")
                    break;
                CarType type;
                if (Enum.TryParse(input, true, out type))
                {
                    ICar car = CarFactory.CreateCar(type);
                    Console.WriteLine(car.GetDescription());
                }
                else
                {
                    Console.WriteLine("Неверный ввод");
                }
            }
        }
    }
}
