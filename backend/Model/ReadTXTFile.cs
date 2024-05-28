using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
//using Newtonsoft.Json;

namespace RoutingApi.Model
{
    internal class ReadTXTFile
    {
        public static DataModel ReadRouteData(string filePath, int routeIndex)
        {
            try
            {
                // Đọc tất cả nội dung trong file
                string[] fileLines = File.ReadAllLines(filePath);
                routeIndex -= 1;
                //Console.WriteLine(fileLines[0]);
                return ParseDataModel(fileLines[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when reading route data: " + ex.Message);
                return null;
            }
        }

        public static DataModel ParseDataModel(string dataString)
        {
            // Remove curly braces and split the string into individual parts
            string[] parts = dataString.Trim('{', '}').Split(':');
            //TimeMatrix: parts[2]
            //TimeWindows: parts[3]
            //VehicleNumber: parts[4]
            //Depot: part[5]

            DataModel data = new DataModel(ParseTimeMatrix(parts[2]), int.Parse(parts[4].Split(',')[0].Trim()), int.Parse(parts[5].Split(',')[0].Trim()));
            //data.TimeMatrix = ParseTimeMatrix(parts[2]);
            //data.TimeWindows = ParseTimeWindows(parts[3]);
            //data.VehicleNumber = int.Parse(parts[4].Split(',')[0].Trim());
            //data.Depot = int.Parse(parts[5].Split(',')[0].Trim());

            return data;
        }
        public static long[,] ParseTimeWindows(string input)
        {
            // Regular expression to match the time window tuples
            Regex regex = new Regex(@"\((\d+), (\d+)\)");

            // Find all matches
            MatchCollection matches = regex.Matches(input);

            // Create a 2D array to store time windows
            long[,] timeWindows = new long[matches.Count, 2];

            // Extract matched values and store in the array
            for (int i = 0; i < matches.Count; i++)
            {
                timeWindows[i, 0] = long.Parse(matches[i].Groups[1].Value);
                timeWindows[i, 1] = long.Parse(matches[i].Groups[2].Value);
            }
            return timeWindows;
        }



        public static long[,] ParseTimeMatrix(string input)
        {
            // Sử dụng biểu thức chính quy để tách ma trận từ chuỗi đầu vào
            Regex regex = new Regex(@"\[\[(.*?)\]\]");
            Match match = regex.Match(input);

            if (match.Success)
            {
                string matrixString = match.Groups[1].Value;
                string[] rows = matrixString.Split(new[] { "], [" }, StringSplitOptions.None);

                int rowCount = rows.Length;
                int colCount = rows[0].Split(',').Length;

                long[,] matrix = new long[rowCount, colCount];

                for (int i = 0; i < rowCount; i++)
                {
                    string[] elements = rows[i].Split(',');
                    for (int j = 0; j < colCount; j++)
                    {
                        // Parse từng phần tử trong ma trận thành kiểu long
                        matrix[i, j] = long.Parse(elements[j]);
                    }
                }

                return matrix;
            }
            else
            {
                Console.WriteLine("Không tìm thấy ma trận trong chuỗi đầu vào.");
                return null;
            }
        }


        public static void PrintLongMatrix(long[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(matrix[i, j] + "\t");
                }
                Console.WriteLine();
            }
        }

    }
}
