/*
 * Prikazuje graficku istoriju selektovanog analognog ulaznog taga.
 * Crta istorijske vrednosti i alarmne granice i izracunava
 * minimalnu, maksimalnu i prosecnu vrednost signala.
 */

using DataConcentrator;
using DataConcentrator.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ScadaGUI
{
    public partial class HistoryWindow : Window
    {
        private readonly AnalogInput tag;
        private readonly DataConcentratorManager manager;

        private readonly DispatcherTimer refreshTimer;

        public HistoryWindow(AnalogInput selectedTag)
        {
            InitializeComponent();

            tag = selectedTag;
            manager = DataConcentratorManager.Instance;

            titleTextBlock.Text =
                "History - " + tag.Name;

            DrawHistory();

            // Periodicno osvezavanje samo history prozora.
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(1);
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }

        private void DrawHistory()
        {
            historyCanvas.Children.Clear();

            List<AnalogValueRecord> history =
                manager.GetAnalogHistory(tag.Name);

            if (history.Count == 0)
            {
                minTextBlock.Text = "-";
                maxTextBlock.Text = "-";
                averageTextBlock.Text = "-";

                TextBlock emptyText = new TextBlock
                {
                    Text = "No history data available.",
                    FontSize = 16
                };

                Canvas.SetLeft(emptyText, 20);
                Canvas.SetTop(emptyText, 20);

                historyCanvas.Children.Add(emptyText);

                return;
            }

            minTextBlock.Text =
                history.Min(record => record.Value).ToString("F2");

            maxTextBlock.Text =
                history.Max(record => record.Value).ToString("F2");

            averageTextBlock.Text =
                history.Average(record => record.Value).ToString("F2");

            // Prikazujemo poslednjih 100 uzoraka radi citljivosti.
            List<AnalogValueRecord> displayedHistory =
                history.Skip(Math.Max(0, history.Count - 100))
                       .ToList();

            List<Alarm> alarms =
                manager.GetAlarmsForTag(tag.Name);

            double minValue =
                displayedHistory.Min(record => record.Value);

            double maxValue =
                displayedHistory.Max(record => record.Value);

            if (alarms.Count > 0)
            {
                minValue = Math.Min(
                    minValue,
                    alarms.Min(alarm => alarm.Limit));

                maxValue = Math.Max(
                    maxValue,
                    alarms.Max(alarm => alarm.Limit));
            }

            if (Math.Abs(maxValue - minValue) < 0.001)
            {
                minValue -= 1;
                maxValue += 1;
            }

            double width = historyCanvas.ActualWidth;
            double height = historyCanvas.ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            DrawAlarmLines(
                alarms,
                minValue,
                maxValue,
                width,
                height);

            Polyline signalLine = new Polyline
            {
                Stroke = Brushes.SteelBlue,
                StrokeThickness = 2
            };

            for (int i = 0; i < displayedHistory.Count; i++)
            {
                double x;

                if (displayedHistory.Count == 1)
                {
                    x = width / 2;
                }
                else
                {
                    x = i * width /
                        (displayedHistory.Count - 1);
                }

                double normalized =
                    (displayedHistory[i].Value - minValue) /
                    (maxValue - minValue);

                double y =
                    height - normalized * height;

                signalLine.Points.Add(
                    new Point(x, y));
            }

            historyCanvas.Children.Add(signalLine);
        }

        private void DrawAlarmLines(
            List<Alarm> alarms,
            double minValue,
            double maxValue,
            double width,
            double height)
        {
            foreach (Alarm alarm in alarms)
            {
                double normalized =
                    (alarm.Limit - minValue) /
                    (maxValue - minValue);

                double y =
                    height - normalized * height;

                Line alarmLine = new Line
                {
                    X1 = 0,
                    X2 = width,
                    Y1 = y,
                    Y2 = y,
                    Stroke = Brushes.Red,
                    StrokeThickness = 1,
                    StrokeDashArray =
                        new DoubleCollection { 5, 3 }
                };

                historyCanvas.Children.Add(alarmLine);

                TextBlock alarmLabel = new TextBlock
                {
                    Text =
                        alarm.Condition + " " +
                        alarm.Limit.ToString("F2"),

                    Foreground = Brushes.Red
                };

                Canvas.SetLeft(alarmLabel, 5);
                Canvas.SetTop(
                    alarmLabel,
                    Math.Max(0, y - 20));

                historyCanvas.Children.Add(alarmLabel);
            }
        }

        private void RefreshTimer_Tick(
            object sender,
            EventArgs e)
        {
            DrawHistory();
        }

        private void HistoryCanvas_SizeChanged(
            object sender,
            SizeChangedEventArgs e)
        {
            DrawHistory();
        }

        private void Refresh_Click(
            object sender,
            RoutedEventArgs e)
        {
            DrawHistory();
        }

        private void Close_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            refreshTimer?.Stop();

            base.OnClosed(e);
        }
    }
}