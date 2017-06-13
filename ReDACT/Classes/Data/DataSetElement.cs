using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReDACT.Classes.Data
{
    class DataSetElement
    {
        public double X { get; set; }
        public double Y { get; set; }
        public event ElementValueChanged onValueChanged;

        public DataSetElement(double X = 0, double Y = 0)
        {
            this.X = X;
            this.Y = Y;
        }

        private void ValueChanged() { onValueChanged?.Invoke(this); }

        public DataSetElement Copy()
        {
            DataSetElement copy = new DataSetElement()
            {
                X = this.X,
                Y = this.Y
            };
            return copy;
        }

        public delegate void ElementValueChanged(DataSetElement element);
    }
}