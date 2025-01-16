using System.Drawing; // Ensure this namespace is included

namespace VARify.model
{
    public class Cercle
    {
        public Point Position { get; set; }
        public Color ColorTeam { get; set; } // Changed from IsBlueTeam to ColorTeam
        public bool IsOffside { get; set; }
        public decimal Radius { get; set; } // Renamed for better readability

        public Cercle(Point position, Color colorTeam, decimal radius)
        {
            Position = position;
            ColorTeam = colorTeam;
            IsOffside = false;
            Radius = radius;
        }
        
        public static Cercle FindClosestCercle(Point referencePoint, List<Cercle> cercles)
        {
            if (cercles == null || cercles.Count == 0)
            {
                return null; // Aucun cercle n'est détecté
            }

            Cercle closestCercle = null;
            double minDistance = double.MaxValue;

            foreach (var cercle in cercles)
            {
                // Calculer la distance euclidienne entre le point de référence et le centre du cercle
                double distance = Math.Sqrt(Math.Pow(cercle.Position.X - referencePoint.X, 2) +
                                            Math.Pow(cercle.Position.Y - referencePoint.Y, 2));

                // Vérifier si cette distance est la plus petite trouvée
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCercle = cercle;
                }
            }

            return closestCercle;
        }
         
        public static Cercle FindLeftmostCercle(List<Cercle> cercles)
        {
            if (cercles == null || cercles.Count == 0)
            {
                return null; // Aucun cercle n'est détecté
            }

            Cercle leftmostCercle = cercles[0]; // Initialiser avec le premier cercle
            int minX = leftmostCercle.Position.X;

            foreach (var cercle in cercles)
            {
                // Vérifier si le centre du cercle actuel est plus à gauche
                if (cercle.Position.X < minX)
                {
                    minX = cercle.Position.X;
                    leftmostCercle = cercle;
                }
            }
            return leftmostCercle;
        }
        
        public static List<Cercle> GetCerclesSortedByLeftToRight(List<Cercle> cercles)
        {
            if (cercles == null || cercles.Count == 0)
            {
                return new List<Cercle>(); // Retourner une liste vide si aucun cercle n'est détecté
            }

            // Utiliser LINQ pour trier les cercles par la coordonnée X
            return cercles.OrderBy(c => c.Position.X).ToList();
        }

        public static int GetDefensiveLine(List<Cercle> cercles , bool isright )
        {
            List<Cercle> cerclesOrde = GetCerclesSortedByLeftToRight(cercles);
            if (cerclesOrde.Count ==1 )
            {
                return cerclesOrde[0].Position.X;
            }else if ( cerclesOrde.Count <1 )
            {
                Console.Out.WriteLine("La liste ne contient pas assez d'éléments pour avoir un avant-dernier.");
            }

            if (isright)
            {
                return cerclesOrde[cerclesOrde.Count - 2].Position.X;

            }
            return cerclesOrde[1].Position.X;
        }
        
        public static void SetCerclesOffSide(List<Cercle> attack, List<Cercle> defender , bool isRight , int width, Cercle hasBall , Cercle ball)
        {
            int defensiveLine = GetDefensiveLine(defender ,isRight);
           
            
            foreach (var cercle in attack)
            {
                if ( cercle != hasBall)
                {
                    if ( isRight )
                    {
                        if (cercle.Position.X + cercle.Radius > defensiveLine && width /2 < cercle.Position.X+cercle.Radius && ball.Position.X+ ball.Radius < cercle.Position.X+cercle.Radius ) 
                        {
                            cercle.IsOffside = true;
                        }
                        
                    }
                    else
                    {
                        if (cercle.Position.X  - cercle.Radius < defensiveLine && width /2 > cercle.Position.X  - cercle.Radius && ball.Position.X - ball.Radius > cercle.Position.X ) 
                        {
                            cercle.IsOffside = true;
                        }
                        
                    }
                }
                
                
            }
        }

    }
    
    
}