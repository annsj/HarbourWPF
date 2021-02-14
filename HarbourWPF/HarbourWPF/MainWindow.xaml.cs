using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HarbourWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // Om det finns båtar lagrade i en fil måste dockLength vara minst lika stort som högsta index + 1 i filen
        readonly int dock1Length = 32;
        readonly int dock2Length = 32;

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists("BoatsInDock1.txt") == false)
            {
                FileStream fs1 = File.Create("BoatsInDock1.txt");
                fs1.Close();
            }

            var fileText = File.ReadLines("BoatsInDock1.txt", System.Text.Encoding.UTF7);


            DockSpace[] dock1 = new DockSpace[dock1Length];
            for (int i = 0; i < dock1.Length; i++)
            {
                dock1[i] = new DockSpace(i);
            }

            AddBoatsFromFileToDock(fileText, dock1);

            if (File.Exists("BoatsInDock2.txt") == false)
            {
                FileStream fs2 = File.Create("BoatsInDock2.txt");
                fs2.Close();
            }

            fileText = File.ReadLines("BoatsInDock2.txt", System.Text.Encoding.UTF7);

            DockSpace[] dock2 = new DockSpace[dock2Length];
            for (int i = 0; i < dock2.Length; i++)
            {
                dock2[i] = new DockSpace(i);
            }

            AddBoatsFromFileToDock(fileText, dock2);

            PrintHarbourTable(dock1, dock2);

            string dockName = "Kaj 1";
            dock1Canvas.Children.Clear();
            AddSpacesToDockCanvas(dockName, dock1.Length, dock1Canvas);
            PlaceBoatsInDockCanvas(dock1, dock1Canvas);

            dockName = "Kaj 2";
            dock2Canvas.Children.Clear();
            AddSpacesToDockCanvas(dockName, dock2.Length, dock2Canvas);
            PlaceBoatsInDockCanvas(dock2, dock2Canvas);


            List<Boat> boatsInDock1 = GenerateBoatsInHarbourList(dock1);
            List<Boat> boatsInDock2 = GenerateBoatsInHarbourList(dock2);

            var boatsInBothDocks = boatsInDock1
                .Concat(boatsInDock2);

            if (boatsInBothDocks.Count() > 0)
            {
                int sumOfWeight = GenerateSumOfWeight(boatsInBothDocks);
                double averageSpeed = GenerateAverageSpeed(boatsInBothDocks);
                int availableSpacesDock1 = CountAvailableSpaces(dock1);
                int availableSpacesDock2 = CountAvailableSpaces(dock2);

                summaryListBox.Items.Clear();

                PrintSummaryOfBoats(boatsInBothDocks);
                summaryListBox.Items.Add("\n");

                PrintStatistics(sumOfWeight, averageSpeed, availableSpacesDock1, availableSpacesDock2);
            }

            StreamWriter sw1 = new StreamWriter("BoatsInDock1.txt", false, System.Text.Encoding.UTF7);
            SaveToFile(sw1, dock1);
            sw1.Close();

            StreamWriter sw2 = new StreamWriter("BoatsInDock2.txt", false, System.Text.Encoding.UTF7);
            SaveToFile(sw2, dock2);
            sw2.Close();
        }

        private void NextDayButton_Click(object sender, RoutedEventArgs e)
        {
            var fileText = File.ReadLines("BoatsInDock1.txt", System.Text.Encoding.UTF7);

            DockSpace[] dock1 = new DockSpace[dock1Length];
            for (int i = 0; i < dock1.Length; i++)
            {
                dock1[i] = new DockSpace(i);
            }

            AddBoatsFromFileToDock(fileText, dock1);

            fileText = File.ReadLines("BoatsInDock2.txt", System.Text.Encoding.UTF7);

            DockSpace[] dock2 = new DockSpace[dock2Length];
            for (int i = 0; i < dock2.Length; i++)
            {
                dock2[i] = new DockSpace(i);
            }

            AddBoatsFromFileToDock(fileText, dock2);

            List<Boat> boatsInDock1 = GenerateBoatsInHarbourList(dock1);
            List<Boat> boatsInDock2 = GenerateBoatsInHarbourList(dock2);

            AddDayToDaysSinceArrival(boatsInDock1);
            AddDayToDaysSinceArrival(boatsInDock2);

            bool boatRemoved = true;
            while (boatRemoved)
            {
                boatRemoved = RemoveBoats(dock1);
            }

            boatRemoved = true;
            while (boatRemoved)
            {
                boatRemoved = RemoveBoats(dock2);
            }

            int rejectedRowingBoats = 0;
            int rejectedMotorBoats = 0;
            int rejectedSailingBoats = 0;
            int rejectedCatamarans = 0;
            int rejectedCargoShips = 0;

            List<Boat> arrivingBoats = new List<Boat>();
            int NumberOfArrivingBoats = 10;             // Det blir nästan aldrig fullt i hamnen om det kommer 5 båtar, ändrade till 10 för att vis att tabellen för avvisade båtar funkar
            AddNewBoats(arrivingBoats, NumberOfArrivingBoats);

            arrivingBoats.Add(new RowingBoat());

            foreach (Boat boat in arrivingBoats)
            {
                bool boatParked;

                if (boat is RowingBoat)
                {
                    boatParked = RowingBoat.ParkRowingBoatInHarbour(boat, dock1, dock2);
                    if (boatParked == false)
                    {
                        rejectedRowingBoats++;
                    }
                }

                if (boat is MotorBoat)
                {
                    boatParked = MotorBoat.ParkMotorBoatInHarbour(boat, dock1, dock2);
                    if (boatParked == false)
                    {
                        rejectedMotorBoats++;
                    }
                }

                if (boat is SailingBoat)
                {
                    boatParked = SailingBoat.ParkSailingBoatInHarbour(boat, dock1, dock2);
                    if (boatParked == false)
                    {
                        rejectedSailingBoats++;
                    }
                }

                if (boat is Catamaran)
                {
                    boatParked = Catamaran.ParkCatamaranInHarbour(boat, dock1, dock2);
                    if (boatParked == false)
                    {
                        rejectedCatamarans++;
                    }
                }

                if (boat is CargoShip)
                {
                    boatParked = CargoShip.ParkCargoshipInHarbour(boat, dock1, dock2);
                    if (boatParked == false)
                    {
                        rejectedCargoShips++;
                    }
                }
            }

            PrintHarbourTable(dock1, dock2);

            string dockName = "Kaj 1";
            int dockLenght = dock1.Length;
            dock1Canvas.Children.Clear();
            AddSpacesToDockCanvas(dockName, dockLenght, dock1Canvas);
            PlaceBoatsInDockCanvas(dock1, dock1Canvas);

            dockName = "Kaj 2";
            dockLenght = dock2.Length;
            dock2Canvas.Children.Clear();
            AddSpacesToDockCanvas(dockName, dockLenght, dock2Canvas);
            PlaceBoatsInDockCanvas(dock2, dock2Canvas);

            boatsInDock1 = GenerateBoatsInHarbourList(dock1);
            boatsInDock2 = GenerateBoatsInHarbourList(dock2);

            var boatsInBothDocks = boatsInDock1
                .Concat(boatsInDock2);

            if (boatsInBothDocks.Count() > 0)
            {
                int sumOfWeight = GenerateSumOfWeight(boatsInBothDocks);
                double averageSpeed = GenerateAverageSpeed(boatsInBothDocks);
                int availableSpacesDock1 = CountAvailableSpaces(dock1);
                int availableSpacesDock2 = CountAvailableSpaces(dock2);

                summaryListBox.Items.Clear();
                PrintSummaryOfBoats(boatsInBothDocks);
                summaryListBox.Items.Add("\n");
                PrintStatistics(sumOfWeight, averageSpeed, availableSpacesDock1, availableSpacesDock2);
                summaryListBox.Items.Add("");
                PrintRejectedBoats(rejectedRowingBoats, rejectedMotorBoats, rejectedSailingBoats, rejectedCatamarans, rejectedCargoShips);
            }

            StreamWriter sw1 = new StreamWriter("BoatsInDock1.txt", false, System.Text.Encoding.UTF7);
            SaveToFile(sw1, dock1);
            sw1.Close();

            StreamWriter sw2 = new StreamWriter("BoatsInDock2.txt", false, System.Text.Encoding.UTF7);
            SaveToFile(sw2, dock2);
            sw2.Close();
        }

        private void AddBoatsFromFileToDock(IEnumerable<string> fileText, DockSpace[] dock)
        {
            // File:  spaceId;Id;Weight;MaxSpeed;Type;DaysStaying;DaySinceArrival;Special
            // Index: 0       1  2      3        4    5           6               7

            foreach (var line in fileText)
            {
                int index;
                string[] boatData = line.Split(";");

                switch (boatData[4])
                {
                    case "Roddbåt":
                        index = int.Parse(boatData[0]);
                        RowingBoat rowingBoat = new RowingBoat(boatData[4], boatData[1], int.Parse(boatData[2]), int.Parse(boatData[3]),
                            int.Parse(boatData[5]), int.Parse(boatData[6]), int.Parse(boatData[7]));
                        dock[index].ParkedBoats.Add(rowingBoat);
                        break;

                    case "Motorbåt":
                        index = int.Parse(boatData[0]);
                        MotorBoat motorBoat = new MotorBoat(boatData[4], boatData[1], int.Parse(boatData[2]), int.Parse(boatData[3]),
                            int.Parse(boatData[5]), int.Parse(boatData[6]), int.Parse(boatData[7]));
                        dock[index].ParkedBoats.Add(motorBoat);
                        break;

                    case "Segelbåt":
                        index = int.Parse(boatData[0]);
                        if (dock[index].ParkedBoats.Count == 0) // När andra halvan av segelbåten kommmer från foreach är den redan tillagd, se 6 rader ned
                        {
                            SailingBoat sailingBoat = new SailingBoat(boatData[4], boatData[1], int.Parse(boatData[2]), int.Parse(boatData[3]),
                                int.Parse(boatData[5]), int.Parse(boatData[6]), int.Parse(boatData[7]));

                            dock[index].ParkedBoats.Add(sailingBoat);   // samma båt täcker två platser
                            dock[index + 1].ParkedBoats.Add(sailingBoat);
                        }
                        break;

                    case "Katamaran":
                        index = int.Parse(boatData[0]);
                        if (dock[index].ParkedBoats.Count == 0) // När resten av lastfartyget kommmer från foreach är det redan tillagt, se 6 rader ned
                        {
                            Catamaran catamaran = new Catamaran(boatData[4], boatData[1], int.Parse(boatData[2]), int.Parse(boatData[3]),
                            int.Parse(boatData[5]), int.Parse(boatData[6]), int.Parse(boatData[7]));

                            dock[index].ParkedBoats.Add(catamaran);     // samma båt täcker tre platser
                            dock[index + 1].ParkedBoats.Add(catamaran);
                            dock[index + 2].ParkedBoats.Add(catamaran);
                        }
                        break;

                    case "Lastfartyg":
                        index = int.Parse(boatData[0]);
                        if (dock[index].ParkedBoats.Count == 0) // När resten av lastfartyget kommmer från foreach är det redan tillagt, se 6 rader ned
                        {
                            CargoShip cargoship = new CargoShip(boatData[4], boatData[1], int.Parse(boatData[2]), int.Parse(boatData[3]),
                            int.Parse(boatData[5]), int.Parse(boatData[6]), int.Parse(boatData[7]));

                            dock[index].ParkedBoats.Add(cargoship);     // samma båt täcker fyra platser
                            dock[index + 1].ParkedBoats.Add(cargoship);
                            dock[index + 2].ParkedBoats.Add(cargoship);
                            dock[index + 3].ParkedBoats.Add(cargoship);
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private static List<Boat> GenerateBoatsInHarbourList(DockSpace[] dock)
        {
            // Större båtar finns på flera platser i harbour, gör lista med endast en kopia av vardera båt

            var q1 = dock
                .Where(h => h.ParkedBoats.Count != 0);

            List<Boat> allCopies = new List<Boat>();

            foreach (DockSpace space in q1)
            {
                foreach (Boat boat in space.ParkedBoats)
                {
                    allCopies.Add(boat); // Innehåller kopior
                }
            }

            var q2 = allCopies
                .GroupBy(b => b.IdNumber);

            List<Boat> singleBoats = new List<Boat>();

            foreach (var group in q2)
            {
                var q = group
                    .FirstOrDefault();

                singleBoats.Add(q);  // Lista utan kopior
            }

            return singleBoats;
        }

        private static void AddDayToDaysSinceArrival(List<Boat> boats)
        {
            foreach (Boat boat in boats)
            {
                boat.DaysSinceArrival++;
            }
        }

        private static bool RemoveBoats(DockSpace[] dock)
        {
            bool boatRemoved = false;

            foreach (DockSpace space in dock)
            {
                foreach (Boat boat in space.ParkedBoats)
                {
                    if (boat.DaysSinceArrival == boat.DaysStaying)
                    {
                        space.ParkedBoats.Remove(boat);
                        boatRemoved = true;
                        break;
                    }
                }
                if (boatRemoved)
                {
                    break;
                }
            }

            return boatRemoved;
        }

        private static void AddNewBoats(List<Boat> boats, int newBoats)
        {
            for (int i = 0; i < newBoats; i++)
            {
                int boatType = Utils.random.Next(4 + 1);

                switch (boatType)
                {
                    case 0:
                        boats.Add(new RowingBoat());
                        //RowingBoat.AddRowingBoat(boats);
                        break;
                    case 1:
                        boats.Add(new MotorBoat());
                        //MotorBoat.AddMotorBoat(boats);
                        break;
                    case 2:
                        boats.Add(new SailingBoat());
                        //SailingBoat.AddSailingBoat(boats);
                        break;
                    case 3:
                        boats.Add(new Catamaran());
                        //Catamaran.AddCatamaran(boats);
                        break;
                    case 4:
                        boats.Add(new CargoShip());
                        //CargoShip.AddCargoShip(boats);
                        break;
                }
            }
        }

        private void PrintHarbourTable(DockSpace[] dock1, DockSpace[] dock2)
        {
            boatsInHarbourListBox.Items.Clear();
            boatsInHarbourListBox.Items.Add("Båtar i hamn\n--------------");
            boatsInHarbourListBox.Items.Add("");
            boatsInHarbourListBox.Items.Add("Kaj 1");
            List<string> dock1EndOfDayTable = CreateHarbourTable(dock1);
            foreach (var line in dock1EndOfDayTable)
            {
                boatsInHarbourListBox.Items.Add(line);
            }
            boatsInHarbourListBox.Items.Add("");

            boatsInHarbourListBox.Items.Add("Kaj 2");
            List<string> dock2EndOfDayTable = CreateHarbourTable(dock2);
            foreach (var line in dock2EndOfDayTable)
            {
                boatsInHarbourListBox.Items.Add(line);
            }
        }

        private static List<string> CreateHarbourTable(DockSpace[] dock)
        {
            List<string> text = new List<string>();

            text.Add("Båtplats\tBåttyp\t\tID\tVikt\tMaxfart\tÖvrigt\n" +
                      "        \t      \t\t  \t(kg)\t(km/h)\n" +
                      "--------\t----------\t-----\t-----\t-------\t------------------------------");

            foreach (var space in dock)
            {
                if (space.ParkedBoats.Count() == 0)
                {
                    text.Add($"{space.SpaceId + 1}\tLedigt");
                }
                foreach (var boat in space.ParkedBoats)
                {
                    if (space.SpaceId > 0 && dock[space.SpaceId - 1].ParkedBoats.Contains(boat))
                    {
                        // Samma båt som på space innan -> skriv ingenting
                    }

                    else
                    {
                        if (boat is RowingBoat || boat is MotorBoat)
                        {
                            text.Add($"{space.SpaceId + 1}\t{boat}");
                        }
                        else if (boat is SailingBoat)
                        {
                            text.Add($"{space.SpaceId + 1}-{space.SpaceId + 2}\t{boat}");
                        }
                        else if (boat is Catamaran)
                        {
                            text.Add($"{space.SpaceId + 1}-{space.SpaceId + 3}\t{boat}");
                        }
                        else if (boat is CargoShip)
                        {
                            text.Add($"{space.SpaceId + 1}-{space.SpaceId + 4}\t{boat}");
                        }
                    }
                }
            }

            return text;
        }

        private static int GenerateSumOfWeight(IEnumerable<Boat> boatsInHarbour)
        {
            var q = boatsInHarbour
                .Select(b => b.Weight)
                .Sum();

            return q;
        }

        private static double GenerateAverageSpeed(IEnumerable<Boat> boatsInHarbour)
        {
            var q = boatsInHarbour
                //.Select()
                .Average(b => b.MaximumSpeed);

            return q;
        }

        private static int CountAvailableSpaces(DockSpace[] dock)
        {
            var q = dock
                .Where(s => s.ParkedBoats.Count() == 0);

            return q.Count();
        }

        private void PrintSummaryOfBoats(IEnumerable<Boat> boatsInHarbour)
        {
            summaryListBox.Items.Add("Summering av båtar i hamn\n-------------------------------");

            var q = boatsInHarbour
                .GroupBy(b => b.Type)
                .OrderBy(g => g.Key);

            int totalNumberOfBoats = 0;

            foreach (var group in q)
            {
                summaryListBox.Items.Add($"{group.Key}:  \t{group.Count()} st");
                totalNumberOfBoats += group.Count();
            }

            summaryListBox.Items.Add($"Totalt:\t\t{totalNumberOfBoats} st");
        }

        private void PrintStatistics(int sumOfWeight, double averageSpeed, int availableSpacesDock1, int availableSpacesDock2)
        {
            summaryListBox.Items.Add("Statistik\n---------");
            summaryListBox.Items.Add($"Total båtvikt i hamn:\t{sumOfWeight} kg");
            summaryListBox.Items.Add($"Medel av maxhastighet:\t{Math.Round(Utils.ConvertKnotToKmPerHour(averageSpeed), 1)} km/h");
            summaryListBox.Items.Add($"Lediga platser vid kaj 1:\t{availableSpacesDock1} st");
            summaryListBox.Items.Add($"Lediga platser vid kaj 2:\t{availableSpacesDock2} st");
        }

        private void PrintRejectedBoats(int rejectedRowingBoats, int rejectedMotorBoats, int rejectedSailingBoats, int rejectedCatamarans, int rejectedCargoShips)
        {
            summaryListBox.Items.Add("Avvisade båtar idag");
            summaryListBox.Items.Add($"\tRoddbåtar:\t{rejectedRowingBoats} st");
            summaryListBox.Items.Add($"\tMotorbåtar:\t{rejectedMotorBoats} st");
            summaryListBox.Items.Add($"\tSegelbåtar:\t{rejectedSailingBoats} st");
            summaryListBox.Items.Add($"\tKatamaraner:\t{rejectedCatamarans} st");
            summaryListBox.Items.Add($"\tLastfartyg:\t{rejectedCargoShips} st");
            summaryListBox.Items.Add($"\tTotalt:\t\t{rejectedRowingBoats + rejectedMotorBoats + rejectedSailingBoats + rejectedCatamarans + rejectedCargoShips} st");
        }

        private void AddSpacesToDockCanvas(string dockName, int dockLenght, Canvas dockCanvas)
        {
            TextBox dockSign = new TextBox();
            dockSign.Text = dockName;
            dockSign.Width = 40;
            dockSign.Height = 25;
            Canvas.SetLeft(dockSign, 5);

            dockCanvas.Children.Add(dockSign);

            for (int i = 0; i < dockLenght; i++)
            {
                TextBox dockSpace = new TextBox();
                dockSpace.Width = 30;
                dockSpace.Height = 25;
                dockSpace.Text = $"{i + 1}";
                Canvas.SetLeft(dockSpace, 5);
                Canvas.SetTop(dockSpace, 26 + (i * 26));

                dockCanvas.Children.Add(dockSpace);
            }
        }

        private void PlaceBoatsInDockCanvas(DockSpace[] dock, Canvas dockCanvas)
        {
            for (int i = 0; i < dock.Length; i++)
            {
                foreach (Boat boat in dock[i].ParkedBoats)
                {
                    int width, height;

                    if (boat is RowingBoat)
                    {
                        AddRowingBoatsToHarbourCanvas(dock[i], dockCanvas);
                    }

                    else if (boat is MotorBoat)
                    {
                        width = 45;
                        height = 25;
                        dockCanvas.Children.Add(CreateBoatBox(dock[i].SpaceId, boat, width, height));
                    }

                    else if (boat is SailingBoat)
                    {
                        width = 45;
                        height = 51;
                        if (i == 0)
                        {
                            dockCanvas.Children.Add(CreateBoatBox(dock[i].SpaceId, boat, width, height));
                        }
                        else if (dock[i - 1].ParkedBoats.Contains(boat) == false)
                        {
                            dockCanvas.Children.Add(CreateBoatBox(dock[i].SpaceId, boat, width, height));
                        }
                    }
                    else if (boat is Catamaran)
                    {
                        width = 45;
                        height = 77;
                        if (i == 0)
                        {
                            dockCanvas.Children.Add(CreateBoatBox(dock[i].SpaceId, boat, width, height));
                        }
                        else if (dock[i - 1].ParkedBoats.Contains(boat) == false)
                        {
                            dockCanvas.Children.Add(CreateBoatBox(dock[i].SpaceId, boat, width, height));
                        }
                    }
                    else if (boat is CargoShip)
                    {
                        width = 45;
                        height = 103;
                        if (i == 0)
                        {
                            dockCanvas.Children.Add(CreateBoatBox(dock[i].SpaceId, boat, width, height));
                        }
                        else if (dock[i - 1].ParkedBoats.Contains(boat) == false)
                        {
                            dockCanvas.Children.Add(CreateBoatBox(dock[i].SpaceId, boat, width, height));
                        }
                    }
                }
            }
        }

        private void AddRowingBoatsToHarbourCanvas(DockSpace harbourSpace, Canvas dockCanvas)
        {
            int addToTopAlign = 0;
            foreach (var boat in harbourSpace.ParkedBoats)
            {
                TextBox boatBox = new TextBox();

                boatBox.Width = 45;
                boatBox.Height = 12;
                boatBox.Text = boat.IdNumber;
                boatBox.FontSize = 9;
                Canvas.SetLeft(boatBox, 37);
                Canvas.SetTop(boatBox, 26 + addToTopAlign + (harbourSpace.SpaceId * 26));

                dockCanvas.Children.Add(boatBox);
                addToTopAlign += 13;
            }
        }

        private TextBox CreateBoatBox(int dockIndex, Boat boat, int width, int height)
        {
            TextBox boatBox = new TextBox();

            boatBox.Width = width;
            boatBox.Height = height;
            boatBox.Text = boat.IdNumber;
            boatBox.FontSize = 10;
            Canvas.SetLeft(boatBox, 37);
            Canvas.SetTop(boatBox, 26 + (dockIndex * 26));

            return boatBox;
        }

        private static void SaveToFile(StreamWriter sw, DockSpace[] harbour)
        {
            foreach (var space in harbour)
            {
                if (space != null)
                {
                    foreach (Boat boat in space.ParkedBoats)
                    {
                        if (space.ParkedBoats != null)
                        {
                            sw.WriteLine(boat.TextToFile(space.SpaceId), System.Text.Encoding.UTF7);
                        }
                    }
                }
            }

            sw.Close();
        }
    }
}
