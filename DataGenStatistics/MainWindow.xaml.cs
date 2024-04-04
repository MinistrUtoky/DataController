using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DataGenStatistics.classes;
using ScottPlot;
using ScottPlot.WPF;

namespace DataGenStatistics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int maxSteps = 10,
            numberOfRowsPerStep = 2;
        /*
         * Написать бы для REMOVE и UPDATE прокси функции в генераторе для контроля ID в списках мастеров (в реальной дб не понадобилось бы в виду триггеров
         * Пофиксить что при одновременной генерации таблицы мастера и зависимой таблицы мастеру добавляется в список зависимых 1 вместо id
         * Сделать так чтобы UPDATE и REMOVE имели генераторы снаружи
         * Предположительно все вышеперечисленные проблемы, как и странные рывки графиков времени предположительно возникают из-за асинхронности потоков
        */
        /// <summary>
        /// Constructor for the main window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DatabaseSandbox.Instance.Init();
            ProcessTimers.TimerTest();
            PlotGenerators();
            //PlotSelectQueries();
            //PlotInsertQueries();
            //PlotRemoveQueries();
            //PlotUpdateQueries();
        }
        /// <summary>
        /// Calculates generation time result data for globally given numbers of rows
        /// Plots with Plot()
        /// </summary>
        private void PlotGenerators()
        {
            int[] numberOfRows = Enumerable.Range(1, maxSteps).Select(a => a * numberOfRowsPerStep).ToArray();

            List<Func<Task>>  libraryDelegates = new (),
                            userDelegates = new (),
                            playerDelegates = new (),
                            archiveDelegates = new (),
                            serverDelegates = new (),
                            sessionDelegates = new (),
                            lobbyDelegates = new ();
            foreach (int n in numberOfRows)
            {
                libraryDelegates.Add(async () => DatabaseSandbox.Instance.GenerateLibraries(n));
                userDelegates.Add(async () => DatabaseSandbox.Instance.GenerateUsers(n));
                playerDelegates.Add(async () => DatabaseSandbox.Instance.GeneratePlayers(n));
                archiveDelegates.Add(async () => DatabaseSandbox.Instance.GenerateArchives(n));
                serverDelegates.Add(async () => DatabaseSandbox.Instance.GenerateServers(n));
                sessionDelegates.Add(async () => DatabaseSandbox.Instance.GenerateSessions(n));
                lobbyDelegates.Add(async () => DatabaseSandbox.Instance.GenerateLobbies(n));
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {"library", ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {"user", ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {"player", ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {"archive", ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {"server", ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {"session", ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {"lobby", ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };

            Plot(GeneratorTimings, "Time required for generation",
                 numberOfRows, results, "generator_timings.png");
        }

        /// <summary>
        /// Calculates selection time result data for globally given numbers of rows
        /// Plots with Plot()
        /// </summary>
        private void PlotSelectQueries()
        {
            int[] numberOfRows = Enumerable.Range(1, maxSteps).Select(a => a * numberOfRowsPerStep).ToArray();
            List<Func<Task>> libraryDelegates = new(),
                            userDelegates = new(),
                            playerDelegates = new(),
                            archiveDelegates = new(),
                            serverDelegates = new(),
                            sessionDelegates = new(),
                            lobbyDelegates = new();
            foreach (int n in numberOfRows)
            {
                libraryDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [library]");
                });
                userDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [user]");
                });
                playerDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [player]");
                });
                archiveDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [archive]");
                });
                serverDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [dedicated_server]");
                });
                sessionDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [session]");
                });
                lobbyDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [lobby]");
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {"library", ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {"user", ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {"player", ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {"archive", ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {"server", ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {"session", ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {"lobby", ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            Plot(SelectQueryTimings, "Time required for selection",
                 numberOfRows, results, "selection_timings.png");
        }

        /// <summary>
        /// Calculates insertion time result data for globally given numbers of rows
        /// Plots with Plot()
        /// </summary>
        private void PlotInsertQueries()
        {
            int[] numberOfRows = Enumerable.Range(1, maxSteps).Select(a => a * numberOfRowsPerStep).ToArray();

            List<Func<Task>> libraryDelegates = new(),
                            userDelegates = new(),
                            playerDelegates = new(),
                            archiveDelegates = new(),
                            serverDelegates = new(),  
                            sessionDelegates = new(),
                            lobbyDelegates = new();
            int totalDataAmount = numberOfRows.Sum();
            DatabaseSandbox.Instance.GenerateAdditionalData(totalDataAmount, totalDataAmount, totalDataAmount, totalDataAmount,
                                                          totalDataAmount, totalDataAmount, totalDataAmount);
            foreach (int n in numberOfRows)
            {
                libraryDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(n, 0, 0, 0, 0, 0, 0);
                });
                userDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(0, n, 0, 0, 0, 0, 0);
                });
                playerDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(0, 0, n, 0, 0, 0, 0);
                });
                archiveDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(0, 0, 0, n, 0, 0, 0);
                });
                serverDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(0, 0, 0, 0, n, 0, 0);
                });
                sessionDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(0, 0, 0, 0, 0, n, 0);
                });
                lobbyDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(0, 0, 0, 0, 0, 0, n);
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {"library", ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {"user", ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {"player", ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {"archive", ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {"server", ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {"session", ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {"lobby", ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            Plot(InsertQueryTimings, "Time required for insertion",
                 numberOfRows, results, "insertion_timings.png");
        }

        /// <summary>
        /// Calculates removal time result data for globally given numbers of rows
        /// Plots with Plot()
        /// </summary>
        private void PlotRemoveQueries()
        {
            int[] numberOfRows = Enumerable.Range(1, maxSteps).Select(a => a * numberOfRowsPerStep).ToArray();

            List<Func<Task>> libraryDelegates = new(),
                            userDelegates = new(),
                            playerDelegates = new(),
                            archiveDelegates = new(),
                            serverDelegates = new(),
                            sessionDelegates = new(),
                            lobbyDelegates = new();
            List<int> ids = new();
            foreach (int n in numberOfRows)
            {                
                libraryDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomLibraryIds(n).ToList().ForEach(id => DBClass.DBRemove("library", id));
                });
                
                userDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomUserIds(n).ToList().ForEach(id => DBClass.DBRemove("user", id));
                });
                
                playerDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomPlayerIds(n).ToList().ForEach(id => DBClass.DBRemove("player", id));
                });
                
                archiveDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomArchiveIds(n).ToList().ForEach(id => DBClass.DBRemove("archive", id));
                });
                
                serverDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomServerIds(n).ToList().ForEach(id => DBClass.DBRemove("dedicated_server", id));
                });
                
                sessionDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomSessionIds(n).ToList().ForEach(id => DBClass.DBRemove("session", id));
                });
                
                lobbyDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomLobbyIds(n).ToList().ForEach(id => DBClass.DBRemove("lobby", id));
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {"library", ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {"user", ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {"player", ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {"archive", ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {"server", ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {"session", ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {"lobby", ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            Plot(RemoveQueryTimings, "Time required for removal",
                 numberOfRows, results, "removal_timings.png");
        }

        /// <summary>
        /// Calculates update time result data for globally given numbers of rows
        /// Plots with Plot()
        /// </summary>
        private void PlotUpdateQueries()
        {
            int[] numberOfRows = Enumerable.Range(1, maxSteps).Select(a => a * numberOfRowsPerStep).ToArray();

            List<Func<Task>> libraryDelegates = new(),
                            userDelegates = new(),
                            playerDelegates = new(),
                            archiveDelegates = new(),
                            serverDelegates = new(),
                            sessionDelegates = new(),
                            lobbyDelegates = new();
            List<int> ids = new();
            List<Data> newValues = new();
            int totalDataAmount = numberOfRows.Sum();
            //DatabaseSandbox.Instance.GenerateAdditionalData(totalDataAmount, totalDataAmount, totalDataAmount, totalDataAmount, totalDataAmount, totalDataAmount, totalDataAmount);
            foreach (int n in numberOfRows)
            {
                libraryDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("library", DatabaseSandbox.Instance.GenerateLibraries(n).Cast<Data>().ToList(),
                                            DatabaseSandbox.Instance.GetRandomLibraryIds(n).ToList());
                });
                userDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("user", DatabaseSandbox.Instance.GenerateUsers(n).Cast<Data>().ToList(),
                                            DatabaseSandbox.Instance.GetRandomUserIds(n).ToList());
                });
                playerDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("player", DatabaseSandbox.Instance.GeneratePlayers(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomPlayerIds(n).ToList());
                });
                archiveDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("archive", DatabaseSandbox.Instance.GenerateArchives(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomArchiveIds(n).ToList());
                });
                serverDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("dedicated_server", DatabaseSandbox.Instance.GenerateServers(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomServerIds(n).ToList());
                });
                sessionDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("session", DatabaseSandbox.Instance.GenerateSessions(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomSessionIds(n).ToList());
                });
                lobbyDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("lobby", DatabaseSandbox.Instance.GenerateLobbies(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomLobbyIds(n).ToList());
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {"library", ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {"user", ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {"player", ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {"archive", ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {"server", ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {"session", ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {"lobby", ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            //DatabaseSandbox.Instance.DumpDelta();
            Plot(UpdateQueryTimings, "Time required for update",
                 numberOfRows, results, "update_timings.png");
        }

        /// <summary>
        /// General plotting method
        /// </summary>
        /// <param name="plot">
        /// WpfPlot to draw on
        /// </param>
        /// <param name="plotName">
        /// Title of the plot shown on top of WpfPlot widget
        /// </param>
        /// <param name="numberOfRows">
        /// Array of row counts for each test sequence
        /// </param>
        /// <param name="results">
        /// Test sequences results
        /// </param> 
        /// <param name="saveFile">
        /// File to save plot image to
        /// </param>
        private void Plot(WpfPlot plot, string plotName,
                          int[] numberOfRows, Dictionary<string, List<long>> results,
                          string saveFile)
        {
            MarkerShape[] markerShapes = Enum.GetValues<MarkerShape>().ToArray();
            LinePattern[] linePatterns = Enum.GetValues<LinePattern>().ToArray();
            plot.Plot.ShowLegend();
            plot.Plot.Axes.Title.Label.Text = plotName;
            plot.Plot.Axes.Left.Label.Text = "Milliseconds";
            plot.Plot.Axes.Bottom.Label.Text = "Number of rows";
            int i = 1;
            foreach (var result in results)
            {
                double[] dataX = numberOfRows.Select(x => (double)x).ToArray();
                double[] dataY = result.Value.Select(x => (double)x).ToArray();
                var pl = plot.Plot.Add.Scatter(dataX, dataY);
                pl.Label = result.Key;
                pl.MarkerStyle.Shape = markerShapes[i];
                pl.LinePattern = linePatterns[((int)i / 2) % 4];
                pl.MarkerStyle.Size = 8;
                pl.LineWidth = 3;
                i += 2;
            }
            plot.Plot.Axes.AutoScale();
            plot.Refresh();
            plot.Plot.SavePng(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + @"\data\" + saveFile, 960, 600);
        }
    }
}
