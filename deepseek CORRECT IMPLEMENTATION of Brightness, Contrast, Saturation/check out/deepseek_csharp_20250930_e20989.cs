public static ColorMatrix MultiplyColorMatrices(ColorMatrix a, ColorMatrix b)
{
    float[][] result = new float[5][];
    for (int i = 0; i < 5; i++)
    {
        result[i] = new float[5];
        for (int j = 0; j < 5; j++)
        {
            result[i][j] = 0;
            for (int k = 0; k < 5; k++)
            {
                result[i][j] += a[i, k] * b[k, j];
            }
        }
    }
    return new ColorMatrix(result);
}

// Helper method to access ColorMatrix elements
private static float GetElement(ColorMatrix matrix, int row, int col)
{
    return matrix.GetType().GetField("matrix", 
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?.GetValue(matrix) is float[,] arr ? arr[row, col] : matrix[row, col];
}