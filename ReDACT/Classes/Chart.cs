using ReDACT.Classes.DataStructures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReDACT.Classes
{
    class Chart
    {
        private Canvas targetCanvas;
        private LinkedList<ChartData> Data;
        private volatile bool updatePending = false;
        private double BaseScale = .1;
        private double PointScale = 2;
        private double LineScale = 1;
        private object listlock = new object();
        public int MaxDataPoints = 0;
        private ulong sequenceCount = 0;

        public Chart(Canvas targetCanvas)
        {
            this.targetCanvas = targetCanvas;
            this.Data = new LinkedList<ChartData>();            
        }

        public void AddData(double X, double Y)
        {
            lock (listlock)
            {
                Data.AddLast(new ChartData(X, Y));
            }
            Trim();
            tryUpdate();
        }

        public void AddSequential(double Y)
        {            
            this.AddData(sequenceCount, Y);
            sequenceCount++;
        }

        private void Trim()
        {
            if(MaxDataPoints != 0)
            {
                lock (listlock)
                {
                    while (Data.Count > MaxDataPoints)
                        Data.RemoveFirst();
                }
            }
        }

        public void Clear()
        {
            lock (listlock)
            {
                Data.Clear();
                sequenceCount = 0;
            }
            tryUpdate();
        }

        void tryUpdate()
        {
            if (!updatePending)
            {
                updatePending = true;
                targetCanvas.Dispatcher.Invoke((Action)(() =>
                {
                    updateGraph();
                }));
            } else
            {
                Debug.WriteLine("Unable to update chart, existing update pending");
            }
        }

        void updateGraph()
        {
            lock (listlock)
            {
                targetCanvas.Children.Clear();
                if (Data.Count == 0)
                {
                    updatePending = false;
                    return;
                }

                targetCanvas.Visibility = System.Windows.Visibility.Hidden;

                double yMin = SeriesMinY();
                double yMax = SeriesMaxY();
                double yRange = yMax - yMin;

                double xMin = SeriesMinX();
                double xMax = SeriesMaxX();
                double xRange = xMax - xMin;

                double ElementSizeBase = BaseScale * yMax;

                LinkedListNode<ChartData> CurrentNode = Data.First;

                TextBlock yMinText = new TextBlock();
                yMinText.Foreground = Brushes.White;
                yMinText.Text = yMin.ToString("F4");
                Canvas.SetRight(yMinText, .91 * targetCanvas.Width);
                Canvas.SetBottom(yMinText, .1 * targetCanvas.Height);
                targetCanvas.Children.Add(yMinText);

                TextBlock yMaxText = new TextBlock();
                yMaxText.Foreground = Brushes.White;
                yMaxText.Text = yMax.ToString("F4");
                Canvas.SetRight(yMaxText, .91 * targetCanvas.Width);
                Canvas.SetTop(yMaxText, .1 * targetCanvas.Height);
                targetCanvas.Children.Add(yMaxText);

                TextBlock xMinText = new TextBlock();
                xMinText.Foreground = Brushes.White;
                xMinText.Text = xMin.ToString("F4");
                xMinText.RenderTransform = new RotateTransform(90);
                Canvas.SetLeft(xMinText, .11 * targetCanvas.Width);
                Canvas.SetTop(xMinText, .91 * targetCanvas.Height);
                targetCanvas.Children.Add(xMinText);

                TextBlock xMaxText = new TextBlock();
                xMaxText.Foreground = Brushes.White;
                xMaxText.Text = xMax.ToString("F4");
                xMaxText.RenderTransform = new RotateTransform(90);
                Canvas.SetLeft(xMaxText, .9 * targetCanvas.Width);
                Canvas.SetTop(xMaxText, .91 * targetCanvas.Height);
                targetCanvas.Children.Add(xMaxText);

                Line xAxis = new Line()
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = ElementSizeBase * LineScale,
                    X1 = .1 * targetCanvas.Width,
                    X2 = .9 * targetCanvas.Width,
                    Y1 = .9 * targetCanvas.Height,
                    Y2 = .9 * targetCanvas.Height
                };
                targetCanvas.Children.Add(xAxis);

                Line yAxis = new Line()
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = ElementSizeBase * LineScale,
                    X1 = .1 * targetCanvas.Width,
                    X2 = .1 * targetCanvas.Width,
                    Y1 = .1 * targetCanvas.Height,
                    Y2 = .9 * targetCanvas.Height
                };
                targetCanvas.Children.Add(yAxis);


                while (CurrentNode.Next != null)
                {
                    Ellipse datapoint = new Ellipse()
                    {
                        ToolTip = CurrentNode.Value,
                        Width = ElementSizeBase * PointScale,
                        Height = ElementSizeBase * PointScale,
                        Fill = Brushes.Yellow,
                        Opacity = .8

                    };

                    Canvas.SetLeft(datapoint, Conversion.scale(CurrentNode.Value.X, xMin, xMax, .1 * targetCanvas.Width, .9 * targetCanvas.Width) - (datapoint.Width / 2));
                    Canvas.SetTop(datapoint, Conversion.scale(CurrentNode.Value.Y, yMin, yMax, .9 * targetCanvas.Height, .1 * targetCanvas.Height) - (datapoint.Width / 2));

                    Line dataline = new Line()
                    {
                        ToolTip = String.Format("{0} -> {1}", CurrentNode.Value, CurrentNode.Next.Value),
                        X1 = Conversion.scale(CurrentNode.Value.X, xMin, xMax, .1 * targetCanvas.Width, .9 * targetCanvas.Width),
                        X2 = Conversion.scale(CurrentNode.Next.Value.X, xMin, xMax, .1 * targetCanvas.Width, .9 * targetCanvas.Width),
                        Y1 = Conversion.scale(CurrentNode.Value.Y, yMin, yMax, .9 * targetCanvas.Height, .1 * targetCanvas.Height),
                        Y2 = Conversion.scale(CurrentNode.Next.Value.Y, yMin, yMax, .9 * targetCanvas.Height, .1 * targetCanvas.Height),
                        Stroke = Brushes.LimeGreen,
                        StrokeThickness = ElementSizeBase * LineScale, 
                        Opacity = .8
                    };

                    targetCanvas.Children.Add(dataline);
                    targetCanvas.Children.Add(datapoint);

                    CurrentNode = CurrentNode.Next;
                }
            }
            targetCanvas.Visibility = System.Windows.Visibility.Visible;
            updatePending = false;            
        }

        double SeriesMinY()
        {
            double min = Data.ElementAt(0).Y;
            foreach(ChartData d in Data)
            {
                if (d.Y < min)
                    min = d.Y;
            }
            return min;
        }

        double SeriesMaxY()
        {
            double max = Data.ElementAt(0).Y;
            foreach (ChartData d in Data)
            {
                if (d.Y > max)
                    max = d.Y;
            }
            return max;
        }

        double SeriesMinX()
        {
            double min = Data.ElementAt(0).X;
            foreach (ChartData d in Data)
            {
                if (d.X < min)
                    min = d.X;
            }
            return min;
        }

        double SeriesMaxX()
        {
            double max = Data.ElementAt(0).X;
            foreach (ChartData d in Data)
            {
                if (d.X > max)
                    max = d.X;
            }
            return max;
        }

        private void Data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.updateGraph();
        }
    }

    class ChartData
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public ChartData(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public override string ToString()
        {
            return String.Format("[{0}, {1}]", this.X, this.Y);
        }
    }
}
