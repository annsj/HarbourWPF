namespace HarbourWPF
{
    class MotorBoat : Boat
    {
        public int Power { get; set; }

        public MotorBoat(string type, string id, int weight, int maxSpeed, int daysStaying, int daysSinceArrival, int power)
            : base(type, id, weight, maxSpeed, daysStaying, daysSinceArrival)
        {
            Power = power;
        }

        public MotorBoat()
        {
            Type = "Motorbåt";
            IdNumber = "M-" + GenerateID();
            Weight = Utils.random.Next(200, 3000 + 1);
            MaximumSpeed = Utils.random.Next(1, 60 + 1);
            DaysStaying = 3;
            Power = Utils.random.Next(10, 1000 + 1);
        }

        public override string ToString()
        {
            return base.ToString() + $"\tMotoreffekt:\t{Power} hästkrafter";
        }

        public override string TextToFile(int index)
        {
            return base.TextToFile(index) + $"{Power}";
        }

        internal static bool ParkMotorBoatInHarbour(Boat boat, DockSpace[] dock1, DockSpace[] dock2)
        {
            bool boatParked;

            while (true)
            {
                int selectedSpace;

                (selectedSpace, boatParked) = RowingBoat.FindSingleSpaceBetweenOccupiedSpaces(dock1);
                if (boatParked)
                {
                    dock1[selectedSpace].ParkedBoats.Add(boat);
                    break;
                }

                (selectedSpace, boatParked) = RowingBoat.FindSingleSpaceBetweenOccupiedSpaces(dock2);
                if (boatParked)
                {
                    dock2[selectedSpace].ParkedBoats.Add(boat);
                    break;
                }

                (selectedSpace, boatParked) = RowingBoat.FindFirstFreeSpace(dock1);
                if (boatParked)
                {
                    dock1[selectedSpace].ParkedBoats.Add(boat);
                    break;
                }

                (selectedSpace, boatParked) = RowingBoat.FindFirstFreeSpace(dock2);
                if (boatParked)
                {
                    dock2[selectedSpace].ParkedBoats.Add(boat);
                    break;
                }

                break;
            }

            return boatParked;
        }
    }
}
