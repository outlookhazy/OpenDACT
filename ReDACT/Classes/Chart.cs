using MathNet.Numerics;
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
        private double BaseScale = .01;
        private double PointScale = 1.5;
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
            Update();
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
            Update();
        }

        public void Update()
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
                if (Data.Count < 2)
                {
                    TextBlock waiting = new TextBlock();
                    waiting.Foreground = Brushes.White;
                    waiting.Text = "2 data points required for graph.";
                    waiting.FontSize = 40;
                    Canvas.SetBottom(waiting, 0);
                    Canvas.SetRight(waiting, 0);
                    targetCanvas.Children.Add(waiting);

                    updatePending = false;
                    return;
                }

                targetCanvas.Visibility = System.Windows.Visibility.Hidden;

                double yMin = SeriesMinY();
                double yMax = SeriesMaxY();

                double xMin = SeriesMinX();
                double xMax = SeriesMaxX();

                double ElementSizeBase = BaseScale * Math.Min(targetCanvas.Width, targetCanvas.Height);

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

                Line xAxis = new Line();

                xAxis.Stroke = Brushes.Red;
                xAxis.StrokeThickness = ElementSizeBase * LineScale;
                xAxis.X1 = .1 * targetCanvas.Width;
                xAxis.X2 = .9 * targetCanvas.Width;
                xAxis.Y1 = .9 * targetCanvas.Height;
                xAxis.Y2 = .9 * targetCanvas.Height;
                xAxis.StrokeStartLineCap = PenLineCap.Round;
                xAxis.StrokeEndLineCap = PenLineCap.Round;

                targetCanvas.Children.Add(xAxis);

                Line yAxis = new Line();
                yAxis.Stroke = Brushes.Red;
                yAxis.StrokeThickness = ElementSizeBase * LineScale;
                yAxis.X1 = .1 * targetCanvas.Width;
                yAxis.X2 = .1 * targetCanvas.Width;
                yAxis.Y1 = .1 * targetCanvas.Height;
                yAxis.Y2 = .9 * targetCanvas.Height;
                yAxis.StrokeStartLineCap = PenLineCap.Round;
                yAxis.StrokeEndLineCap = PenLineCap.Round;

                targetCanvas.Children.Add(yAxis);


                Line trendline = new Line();
                trendline.Opacity = .75;
                trendline.StrokeDashArray = new DoubleCollection(new double[] { 1, 1 });
                trendline.Stroke = Brushes.Orange;
                trendline.StrokeThickness = ElementSizeBase * LineScale*.75;
                trendline.X1 = .1 * targetCanvas.Width;
                trendline.X2 = .9 * targetCanvas.Width;
                //trendline.Y1 = .9 * targetCanvas.Height;
                //trendline.Y2 = .9 * targetCanvas.Height;
                trendline.StrokeStartLineCap = PenLineCap.Round;
                trendline.StrokeEndLineCap = PenLineCap.Round;


                


                Polyline p = new Polyline()
                {
                    Stroke = Brushes.LimeGreen,
                    StrokeThickness = ElementSizeBase * LineScale,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    Opacity = .8,
                    StrokeLineJoin = PenLineJoin.Round
                };

                List<Ellipse> elist = new List<Ellipse>();

                double[] trendDataX = new double[Data.Count];
                double[] trendDataY = new double[Data.Count];

                for(int i=0; i< Data.Count; i++)
                {
                    ChartData node = Data.ElementAt(i);
                    trendDataX[i] = node.X;
                    trendDataY[i] = node.Y;

                    p.Points.Add(new System.Windows.Point(
                        Conversion.scale(node.X, xMin, xMax, .1 * targetCanvas.Width, .9 * targetCanvas.Width),
                        Conversion.scale(node.Y, yMin, yMax, .9 * targetCanvas.Height, .1 * targetCanvas.Height)));

                    Ellipse pe = new Ellipse()
                    {
                        ToolTip = node,
                        Width = ElementSizeBase * PointScale,
                        Height = ElementSizeBase * PointScale,
                        Fill = Brushes.Yellow,
                        Opacity = .8
                    };

                    Canvas.SetLeft(pe, Conversion.scale(node.X, xMin, xMax, .1 * targetCanvas.Width, .9 * targetCanvas.Width) - (pe.Width / 2));
                    Canvas.SetTop(pe, Conversion.scale(node.Y, yMin, yMax, .9 * targetCanvas.Height, .1 * targetCanvas.Height) - (pe.Width / 2));
                    elist.Add(pe);
                }



                Tuple<double, double> trend = Fit.Line(trendDataX,trendDataY);
                double y1 = trend.Item1 + (trend.Item2 * trendDataX[0]);
                double y2 = trend.Item1 + (trend.Item2 * trendDataX[Data.Count - 1]);
                trendline.Y1 = Conversion.scale(y1, yMin, yMax, .9 * targetCanvas.Height, .1 * targetCanvas.Height);
                trendline.Y2 = Conversion.scale(y2, yMin, yMax, .9 * targetCanvas.Height, .1 * targetCanvas.Height);

                TextBlock slopeText = new TextBlock();
                slopeText.Foreground = trendline.Stroke;
                slopeText.Text = trend.Item2.ToString("F4");
                Canvas.SetLeft(slopeText, .9 * targetCanvas.Width + ElementSizeBase);
                Canvas.SetTop(slopeText, Conversion.scale(y2, yMin, yMax, .9 * targetCanvas.Height, .1 * targetCanvas.Height) - ElementSizeBase);      

                targetCanvas.Children.Add(trendline);
                targetCanvas.Children.Add(slopeText);

                targetCanvas.Children.Add(p);
                foreach (Ellipse e in elist)
                    targetCanvas.Children.Add(e);


                /*
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
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round,
                        Opacity = .8
                    };
                    
                    targetCanvas.Children.Add(dataline);
                    targetCanvas.Children.Add(datapoint);

                    CurrentNode = CurrentNode.Next;
                }*/
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
