﻿namespace Savanna.Frontend.Managers
{
    using System;
    using System.Text;
    using Savanna.Backend.Constants;
    using Savanna.Backend.Plugins;
    using Savanna.Frontend.Constants;

    /// <summary>
    /// Manages the console display.
    /// </summary>
    public class ConsoleManager
    {
        /// <summary>
        /// Initializes the console window with the appropriate title, size, and settings.
        /// </summary>
        public void Initialize()
        {
            Console.Title = ConsoleConstants.GameTitle;
            Console.CursorVisible = false;
            Console.Clear();

            // Calculate console window size based on grid and info panel
            int windowWidth = (GameConstants.GridWidth * 2) + 3 + ConsoleConstants.InfoPanelWidth;
            
            // Calculate height including space for plugin animals
            int pluginCount = PluginManager.Instance.RegisteredPlugins.Count;
            int windowHeight = GameConstants.GridHeight + 4 + Math.Max(0, pluginCount);

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

        /// <summary>
        /// Renders the game grid and information panel on the console.
        /// </summary>
        /// <param name="displayGrid">The 2D character array representing the game grid.</param>
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
            output.AppendLine();

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
                else
                {
                    // Add plugin animal information
                    int pluginIndex = y - 4;
                    if (pluginIndex >= 0)
                    {
                        var plugins = PluginManager.Instance.RegisteredPlugins.Values.ToList();
                        if (pluginIndex < plugins.Count)
                        {
                            var plugin = plugins[pluginIndex];
                            output.Append($"  {plugin.GetAnimalConfig().Symbol} = {plugin.AnimalType}");
                        }
                    }
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
            output.AppendLine();

            Console.Write(output.ToString());
        }
    }
}