using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DuplicateImageFinder
{
    public partial class FormConfirmation : Form
    {
        // Higher value
        //private static readonly Color COLOR_HIGHER = Color.LimeGreen;
        private static readonly Color COLOR_HIGHER = Color.ForestGreen;

        // Lower value
        private static readonly Color COLOR_LOWER = Color.Red;

        // Both images share the same value
        private static readonly Color COLOR_EQUAL = Color.LightSlateGray;

        // Path of the file marked as an original
        private readonly string PATH_ORIGINAL;

        // Path of the file marked as a duplicate
        private readonly string PATH_DUPLICATE;

        /// <summary>
        /// Initializes the form with the two provided images and displays their similarity coefficient.
        /// </summary>
        /// <param name="pathOriginal">Path of the original image file.</param>
        /// <param name="pathDuplicate">Path of the duplicate image file.</param>
        /// <param name="similarity">Similarity coefficient of the two images.</param>
        public FormConfirmation(string pathOriginal, long fileSizeOriginal, string pathDuplicate, long fileSizeDuplicate, double similarity)
        {
            InitializeComponent();

            // Open both of the images from their respective files
            Image imgOriginal = Image.FromFile(pathOriginal);
            Image imgDuplicate = Image.FromFile(pathDuplicate);

            // Display both images in their picture boxes
            pictureBoxOriginal.Image = imgOriginal;
            pictureBoxDuplicate.Image = imgDuplicate;

            // Get information about each image
            Size pixelSizeOriginal = imgOriginal.Size;
            Size pixelSizeDuplicate = imgDuplicate.Size;

            // Display information about each image
            labelOriginalResolution.Text = GetFormattedResolution(pixelSizeOriginal);
            labelOriginalFileSize.Text = GetFormattedFileSizeInKB(fileSizeOriginal);

            labelDuplicateResolution.Text = GetFormattedResolution(pixelSizeDuplicate);
            labelDuplicateFileSize.Text = GetFormattedFileSizeInKB(fileSizeDuplicate);

            // Use colors to make it easier for the user to tell which of the images has a higher resolution / file size

            // Resolution
            int resolutionOriginal = pixelSizeOriginal.Width * pixelSizeOriginal.Height;
            int resolutionDuplicate = pixelSizeDuplicate.Width * pixelSizeDuplicate.Height;

            // The original has a higher resolution than the duplicate
            if (resolutionOriginal > resolutionDuplicate)
            {
                labelOriginalResolution.ForeColor = COLOR_HIGHER;
                labelDuplicateResolution.ForeColor = COLOR_LOWER;
            }
            // The duplicate should never have a higher resolution then the original
            // because then it would have been marked as the original image instead
            else if (resolutionOriginal < resolutionDuplicate)
            {
                labelOriginalResolution.ForeColor = COLOR_LOWER;
                labelDuplicateResolution.ForeColor = COLOR_HIGHER;
            }
            // Both images have equal resolution
            else
            {
                labelOriginalResolution.ForeColor = labelDuplicateResolution.ForeColor = COLOR_EQUAL;
            }

            // File size
            // Original image file is larger than the duplicate
            if (fileSizeOriginal > fileSizeDuplicate)
            {
                labelOriginalFileSize.ForeColor = COLOR_HIGHER;
                labelDuplicateFileSize.ForeColor = COLOR_LOWER;
            }
            // Original image file is smaller
            else if (fileSizeOriginal < fileSizeDuplicate)
            {
                labelOriginalFileSize.ForeColor = COLOR_LOWER;
                labelDuplicateFileSize.ForeColor = COLOR_HIGHER;
            }
            // Both image files have the same size
            else
            {
                labelOriginalFileSize.ForeColor = COLOR_EQUAL;
                labelDuplicateFileSize.ForeColor = COLOR_EQUAL;
            }

            // Display the similarity coefficient
            labelSimilarity.Text = string.Format("Similarity: {0:0.##%}", similarity);

            // Copy the file paths
            PATH_ORIGINAL = pathOriginal;
            PATH_DUPLICATE = pathDuplicate;
        }

        private void ButtonKeep_Click(object sender, EventArgs e)
        {
            // Keeps both images, only closes the window
            DisposeAndClose(true);
        }

        private void ButtonDeleteDuplicate_Click(object sender, EventArgs e)
        {
            // Deletes the duplicate image and closes the window
            // The duplicate image must be first disposed before being deleted
            // to avoid the "file is used by another process" error
            pictureBoxDuplicate.Image.Dispose();
            File.Delete(PATH_DUPLICATE);

            DisposeAndClose(true, false);
        }

        private void ButtonDeleteOriginal_Click(object sender, EventArgs e)
        {
            // Deletes the original image and closes the window
            // The original image must be first disposed before being deleted
            // to avoid the "file is used by another process" error
            pictureBoxOriginal.Image.Dispose();
            File.Delete(PATH_ORIGINAL);

            DisposeAndClose(false, true);
        }

        private void PictureBoxOriginal_Click(object sender, EventArgs e)
        {
            // Displays the original picture in a separate full screen window
            new FormFullscreenImage(pictureBoxOriginal.Image).ShowDialog();
        }

        private void PictureBoxDuplicate_Click(object sender, EventArgs e)
        {
            // Displays the duplicate picture in a separate full screen window
            new FormFullscreenImage(pictureBoxDuplicate.Image).ShowDialog();
        }

        /// <summary>
        /// Disposes both images and closes the form.
        /// </summary>
        /// <param name="disposeDuplicate">Set this to false if the duplicate image has already been disposed.</param>
        private void DisposeAndClose(bool disposeOriginal = true, bool disposeDuplicate = true)
        {
            // Images are disposed unless specified otherwise

            // Dispose the original image
            if (disposeOriginal)
            {
                pictureBoxOriginal.Image.Dispose();
            }

            // Dispose the duplicate image
            if (disposeDuplicate)
            {
                pictureBoxDuplicate.Image.Dispose();
            }

            // Close the window
            Close();
        }

        /// <summary>
        /// Converts a Size object into a string with a simple format.
        /// </summary>
        /// <param name="resolution">Size object representing an image resolution.</param>
        /// <returns>Returns a formatted string represeting the resolution.</returns>
        private static string GetFormattedResolution(Size resolution)
        {
            return string.Format("{0}×{1}", resolution.Width, resolution.Height);
        }

        /// <summary>
        /// Converts a file size into a formatted string.
        /// </summary>
        /// <param name="fileSize">Size of a file in bytes.</param>
        /// <returns>Returns a formatted string representing the size of a file in kilobytes.</returns>
        private static string GetFormattedFileSizeInKB(long fileSize)
        {
            return string.Format("{0:0.#} KB", fileSize / 1000.0);
        }
    }
}