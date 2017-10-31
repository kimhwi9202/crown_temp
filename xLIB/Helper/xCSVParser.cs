using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;
using System.Text;

namespace xLIB
{
    public class xCSVParser : MonoBehaviour
    {
        // outputs the content of a 2D array, useful for checking the importer
        static public void DebugOutputGrid(string[,] grid)
        {
            string textOutput = "";
            for (int y = 0; y < grid.GetUpperBound(1); y++)
            {
                for (int x = 0; x < grid.GetUpperBound(0); x++)
                {
                    textOutput += grid[x, y];
                    textOutput += ",";
                }
                textOutput += "\n";
            }
            Debug.Log(textOutput);
        }

        static public string LoadFile(string fileFullPath)
        {
            TextReader _reader = null;
            FileInfo _sourceFile = new FileInfo(fileFullPath);
            if (_sourceFile != null && _sourceFile.Exists)
            {
                _reader = _sourceFile.OpenText();
            }

            if (_reader == null)
            {
                Debug.LogError("File not found or not readable : " + fileFullPath);
                return "";
            }

            string inputData = _reader.ReadLine();
            string textOutput = inputData;
            textOutput += "\n";
            while (inputData != null)
            {
                inputData = _reader.ReadLine();
                textOutput += inputData;
                textOutput += "\n";
            }

            _sourceFile = null;
            _reader.Dispose();
            _reader = null;

            return textOutput;
        }

        static public void SaveFile(string fullPath, string[,] grid)
        {
            string textOutput = "";
            for (int y = 0; y < grid.GetUpperBound(1); y++)
            {
                for (int x = 0; x < grid.GetUpperBound(0); x++)
                {
                    textOutput += grid[x, y];
                    textOutput += ",";
                }
                textOutput += "\r\n";
            }
            //Debug.Log(textOutput);
            File.WriteAllText(fullPath, textOutput, Encoding.UTF8);
        }

        // splits a CSV file into a 2D string array
        static public string[,] SplitCsvGrid(string csvText)
        {
            string[] lines = csvText.Split("\n"[0]);

            // finds the max width of row
            int width = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] row = SplitCsvLine(lines[i]);
                width = Mathf.Max(width, row.Length);
            }

            // creates new 2D string grid to output to
            string[,] outputGrid = new string[width + 1, lines.Length + 1];
            for (int y = 0; y < lines.Length; y++)
            {
                string[] row = SplitCsvLine(lines[y]);
                for (int x = 0; x < row.Length; x++)
                {
                    outputGrid[x, y] = row[x];

                    // This line was to replace "" with " in my output. 
                    // Include or edit it as you wish.
                    outputGrid[x, y] = outputGrid[x, y].Replace("\"\"", "\"");
                }
            }

            return outputGrid;
        }

        // splits a CSV row 
        static public string[] SplitCsvLine(string line)
        {
            return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
            @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
            System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
                    select m.Groups[1].Value).ToArray();
        }
    }
}