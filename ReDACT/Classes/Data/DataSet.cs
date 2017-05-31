using MathNet.Numerics;
using ReDACT.Classes.DataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReDACT.Classes.Data
{
    class DataSet : IList<DataSetElement>
    {
        private List<DataSetElement> privateData = new List<DataSetElement>();
        private Dictionary<Stat, StatValue> privateStats = new Dictionary<Stat, StatValue>();

        public event DataSetModified onDataSetModified;
        private void SetModified() { onDataSetModified?.Invoke(this); }

        public DataSet()
        {
            foreach(Stat stat in typeof(Stat).GetEnumValues())
            {
                privateStats.Add(stat, new StatValue());
            }
        }

        #region Utility

        public double MapX(double value, double toMin, double toMax)
        {
            return Conversion.scale(value, this.XMIN, this.XMAX, toMin, toMax);
        }

        public double MapY(double value, double toMin, double toMax)
        {
            return Conversion.scale(value, this.YMIN, this.YMAX, toMin, toMax);
        }

        #endregion

        #region Statistics

        private void InvalidateStats()
        {
            foreach (Stat stat in privateStats.Keys)
            {
                privateStats[stat].Invalidate();
            }
        }        

        public int Count { get {
                if (!privateStats[Stat.COUNT].Valid)
                    privateStats[Stat.COUNT].Value = privateData.Count;
                return (int)privateStats[Stat.COUNT].Value;
            } }

        #region Linear Regression

        public double Slope { get {
                if (!privateStats[Stat.SLOPE].Valid)
                    doLinearRegression();
                return privateStats[Stat.SLOPE].Value;
            } }

        public double Intercept {
            get {
                if (!privateStats[Stat.INTERCEPT].Valid)
                    doLinearRegression();
                return privateStats[Stat.INTERCEPT].Value;
            }
        }

        private void doLinearRegression()
        {
            double[] dataXValues = new double[this.Count];
            double[] dataYValues = new double[this.Count];

            for(int i=0; i< this.Count; i++)
            {
                dataXValues[i] = this.privateData[i].X;
                dataYValues[i] = this.privateData[i].Y;
            }

            Tuple<double, double> trend = Fit.Line(dataXValues, dataYValues);

            this.privateStats[Stat.SLOPE].Value = trend.Item2;
            this.privateStats[Stat.INTERCEPT].Value = trend.Item1;
        }

        #endregion

        #region Means

        public double XMEAN { get {
                if (!privateStats[Stat.XMEAN].Valid) {
                    double mean = 0;
                    foreach (DataSetElement dse in privateData)
                        mean += dse.X;
                    mean /= (double)this.Count;
                    privateStats[Stat.XMEAN].Value = mean;
            }
                return privateStats[Stat.XMEAN].Value;
            } }

        public double YMEAN {
            get {
                if (!privateStats[Stat.YMEAN].Valid)
                {
                    double mean = 0;
                    foreach (DataSetElement dse in privateData)
                        mean += dse.Y;
                    mean /= (double)this.Count;
                    privateStats[Stat.YMEAN].Value = mean;
                }
                return privateStats[Stat.YMEAN].Value;
            }
        }

        public double XRMS {
            get {
                if (!privateStats[Stat.XRMS].Valid)
                {
                    double meansq = 0;
                    foreach (DataSetElement dse in privateData)
                        meansq += Math.Pow(dse.X,2);
                    meansq /= (double)this.Count;
                    privateStats[Stat.XRMS].Value = Math.Sqrt(meansq);
                }
                return privateStats[Stat.XRMS].Value;
            }
        }

        public double YRMS {
            get {
                if (!privateStats[Stat.YRMS].Valid)
                {
                    double meansq = 0;
                    foreach (DataSetElement dse in privateData)
                        meansq += Math.Pow(dse.Y, 2);
                    meansq /= (double)this.Count;
                    privateStats[Stat.YRMS].Value = Math.Sqrt(meansq);
                }
                return privateStats[Stat.YRMS].Value;
            }
        }

        public double STDDEVX {
            get {
                if (!privateStats[Stat.STDDEVX].Valid)
                {
                    double sumDeltaSquared = 0;

                    foreach (DataSetElement dse in privateData)
                        sumDeltaSquared += Math.Pow((dse.X - XMEAN), 2);

                    double avgDeltaSquared = sumDeltaSquared / (double)this.Count;

                    privateStats[Stat.STDDEVX].Value = Math.Sqrt(avgDeltaSquared);
                }
                return privateStats[Stat.STDDEVX].Value;
            }
        }

        #endregion

        #region MinMax

        public double XMIN {
            get {
                if (!privateStats[Stat.XMIN].Valid)
                    calcMinMax();
                return privateStats[Stat.XMIN].Value;
            }
        }

        public double YMIN {
            get {
                if (!privateStats[Stat.YMIN].Valid)
                    calcMinMax();
                return privateStats[Stat.YMIN].Value;
            }
        }

        public double XMAX {
            get {
                if (!privateStats[Stat.XMAX].Valid)
                    calcMinMax();
                return privateStats[Stat.XMAX].Value;
            }
        }

        public double YMAX {
            get {
                if (!privateStats[Stat.YMAX].Valid)
                    calcMinMax();
                
                return privateStats[Stat.YMAX].Value;
            }
        }

        private void calcMinMax()
        {
            double xmin = double.MaxValue;
            double ymin = double.MaxValue;
            double xmax = double.MinValue;
            double ymax = double.MinValue;

            foreach (DataSetElement dse in privateData)
            {
                if (dse.X > xmax)
                    xmax = dse.X;
                if (dse.Y > ymax)
                    ymax = dse.Y;

                if (dse.X < xmin)
                    xmin = dse.X;
                if (dse.Y < ymin)
                    ymin = dse.Y;
            }
            privateStats[Stat.XMIN].Value = xmin;
            privateStats[Stat.YMIN].Value = ymin;
            privateStats[Stat.XMAX].Value = xmax;
            privateStats[Stat.YMAX].Value = ymax;
        }

#endregion


        #endregion Statistics

        #region List Manipulation

        public DataSetElement this[int index] {
            get { return privateData[index]; }
            set {
                this.privateData[index].onValueChanged -= Item_onValueChanged;
                this.privateData[index] = value.Copy();
                this.privateData[index].onValueChanged += Item_onValueChanged;
                this.InvalidateStats();
                this.SetModified();
            }
        }

        public bool IsReadOnly { get { return false; } }

        public void Add(DataSetElement item)
        {
            this.privateData.Add(item);
            item.onValueChanged += Item_onValueChanged;
            this.InvalidateStats();
            this.SetModified();
        }

        private void Item_onValueChanged(DataSetElement element)
        {
            InvalidateStats();
            SetModified();
        }

        public void Clear()
        {
            foreach(DataSetElement dse in privateData)
            {
                dse.onValueChanged -= Item_onValueChanged;
            }
            privateData.Clear();
            InvalidateStats();
            SetModified();            
        }

        public bool Contains(DataSetElement item)
        {
            return privateData.Contains(item);
        }

        public void CopyTo(DataSetElement[] array, int arrayIndex)
        {
            for(int i=0; i< privateData.Count; i++)
            {
                array[arrayIndex + i] = privateData[i];
            }
        }

        public IEnumerator<DataSetElement> GetEnumerator()
        {
            return privateData.GetEnumerator();
        }

        public int IndexOf(DataSetElement item)
        {
            return privateData.IndexOf(item);
        }

        public void Insert(int index, DataSetElement item)
        {
            privateData.Insert(index, item);
            item.onValueChanged += Item_onValueChanged;
            InvalidateStats();
            SetModified();
        }

        public bool Remove(DataSetElement item)
        {
            if (privateData.Remove(item))
            {
                item.onValueChanged -= Item_onValueChanged;
                InvalidateStats();
                SetModified();
                return true;
            } else
            {
                return false;
            }
        }

        public void RemoveAt(int index)
        {
            privateData.ElementAt(index).onValueChanged -= Item_onValueChanged;
            privateData.RemoveAt(index);
            InvalidateStats();
            SetModified();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return privateData.GetEnumerator();
        }

        public delegate void DataSetModified(DataSet Origin);

        #endregion


        #region Data Structures
        private enum Stat
        {
            COUNT,
            XMEAN,
            YMEAN,
            XRMS,
            YRMS,
            XMIN,
            YMIN,
            XMAX,
            YMAX,
            SLOPE,
            INTERCEPT,
            STDDEVX,
            STDDEVY
        }

        internal class StatValue
        {
            public bool Valid { get; private set; }
            private double value;
            public double Value { get { return this.value; } set { this.value = value; this.Valid = true; } }

            public StatValue()
            {
                this.Value = 0;
                this.Valid = false;
            }

            public void Invalidate()
            {
                this.Valid = false;
            }
        }
        #endregion
    }
}
