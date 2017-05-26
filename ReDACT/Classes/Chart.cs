using ReDACT.Classes.DataStructures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private double PointScale = 1.5;
        private double LineScale = 1;

        public Chart(Canvas targetCanvas)
        {
            this.targetCanvas = targetCanvas;
            this.Data = new LinkedList<ChartData>();            
        }

        public void AddData(double X, double Y)
        {
            Data.AddLast(new ChartData(X, Y));
            tryUpdate();
        }

        public void AddSequential(double Y)
        {
            Data.AddLast(new ChartData(Data.Count, Y));
            tryUpdate();
        }

        public void Clear()
        {
            Data.Clear();
            tryUpdate();
        }

        void tryUpdate()
        {
            if (!updatePending)
                targetCanvas.Dispatcher.Invoke((Action)(() => {
                    updateGraph();
                }));
        }

        void updateGraph()
        {
            targetCanvas.Children.Clear();
            if (Data.Count == 0)
                return;

            double yMin = SeriesMinY();
            double yMax = SeriesMaxY();
            double yRange = yMax - yMin;
            double yPadding = .2 * yRange;

            yMin -= yPadding;
            yMax += yPadding;

            double xMin = SeriesMinX();
            double xMax = SeriesMaxX();
            double xRange = xMax - xMin;
            double xPadding = .2 * xRange;

            xMin -= xPadding;
            xMax += xPadding;

            double ElementSizeBase = BaseScale * targetCanvas.Height;

            LinkedListNode<ChartData> CurrentNode = Data.First;

            while(CurrentNode.Next != null)
            {
                Ellipse datapoint = new Ellipse()
                {
                    ToolTip = CurrentNode.Value,
                    Width = ElementSizeBase * PointScale,
                    Height = ElementSizeBase * PointScale,
                    Fill = Brushes.Green
                };
                
                Canvas.SetRight(datapoint, Conversion.scale(CurrentNode.Value.X, xMin, xMax, 0, targetCanvas.Width) + (datapoint.Width/2));
                Canvas.SetBottom(datapoint, Conversion.scale(CurrentNode.Value.Y, yMin, yMax, targetCanvas.Height, 0) + (datapoint.Width / 2));

                Line dataline = new Line()
                {
                    ToolTip = String.Format("{0} -> {1}", CurrentNode.Value, CurrentNode.Next.Value),
                    X1 = Conversion.scale(CurrentNode.Value.X, xMin, xMax, 0, targetCanvas.Width),
                    X2 = Conversion.scale(CurrentNode.Next.Value.X, xMin, xMax, 0, targetCanvas.Width),
                    Y1 = Conversion.scale(CurrentNode.Value.Y, yMin, yMax, targetCanvas.Height, 0),
                    Y2 = Conversion.scale(CurrentNode.Next.Value.Y, yMin, yMax, targetCanvas.Height, 0),
                    Stroke = Brushes.LimeGreen,
                    StrokeThickness = ElementSizeBase * LineScale
                };
                
                targetCanvas.Children.Add(dataline);
                targetCanvas.Children.Add(datapoint);
            }
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
