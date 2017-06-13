using MathNet.Numerics;
using ReDACT.Classes.Data;
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
        public DataSet Data {get; private set;}

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
            this.Data = new DataSet();
            this.Data.onDataSetModified += Data_onDataSetModified;
        }

        private void Data_onDataSetModified(DataSet Origin)
        {
            this.Update();
        }

        public void AddData(double X, double Y)
        {
            lock (listlock)
            {
                Data.Add(new DataSetElement(X, Y));
            }
            Trim();
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
                        Data.RemoveAt(0);
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
                    TextBlock waiting = new TextBlock()
                    {
                        Foreground = Brushes.White,
                        Text = "2 data points required for graph.",
                        FontSize = 40
                    };
                    Canvas.SetBottom(waiting, 0);
                    Canvas.SetRight(waiting, 0);
                    targetCanvas.Children.Add(waiting);

                    updatePending = false;
                    return;
                }

                targetCanvas.Visibility = System.Windows.Visibility.Hidden;

                double ElementSizeBase = BaseScale * Math.Min(targetCanvas.Width, targetCanvas.Height);

                double graphXmin = .1 * targetCanvas.Width;
                double graphXmax = .9 * targetCanvas.Width;

                double graphYmin = .9 * targetCanvas.Height;
                double graphYmax = .1 * targetCanvas.Height;

                TextBlock yMinText = new TextBlock()
                {
                    Foreground = Brushes.White,
                    Text = Data.YMIN.ToString("F4")
                };
                yMinText.Margin = new System.Windows.Thickness(0, 0, ElementSizeBase, 0);
                yMinText.RenderTransformOrigin = new System.Windows.Point(1, 1);
                Canvas.SetRight(yMinText, graphXmax);
                Canvas.SetBottom(yMinText, graphYmax - ElementSizeBase);
                targetCanvas.Children.Add(yMinText);

                TextBlock yMaxText = new TextBlock()
                {
                    Foreground = Brushes.White,
                    Text = Data.YMAX.ToString("F4")
                };
                yMaxText.Margin = new System.Windows.Thickness(0,0,ElementSizeBase,0);
                yMaxText.RenderTransformOrigin = new System.Windows.Point(1, 0);
                Canvas.SetRight(yMaxText, graphXmax);
                Canvas.SetTop(yMaxText, graphYmax - ElementSizeBase);
                targetCanvas.Children.Add(yMaxText);

                TextBlock xMinText = new TextBlock()
                {
                    Foreground = Brushes.White,
                    Text = Data.XMIN.ToString("F4"),
                    RenderTransform = new RotateTransform(90)
                };
                xMinText.RenderTransformOrigin = new System.Windows.Point(0, 1);
                Canvas.SetLeft(xMinText, graphXmin - ElementSizeBase);
                Canvas.SetTop(xMinText, graphYmin);
                targetCanvas.Children.Add(xMinText);

                TextBlock xMaxText = new TextBlock()
                {
                    Foreground = Brushes.White,
                    Text = Data.XMAX.ToString("F4"),
                    RenderTransform = new RotateTransform(90)
                };
                xMaxText.RenderTransformOrigin = new System.Windows.Point(0, 1);
                Canvas.SetLeft(xMaxText, graphXmax - ElementSizeBase);
                Canvas.SetTop(xMaxText, graphYmin);
                targetCanvas.Children.Add(xMaxText);

                Line xAxis = new Line()
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = ElementSizeBase * LineScale,
                    X1 = graphXmin,
                    X2 = graphXmax,
                    Y1 = graphYmin,
                    Y2 = graphYmin,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                targetCanvas.Children.Add(xAxis);

                Line yAxis = new Line()
                {
                    Stroke = Brushes.Red,
                    StrokeThickness = ElementSizeBase * LineScale,
                    X1 = graphXmin,
                    X2 = graphXmin,
                    Y1 = graphYmax,
                    Y2 = graphYmin,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                targetCanvas.Children.Add(yAxis);


                Line trendline = new Line()
                {
                    Opacity = .75,
                    StrokeDashArray = new DoubleCollection(new double[] { 1, 1 }),
                    Stroke = Brushes.Orange,
                    StrokeThickness = ElementSizeBase * LineScale * .75,
                    X1 = graphXmin,
                    X2 = graphXmax,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };

                var test = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(255, 0, 0), 0),
                    new GradientStop(Color.FromRgb(0, 0, 255), .5),
                    new GradientStop(Color.FromRgb(0, 255, 0), 1)
                };

                LinearGradientBrush brush = new LinearGradientBrush(test)
                {
                    StartPoint = new System.Windows.Point(0, 0),
                    EndPoint = new System.Windows.Point(0, 1)
                };

                Polyline p = new Polyline()
                {
                    Stroke = brush,
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
                    DataSetElement node = Data.ElementAt(i);
                    trendDataX[i] = node.X;
                    trendDataY[i] = node.Y;

                    p.Points.Add(new System.Windows.Point(
                        Data.MapX(node.X, graphXmin, graphXmax),
                        Data.MapY(node.Y, graphYmin, graphYmax)));

                    Ellipse pe = new Ellipse()
                    {
                        ToolTip = node,
                        Width = ElementSizeBase * PointScale,
                        Height = ElementSizeBase * PointScale,
                        Fill = Brushes.Yellow,
                        Opacity = .8
                    };

                    Canvas.SetLeft(pe, Data.MapX(node.X, graphXmin, graphXmax) - (pe.Width / 2));
                    Canvas.SetTop(pe, Data.MapY(node.Y, graphYmin, graphYmax) - (pe.Width / 2));
                    elist.Add(pe);
                }

                double Y2 = (Data.Slope * Data.ElementAt(Data.Count - 1).X) + Data.Intercept;
                trendline.Y1 = Data.MapY((Data.Slope * Data.ElementAt(0).X) + Data.Intercept, graphYmin, graphYmax);
                trendline.Y2 = Data.MapY(Y2, graphYmin, graphYmax);

                TextBlock slopeText = new TextBlock()
                {
                    Foreground = trendline.Stroke,
                    Text = Data.Slope.ToString("F4")
                };
                Canvas.SetLeft(slopeText, graphXmax + 2 * ElementSizeBase);
                Canvas.SetTop(slopeText, Data.MapY(Y2, graphYmin, graphYmax) - ElementSizeBase);      

                targetCanvas.Children.Add(trendline);
                targetCanvas.Children.Add(slopeText);

                targetCanvas.Children.Add(p);
                foreach (Ellipse e in elist)
                    targetCanvas.Children.Add(e);
            }
            targetCanvas.Visibility = System.Windows.Visibility.Visible;
            updatePending = false;            
        }
    }    
}
