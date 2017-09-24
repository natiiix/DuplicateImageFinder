using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DuplicateImageFinder
{
    public class Program
    {
        //private const int PRINT_LOADING_PROGRESS_EVERY_N = 10;
        private const int PRINT_COMPARING_PROGRESS_EVERY_N = 20000;

        // Lowest required similarity coefficient for two bitmaps to be considered similar
        private const double SIMILARITY_THRESHOLD = 0.9;

        // Suffix that is added to the name of the directory in which the scaled images are stored
        private const string SCALED_DIR_SUFFIX = "_scaled";

        // Extension used for serialized image data objects
        private const string SCALED_BINARY_EXTENSION = "dat";

        // An array of file extensions that will be considered as images
        private static readonly string[] IMAGE_FILE_EXTENSIONS = { "bmp", "png", "jpg", "jpeg", "gif", "tiff" };

        /// <summary>
        /// Converts the source path to a path for scaled images.
        /// </summary>
        /// <param name="sourcePath">Source path.</param>
        /// <returns>Returns full path of the directory for scaled images.</returns>
        private static string PathSourceToScaled(string sourcePath)
        {
            // Convert the source path to full path
            string fullSourcePath = Path.GetFullPath(sourcePath);

            // Determine whether the path points to a file or a directory
            bool isFile = false;

            // If the path points to a file
            if (File.Exists(fullSourcePath))
            {
                isFile = true;
            }
            // If the path points to a directory
            else if (Directory.Exists(fullSourcePath))
            {
                isFile = false;
            }
            // Invalid path
            else
            {
                throw new ArgumentException("Invalid path.");
            }

            // Split the path into individual parts
            string[] parts = sourcePath.Split(
                new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);

            // There must be at least two parts in a proper full path
            if (parts.Length < 2)
            {
                throw new ArgumentException("Invalid path.");
            }

            // Add a suffix to the name of the last directory in the path
            parts[parts.Length - (isFile ? 2 : 1)] += SCALED_DIR_SUFFIX;

            // Scaled versions of files must have a different extension because they are binary
            if (isFile)
            {
                // Get the index of the file name part of the path
                int fileNameIdx = parts.Length - 1;

                // Replace the extensions of the source file with the binary extension
                parts[fileNameIdx] = string.Join(".", parts[fileNameIdx].Split('.')[0], SCALED_BINARY_EXTENSION);
            }

            // Join all the parts of the path back together and return it
            return string.Join(Path.DirectorySeparatorChar.ToString(), parts);
        }

        /// <summary>
        /// Gets image data objects of all images in a specified directory.
        /// </summary>
        /// <param name="imageDir">Path of directory to load the image from.</param>
        /// <param name="imagePaths">Output array for returning paths of all the images.</param>
        /// <returns>Returns an array of image data.</returns>
        private static ImageData[] GetImageDataFromDirectory(string imageDir, out string[] imagePaths)
        {
            // Make sure the source directory exists
            if (!Directory.Exists(imageDir))
            {
                Console.WriteLine("Invalid source directory path!");

                imagePaths = new string[0];
                return new ImageData[0];
            }

            // Get path of the directory for scaled images
            string pathScaled = PathSourceToScaled(imageDir);

            // Create the directory for scaled images if it doesn't already exist
            Directory.CreateDirectory(pathScaled);

            // Get a list of files in the source directory and filter out all the non-image files
            imagePaths = FilterImageFiles(Directory.GetFiles(imageDir));

            // Load and sacle all the images and return their image data
            return LoadScaledImages(imagePaths);
        }

        /// <summary>
        /// Takes an array of strings containing all the files in a directory and filters out
        /// files that are not images (text files, etc.).
        /// </summary>
        /// <param name="allFiles">Array of all the file names.</param>
        /// <returns>Returns an array of file names that represent images.</returns>
        private static string[] FilterImageFiles(string[] allFiles)
        {
            // Create a list to store the file names of image files
            List<string> imageFiles = new List<string>();

            // Iterate through all the files
            foreach (string fileName in allFiles)
            {
                // For each of the files iterate through all the recognized image file extensions
                foreach (string ext in IMAGE_FILE_EXTENSIONS)
                {
                    // If the file name has an image file extension
                    if (fileName.EndsWith("." + ext))
                    {
                        // Add the file name to the list of image files and break out of the extension-checking foreach
                        imageFiles.Add(fileName);
                        break;
                    }
                }
            }

            // Return the list of image files as an array
            return imageFiles.ToArray();
        }

        /// <summary>
        /// Finds duplicate images in a specified directory.
        /// </summary>
        /// <param name="imageDir">Path of the directory to be checked for duplicate images.</param>
        private static void FindDuplicatesWithinDirectory(string imageDir)
        {
            // Get the image data of images in the source directory
            ImageData[] images = GetImageDataFromDirectory(imageDir, out string[] imagePaths);

            // There must be at least two images for any comparison to be possible
            if (images.Length < 2)
            {
                Console.WriteLine("Insufficient amount of source images!");
                return;
            }

            // Compare the images
            Similarity[] similarities = CompareImagesWithEachOther(images);

            // Print the information about similar images and
            // let the user decide which of the images are supposed to be deleted
            ResolveSimilarImages(similarities, images, imagePaths);
        }

        /// <summary>
        /// Finds images similar to a single image in a directory with images.
        /// </summary>
        /// <param name="imageDir">Path of directory with images to compare against.</param>
        /// <param name="singleImagePath">Path of single image to compare with images from the directory.</param>
        private static void FindDuplicatesOfSingleFile(string imageDir, string singleImagePath)
        {
            // Make sure the single image file exists
            if (!File.Exists(singleImagePath))
            {
                Console.WriteLine("The specified single image file does not exist!");
                return;
            }

            // Load the images from the source directory
            ImageData[] images = GetImageDataFromDirectory(imageDir, out string[] imagePaths);

            if (images.Length < 1)
            {
                Console.WriteLine("Nothing to compare against! At least one image file is required in the source directory.");
                return;
            }

            // Get image data of the single image
            // Its image data are not supposed to be serialized to avoid creating pointless directories / files
            ImageData singleImage = new ImageData(singleImagePath);

            // Compare the images
            Similarity[] similarities = CompareImagesWithSingleImage(images, singleImage);

            // Append the image data of the single image to the array of image data from the source directory
            List<ImageData> listImageData = new List<ImageData>();
            listImageData.AddRange(images);
            listImageData.Add(singleImage);

            // Add the path of the single image to the paths of the image from the source directory
            List<string> listImagePaths = new List<string>();
            listImagePaths.AddRange(imagePaths);
            listImagePaths.Add(singleImagePath);

            // Print the information about similar images and
            // let the user decide which of the images are supposed to be deleted
            ResolveSimilarImages(similarities, listImageData.ToArray(), listImagePaths.ToArray());
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
                    return GetImageData(imagePaths[pathIndex]);
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
        /// Finds or generates image data object from an image at specified path.
        /// Image data objects are serialized into a special folder to make future use of them faster.
        /// </summary>
        /// <param name="imagePath">Path of the image for which the image data are requested.</param>
        /// <returns>Returns image data of an image with the specified path.</returns>
        private static ImageData GetImageData(string imagePath)
        {
            // Get the path to the image data serialization file for this image
            string scaledPath = PathSourceToScaled(imagePath);

            ImageData imgData = null;

            // If this image file already has a serialized image data object
            if (File.Exists(scaledPath))
            {
                // Deserialize the image data object from its serialization file
                using (Stream s = new FileStream(scaledPath, FileMode.Open, FileAccess.Read))
                {
                    imgData = (ImageData)new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(s);
                }
            }
            // If image data for this image file haven't been serialized yet
            else
            {
                imgData = new ImageData(imagePath);

                // Serialize the image data object to its serialization file
                using (Stream s = new FileStream(scaledPath, FileMode.Create, FileAccess.Write))
                {
                    new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(s, imgData);
                }
            }

            // Return the image data object
            return imgData;
        }

        /// <summary>
        /// Retrieves the size of a file in bytes and returns it.
        /// </summary>
        /// <param name="filePath">Path of the file whose size is requested.</param>
        /// <returns>Returns the size of the specified file in bytes.</returns>
        private static long GetFileSize(string filePath)
        {
            return new FileInfo(filePath).Length;
        }

        /// <summary>
        /// Compares all the input images with each other and returns information about similar image pairs.
        /// </summary>
        /// <param name="images">An array of images to compare.</param>
        /// <returns>Returns an array of all the similarities found in the input image array.</returns>
        private static Similarity[] CompareImagesWithEachOther(ImageData[] images)
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
        /// Compares a single image with each image in an array of image data.
        /// </summary>
        /// <param name="images">Array of images to compare the single image with.</param>
        /// <param name="singleImage">Image data of the single image.</param>
        /// <returns>Returns discovered similarities between the single image and the array of images.</returns>
        private static Similarity[] CompareImagesWithSingleImage(ImageData[] images, ImageData singleImage)
        {
            // Create a list to hold all the information regarding similar images
            List<Similarity> similarImages = new List<Similarity>();

            // Compare the single image to every image in the array of image data
            for (int i = 0; i < images.Length; i++)
            {
                // Get the similarity coefficient between the two images
                double similarity = singleImage.Compare(images[i]);

                // These two images are similar
                if (similarity >= SIMILARITY_THRESHOLD)
                {
                    // Append information about the discovered similarity to the list of similar images
                    // The single image is represented by the second image in the similarity because in case of an exact
                    // similarity, the second image will be marked as a duplicate and can be deleted
                    // It has an index 1 higher than the currently highest index because it will be appended to it later
                    similarImages.Add(new Similarity(i, images.Length, similarity));
                }

                // This needs to be done because indexes start at 0
                int progress = i + 1;

                // Print the progress every N iterations
                if (progress % PRINT_COMPARING_PROGRESS_EVERY_N == 0)
                {
                    Console.WriteLine("Comparing: {0:0.##}% ({1} / {2})", ((double)progress / images.Length * 100.0), progress, images.Length);
                }
            }

            Console.WriteLine("Comparing: Done!");

            // Return the list of similar image pairs as an array
            return similarImages.ToArray();
        }

        /// <summary>
        /// Prints information about discovered similar images.
        /// Then prompts the user to choose whether they want to delete the duplicate image or not.
        /// </summary>
        /// <param name="similarities">Information about all the similar image pairs that were found.</param>
        /// <param name="images">Image data of all the images.</param>
        /// <param name="imagePaths">File names of all the images.</param>
        private static void ResolveSimilarImages(Similarity[] similarities, ImageData[] images, string[] imagePaths)
        {
            // This needs to be called in order to get the modern looking buttons in our dialog.
            // Without this call they will look like the default system buttons from Windows 95/98.
            System.Windows.Forms.Application.EnableVisualStyles();

            // Print all the discovered similarities
            foreach (Similarity sim in similarities)
            {
                // Skip this similar image pair if either of the images doesn't exist anymore
                if (!File.Exists(imagePaths[sim.FirstIndex]) || !File.Exists(imagePaths[sim.SecondIndex]))
                {
                    continue;
                }

                // Get size of each of the images
                long fileSizeFirst = GetFileSize(imagePaths[sim.FirstIndex]);
                long fileSizeSecond = GetFileSize(imagePaths[sim.SecondIndex]);

                // Determine which of the similar images has a higher resolution
                int resFirst = images[sim.FirstIndex].GetFullResolution();
                int resSecond = images[sim.SecondIndex].GetFullResolution();

                // Indicates whether the first image is considered a duplicate or not
                // Duplicate image is the one with a lower resolution
                // In case both images have the same resolution, file size becomes the determining factor
                // Higher file size indicates a potentially higher image quality
                // If both properties are equal for both images, the first image is considered to be the original
                bool firstIsDuplicate = (resSecond > resFirst || (resFirst == resSecond && fileSizeSecond > fileSizeFirst));

                // Get the path of each of the image files
                string pathOriginal = (firstIsDuplicate ? imagePaths[sim.SecondIndex] : imagePaths[sim.FirstIndex]);
                string pathDuplicate = (firstIsDuplicate ? imagePaths[sim.FirstIndex] : imagePaths[sim.SecondIndex]);

                // Print a message about the discovered duplicate image
                PrintSimilarImageInfo(pathOriginal, pathDuplicate, sim.Coefficient);

                // Get the file size of both images
                long fileSizeOriginal = (firstIsDuplicate ? fileSizeSecond : fileSizeFirst);
                long fileSizeDuplicate = (firstIsDuplicate ? fileSizeFirst : fileSizeSecond);

                // Show a dialog asking the user whether the image marked as a duplicate should be deleted or not
                //(new FormConfirmation(pathOriginal, fileSizeOriginal, pathDuplicate, fileSizeDuplicate, sim.Coefficient)).ShowDialog();

                // If the form is not closed using one of the buttons, don't show the user any more information about similar images
                if (new FormConfirmation(pathOriginal, fileSizeOriginal, pathDuplicate, fileSizeDuplicate, sim.Coefficient).ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Prints information about a discovered similarity between two images.
        /// </summary>
        /// <param name="fileOriginal">Path of the original image file.</param>
        /// <param name="fileDuplicate">Path of the duplicate image file.</param>
        /// <param name="similarity">Similarity coefficient of these two images.</param>
        private static void PrintSimilarImageInfo(string fileOriginal, string fileDuplicate, double similarity)
        {
            Console.WriteLine(Environment.NewLine +
                "Duplicate image found!" + Environment.NewLine +
                "Original: {0}" + Environment.NewLine +
                "Duplicate: {1}" + Environment.NewLine +
                "Similarity: {2:0.##}%",
                fileOriginal, fileDuplicate, similarity * 100.0);
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
        /// The entry point of the program.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        [STAThread]
        private static void Main(string[] args)
        {
            string imageDir = string.Empty;
            string singleImagePath = string.Empty;

            // Source directory has been provided as a command line argument
            if (args.Length == 1 || args.Length == 2)
            {
                // Use the first argument as a path of the image directory
                imageDir = args[0];

                // Arguments also contain the path of the single image file
                if (args.Length == 2)
                {
                    // Copy the path of the file
                    singleImagePath = args[1];
                }
            }
            // No arguments were provided
            else if (args.Length == 0)
            {
                // Ask the user to enter the path to the directory that contains the images to be checked
                Console.Write("Enter path to image source directory: ");
                imageDir = Console.ReadLine();

                // Ask the user to provide the optional single image file path
                Console.Write("Enter path to single image file (leave empty to compare images within the directory): ");
                singleImagePath = Console.ReadLine();
            }
            // More than two command line arguments
            else
            {
                Console.WriteLine("Invalid arguments!" + Environment.NewLine +
                    "Syntax: DuplicateImageFinder [path to image directory [path to single image file]]");
            }

            // Path to a single image file hasn't been provided
            // Find duplicates within the image directory
            if (singleImagePath.Length == 0)
            {
                FindDuplicatesWithinDirectory(imageDir);
            }
            // Path to a single image file has been provided
            // Find duplicates of that image file in the image directory
            else
            {
                FindDuplicatesOfSingleFile(imageDir, singleImagePath);
            }

            // Wait for input before exitting
            Console.Write("Press ENTER to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// Class for storing all the important information about an image.
        /// </summary>
        [Serializable]
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

                // Calculate the stride and how much of it is filled with valid values
                int stride = RawBytes.Length / ScaledHeight;
                int validStride = ScaledWidth * 3;

                for (int i = 0; i < RawBytes.Length; i++)
                {
                    // Skip invalid bytes (those that are outside the image boundaries)
                    if (i % stride >= validStride)
                    {
                        continue;
                    }

                    // Add the difference of this byte to the total difference
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
            public int GetFullResolution()
            {
                return FullWidth * FullHeight;
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