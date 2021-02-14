using System.Linq;

namespace HarbourWPF
{
    class RowingBoat : Boat
    {
        public int MaximumPassengers { get; set; }

        public RowingBoat(string type, string id, int weight, int maxSpeed, int daysStaying, int daysSinceArrival, int maxPassengers)
            : base(type, id, weight, maxSpeed, daysStaying, daysSinceArrival)
        {
            MaximumPassengers = maxPassengers;
        }

        public RowingBoat()
        {
            Type = "Roddbåt";
            IdNumber = "R-" + GenerateID();
            Weight = Utils.random.Next(100, 300 + 1);
            MaximumSpeed = Utils.random.Next(1, 3 + 1);
            DaysStaying = 1;
            MaximumPassengers = Utils.random.Next(1, 6 + 1);
        }

        public override string ToString()
        {
            return base.ToString() + $"\tKapacitet:\t{MaximumPassengers} personer";
        }

        public override string TextToFile(int index)
        {
            return base.TextToFile(index) + $"{MaximumPassengers}";
        }

        public static bool ParkRowingBoatInHarbour(Boat boat, DockSpace[] dock1, DockSpace[] dock2)
        {
            bool boatParked;

            while (true)
            {
                int selectedSpace;

                (selectedSpace, boatParked) = FindSpaceWithParkedRowingBoat(dock1);
                if (boatParked)
                {
                    dock1[selectedSpace].ParkedBoats.Add(boat);
                    break;
                }

                (selectedSpace, boatParked) = FindSpaceWithParkedRowingBoat(dock2);
                if (boatParked)
                {
                    dock2[selectedSpace].ParkedBoats.Add(boat);
                    break;
                }

                (selectedSpace, boatParked) = FindSingleSpaceBetweenOccupiedSpaces(dock1);
                if (boatParked)
                {
                    dock1[selectedSpace].ParkedBoats.Add(boat);
                    break;
                }

                (selectedSpace, boatParked) = FindSingleSpaceBetweenOccupiedSpaces(dock2);
                if (boatParked)
                {
                    dock2[selectedSpace].ParkedBoats.Add(boat);
                    break;
                }

                (selectedSpace, boatParked) = FindFirstFreeSpace(dock1);
                if (boatParked)
                {
                    dock1[selectedSpace].ParkedBoats.Add(boat);
                    break;
                }

                (selectedSpace, boatParked) = FindFirstFreeSpace(dock2);
                if (boatParked)
                {
                    dock2[selectedSpace].ParkedBoats.Add(boat);
                    break;
                }

                break;
            }

            return boatParked;
        }

        internal static (int selectedSpace, bool boatParked) FindSpaceWithParkedRowingBoat(DockSpace[] dock)
        {
            int selectedSpace = 0;
            bool spaceFound = false;

            foreach (var space in dock)
            {
                foreach (var boat in space.ParkedBoats)
                {
                    if (boat is RowingBoat && space.ParkedBoats.Count() == 1)
                    {
                        selectedSpace = space.SpaceId;
                        spaceFound = true;
                        break;
                    }
                }
                if (spaceFound)
                {
                    break;
                }
            }
            return (selectedSpace, spaceFound);
        }

        internal static (int harbourPosition, bool spaceFound) FindSingleSpaceBetweenOccupiedSpaces(DockSpace[] dock)
        {
            int selectedSpace = 0;
            bool spaceFound = false;

            // Om index 0 är ledigt och index 1 upptaget
            if (dock[0].ParkedBoats.Count == 0 && dock[1].ParkedBoats.Count > 0)
            {
                selectedSpace = 0;
                spaceFound = true;
            }
            // Annars, hitta ensam plats med upptagna platser runtom
            if (spaceFound == false)
            {
                var q = dock
                    .FirstOrDefault(h => h.ParkedBoats.Count == 0
                    && h.SpaceId > 0
                    && h.SpaceId < dock.Length - 2
                    && dock[h.SpaceId - 1].ParkedBoats.Count > 0
                    && dock[h.SpaceId + 1].ParkedBoats.Count > 0);

                if (q != null)
                {
                    selectedSpace = q.SpaceId;
                    spaceFound = true;
                }
            }
            // Annars, om sista index är ledigt och index innan upptaget
            if (spaceFound == false)
            {
                if (dock[dock.Length - 1].ParkedBoats.Count == 0 && dock[dock.Length - 2].ParkedBoats.Count > 0)
                {
                    selectedSpace = dock.Length - 1;
                    spaceFound = true;
                }
            }

            return (selectedSpace, spaceFound);
        }

        internal static (int harbourPosition, bool spaceFound) FindFirstFreeSpace(DockSpace[] dock)
        {
            int selectedSpace = 0;
            bool spaceFound = false;

            // Hitta första lediga plats
            var q = dock
               .FirstOrDefault(h => h.ParkedBoats.Count == 0);

            if (q != null)
            {
                selectedSpace = q.SpaceId;
                spaceFound = true;
            }
            return (selectedSpace, spaceFound);
        }
    }
}
