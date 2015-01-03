// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EdgePoint.cs" company="">
//   
// </copyright>
// <summary>
//   Used to calculate all the points on the edge of an object (see
//   <see cref="MindMapLink.GetGeometryPoint" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Used to calculate all the points on the edge of an object (see
    ///     <see cref="MindMapLink.GetGeometryPoint" /> .
    /// </summary>
    public class EdgePoint
    {
        /// <summary>
        ///     Gets or sets the point.
        /// </summary>
        /// <value>
        ///     The point.
        /// </value>
        public System.Windows.Point Point { get; set; }

        /// <summary>
        ///     Gets or sets the angle.
        /// </summary>
        /// <value>
        ///     The angle.
        /// </value>
        public double Angle { get; set; }
    }
}