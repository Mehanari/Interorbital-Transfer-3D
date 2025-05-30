using System;

public static class PermutationGenerator
{
    /// <summary>
    /// Generates all unique permutations of integers from 0 to n.
    /// </summary>
    /// <param name="n">A non-negative integer specifying the upper bound of the range.</param>
    /// <returns>A two-dimensional array where each row is a unique permutation.</returns>
    public static int[,] GeneratePermutations(int n)
    {
        if (n < 0)
            throw new ArgumentException("MissionParameters must be a non-negative integer.", nameof(n));

        // Calculate factorial for the size of the result array
        int factorial = 1;
        for (int i = 2; i <= n + 1; i++)
            factorial *= i;

        // Create result array with dimensions [factorial, n+1]
        int[,] result = new int[factorial, n + 1];
        
        // Create an array to hold the current permutation
        int[] current = new int[n + 1];
        for (int i = 0; i <= n; i++)
            current[i] = i;
        
        // Add the first permutation (identity permutation)
        CopyToResult(result, current, 0);
        
        int permCount = 1;
        
        // Generate all permutations using Heap's algorithm
        GenerateHeapPermutations(n + 1, current, result, ref permCount);
        
        return result;
    }
    
    /// <summary>
    /// Recursively generates permutations using Heap's algorithm.
    /// </summary>
    private static void GenerateHeapPermutations(int n, int[] elements, int[,] result, ref int permCount)
    {
        if (n <= 1)
            return;
            
        for (int i = 0; i < n - 1; i++)
        {
            GenerateHeapPermutations(n - 1, elements, result, ref permCount);
            
            // If n is odd, swap the first element with the last element
            // If n is even, swap the i-th element with the last element
            int swapIdx = (n % 2 == 0) ? i : 0;
            
            // Swap elements
            (elements[swapIdx], elements[n - 1]) = (elements[n - 1], elements[swapIdx]);

            // Add the new permutation to the result
            CopyToResult(result, elements, permCount++);
        }
        
        GenerateHeapPermutations(n - 1, elements, result, ref permCount);
    }
    
    /// <summary>
    /// Copies the current permutation to the result array at the specified index.
    /// </summary>
    private static void CopyToResult(int[,] result, int[] permutation, int index)
    {
        for (int i = 0; i < permutation.Length; i++)
            result[index, i] = permutation[i];
    }
}