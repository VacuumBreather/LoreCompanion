namespace LoreCompanion.Views.Controls
{
    /// <summary>A <see cref="ITransitionSubject"/> effect which zooms the <see cref="ZoomTransitionBase"/> and fades it in.</summary>
    /// <seealso cref="ITransition"/>
    /// <seealso cref="ITransition"/>
    public class ZoomInTransition : ZoomTransitionBase
    {
        /// <summary>Initializes a new instance of the <see cref="ZoomInTransition"/> class.</summary>
        public ZoomInTransition()
            : base(0.5, 1.0, 0.0, 1.0)
        {
        }
    }
}