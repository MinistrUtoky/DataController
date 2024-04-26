using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using DataGenStatistics.classes;
using DataGenStatistics.investigation;
using ScottPlot;
using ScottPlot.WPF;

namespace DataGenStatistics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            InitializeComponent(); RunAllStatistics();
        }

        public void RunAllStatistics()
        {
            int[] numberOfRows = Enumerable.Range(1, Investigation.maxSteps).Select(a => a * Investigation.numberOfRowsPerStep).ToArray();
            DatabaseSandbox.Instance.Init();
            ProcessTimers.TimerTest();
            Plot(GeneratorTimings, "Time required for generation",
                 numberOfRows, Investigation.PlotGenerators(numberOfRows), "generator_timings.png");
            Plot(SelectQueryTimings, "Time required for selection",
                 numberOfRows, Investigation.PlotSelectQueries(numberOfRows), "selection_timings.png");
            Plot(InsertQueryTimings, "Time required for insertion",
                 numberOfRows, Investigation.PlotInsertQueries(numberOfRows), "insertion_timings.png");
            Plot(RemoveQueryTimings, "Time required for removal",
                 numberOfRows, Investigation.PlotRemoveQueries(numberOfRows), "removal_timings.png");
            Plot(UpdateQueryTimings, "Time required for update",
                 numberOfRows, Investigation.PlotUpdateQueries(numberOfRows), "update_timings.png");
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
