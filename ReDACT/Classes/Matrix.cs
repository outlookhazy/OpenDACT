using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReDACT.Classes
{
    public class Matrix
    {
        public double[,] matrixData;
        public Matrix(int rows, int cols)
        {
            matrixData = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrixData[i, j] = 0.0;
                }
            }
        }

        public void SwapRows(int i, int j, int numCols)
        {
            if (i != j)
            {
                for (int k = 0; k < numCols; ++k)
                {
                    var temp = this.matrixData[i, k];
                    this.matrixData[i, k] = this.matrixData[j, k];
                    this.matrixData[j, k] = temp;
                }
            }
        }

        // Perform Gauus-Jordan elimination on a matrix with numRows rows and (njumRows + 1) columns
        public void GaussJordan(ref double[] solution, int numRows)
        {
            for (var i = 0; i < numRows; ++i)
            {
                // Swap the rows around for stable Gauss-Jordan elimination
                var vmax = Math.Abs(this.matrixData[i, i]);
                for (var j = i + 1; j < numRows; ++j)
                {
                    var rmax = Math.Abs(this.matrixData[j, i]);
                    if (rmax > vmax)
                    {
                        this.SwapRows(i, j, numRows + 1);
                        vmax = rmax;
                    }
                }

                // Use row i to eliminate the ith element from previous and subsequent rows
                var v = this.matrixData[i, i];
                for (var j = 0; j < i; ++j)
                {
                    var factor = this.matrixData[j, i] / v;
                    this.matrixData[j, i] = 0.0;
                    for (var k = i + 1; k <= numRows; ++k)
                    {
                        this.matrixData[j, k] -= this.matrixData[i, k] * factor;
                    }
                }

                for (var j = i + 1; j < numRows; ++j)
                {
                    var factor = this.matrixData[j, i] / v;
                    this.matrixData[j, i] = 0.0;
                    for (var k = i + 1; k <= numRows; ++k)
                    {
                        this.matrixData[j, k] -= this.matrixData[i, k] * factor;
                    }
                }
            }

            for (var i = 0; i < numRows; ++i)
            {
                solution[i] = (this.matrixData[i, numRows] / this.matrixData[i, i]);
            }
        }
    }
}
