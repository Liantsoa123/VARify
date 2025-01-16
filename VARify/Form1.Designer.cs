using OpenCvSharp;

using VARify.model;
using EventArgs = System.EventArgs;
using Point = OpenCvSharp.Point;

namespace VARify;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    private PictureBox pictureBox;
    private PictureBox pictureAnalyse;
    private List<Cercle> cercles = new List<Cercle>();
    private Color colorTeamHaveBallon;
    private bool isRight; // true si l'équipe qui deffend est à droite, false sinon / sens vers la quelle les attaquant tire le ballon
    private int ligneDeffensive; // position de la ligne deffensive
    private List<Cercle> teamDeffender; 
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
        this.ClientSize = new System.Drawing.Size(1350, 700);
        this.Text = "VARify";

        // Ajouter un bouton pour télécharger l'image
        Button uploadButton = new Button();
        uploadButton.Text = "Télécharger l'image";
        uploadButton.Size = new System.Drawing.Size(150, 40);
        uploadButton.Location = new System.Drawing.Point(50, 50);
        uploadButton.Click += new EventHandler(this.UploadButton_Click);
        this.Controls.Add(uploadButton);
        
        // Ajouter un bouton pour analyser l'image
        Button analyseButton = new Button();
        analyseButton.Text = "Analyser l'image";
        analyseButton.Size = new System.Drawing.Size(150, 40);
        analyseButton.Location = new System.Drawing.Point(250, 50);
        analyseButton.Click += new EventHandler(this.AnalyseButton_Click);
        this.Controls.Add(analyseButton);
        
        // Initialiser la variable membre pictureBox
        this.pictureBox = new PictureBox();
        this.pictureBox.Size = new System.Drawing.Size(600, 500);
        this.pictureBox.Location = new System.Drawing.Point(50, 100);
        this.pictureBox.BorderStyle = BorderStyle.FixedSingle;
        this.pictureBox.SizeMode = PictureBoxSizeMode.Zoom; // Afficher toute l'image en conservant les proportions
        this.Controls.Add(this.pictureBox);
        
        // Initialiser la variable membre pictureAnalyse
        this.pictureAnalyse = new PictureBox();
        this.pictureAnalyse.Size = new System.Drawing.Size(600, 500);
        this.pictureAnalyse.Location = new System.Drawing.Point(700, 100);
        this.pictureAnalyse.BorderStyle = BorderStyle.FixedSingle;
        this.pictureAnalyse.SizeMode = PictureBoxSizeMode.Zoom; // Afficher toute l'image en conservant les proportions
        this.Controls.Add(this.pictureAnalyse);
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
    
    // Détecter les joueurs par couleur en fonction des limites HSV
    private List<Cercle> DetectCerclesByColor(Bitmap image, Scalar lowerBound, Scalar upperBound, Color color)
    {
        List<Cercle> Cercles = new List<Cercle>();
        // Convertir Bitmap en Mat
        Mat matImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(image);

        // Convertir l'image en HSV (Teinte, Saturation, Valeur)
        Mat hsvImage = new Mat();
        Cv2.CvtColor(matImage, hsvImage, ColorConversionCodes.BGR2HSV);

        // Filtrer l'image HSV pour n'obtenir que la couleur souhaitée
        Mat mask = new Mat();
        Cv2.InRange(hsvImage, lowerBound, upperBound, mask);

        // Trouver les contours dans le masque
        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(mask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        // Parcourir les contours pour déterminer les positions des joueurs
        foreach (var contour in contours)
        {
            // Calculer le cercle englobant pour chaque contour
            Point2f center;
            float radius;
            Cv2.MinEnclosingCircle(contour, out center, out radius);

            if (radius > 1) // Ignorer les petits bruits
            {
                Cercles.Add(new Cercle(new System.Drawing.Point((int)center.X, (int)center.Y), color, (decimal)radius));
            }
        }

        return Cercles;
    }

    private void AnalyseButton_Click(object sender, EventArgs e)
    {
        if (pictureBox.Image == null) return;
        
        Bitmap image = new Bitmap(pictureBox.Image);

        // Définir les plages de couleurs HSV pour les joueurs en bleu et en rouge
        Scalar blueLower = new Scalar(100, 150, 0); // Ajuster en fonction de votre image
        Scalar blueUpper = new Scalar(140, 255, 255); // Ajuster en fonction de votre image
        Scalar redLower1 = new Scalar(0, 150, 0); // Ajuster en fonction de votre image
        Scalar redUpper1 = new Scalar(10, 255, 255); // Ajuster en fonction de votre image
        Scalar redLower2 = new Scalar(170, 150, 0); // Ajuster pour le rouge supérieur
        Scalar redUpper2 = new Scalar(180, 255, 255); // Ajuster pour le rouge supérieur
        Scalar blackLower = new Scalar(0, 0, 0); // Plage inférieure pour le noir
        Scalar blackUpper = new Scalar(180, 255, 50); // Plage supérieure pour le noir (faible valeur de V)


        // Détecter les joueurs par couleur
        List<Cercle> team1 = DetectCerclesByColor(image, blueLower, blueUpper, Color.Blue);
        List<Cercle> team2 = DetectCerclesByColor(image, redLower1, redUpper1, Color.Red);
        List<Cercle> ballon = DetectCerclesByColor(image, blackLower, blackUpper, Color.Black);
        team2.AddRange(DetectCerclesByColor(image, redLower2, redUpper2, Color.Red));

        cercles.Clear();
        cercles.AddRange(team1);
        cercles.AddRange(team2);

        // Mettre à jour le deuxième PictureBox pour afficher l'analyse
        Mat result = OpenCvSharp.Extensions.BitmapConverter.ToMat(image);
        
        // Trier les cercles de gauche à droite
        List<Cercle> sortedCercles = Cercle.GetCerclesSortedByLeftToRight(cercles);
        //Detecter le joueur qui a le ballon
        Cercle cercleHaveBallon = Cercle.FindClosestCercle(ballon[0].Position, sortedCercles);
        this.colorTeamHaveBallon = cercleHaveBallon.ColorTeam;
        
        //Detecter le sens de jeu 
        if ( sortedCercles[sortedCercles.Count - 1].ColorTeam == cercleHaveBallon.ColorTeam  )
        {
            this.isRight = false;
        }
        else
        {
            this.isRight = true;
        }
        
        //Detecter les hors jeu 
        if ( team1[0].ColorTeam == this.colorTeamHaveBallon )
        {
            Cercle.SetCerclesOffSide(team1,team2,this.isRight, image.Width ,cercleHaveBallon , ballon[0] );
            this.teamDeffender = team2; 
        }
        else
        {
            Cercle.SetCerclesOffSide(team2,team1,this.isRight, image.Width , cercleHaveBallon,ballon[0]);
            this.teamDeffender = team1;
        }
        
        // Afficher les informations dans la console
        Console.Out.WriteLine("Postion du ballon: " + ballon[0].Position);
        Console.Out.WriteLine("Equipe qui a le ballon: " + this.colorTeamHaveBallon);
        
        
        // Dessiner une ligne defensive
        if (this.isRight)
        {
            this.ligneDeffensive = (int)(Cercle.GetDefensiveLine(this.teamDeffender, this.isRight)+this.teamDeffender[0].Radius);
        }
        else
        {
            this.ligneDeffensive = (int)(Cercle.GetDefensiveLine(this.teamDeffender, this.isRight)-this.teamDeffender[0].Radius);;
        }
        Cv2.Line(
            result,
            new Point(this.ligneDeffensive, 0), // Starting point at the top of the image
            new Point(this.ligneDeffensive, result.Rows), // Ending point at the bottom of the image
            Scalar.Black, // Color of the line (Green in this case)
            2 // Thickness of the line
        );
        
        
        //Printer les joueur sur terminal 
        foreach (var cercle in sortedCercles)
        {
            Console.Out.WriteLine(cercle.ColorTeam);
            if (cercle.IsOffside )
            {
                Console.Out.WriteLine("Hors-jeu");
            }
        }
        
        // Dessiner les joueur detectés
        foreach (var cercle in cercles)
        {
            Cv2.Circle(result, new Point((double)cercle.Position.X, cercle.Position.Y), 10, cercle.ColorTeam == Color.Blue ? Scalar.Blue : Scalar.Red, 2);
            if ( cercle.IsOffside )
            {
                Cv2.Circle(result, new Point((double)cercle.Position.X, cercle.Position.Y), 10, Scalar.Yellow, 2);
                Cv2.PutText(
                    result,            // Image to draw on
                    "HR",           // Text to display
                    new Point(cercle.Position.X, cercle.Position.Y-5),  // Bottom-left corner of the text
                    HersheyFonts.HersheySimplex, // Font style
                    0.5,                        // Font scale (size)
                    Scalar.White,               // Text color (White in this case)
                    2                           // Thickness of the text
                );

            }
            else
            {
                Cv2.Circle(result, new Point((double)cercle.Position.X, cercle.Position.Y), 10, cercle.ColorTeam == Color.Blue ? Scalar.Blue : Scalar.Red, 2);
            }
            Console.Out.WriteLine("Position: " + cercle.Position + " | Couleur: " + cercle.ColorTeam + " | Rayon: " + cercle.Radius);
        }
        pictureAnalyse.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(result);

    }
}
    