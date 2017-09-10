namespace DuplicateImageFinder
{
    partial class FormConfirmation
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBoxOriginal = new System.Windows.Forms.PictureBox();
            this.pictureBoxDuplicate = new System.Windows.Forms.PictureBox();
            this.buttonKeep = new System.Windows.Forms.Button();
            this.labelSimilarity = new System.Windows.Forms.Label();
            this.labelOriginalResolution = new System.Windows.Forms.Label();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.labelOriginalFileSize = new System.Windows.Forms.Label();
            this.labelDuplicateFileSize = new System.Windows.Forms.Label();
            this.labelDuplicateResolution = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOriginal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDuplicate)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxOriginal
            // 
            this.pictureBoxOriginal.Location = new System.Drawing.Point(12, 40);
            this.pictureBoxOriginal.Name = "pictureBoxOriginal";
            this.pictureBoxOriginal.Size = new System.Drawing.Size(360, 360);
            this.pictureBoxOriginal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxOriginal.TabIndex = 8;
            this.pictureBoxOriginal.TabStop = false;
            this.pictureBoxOriginal.Click += new System.EventHandler(this.PictureBoxOriginal_Click);
            // 
            // pictureBoxDuplicate
            // 
            this.pictureBoxDuplicate.Location = new System.Drawing.Point(378, 40);
            this.pictureBoxDuplicate.Name = "pictureBoxDuplicate";
            this.pictureBoxDuplicate.Size = new System.Drawing.Size(360, 360);
            this.pictureBoxDuplicate.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxDuplicate.TabIndex = 7;
            this.pictureBoxDuplicate.TabStop = false;
            this.pictureBoxDuplicate.Click += new System.EventHandler(this.PictureBoxDuplicate_Click);
            // 
            // buttonKeep
            // 
            this.buttonKeep.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonKeep.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonKeep.Location = new System.Drawing.Point(12, 453);
            this.buttonKeep.Name = "buttonKeep";
            this.buttonKeep.Size = new System.Drawing.Size(360, 36);
            this.buttonKeep.TabIndex = 0;
            this.buttonKeep.Text = "Keep";
            this.buttonKeep.UseVisualStyleBackColor = true;
            this.buttonKeep.Click += new System.EventHandler(this.ButtonKeep_Click);
            // 
            // labelSimilarity
            // 
            this.labelSimilarity.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSimilarity.Location = new System.Drawing.Point(254, 403);
            this.labelSimilarity.Name = "labelSimilarity";
            this.labelSimilarity.Size = new System.Drawing.Size(240, 47);
            this.labelSimilarity.TabIndex = 4;
            this.labelSimilarity.Text = "Similarity";
            this.labelSimilarity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelOriginalResolution
            // 
            this.labelOriginalResolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOriginalResolution.Location = new System.Drawing.Point(12, 408);
            this.labelOriginalResolution.Name = "labelOriginalResolution";
            this.labelOriginalResolution.Size = new System.Drawing.Size(236, 20);
            this.labelOriginalResolution.TabIndex = 5;
            this.labelOriginalResolution.Text = "Original Resolution";
            this.labelOriginalResolution.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonDelete
            // 
            this.buttonDelete.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDelete.Location = new System.Drawing.Point(378, 453);
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.Size = new System.Drawing.Size(360, 36);
            this.buttonDelete.TabIndex = 1;
            this.buttonDelete.Text = "Delete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.ButtonDelete_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(360, 24);
            this.label1.TabIndex = 9;
            this.label1.Text = "Original";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(378, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(360, 24);
            this.label2.TabIndex = 10;
            this.label2.Text = "Duplicate";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelOriginalFileSize
            // 
            this.labelOriginalFileSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOriginalFileSize.Location = new System.Drawing.Point(12, 428);
            this.labelOriginalFileSize.Name = "labelOriginalFileSize";
            this.labelOriginalFileSize.Size = new System.Drawing.Size(236, 22);
            this.labelOriginalFileSize.TabIndex = 11;
            this.labelOriginalFileSize.Text = "Original File Size";
            this.labelOriginalFileSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelDuplicateFileSize
            // 
            this.labelDuplicateFileSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDuplicateFileSize.Location = new System.Drawing.Point(500, 428);
            this.labelDuplicateFileSize.Name = "labelDuplicateFileSize";
            this.labelDuplicateFileSize.Size = new System.Drawing.Size(238, 22);
            this.labelDuplicateFileSize.TabIndex = 13;
            this.labelDuplicateFileSize.Text = "Duplicate File Size";
            this.labelDuplicateFileSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelDuplicateResolution
            // 
            this.labelDuplicateResolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDuplicateResolution.Location = new System.Drawing.Point(500, 408);
            this.labelDuplicateResolution.Name = "labelDuplicateResolution";
            this.labelDuplicateResolution.Size = new System.Drawing.Size(238, 20);
            this.labelDuplicateResolution.TabIndex = 12;
            this.labelDuplicateResolution.Text = "Duplicate Resolution";
            this.labelDuplicateResolution.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FormConfirmation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 501);
            this.Controls.Add(this.labelDuplicateFileSize);
            this.Controls.Add(this.labelDuplicateResolution);
            this.Controls.Add(this.labelOriginalFileSize);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelOriginalResolution);
            this.Controls.Add(this.labelSimilarity);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonKeep);
            this.Controls.Add(this.pictureBoxDuplicate);
            this.Controls.Add(this.pictureBoxOriginal);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "FormConfirmation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Confirmation";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOriginal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDuplicate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxOriginal;
        private System.Windows.Forms.PictureBox pictureBoxDuplicate;
        private System.Windows.Forms.Button buttonKeep;
        private System.Windows.Forms.Label labelSimilarity;
        private System.Windows.Forms.Label labelOriginalResolution;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelOriginalFileSize;
        private System.Windows.Forms.Label labelDuplicateFileSize;
        private System.Windows.Forms.Label labelDuplicateResolution;
    }
}