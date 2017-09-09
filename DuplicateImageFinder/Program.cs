using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DuplicateImageFinder
{
    public class Program
    {
        private const int PRINT_LOADING_PROGRESS_EVERY_N = 10;
        private const int PRINT_COMPARING_PROGRESS_EVERY_N = PRINT_LOADING_PROGRESS_EVERY_N * 1000;

        // Lowest required similarity coefficient for two bitmaps to be considered similar
        private const double SIMILARITY_THRESHOLD = 0.9;

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

            // Load the images
            ImageData[] images = LoadScaledImages(files);

            // Compare the images
            Similarity[] similarities = CompareImages(images);

            // Print the information about similar images
            PrintSimilarityInfo(similarities, images, files);
        }

        /// <summary>
        /// Loads images from files and returns their scaled versions.
        /// Prints progress information about the loading.
        /// </summary>
        /// <param name="imagePaths">Source paths of the images.</param>
        /// <returns>Returns an array of scaled images.</returns>
        private static ImageData[] LoadScaledImages(string[] imagePaths)
        {
            ImageData[] images = new ImageData[imagePaths.Length];

            // Original synchronous code (obsolete)
            //for (int i = 0; i < imagePaths.Length; i++)
            //{
            //    // Get the image data from a file
            //    images[i] = new ImageData(imagePaths[i]);

            //    // Number of already scaled images
            //    int progress = i + 1;

            //    // Print the loading progress every N scaled images
            //    if (progress % PRINT_LOADING_PROGRESS_EVERY_N == 0)
            //    {
            //        Console.WriteLine("Loading: {0:0.##}% ({1} / {2})", ((double)progress / imagePaths.Length * 100.0), progress, imagePaths.Length);
            //    }
            //}

            // Create and start all the tasks
            Task<ImageData>[] scalingTasks = new Task<ImageData>[imagePaths.Length];

            for (int i = 0; i < imagePaths.Length; i++)
            {
                // The index needs to be copied to avoid being rewritten before being actually used
                int pathIndex = i;

                scalingTasks[i] = Task.Run(() =>
                {
                    // Get the image data from a file
                    return new ImageData(imagePaths[pathIndex]);
                });
            }

            Console.WriteLine("Loading and scaling images...");

            // Wait for all the scaling tasks to complete
            Task.WaitAll(scalingTasks);

            // Retrieve the scaled images
            for (int i = 0; i < images.Length; i++)
            {
                images[i] = scalingTasks[i].Result;
            }

            Console.WriteLine("Loading: Done!");

            return images;
        }

        /// <summary>
        /// Compares all the input images with each other and returns information about similar image pairs.
        /// </summary>
        /// <param name="images">An array of images to compare.</param>
        /// <returns>Returns an array of all the similarities found in the input image array.</returns>
        private static Similarity[] CompareImages(ImageData[] images)
        {
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
                    // Compare the two images and get their similarity coefficient
                    double similarity = images[i].Compare(images[j]);

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

            // Return the list of similar image pairs as an array
            return similarImages.ToArray();
        }

        /// <summary>
        /// Prints information about discovered similar images.
        /// </summary>
        /// <param name="similarities">Information about all the similar image pairs that were found.</param>
        /// <param name="images">Image data of all the images.</param>
        /// <param name="files">File names of all the images.</param>
        private static void PrintSimilarityInfo(Similarity[] similarities, ImageData[] images, string[] files)
        {
            // Print all the discovered similarities
            foreach (Similarity sim in similarities)
            {
                // Determine which of the similar images has a higher resolution
                int higherResIdx = ImageData.GetHigherResolutionIndex(images[sim.FirstIndex], images[sim.SecondIndex]);

                // The images have the same resolution
                if (higherResIdx == 0)
                {
                    Console.WriteLine("Similar images (equal resolution): {0} | {1} ({2:0.##}% similarity)",
                        files[sim.FirstIndex],
                        files[sim.SecondIndex],
                        sim.Coefficient * 100.0);
                }
                else
                {
                    // Get index of the larger and the smaller image (to make printing the message easier)
                    // First assume that the first image is larger
                    int idxLarger = sim.FirstIndex;
                    int idxSmaller = sim.SecondIndex;

                    // The second image is larger
                    if (higherResIdx == 2)
                    {
                        idxLarger = sim.SecondIndex;
                        idxSmaller = sim.FirstIndex;
                    }

                    Console.WriteLine("Duplicate of {0}: {1} ({2:0.##}% similarity)",
                        files[idxLarger],
                        files[idxSmaller],
                        sim.Coefficient * 100.0);
                }
            }
        }

        /// <summary>
        /// Calculates the number of combinations that will be checked in the similarity checking process.
        /// </summary>
        /// <param name="numberOfImages">The number of images that are being checked.</param>
        /// <returns>Returns the number of image combinations.</returns>
        private static long GetNumberOfCombinations(int numberOfImages)
        {
            long combinations = 0;

            for (int i = 0; i < numberOfImages - 1; i++)
            {
                combinations += numberOfImages - i - 1;
            }

            return combinations;
        }

        /// <summary>
        /// The main function.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
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

        /// <summary>
        /// Class for storing all the important information about an image.
        /// </summary>
        private class ImageData
        {
            // Size of the longest dimension of the scaled bitmap
            private const int MAX_DIMENSION = 64;

            public int FullWidth;
            public int FullHeight;

            public int ScaledWidth;
            public int ScaledHeight;

            private byte[] RawBytes;

            /// <summary>
            /// Initializes an ImageData object from a file.
            /// </summary>
            /// <param name="imagePath">Path of the source image file.</param>
            public ImageData(string imagePath)
            {
                // Get the scaled version of a bitmap from file
                using (Bitmap bmp = GetScaledImage(imagePath, out FullWidth, out FullHeight))
                {
                    // Copy the information about size of the scaled image
                    ScaledWidth = bmp.Width;
                    ScaledHeight = bmp.Height;

                    // Lock the bitmap's bits
                    Rectangle rect = new Rectangle(0, 0, ScaledWidth, ScaledHeight);
                    BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);

                    // Get the address of the first line
                    IntPtr ptr = bmpData.Scan0;

                    // Declare an array to hold the bytes of the bitmap
                    int bytes = bmpData.Stride * ScaledHeight;
                    RawBytes = new byte[bytes];

                    // Copy the RGB values into the array
                    Marshal.Copy(ptr, RawBytes, 0, bytes);

                    // Unlock the bits
                    bmp.UnlockBits(bmpData);
                }
            }

            /// <summary>
            /// Calculates the length of the shorter dimension of a scaled bitmap based on the aspect ratio of the image.
            /// </summary>
            /// <param name="aspectRatio">Aspect ratio of the image being scaled.</param>
            /// <returns>Returns the length of the shorter dimension.</returns>
            private static int GetShorterDimension(double aspectRatio)
            {
                return (int)Math.Round(aspectRatio >= 1.0 ? MAX_DIMENSION / aspectRatio : MAX_DIMENSION * aspectRatio);
            }

            /// <summary>
            /// Load a bitmap from file and scales it so that the longest dimension is equal to the preset value.
            /// </summary>
            /// <param name="imagePath">Path of the source image file.</param>
            /// <param name="fullWidth">Width of the original image.</param>
            /// <param name="fullHeight">Height of the original image.</param>
            /// <returns>Returns a scaled bitmap.</returns>
            private static Bitmap GetScaledImage(string imagePath, out int fullWidth, out int fullHeight)
            {
                using (Bitmap bmpFullSize = new Bitmap(imagePath))
                {
                    // Return the full size of the original image
                    fullWidth = bmpFullSize.Width;
                    fullHeight = bmpFullSize.Height;

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

                    // RGB works better than ARGB in most cases
                    // The images are expected not to have any transparency
                    Bitmap bmpScaled = new Bitmap(width, height, PixelFormat.Format24bppRgb);

                    // Scale the bitmap
                    using (Graphics g = Graphics.FromImage(bmpScaled))
                    {
                        // The following settings can be used to improve the scaling quality and to get more accurate results.
                        // However they tend to increase the time required to scale each image quite significantly.
                        /*g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;*/

                        // Copy the scaled image
                        g.DrawImage(bmpFullSize, new Rectangle(0, 0, width, height));
                    }

                    return bmpScaled;
                }
            }

            /// <summary>
            /// Compares this image with another image and returns their similarity coefficient.
            /// Similarity 0.0 => The images are completely different.
            /// Similarity 1.0 => The Images are completely similar.
            /// </summary>
            /// <param name="imgData">The image to compare with.</param>
            /// <returns>Returns a similarity coefficient of the two images.</returns>
            public double Compare(ImageData imgData)
            {
                // If the images have different shape, they cannot be similar
                if (imgData.ScaledWidth != ScaledWidth ||
                    imgData.ScaledHeight != ScaledHeight ||
                    imgData.RawBytes.Length != RawBytes.Length)
                {
                    return 0.0;
                }

                // Calculate the difference between the raw image bytes
                int difference = 0;

                for (int i = 0; i < RawBytes.Length; i++)
                {
                    difference += Math.Abs(imgData.RawBytes[i] - RawBytes[i]);
                }

                // Calculate the relative image difference
                double relativeDifference = (double)difference / (RawBytes.Length * byte.MaxValue);
                // Get the similarity coefficient from it
                return 1 - (relativeDifference);
            }

            /// <summary>
            /// Calculates and returns the full resolution of the image in pixels.
            /// </summary>
            /// <returns>Returns the resolution of the original image in pixels.</returns>
            private int GetFullResolution()
            {
                return FullWidth * FullHeight;
            }

            /// <summary>
            /// Compares the resolution of two images and returns the index of the image with a higher resolution.
            /// 0 => The images have equal resolution.
            /// 1 => The first image has a higher resolution.
            /// 2 => The second image has a higher resolution.
            /// </summary>
            /// <param name="imgData1">First image.</param>
            /// <param name="imgData2">Second image.</param>
            /// <returns>Index of the image with a higher resolution.</returns>
            public static int GetHigherResolutionIndex(ImageData imgData1, ImageData imgData2)
            {
                int res1 = imgData1.GetFullResolution();
                int res2 = imgData2.GetFullResolution();

                // The first image has a higher resolution
                if (res1 > res2)
                {
                    return 1;
                }
                // The second image has a higher resolution
                else if (res2 > res1)
                {
                    return 2;
                }
                // Both images have the same resolution
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Class for storing information about similarity between two images.
        /// </summary>
        private class Similarity
        {
            public int FirstIndex;
            public int SecondIndex;
            public double Coefficient;

            public Similarity(int first, int second, double coefficient)
            {
                FirstIndex = first;
                SecondIndex = second;
                Coefficient = coefficient;
            }
        }
    }
}