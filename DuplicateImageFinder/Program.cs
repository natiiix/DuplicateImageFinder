using System;
using System.Drawing;
using System.IO;

namespace DuplicateImageFinder
{
    public class Program
    {
        // Size of the longer dimension of the scaled bitmap
        private const int MAX_DIMENSION = 64;

        // Lowest required similarity coefficient for two bitmaps to be considered similar
        private const double SIMILARITY_THRESHOLD = 0.9;

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
                images[i] = GetScaledImage(files[i]);
            }

            // Compare the bitmaps
            for (int i = 0; i < images.Length - 1; i++)
            {
                for (int j = i + 1; j < images.Length; j++)
                {
                    double similarity = CompareBitmaps(images[i], images[j]);

                    if (similarity > SIMILARITY_THRESHOLD)
                    {
                        Console.WriteLine("[{0} | {1}] {2}", files[i], files[j], similarity);
                    }
                }
            }
        }

        private static int GetColorDifference(Color col1, Color col2)
        {
            // Return the sum of absolute values of color channel differences
            return Math.Abs(col1.R - col2.R) +
                Math.Abs(col1.G - col2.G) +
                Math.Abs(col1.B - col2.B);
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
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    //g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

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
            // There is supposed to be one argument - path of the source directory
            if (args.Length == 1)
            {
                FindDuplicates(args[0]);
            }
            else
            {
                Console.WriteLine("Invalid arguments! Expected source directory path.");
            }

            // Let user see the output before closing the console
            Console.Write("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}