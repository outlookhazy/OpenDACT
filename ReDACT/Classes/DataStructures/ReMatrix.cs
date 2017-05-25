using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReDACT.Classes
{
    class ReMatrix
    {
        public double[][] data;

        public ReMatrix(int rows, int cols)
        {
            data = new double[rows][];
            
            for(int i=0; i<rows; i++)
            {
                double[] col = new double[cols];
                for(int j=0; j<cols; j++)
                {
                    col[j] = 0.0;
                }
                data[i] = col;
            }
        }

        public void SwapRows(int i, int j, int numCols)
        {
            if (i != j)
            {
                for (var k = 0; k < numCols; ++k)
                {
                    double temp = this.data[i][k];
                    this.data[i][k] = this.data[j][k];
                    this.data[j][k] = temp;
                }
            }
        }

        public void GaussJordan(out double[] solution, int numRows)
        {
            for (var i = 0; i < numRows; ++i)
            {
                // Swap the rows around for stable Gauss-Jordan elimination
                var vmax = Math.Abs(this.data[i][i]);
                for (var j = i + 1; j < numRows; ++j)
                {
                    var rmax = Math.Abs(this.data[j][i]);
                    if (rmax > vmax)
                    {
                        this.SwapRows(i, j, numRows + 1);
                        vmax = rmax;
                    }
                }

                // Use row i to eliminate the ith element from previous and subsequent rows
                var v = this.data[i][i];
                for (var j = 0; j < i; ++j)
                {
                    var factor = this.data[j][i] / v;
                    this.data[j][i] = 0.0;
                    for (var k = i + 1; k <= numRows; ++k)
                    {
                        this.data[j][k] -= this.data[i][k] * factor;
                    }
                }

                for (var j = i + 1; j < numRows; ++j)
                {
                    var factor = this.data[j][i] / v;
                    this.data[j][i] = 0.0;
                    for (var k = i + 1; k <= numRows; ++k)
                    {
                        this.data[j][k] -= this.data[i][k] * factor;
                    }
                }
            }

            solution = new double[numRows];
            for (var i = 0; i < numRows; ++i)
            {
                solution[i] = (this.data[i][numRows] / this.data[i][i]);
            }
        }

        public string Print(string tag)
        {
            var rslt = tag + " {\n";
            for (var i = 0; i < this.data.GetLength(0); ++i)
            {
                var row = this.data[i];
                rslt += (i == 0) ? '{' : ' ';
                for (var j = 0; j < row.Length; ++j)
                {
                    rslt += row[j].ToString("F4");
                    if (j + 1 < row.Length)
                    {
                        rslt += ", ";
                    }
                }
                rslt += "\n";
            }
            rslt += '}';
            return rslt;
        }
    }
}
