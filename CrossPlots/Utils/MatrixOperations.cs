using System;

namespace CrossPlots.Utils
{
    public class MatrixOperations
    {
        public static double[][] CreateFullRotationMatrix(double angle, double x, double y)
        {
            var translation_matrix = CreateTranslationMatrix(-x, -y);
            var rotation_matrix = CreateRotationMatrix(angle);
            var translate_back_matrix = CreateTranslationMatrix(x, y);

            var resultant_matrix = MatrixMultiplication(translation_matrix, rotation_matrix);
            resultant_matrix = MatrixMultiplication(resultant_matrix, translate_back_matrix);

            return resultant_matrix;
        }

        private static double[][] CreateTranslationMatrix(double x, double y)
        {
            double[][] arr = new double[][]
            {
                new double[] { 1, 0, 0 },
                new double[] { 0, 1, 0 },
                new double[] { x, y, 1 }
            };
            return arr;
        }

        private static double[][] CreateRotationMatrix(double angle)
        {
            double[][] arr = new double[][]
            {
                new double[] { Math.Cos(angle), Math.Sin(angle), 0 },
                new double[] { -Math.Sin(angle), Math.Cos(angle), 0 },
                new double[] { 0, 0, 1 }
            };
            return arr;
        }

        public static double[][] MatrixMultiplication(double[][] matrixA, double[][] matrixB)
        {
            int m = matrixA.Length;
            int n = matrixA[0].Length;
            int p = matrixB[0].Length;

            double[][] result = new double[m][];

            for (int i = 0; i < m; i++)
            {
                result[i] = new double[p];
                for (int j = 0; j < p; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < n; k++)
                    {
                        sum += matrixA[i][k] * matrixB[k][j];
                    }
                    result[i][j] = sum;
                }
            }

            return result;
        }

        public static double[] MatrixMultiplication(double[] objectMatrix, double[][] transformationMatrix)
        {
            if (
                objectMatrix.Length != 3 ||
                transformationMatrix[0].Length != 3 ||
                transformationMatrix[1].Length != 3 ||
                transformationMatrix[2].Length != 3
            )
            {
                throw new ArgumentException("Input sizes are incorrect. Vector size must be 3 and matrix size must be 3x3.");
            }

            double[] result = new double[3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    result[i] += objectMatrix[j] * transformationMatrix[j][i];
                }
            }

            return result;
        }
    }
}
