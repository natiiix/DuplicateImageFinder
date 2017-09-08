using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace DuplicateImageFinder
{
    public class Program
    {
        // Size of the longer dimension of the scaled bitmap
        private const int MAX_DIMENSION = 64;

        private const int PRINT_COMPARING_PROGRESS_EVERY_N = PRINT_SCALING_PROGRESS_EVERY_N * 100;

        private const int PRINT_SCALING_PROGRESS_EVERY_N = 10;

        // Lowest required similarity coefficient for two bitmaps to be considered similar
        private const double SIMILARITY_THRESHOLD = 0.8;

        // Compares two bitmaps pixel by pixel and returns a double value representing their similarity.
        private static double CompareBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            // Cannot compare bitmaps of different size
            if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height)
            {
                return 0.0;
            }

            // Sum the color difference of each pixel
            long totalDiff = 0;

            for (int y = 0; y < bmp1.Height; y++)
            {
                for (int x = 0; x < bmp1.Width; x++)
                {
                    totalDiff += GetColorDifference(bmp1.GetPixel(x, y), bmp2.GetPixel(x, y));
                }
            }

            // Return the similarity coefficient
            // Divided by the bitmap dimensions and 3 * 0xFF because each pixel has 3 8-bit channels
            return 1.0 - ((double)totalDiff / (bmp1.Width * bmp1.Height * 3 * 0xFF));
        }

        private static void FindDuplicates(string imageDir)
        {
            // Make sure the source directory exists
            if (!Directory.Exists(imageDir))
            {
                Console.WriteLine("Invalid source directory path!");
                return;
            }

            // Get a list of files in the source directory
            string[] files = Directory.GetFiles(imageDir);

            if (files.Length < 2)
            {
                Console.WriteLine("Insufficient amount of source images!");
            }

            // Load the bitmaps
            Bitmap[] images = new Bitmap[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                // Scale the image
                images[i] = GetScaledImage(files[i]);

                // Number of already scaled images
                int scaled = i + 1;

                // Print the scaling progress every N scaled images
                if (scaled % PRINT_SCALING_PROGRESS_EVERY_N == 0)
                {
                    Console.WriteLine("Scaling: {0:0.##}% ({1} / {2})", ((double)scaled / files.Length * 100.0), scaled, files.Length);
                }
            }

            Console.WriteLine("Scaling: Done!");

            // Create a list to hold all the information regarding similar images
            List<Similarity> similarImages = new List<Similarity>();

            // Keeps track of progress
            long combinationsAvailable = GetNumberOfCombinations(images.Length);
            long combinationsChecked = 0;

            // Compare the bitmaps
            for (int i = 0; i < images.Length - 1; i++)
            {
                for (int j = i + 1; j < images.Length; j++)
                {
                    double similarity = CompareBitmaps(images[i], images[j]);

                    // These two images are similar
                    if (similarity >= SIMILARITY_THRESHOLD)
                    {
                        // Append information about the discovered similarity to the list of similar images
                        similarImages.Add(new Similarity(i, j, similarity));
                    }

                    // Increment the combination counter
                    if (++combinationsChecked % PRINT_COMPARING_PROGRESS_EVERY_N == 0)
                    {
                        // Print the progress every N checked combinations
                        Console.WriteLine("Comparing: {0:0.##}% ({1} / {2})", ((double)combinationsChecked / combinationsAvailable * 100.0), combinationsChecked, combinationsAvailable);
                    }
                }
            }

            Console.WriteLine("Comparing: Done!");

            // Print all the discovered similarities
            foreach (Similarity sim in similarImages)
            {
                Console.WriteLine("[{0} | {1}] {2}", files[sim.FirstIndex], files[sim.SecondIndex], sim.Coefficient);
            }
        }

        private static int GetColorDifference(Color col1, Color col2)
        {
            // Return the sum of absolute values of color channel differences
            return Math.Abs(col1.R - col2.R) +
                Math.Abs(col1.G - col2.G) +
                Math.Abs(col1.B - col2.B);
        }

        // Returns the number of possible combinations you can make with a specified number of images
        private static long GetNumberOfCombinations(int numberOfImages)
        {
            long combinations = 0;

            for (int i = 0; i < numberOfImages - 1; i++)
            {
                combinations += numberOfImages - i - 1;
            }

            return combinations;
        }

        private static Bitmap GetScaledImage(string imagePath)
        {
            using (Bitmap bmpFullSize = new Bitmap(imagePath))
            {
                // Determine the size of the scaled bitmap
                double aspectRatio = (double)bmpFullSize.Width / bmpFullSize.Height;
                int width = MAX_DIMENSION;
                int height = MAX_DIMENSION;

                // width > height
                if (aspectRatio > 1.0)
                {
                    height = GetShorterDimension(aspectRatio);
                }
                // height > width
                else if (aspectRatio < 1.0)
                {
                    width = GetShorterDimension(aspectRatio);
                }

                Bitmap bmpScaled = new Bitmap(width, height);

                // Scale the bitmap
                using (Graphics g = Graphics.FromImage(bmpScaled))
                {
                    // The following settings improve the scaling quality and help get more accurate results
                    // However they increase the time required to scale each image by ~50%
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                    // Copy the scaled image
                    g.DrawImage(bmpFullSize, new Rectangle(0, 0, width, height));
                }

                return bmpScaled;
            }
        }

        private static int GetShorterDimension(double aspectRatio)
        {
            return (int)Math.Round(MAX_DIMENSION / aspectRatio);
        }

        private static void Main(string[] args)
        {
            // There is supposed to be a single argument - path of the directory containing the images meant for comparison
            if (args.Length == 1)
            {
                FindDuplicates(args[0]);
            }
            else
            {
                Console.WriteLine("Invalid arguments! Expected source directory path.");
            }
        }

        private class Similarity
        {
            public double Coefficient;
            public int FirstIndex;
            public int SecondIndex;

            public Similarity(int first, int second, double coefficient)
            {
                FirstIndex = first;
                SecondIndex = second;
                Coefficient = coefficient;
            }
        }
    }
}