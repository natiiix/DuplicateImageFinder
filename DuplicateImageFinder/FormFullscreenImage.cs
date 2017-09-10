using System;
using System.Drawing;
using System.Windows.Forms;

namespace DuplicateImageFinder
{
    public partial class FormFullscreenImage : Form
    {
        public FormFullscreenImage(Image image)
        {
            InitializeComponent();

            // Make the window fill the whole screen
            // The picture box should automatically adjust its location and size
            Bounds = Screen.PrimaryScreen.WorkingArea;

            // Display the image in the picture box
            pictureBoxImage.Image = image;
        }

        private void PictureBoxImage_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormFullscreenImage_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}