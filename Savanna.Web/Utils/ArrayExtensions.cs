namespace Savanna.Web.Utils 
{
    public static class ArrayExtensions
    {
        /// <summary>
        /// Converts a 2D rectangular array (multidimensional) into a jagged array (array of arrays).
        /// Assumes input is [Width, Height] and output should be [Height][Width] for typical grid display [y][x].
        /// </summary>
        /// <param name="multiArray">The multidimensional array to convert.</param>
        /// <returns>A jagged array representation, or an empty jagged array if input is null/invalid.</returns>
        public static char[][] ToJaggedArray(this char[,] multiArray)
        {
            if (multiArray == null || multiArray.Rank != 2)
            {
                return Array.Empty<char[]>();
            }

            int width = multiArray.GetLength(0);  // Dimension 0 (e.g., X)
            int height = multiArray.GetLength(1); // Dimension 1 (e.g., Y)

            // Handle empty array case after rank check
            if (width == 0 || height == 0)
            {
                return Array.Empty<char[]>();
            }


            // Create jagged array [Height][Width]
            char[][] jaggedArray = new char[height][]; // Outer dimension is rows (Y)

            for (int y = 0; y < height; y++) // Iterate rows (Y)
            {
                jaggedArray[y] = new char[width]; // Inner dimension is columns (X)
                for (int x = 0; x < width; x++) // Iterate columns (X)
                {
                    jaggedArray[y][x] = multiArray[x, y]; // Access source as [x, y]
                }
            }
            return jaggedArray;
        }
    }
}