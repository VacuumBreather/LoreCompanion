namespace LoreCompanion.Views.Controls
{
    /// <summary>A <see cref="ITransitionSubject"/> effect which shrinks the <see cref="ZoomTransitionBase"/> and fades it out.</summary>
    /// <seealso cref="ITransition"/>
    /// <seealso cref="ITransition"/>
    public class ShrinkOutTransition : ZoomTransitionBase
    {
        /// <summary>Initializes a new instance of the <see cref="ShrinkOutTransition"/> class.</summary>
        public ShrinkOutTransition()
            : base(1.0, 0.8, 1.0, 0.0)
        {
        }
    }
}