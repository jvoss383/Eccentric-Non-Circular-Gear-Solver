using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;

namespace Eccentric_Non_Circular_Gear_Solver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            double centreDist = 4.42986023500648;
            double angleIncrement = Math.PI * 2 / 1024;

            GenerateGears(centreDist, angleIncrement, true); // generates full animation with constant centreDist


            // used to check for other possible solutions - higher order gear ratios
            /*for(double centreDistTest = 4; centreDistTest < 30; centreDistTest += 0.1)
            {
                GenerateGears(centreDistTest, angleIncrement);
            }*/

            double ax = 4;
            double ay;
            double bx = 5;
            double by;

            // used to find precise solution for centredistnace
            for(int binarySearchIndex = 0; binarySearchIndex < 500; binarySearchIndex++)
            {
                ay = GenerateGears(ax, angleIncrement).Item3;
                by = GenerateGears(bx, angleIncrement).Item3;

                double cx = (ax + bx) / 2;
                double cy = GenerateGears(cx, angleIncrement).Item3;

                if (cy * ay < 0)
                    bx = cx;
                else
                    ax = cx;
                Console.WriteLine(cx);
            }//*/
        }

        static (List<(double, double)>, List<(double, double)>, double) GenerateGears(double centreDist, double angleIncrement, bool exportImages = false)
        {
            List<(double, double)> gear1 = new List<(double, double)>();
            //gear1.Add((0d, 0d));
            List<(double, double)> gear2 = new List<(double, double)>();
            //gear2.Add((0d, 0d));

            double theta2 = 0;
            int vertIndex = 0;
            for (; angleIncrement * vertIndex <= Math.PI * 2 * 8; vertIndex++)
            {
                double theta1 = vertIndex * angleIncrement; // angular displacement of gear 1
                double r1 = Radius1(theta1);                // radius of gear 1
                double r2 = centreDist - r1;                // radius of gear 2
                double s = angleIncrement * r1;             // surface displacement for 1 angle increment

                gear1.Add((theta1, r1));
                gear2.Add((theta2, -r2));

                theta2 -= s / r2;

                if (vertIndex % 10 == 0 && exportImages)
                {
                    Image f = RenderFrame(6, centreDist, gear1, gear2, 800, theta1, theta2);
                    ((Bitmap)f).Save(vertIndex / 20 + ".PNG");
                }
            }

            Image frame = RenderFrame(6, centreDist, gear1, gear2, 400);
            ((Bitmap)frame).Save(centreDist + ".png");//*/
            double finalTheta1 = vertIndex * angleIncrement;
            double thetaDelta = finalTheta1 + theta2;
            return (gear1, gear2, thetaDelta);
        }

        static Image RenderFrame(double maxRadius, double centreDist, List<(double, double)> gear1, List<(double, double)> gear2, int height, double theta1 = 0, double theta2 = 0)
        {
            double scale = height / (2 * maxRadius);
            //(double, double) gear1Centre = (scale * maxRadius, height / 2d);
            //(double, double) gear2Centre = (gear1Centre.Item1 + centreDist * scale, gear1Centre.Item2);

            Image canvas = new Bitmap((int)((5+2*maxRadius) * scale), height);
            using(Graphics g = Graphics.FromImage(canvas))
            {
                g.FillRectangle(
                        new SolidBrush(Color.White),
                        0, 0, canvas.Width, canvas.Height);

                RenderGear(gear1, maxRadius, maxRadius, g, -theta1 + Math.PI / 2);
                RenderGear(gear2, maxRadius + centreDist, maxRadius, g, -theta2 + Math.PI / 2);
            }

            void RenderGear(List<(double, double)> gear, double unscaledXShift, double unscaledYShift, Graphics g, double theta = 0)
            {
                for (int i = 0; i < gear.Count(); i++)
                {
                    (float, float) rectilinearPair = PolarToRectilinear(gear[i], theta); // unscaled rectilinear coordinates centered about 0, 0
                    (float, float) shiftedRectilinearPair = (rectilinearPair.Item1 + (float)unscaledXShift, rectilinearPair.Item2 + (float)unscaledYShift);
                    (float, float) ScaledShiftedRectilinearPair = (shiftedRectilinearPair.Item1 * (float)scale, shiftedRectilinearPair.Item2 * (float)scale);

                    g.FillRectangle(
                        new SolidBrush(Color.Black),
                        ScaledShiftedRectilinearPair.Item1,
                        ScaledShiftedRectilinearPair.Item2,
                        2, 2);
                }
            }

            (float, float) PolarToRectilinear((double, double) polarPair, double angularOffset)
            {
                return ((float)(Math.Sin(polarPair.Item1 + angularOffset) * polarPair.Item2),
                        (float)(Math.Cos(polarPair.Item1 + angularOffset) * polarPair.Item2));
            }

            return canvas;
        }

        static double Radius1(double theta)
        {
            return Math.Sin(theta) + 2;
        }
    }
}
