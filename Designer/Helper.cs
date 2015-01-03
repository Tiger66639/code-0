// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helper.cs" company="">
//   
// </copyright>
// <summary>
//   A class containing various utility functions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A class containing various utility functions.
    /// </summary>
    internal class Helper
    {
        #region Text

        /// <summary>copies the richtext data <paramref name="from"/> one flow<paramref name="to"/> the other.</summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        public static void CopyRichTextTo(
            System.Windows.Documents.FlowDocument to, 
            System.Windows.Documents.FlowDocument from)
        {
            System.Windows.Documents.TextRange range;
            using (var stream = new System.IO.MemoryStream())
            {
                range = new System.Windows.Documents.TextRange(from.ContentStart, from.ContentEnd);
                range.Save(stream, System.Windows.DataFormats.Xaml);
                range = new System.Windows.Documents.TextRange(to.ContentStart, to.ContentEnd);
                range.Load(stream, System.Windows.DataFormats.Xaml);
            }
        }

        /// <summary>Creates a FlowDocument with all the default values set.</summary>
        /// <returns>The <see cref="FlowDocument"/>.</returns>
        public static System.Windows.Documents.FlowDocument CreateDefaultFlowDoc()
        {
            var iRes = new System.Windows.Documents.FlowDocument();
            var iPar = new System.Windows.Documents.Paragraph();
            var iRun = new System.Windows.Documents.Run();
            iRun.FontSize = 12.0;
            iRun.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            iPar.Inlines.Add(iRun);
            iRes.Blocks.Add(iPar);
            return iRes;
        }

        /// <summary>Creates a FlowDocument with all the default values set and an initial
        ///     text provided.</summary>
        /// <param name="init">The init.</param>
        /// <returns>The <see cref="FlowDocument"/>.</returns>
        public static System.Windows.Documents.FlowDocument CreateDefaultFlowDoc(string init)
        {
            var iRes = new System.Windows.Documents.FlowDocument();
            var iPar = new System.Windows.Documents.Paragraph();
            var iRun = new System.Windows.Documents.Run(init);
            iRun.FontSize = 12.0;
            iRun.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
            iPar.Inlines.Add(iRun);
            iRes.Blocks.Add(iPar);
            return iRes;
        }

        #endregion

        #region Math

        /// <summary>Calculates the angle between 2 points (if you were to draw a
        ///     horizontal line through <paramref name="b"/> and vertical one through
        ///     a).</summary>
        /// <remarks><para>See 'corner calculation explanation.design' (in images dir of project)
        ///         for more info. Note, this function presumes</para>
        /// <list type="number"><item><description>
        ///                 degrees to be vertical: right, horizontal = center.
        ///             </description></item>
        /// </list>
        /// </remarks>
        /// <param name="a">The first point, becomes the center point.</param>
        /// <param name="b">The second point</param>
        /// <returns>the angle in degrees between the 2 points.</returns>
        public static double GetAnlge(System.Windows.Point a, System.Windows.Point b)
        {
            // we need to keep track of the quadrant that b is in compared to a cause the default function only
            // returns the value for the first one.
            if (b.X >= a.X && b.Y <= a.Y)
            {
                return System.Math.Atan(System.Math.Abs(b.Y - a.Y) / System.Math.Abs(b.X - a.X))
                       * (180 / System.Math.PI);
            }

            if (b.X < a.X && b.Y <= a.Y)
            {
                return System.Math.Atan(System.Math.Abs(b.X - a.X) / System.Math.Abs(b.Y - a.Y))
                       * (180 / System.Math.PI) + 90;
            }

            if (b.X <= a.X && b.Y > a.Y)
            {
                return System.Math.Atan(System.Math.Abs(b.Y - a.Y) / System.Math.Abs(b.X - a.X))
                       * (180 / System.Math.PI) + 180;
            }

            return System.Math.Atan(System.Math.Abs(b.X - a.X) / System.Math.Abs(b.Y - a.Y)) * (180 / System.Math.PI)
                   + 270;
        }

        /// <summary>Calculates the size of 1 side in a triangle if only the size of 1
        ///     side is known + the <paramref name="angle"/> between that side and an<paramref name="adjacent"/> one (so on the opposite side of the side
        ///     you want to calculate.</summary>
        /// <param name="angle">The angle.</param>
        /// <param name="adjacent">The adjacent.</param>
        /// <returns>The <see cref="double"/>.</returns>
        public static double GetLengthOfOpposite(double angle, double adjacent)
        {
            return System.Math.Tan(angle / (180 / System.Math.PI)) * adjacent;

                // need to convert the angle to radial first.
        }

        /// <summary>Calculates the size of an side in a triangle when the length of the<paramref name="opposite"/> side is known and the corner between the
        ///     side you want to know and an adjacent side.</summary>
        /// <param name="angle">The angle.</param>
        /// <param name="opposite">The opposite.</param>
        /// <returns>The <see cref="double"/>.</returns>
        public static double GetLengthOfAdjacent(double angle, double opposite)
        {
            return opposite / System.Math.Tan(angle / (180 / System.Math.PI));
        }

        #endregion
    }
}