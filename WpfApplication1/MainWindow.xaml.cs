using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Ink;
namespace Proje
{
    public partial class MainWindow : Window
    { 
        private List<Shape> shapesList = new List<Shape>();
        private List<Shape> shapesGroupTouched = new List<Shape>(); 
        private List<int> currentTimeSec = new List<int>(); 
        private List<int> currentTimeMin = new List<int>();
        public MainWindow()
        {
            InitializeComponent(); 
            Notes.EditingMode = InkCanvasEditingMode.Ink; 
            canvas.ManipulationDelta += new EventHandler<ManipulationDeltaEventArgs>(OnManipulationDelta);
            
        }
         
        private void onGesture(object sender, InkCanvasGestureEventArgs e)
        {
            StrokeCollection hitStrokes = Notes.Strokes.HitTest(e.Strokes.GetBounds(), 10);
            Shape shapeToRemoved = null; 
            Polyline tempShape = new Polyline();
            double distance = 0;
            foreach (Shape shape in shapesList)
            {
                tempShape = (Polyline)shape;
                PointCollection shapePoints = tempShape.Points;
                foreach (Point shapePoint in shapePoints)
                {
                    double deltaX = e.Strokes.GetBounds().X - shapePoint.X;
                    double deltaY = e.Strokes.GetBounds().Y - shapePoint.Y;
                    distance = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY)); 
                    if (distance < 100)
                    {
                        shapeToRemoved = shape;
                    }
                }
            }
            if (shapeToRemoved != null)
            {
                shapesList.Remove(shapeToRemoved);
                Notes.Children.Remove(shapeToRemoved);
            }
                
            Notes.Strokes.Remove(hitStrokes);
        }
         
        Dictionary<TouchDevice, TouchPoint> touchFingers = new Dictionary<TouchDevice, TouchPoint>();
        
        protected override void OnTouchDown(TouchEventArgs e)
        {
            base.OnTouchDown(e);
             Notes.EditingMode = InkCanvasEditingMode.None;
              canvas.CaptureTouch(e.TouchDevice);
            TouchPoint point = e.GetTouchPoint(canvas);
            touchFingers[e.TouchDevice] = point;
             textBox1.Text = "MultiTouch Mode";
            Shape touchedShape = null;
            Polyline tempShape = new Polyline();
            double distance = 0;
            foreach (Shape shape in shapesList) {
                tempShape = (Polyline)shape;
                PointCollection shapePoints = tempShape.Points;
                foreach (Point shapePoint in shapePoints) {
                    double deltaX = point.Position.X - shapePoint.X;
                    double deltaY = point.Position.Y - shapePoint.Y;
                    distance = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
                    if (distance < 90)
                    {
                        touchedShape = shape;
                    }  
                }
            }
             
            DateTime dt = DateTime.Now;
            int minute = dt.Minute;
            int seconds = dt.Second; 
            currentTimeSec.Add(seconds);
            currentTimeMin.Add(minute); 
            int diffTimeInSec = 10;
            if (currentTimeSec.Count > 1 && currentTimeMin.Count > 1)
            {
                if (currentTimeMin[currentTimeMin.Count - 1] - currentTimeMin[currentTimeMin.Count - 2] == 0)
                {
                    diffTimeInSec = currentTimeSec[currentTimeSec.Count - 1] - currentTimeSec[currentTimeSec.Count - 2];
                }
            }
             
            if (touchFingers.Count == 1) { 
                if (diffTimeInSec < 1 && shapesGroupTouched.Count > 0) {
                    if (touchedShape != null && shapesGroupTouched.IndexOf(touchedShape) != -1) { 
                        touchedShape.Stroke = Brushes.DarkGreen;
                        touchedShape.StrokeThickness = 8;
                        shapesGroupTouched.Remove(touchedShape);
                    } else { 
                        foreach (Shape shape in shapesGroupTouched) {
                            shape.Stroke = Brushes.DarkGreen;
                            shape.StrokeThickness = 8;
                        }
                        shapesGroupTouched.Clear();
                    } 
                } else if (touchedShape != null) { 
                    if (shapesGroupTouched.IndexOf(touchedShape) == -1) {
                        touchedShape.Stroke = Brushes.DarkBlue;
                        touchedShape.StrokeThickness = 8;
                        shapesGroupTouched.Add(touchedShape);
                    }
                }
            }
            
        }

        
        protected override void OnTouchUp(TouchEventArgs e)
        {
            base.OnTouchUp(e);
             
            Notes.ReleaseTouchCapture(e.TouchDevice); 
            touchFingers.Clear(); 
            Notes.EditingMode = InkCanvasEditingMode.Ink; 
            textBox1.Text = "Ink Mode";
        }

         
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        { 
            if (shapesGroupTouched.Count > 0)
            {
                Polyline newTempShape = new Polyline(); 
                double minX = double.MaxValue;
                double maxX = double.MinValue;
                double minY = double.MaxValue;
                double maxY = double.MinValue;
                foreach (Shape shape in shapesGroupTouched)
                {
                    newTempShape = (Polyline)shape;
                    PointCollection shapePoints = newTempShape.Points;

                    for (int i = 0; i < shapePoints.Count; i++)
                    {
                        Point p = shapePoints.ElementAt(i);
                        if (p.X < minX)
                        {
                            minX = p.X;
                        }
                        if (p.X > maxX)
                        {
                            maxX = p.X;
                        }
                        if (p.Y < minY)
                        {
                            minY = p.Y;
                        }
                        if (p.Y > maxY)
                        {
                            maxY = p.Y;
                        }
                    }

                } 
                double x = minX;
                double y = minY;
                double w = maxX - minX;
                double h = maxY - minY;

                TranslateTransform translation = new TranslateTransform();
                ScaleTransform scale = new ScaleTransform();
                RotateTransform rotation = new RotateTransform(); 
                rotation.Angle += e.DeltaManipulation.Rotation;
                rotation.CenterX = x + w / 2;
                rotation.CenterY = y + h / 2; 
                scale.ScaleX *= e.DeltaManipulation.Scale.X;
                scale.ScaleY *= e.DeltaManipulation.Scale.Y;
                scale.CenterX = rotation.CenterX;
                scale.CenterY = rotation.CenterY; 
                translation.X += e.DeltaManipulation.Translation.X;
                translation.Y += e.DeltaManipulation.Translation.Y; 
                Polyline tempShape = new Polyline();
                foreach (Shape shape in shapesGroupTouched)
                { 
                    PointCollection newPoints = new PointCollection();
                    tempShape = (Polyline)shape;
                    PointCollection shapePoints = tempShape.Points;

                    for (int i = 0; i < shapePoints.Count; i++)
                    {
                        Point p = shapePoints.ElementAt(i);
                        Point pp = new Point(p.X, p.Y);
                        pp = rotation.Transform(pp);
                        pp = scale.Transform(pp);
                        pp = translation.Transform(pp);
                        p.X = pp.X;
                        p.Y = pp.Y;
                        newPoints.Add(p);
                    } 
                    tempShape.Points = newPoints;
                }
            }
        }
         
        private void shortStraw(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
           
            List<StylusPoint> cornerPoints = shortStraw(e.Stroke);
            String shapeName = shapeRecognition(cornerPoints,e.Stroke);
            textBox1.Text = shapeName;
            listBox.Items.Add(shapeName);
         }

         
        private String shapeRecognition(List<StylusPoint> cornerPoints,Stroke stroke)
        {
           
            StylusPoint p1 = new StylusPoint();
            StylusPoint p2 = new StylusPoint();
            double d;
            double a;
            List<double> distances = new List<double>();
            List<double> angles = new List<double>(); 
            for (int i = 0; i < (cornerPoints.Count - 1); i++)
            {
                p1 = cornerPoints[i];
                p2 = cornerPoints[i + 1];
                d = distance(p1, p2);
                a = angle(p1, p2);
                distances.Add(d);
                angles.Add(a);
            }
            String output; 
            if (angles.Count < 2)
            {
                output = "Line";
            }
            else
            { 
                double sumAngle = 0;
                for (int i = 0; i < angles.Count; i++)
                {
                    if (angles[i] < 0)
                    {
                        sumAngle = sumAngle + (angles[i] * -1);
                    }
                    else
                    {
                        sumAngle = sumAngle + angles[i];
                    }
                }
                 
                for (int i = 0; i < angles.Count; i++) {
                    if (angles[i] < 0) {
                        angles[i] = angles[i] * -1;
                    }
                } 
                for (int i = 0; i < distances.Count; i++) {
                    if (distances[i] < 0) {
                        distances[i] = distances[i] * -1;
                    }
                }
                double diffDistance = 0;
                double diffDistance1 = 0;
                double diffDistance2 = 0;
                if (angles.Count == 4)
                { 
                    diffDistance = distances[1] - distances[0];
                    if (diffDistance < 0)
                    {
                        diffDistance = diffDistance * -1;
                    } 
                    diffDistance1 = distances[2] - distances[0];
                    diffDistance2 = distances[3] - distances[1];
                    if (diffDistance1 < 0)
                    {
                        diffDistance1 = diffDistance1 * -1;
                    }
                    if (diffDistance2 < 0)
                    {
                        diffDistance2 = diffDistance2 * -1;
                    }

                } 
                double sumDistance1 = 0;
                double sumDistance2 = 0;
                double sumDistance3 = 0;
                if (angles.Count == 3 || angles.Count == 4 || angles.Count == 5)
                {
                    sumDistance1 = distances[1] + distances[0];
                    sumDistance2 = distances[2] + distances[0];
                    sumDistance3 = distances[1] + distances[2];
                } 
                double circleDiffDistance = 0;
                if (angles.Count >= 6 && angles.Count <= 9)
                {
                    for (int i = 0; i < distances.Count;i++ )
                    {
                        circleDiffDistance = distances[i] - circleDiffDistance;
                        if ( circleDiffDistance < 0 )
                        {
                            circleDiffDistance = circleDiffDistance * -1;
                        }
                    }
                }
                  if ((angles.Count == 4 || angles.Count == 5) && ((angles[0] >= 70 && angles[0] <= 120) || (angles[1] >= 70 && angles[1] <= 120) || (angles[3] >= 70 && angles[3] <= 120) || (angles[2] >= 70 && angles[2] <= 120)) && (sumAngle >= 350 && sumAngle <= 500))
                {
                    if ( diffDistance1 <= 5 && diffDistance2 <= 5 && diffDistance <= 5) { 
                        output = "Square";
                    } else if ((diffDistance1 <= 50 || diffDistance2 <= 50))
                    {
                        output = "Rectangle";
                    } else { 
                        output = "Rectangle";
                    }
                } 
                else if ((angles.Count == 3 || angles.Count == 4 || angles.Count == 5) && (angles[0] <= 90 || angles[1] <= 90 || angles[2] <= 90) && (distances[0] < sumDistance3 && distances[1] < sumDistance2 && distances[2] < sumDistance1))
                {
                    output = "Triangle";
                } 
                else if ((angles.Count >= 4 && angles.Count <= 7) && (distances[0] > (distances[1] + distances[2] + distances[3]))) {
                    output = "Arrow";
                }  
                else if ((angles.Count >= 6 && angles.Count <= 9) && sumAngle >= 600 && circleDiffDistance <= 50)
                {
                     output = "Circle";
                } 
                else if ((angles.Count >= 6 && angles.Count <= 9) && (sumAngle < 600 && sumAngle > 500))
                {
                    output = "Ellipse";
                }
               else {
                   output = "other";
               }
            }
            if (output != "other" && output != "Line")
            {
                Point[] strokePoints;
                if (output == "Circle" || output == "Ellipse" || output == "Arrow")
                {
                    strokePoints = (Point[])stroke.StylusPoints;
                }
                else
                {
                    strokePoints = new Point[cornerPoints.Count];
                    for (int i = 0; i < cornerPoints.Count; i++)
                    { 
                        strokePoints[i] = (Point)cornerPoints[i];

                    }
                } 
                Polyline shape = new Polyline(); 
                PointCollection points = new PointCollection(strokePoints); 
                shape.Points = points; 
                Shape shapeToAdd = shape;
                shapeToAdd.Stroke = Brushes.DarkGreen;
                shapeToAdd.StrokeThickness = 8;
                shapesList.Add(shapeToAdd);
                Notes.Children.Add(shapeToAdd);
                Notes.Strokes.Remove(stroke);
            }
            return output;
        }
         
        private List<StylusPoint> shortStraw(Stroke stroke)
        { 
            StylusPoint[] points = strokeGetPoints(stroke); 
            double s = determineResamplePoint(points); 
            StylusPoint[] resampled = resamplePoints(points, s); 
            int[] corners = getCorners(resampled);

            List<StylusPoint> cornerPoints = new List<StylusPoint>();
            for (int i = 0; i < corners.Length; i++)
            {
                cornerPoints.Add(resampled[corners[i]]);
            }
            return cornerPoints;
        } 
         
        private StylusPoint[] strokeGetPoints(Stroke oStroke)
        {
            int iRow = -1;
            StylusPointCollection colStylusPoints = oStroke.StylusPoints;
            StylusPoint [] AllPoints = new StylusPoint[colStylusPoints.Count];
            foreach (StylusPoint oPoint in colStylusPoints)
            {
                iRow += 1;
                AllPoints[iRow] = oPoint;
            }
            return AllPoints;
        } 
        private double determineResamplePoint(StylusPoint[] points)       
        { 
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            for (int i = 0; i < points.Length;i++ )
            {
                StylusPoint p = points[i];
                if (p.X < minX)
                {
                    minX = p.X;
                }
                if (p.X > maxX)
                {
                    maxX = p.X;
                }
                if (p.Y < minY)
                {
                    minY = p.Y;
                }
                if (p.Y > maxY)
                {
                    maxY = p.Y;
                }

            }
            double x = minX;
            double y = minY;
            double w = maxX - minX;
            double h = maxY - minY; 
            StylusPoint p1 = new StylusPoint();
            p1.X = x;
            p1.Y = y; 
            StylusPoint p2 = new StylusPoint();
            p2.X = x+w;
            p2.Y = y+h;
            double diagonal = distance(p1,p2);
            double s = diagonal / 40.0;

            return s;
        }
         
        private double angle(StylusPoint p1, StylusPoint p2)
        {
            double deltaX = p2.X - p1.X;
            double deltaY = p2.Y - p1.Y;
            return (Math.Atan2(deltaY,deltaX)*180)/Math.PI;
        }
         
        private double distance(StylusPoint p1, StylusPoint p2)
        {
            double deltaX = p2.X - p1.X;
            double deltaY = p2.Y - p1.Y;
            return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }
         
        private StylusPoint[] resamplePoints(StylusPoint[] points, double s)
        {
            double D = 0;
            List<StylusPoint> resampledList = new List<StylusPoint>();
            resampledList.Add(points[0]);
            for (int i = 1; i < points.Length; i++)
            {
                StylusPoint p1 = points[i-1];
                StylusPoint p2 = points[i];
                double d = distance(p1,p2);
                if ((D + d) >= s)
                {
                    StylusPoint q = new StylusPoint();
                    q.X = p1.X + ((s - D) / d) * (p2.X - p1.X);
                    q.Y = p1.Y + ((s - D) / d) * (p2.Y - p1.Y);
                    resampledList.Add(q);
                    points[i] = q;
                    D = 0;
                }
                else
                {
                    D = D + d;
                }
            }
            StylusPoint[] resampled = resampledList.ToArray();
            return resampled;
        }
         
        private int[] getCorners(StylusPoint[] points)
        {
            int W = 3;
            List<int> cornersList = new List<int>();
            cornersList.Add(0);
            List<double> strawsList = new List<double>();
            for (int j = W; j < (points.Length - W); j++)
            {
                strawsList.Add(distance(points[j-W],points[j+W]));
            }
            double t = getMedian(strawsList) * 0.95;

            for (int i = W; i < (points.Length - W); i++)
            { 
                if (strawsList[i-W] < t)
                { 
                    double localMin = double.MaxValue;
                    int localMinIndex = i;
                    while (i < strawsList.Count && strawsList[i-W] < t)
                    {
                        if (strawsList[i-W] < localMin)
                        {
                            localMin = strawsList[i-W];
                            localMinIndex = i; 
                        }
                        i = i + 1;
                    }
                    cornersList.Add(localMinIndex);
                }
            }
            cornersList.Add(points.Length-1);

            cornersList = postProcessCorners(points, strawsList, cornersList);
            int[] corners = cornersList.ToArray();
            return corners;
        }
         
        private double getMedian(List<double> pNumbers)
        {
            int size = pNumbers.Count;
            int mid = size / 2;
            pNumbers.Sort();
            double median = (size % 2 != 0) ? pNumbers[mid] :
            (pNumbers[mid] + pNumbers[mid - 1]) / 2;
            return median;
        }
         
        private List<int> postProcessCorners(StylusPoint[] points, List<double> straws, List<int> corners)
        {
            bool cont = false;
            while (!cont)
            {
                cont = true;
                for (int i = 1; i < corners.Count; i++)
                {
                    int c1 = corners[i - 1];
                    int c2 = corners[i];
                    if (!isLine(points,c1,c2)) {
                        int newCorner = halfwayCorner(straws,c1,c2); 
                        if (newCorner > c1 && newCorner < c2)
                        {
                            corners.Insert(i,newCorner);
                            cont = false;

                        }
                    }
                }
            }
            for (int i = 1; i < (corners.Count- 1); i++)
            {
                int c1 = corners[i - 1];
                int c2 = corners[i + 1];
                if (isLine(points, c1, c2))
                {
                    corners.RemoveAt(i);
                    i = i - 1;
                }
            }
           
            return corners;
        }
         
        private bool isLine(StylusPoint[] points, int a , int b)
        {
            double threshold = 0.95;
            double dis =distance(points[a],points[b]);
            double d = 0;
            for (int i = a; i < b; i++)
            {
                double newDis = distance(points[i], points[i+1]); ;
                d = d + newDis;
            }
            double pathDistance = d;
            if ((dis/pathDistance) > threshold)
            {
                return true;
            } else {
                return false;
            }
        }
         
        private int halfwayCorner(List<double> straws, int a, int b)
        {
            int quarter = (b - a) / 4; 
            double minValue = double.MaxValue;
            int minIndex = 0;
            
            for (int i = (a + quarter); i < (b - quarter); i++)
            { 
                if ( i < straws.Count()) {
                    if (straws[i] < minValue)
                    {
                        minValue = straws[i];
                        minIndex = i;
                    }
                }
            }

            return minIndex;
        }
         
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            Notes.Strokes.Clear();
            Notes.Children.Clear();
            shapesList.Clear();
            shapesGroupTouched.Clear();
            currentTimeSec.Clear();
            listBox.Items.Clear();
            textBox1.Text = "";
        }
         
         
    }
}
