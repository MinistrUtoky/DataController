using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using DataGenStatistics.classes;
using DataGenStatistics.investigation;
using DataGenStatistics.test;
using ScottPlot;
using ScottPlot.WPF;

///<summary>
/// Head file of a WPF application to show investigation plots
/// </summary>
namespace DataGenStatistics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Constructor for the main window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DatabaseSandbox.Instance.Init();
            SomeAutotests.RunAllTests(); 
            //RunAllStatistics();
        }

        /// <summary>
        /// Starts investigation plotting Int
        /// </summary>
        public void RunAllStatistics()
        {
            int[] numberOfRows = Enumerable.Range(1, Investigation.maxSteps).Select(a => a * Investigation.numberOfRowsPerStep).ToArray();
            Plot(GeneratorTimings, "Time required for generation",
            numberOfRows, Investigation.PlotGenerators(numberOfRows), "generator_timings.png");

            int oddOrEvenIdSelection = new Random().Next(0, 2);
            int randomEndingIdSelection = new Random().Next(0, 10);
            Plot(SelectQueryTimings, "Time required for selection",
                 numberOfRows, Investigation.PlotSelectQueries(numberOfRows, ""), "selection_timings.png");
            Plot(FunnySelectQueryTimings, "Time reqired for selection where id%2 = " + oddOrEvenIdSelection,
                numberOfRows, Investigation.PlotSelectQueries(numberOfRows, "where id%2 = " + oddOrEvenIdSelection), "selection_oddeven_id_timings.png");
            Plot(FunnierSelectQueryTimings, "Time reqired for selection where id ends with " + randomEndingIdSelection,
                numberOfRows, Investigation.PlotSelectQueries(numberOfRows, "WHERE id LIKE '_" + randomEndingIdSelection + "'"), "selection_id_ends_with_randnum_timings.png");

            int oddOrEvenIdSecondSelection = new Random().Next(0, 2);
            int randomDigitInIdSelection = new Random().Next(0, 10);
            Plot(MegafunnySelectQueryTimings, "Time reqired for selection where id % 2 = " + oddOrEvenIdSecondSelection + " and id contains " + randomDigitInIdSelection,
                 numberOfRows, Investigation.PlotSelectQueries(numberOfRows, "WHERE id%2=" + oddOrEvenIdSecondSelection + " AND id LIKE '%" + randomDigitInIdSelection + "%'"), "selection_oddeven_id_containing_smth.png");

            int oddOrEvenIdInsertion = new Random().Next(0, 2);
            int randomEndingIdInsertion = new Random().Next(0, 10);
            Plot(InsertQueryTimings, "Time required for insertion",
                 numberOfRows, Investigation.PlotInsertQueries(numberOfRows), "insertion_timings2.png");
            Plot(FunnyInsertQueryTimings, "Time required for insertion of row duplicates where id is " + (oddOrEvenIdInsertion==0? "even" : "odd"),
                 numberOfRows, Investigation.PlotInsertQueries(numberOfRows, true, "WHERE id%2=" + oddOrEvenIdInsertion), "oddeven_id_copies_insertion_timing2.png");
            Plot(FunnierInsertQueryTimings, "Time required for insertion of row duplicates from same table with ids ending with " + randomEndingIdInsertion,
                 numberOfRows, Investigation.PlotInsertQueries(numberOfRows, true, "WHERE id%10=" + randomEndingIdInsertion), "random_ending_id_cpies_insertion_timings2.png");
            
            int oddOrEvenIdRemoval = new Random().Next(0, 2);
            int randomEndingIdRemoval = new Random().Next(0, 10);
            Plot(RemoveQueryTimings, "Time required for removal",
                 numberOfRows, Investigation.PlotRemoveQueries(numberOfRows, ""), "removal_timings.png");
            Plot(FunnyRemoveQueryTimings, "Time required for removal where id%2=" + oddOrEvenIdRemoval,
                 numberOfRows, Investigation.PlotRemoveQueries(numberOfRows, "id%2=" + oddOrEvenIdRemoval), "removal_oddeven_id_timings.png");
            Plot(FunnierRemoveQueryTimings, "Time required for removal where id ends with " + randomEndingIdRemoval,
                 numberOfRows, Investigation.PlotRemoveQueries(numberOfRows, "id LIKE '_" + randomEndingIdRemoval + "'"), "removal_id_ends_with_randnum_timings.png");


            /*Plot(UpdateQueryTimings, "Time required for update",
                numberOfRows, Investigation.PlotUpdateQueries(numberOfRows), "update_timings.png"); */
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
                if (results.Count > 9)
                {
                    pl.MarkerStyle.Shape = markerShapes[i];
                    pl.MarkerStyle.Size = 8;
                }
                else
                    pl.MarkerStyle = MarkerStyle.None;
                pl.LinePattern = linePatterns[((int)i / 2) % 4];
                pl.LineWidth = 3;
                i += 2;
            }
            plot.Plot.Axes.AutoScale();
            plot.Refresh();
            plot.Plot.SavePng(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + @"\data\" + saveFile, 960, 600);
        }
    }
}
