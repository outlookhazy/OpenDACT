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

        public ObservableCollection<ChartData> Data;

        public Chart(Canvas targetCanvas)
        {
            this.targetCanvas = targetCanvas;
            this.Data = new ObservableCollection<ChartData>();
            this.Data.CollectionChanged += Data_CollectionChanged;
        }

        void updateGraph()
        {
            targetCanvas.Dispatcher.Invoke((Action)(() => {
                DateTime updatestart = DateTime.Now;

                targetCanvas.Children.Clear();

                for (int i = 0; i < Data.Count - 1; i++)
                {                   
                    if (i > 0)
                    {
                        Line e = new Line()
                        {
                            ToolTip = String.Format("{0} -> {1}", Data.ElementAt(i - 1), Data.ElementAt(i)),
                            X1 = Conversion.scale(Data.ElementAt(i - 1).X, 0, Data.Count - 1, 0, targetCanvas.Width),
                            X2 = Conversion.scale(Data.ElementAt(i).X, 0, Data.Count - 1, 0, targetCanvas.Width),
                            Y1 = Conversion.scale(Data.ElementAt(i - 1).Y, SeriesMin(), SeriesMax(), targetCanvas.Height, 0),
                            Y2 = Conversion.scale(Data.ElementAt(i).Y, SeriesMin(), SeriesMax(), targetCanvas.Height, 0),
                            Stroke = Brushes.LimeGreen,
                            StrokeThickness = 5
                        };
                        targetCanvas.Children.Add(e);
                    }
                }
            }));
        }

        double SeriesMin()
        {
            double min = Data.ElementAt(0).Y;
            foreach(ChartData d in Data)
            {
                if (d.Y < min)
                    min = d.Y;
            }
            return min;
        }

        double SeriesMax()
        {
            double max = Data.ElementAt(0).Y;
            foreach (ChartData d in Data)
            {
                if (d.Y > max)
                    max = d.Y;
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
