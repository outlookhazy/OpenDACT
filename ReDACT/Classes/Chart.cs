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

        public void Draw()
        {
            targetCanvas.Background = Brushes.Teal;
            targetCanvas.Children.Clear();
            

            if (Data.Count < 2)
                return;
            Console.WriteLine(String.Format("Drawing {0} points", Data.Count));
            for(int i=0; i< Data.Count-1; i++)
            {
                Console.WriteLine(String.Format("{0} to {1}", Data.ElementAt(i), Data.ElementAt(i + 1)));

                Line dataLine = new Line();
                dataLine.Y1 = Data.ElementAt(i).X;
                dataLine.X1 = Data.ElementAt(i).Y;
                dataLine.Y2 = Data.ElementAt(i+1).X;
                dataLine.X2 = Data.ElementAt(i+1).Y;

                dataLine.Stroke = Brushes.Red;
                dataLine.Width = 5;

                //Canvas.SetLeft(dataLine, dataLine.X1);
                //Canvas.SetBottom(dataLine, 50);

                targetCanvas.Children.Add(dataLine);
            }
        }

        private void Data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Draw();
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
