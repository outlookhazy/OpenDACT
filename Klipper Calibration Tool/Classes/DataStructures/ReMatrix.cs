using System;

namespace Klipper_Calibration_Tool.Classes.DataStructures
{
    internal class ReMatrix
    {
        public double[][] Data;

        public ReMatrix(int rows, int cols)
        {
            Data = new double[rows][];

            for (int i = 0; i < rows; i++)
            {
                double[] col = new double[cols];
                for (int j = 0; j < cols; j++)
                {
                    col[j] = 0.0;
                }
                Data[i] = col;
            }
        }

        public void SwapRows(int i, int j, int numCols)
        {
            if (i == j) return;
            for (int k = 0; k < numCols; ++k)
            {
                double temp = Data[i][k];
                Data[i][k] = Data[j][k];
                Data[j][k] = temp;
            }
        }

        public void GaussJordan(out double[] solution, int numRows)
        {
            for (int i = 0; i < numRows; ++i)
            {
                // Swap the rows around for stable Gauss-Jordan elimination
                double vmax = Math.Abs(Data[i][i]);
                for (int j = i + 1; j < numRows; ++j)
                {
                    double rmax = Math.Abs(Data[j][i]);
                    if (rmax > vmax)
                    {
                        SwapRows(i, j, numRows + 1);
                        vmax = rmax;
                    }
                }

                // Use row i to eliminate the ith element from previous and subsequent rows
                double v = Data[i][i];
                for (int j = 0; j < i; ++j)
                {
                    double factor = Data[j][i] / v;
                    Data[j][i] = 0.0;
                    for (int k = i + 1; k <= numRows; ++k)
                    {
                        Data[j][k] -= Data[i][k] * factor;
                    }
                }

                for (int j = i + 1; j < numRows; ++j)
                {
                    double factor = Data[j][i] / v;
                    Data[j][i] = 0.0;
                    for (int k = i + 1; k <= numRows; ++k)
                    {
                        Data[j][k] -= Data[i][k] * factor;
                    }
                }
            }

            solution = new double[numRows];
            for (int i = 0; i < numRows; ++i)
            {
                solution[i] = Data[i][numRows] / Data[i][i];
            }
        }

        public string Print(string tag)
        {
            string rslt = tag + " {\n";
            for (int i = 0; i < Data.GetLength(0); ++i)
            {
                double[] row = Data[i];
                rslt += i == 0 ? '{' : ' ';
                for (int j = 0; j < row.Length; ++j)
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
