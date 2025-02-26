namespace Savanna.Frontend.Managers
{
    using System;
    using System.Text;
    using Savanna.Backend.Constants;
    using Savanna.Frontend.Constants;

    public class ConsoleManager
    {
        public void Initialize()
        {
            Console.Title = ConsoleConstants.GameTitle;
            Console.CursorVisible = false;
            Console.Clear();

            // Calculate console window size based on grid and info panel
            int windowWidth = (GameConstants.GridWidth * 2) + 3 + ConsoleConstants.InfoPanelWidth;
            int windowHeight = GameConstants.GridHeight + 4;

            try
            {
                // Set console window and buffer size
                Console.SetWindowSize(Math.Min(windowWidth, Console.LargestWindowWidth),
                                      Math.Min(windowHeight, Console.LargestWindowHeight));
                Console.SetBufferSize(Math.Min(windowWidth, Console.LargestWindowWidth),
                                      Math.Min(windowHeight, Console.LargestWindowHeight));
            }
            catch (Exception ex)
            {
                // Fallback if console window resizing fails
                Console.WriteLine($"Unable to resize console window: {ex.Message}");
                Console.WriteLine("Press any key to continue with default console size...");
                Console.ReadKey(true);
                Console.Clear();
            }
        }

        public void Render(char[,] displayGrid)
        {
            Console.SetCursorPosition(0, 0);

            StringBuilder output = new StringBuilder();

            // Draw title
            output.AppendLine(ConsoleConstants.GameTitle);
            output.AppendLine();

            // Draw top border
            output.Append(ConsoleConstants.GridBorderCorner);
            for (int x = 0; x < GameConstants.GridWidth * 2; x++)
            {
                output.Append(ConsoleConstants.GridBorderHorizontal);
            }
            output.Append(ConsoleConstants.GridBorderCorner);
            output.AppendLine(); // Fixed: Changed from AppendLine(char) to Append(char) followed by AppendLine()

            // Draw grid with side borders
            for (int y = 0; y < GameConstants.GridHeight; y++)
            {
                output.Append(ConsoleConstants.GridBorderVertical);

                for (int x = 0; x < GameConstants.GridWidth; x++)
                {
                    char cell = displayGrid[x, y];
                    output.Append(cell);
                    output.Append(' '); // Add space for better visibility
                }

                output.Append(ConsoleConstants.GridBorderVertical);

                // Add info panel content on selected rows
                if (y == 0)
                {
                    output.Append("  " + ConsoleConstants.ControlsInfo);
                }
                else if (y == 2)
                {
                    output.Append("  A = Antelope (Moves 1 cell, flees from Lions)");
                }
                else if (y == 3)
                {
                    output.Append("  L = Lion (Moves 2 cells, hunts Antelopes)");
                }

                output.AppendLine();
            }

            // Draw bottom border
            output.Append(ConsoleConstants.GridBorderCorner);
            for (int x = 0; x < GameConstants.GridWidth * 2; x++)
            {
                output.Append(ConsoleConstants.GridBorderHorizontal);
            }
            output.Append(ConsoleConstants.GridBorderCorner);
            output.AppendLine(); // Fixed: Changed from AppendLine(char) to Append(char) followed by AppendLine()

            Console.Write(output.ToString());
        }
    }
}