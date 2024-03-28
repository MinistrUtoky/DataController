using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataGenStatistics.classes;
using ScottPlot;


namespace DataGenStatistics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*
        3) Построить график времени выполнения запросов (SELECT, INSERT, …)
           для ваших таблиц в зависимости от количества строк в таблице.
        */
        public MainWindow()
        {
            InitializeComponent();
            DataGenerator.Instance.Init();
            PlotGenerators();
            //ProcessTimers.TimerTest();
            //DataGenerator.Instance.Init();
        }

        private void PlotGenerators()
        {
            int maxSteps = 10,
                numberOfRowsPerStep = 10;
            int[] numberOfRows = Enumerable.Range(1, maxSteps).Select(a =>a * numberOfRowsPerStep).ToArray();

            List<Delegate>  libraryGenerations = new (),
                            userGenerations = new (),
                            playerGenerations = new (),
                            archiveGenerations = new (),
                            serverGenerations = new (),
                            sessionGenerations = new (),
                            lobbyGenerations = new ();
            foreach (int n in numberOfRows)
            {
                libraryGenerations.Add(() => DataGenerator.Instance.GenerateLibraries(n));
                userGenerations.Add(() => DataGenerator.Instance.GenerateUsers(n));
                playerGenerations.Add(() => DataGenerator.Instance.GeneratePlayers(n));
                archiveGenerations.Add(() => DataGenerator.Instance.GenerateArchives(n));
                serverGenerations.Add(() => DataGenerator.Instance.GenerateServers(n));
                sessionGenerations.Add(() => DataGenerator.Instance.GenerateSessions(n));
                lobbyGenerations.Add(() => DataGenerator.Instance.GenerateLobbies(n));
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {"library", ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryGenerations) },
                {"user", ProcessTimers.SeveralProcessesTimeInMilliseconds(userGenerations) },
                {"player", ProcessTimers.SeveralProcessesTimeInMilliseconds(playerGenerations) },
                {"archive", ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveGenerations) },
                {"server", ProcessTimers.SeveralProcessesTimeInMilliseconds(serverGenerations) },
                {"session", ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionGenerations) },
                {"lobby", ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyGenerations) }
            };

            MarkerShape[] markerShapes = Enum.GetValues<MarkerShape>().ToArray();
            LinePattern[] linePatterns = Enum.GetValues<LinePattern>().ToArray();
            GeneratorTimings.Plot.ShowLegend();
            GeneratorTimings.Plot.Axes.Title.Label.Text = "Time required for generation";
            GeneratorTimings.Plot.Axes.Left.Label.Text = "Milliseconds";
            GeneratorTimings.Plot.Axes.Bottom.Label.Text = "Number of rows generated";
            int i = 1;
            foreach (var result in results)
            {
                double[] dataX = numberOfRows.Select(x => (double)x).ToArray();
                double[] dataY = result.Value.Select(x => (double)x).ToArray();
                var pl = GeneratorTimings.Plot.Add.Scatter(dataX, dataY);
                pl.Label = result.Key;
                pl.MarkerStyle.Shape = markerShapes[i];
                pl.LinePattern = linePatterns[((int)i / 2) % 4];
                pl.MarkerStyle.Size = 8;
                pl.LineWidth = 3;
                i+=2;
            }
            GeneratorTimings.Plot.Axes.AutoScale();
            GeneratorTimings.Refresh();
            GeneratorTimings.Plot.SavePng(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + @"\data\generator_timings.png", 960, 600);
        }
        private void PlotQueries()
        {
        }
    }
}
