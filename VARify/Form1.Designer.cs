using OpenCvSharp;

using VARify.model;
using EventArgs = System.EventArgs;
using Point = OpenCvSharp.Point;

namespace VARify;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    private PictureBox pictureBox;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 700);
        this.Text = "VARify";

        // Ajouter un bouton pour télécharger l'image
        Button uploadButton = new Button();
        uploadButton.Text = "Télécharger l'image";
        uploadButton.Size = new System.Drawing.Size(150, 40);
        uploadButton.Location = new System.Drawing.Point(50, 50);
        uploadButton.Click += new EventHandler(this.UploadButton_Click);
        this.Controls.Add(uploadButton);

        // Initialiser la variable membre pictureBox
        this.pictureBox = new PictureBox();
        this.pictureBox.Size = new System.Drawing.Size(600, 500);
        this.pictureBox.Location = new System.Drawing.Point(50, 100);
        this.pictureBox.BorderStyle = BorderStyle.FixedSingle;
        this.pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Afficher toute l'image en conservant les proportions
        this.Controls.Add(this.pictureBox);
    }

    private void UploadButton_Click(object sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Fichiers image|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string filePath = openFileDialog.FileName;
            this.pictureBox.Image = Image.FromFile(filePath);
        }
    }
    
}
    