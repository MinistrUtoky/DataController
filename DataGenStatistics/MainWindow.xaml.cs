using System;
using System.Collections.Generic;
using System.Diagnostics;
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


namespace DataGenStatistics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*
        2) Построить график времени генерации данных для ваших таблиц в
           зависимости от количества строк в таблице.
        3) Построить график времени выполнения запросов (SELECT, INSERT, …)
           для ваших таблиц в зависимости от количества строк в таблице.
        */
        public MainWindow()
        {
            InitializeComponent();
            Plot();
            //ProcessTimers.TimerTest();
            //DataGenerator.Instance.Init();
        }

        private void Plot()
        {
            DataGenerator.Instance.Init();
            List<string> tableNames = DBClass.GetTableNames();
            int maxSteps = 10;
            int numberOfRowsPerStep = 10;
            int[] numberOfRows = Enumerable.Range(1, maxSteps + 1).Select(a =>a * numberOfRowsPerStep).ToArray();
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
            List<List<long>> results = new List<List<long>>
            {
                ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryGenerations),
                ProcessTimers.SeveralProcessesTimeInMilliseconds(userGenerations),
                ProcessTimers.SeveralProcessesTimeInMilliseconds(playerGenerations),
                ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveGenerations),
                ProcessTimers.SeveralProcessesTimeInMilliseconds(serverGenerations),
                ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionGenerations),
                ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyGenerations),
            };
            foreach (var result in results)
                Loaded += (s, e) =>
                {
                    double[] dataX = numberOfRows.Select(x => (double)x).ToArray();
                    double[] dataY = result.Select(x => (double)x).ToArray();
                    WpfPlot1.Plot.Add.Scatter(dataX, dataY);
                    WpfPlot1.Plot.Axes.AutoScale();
                    WpfPlot1.Refresh();
                };
        }
    }
}
