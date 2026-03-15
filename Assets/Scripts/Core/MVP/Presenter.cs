namespace SimpleGame.Core.MVP
{
    public abstract class Presenter<TView> where TView : IView
    {
        protected readonly TView View;

        protected Presenter(TView view)
        {
            View = view;
        }

        public virtual void Initialize() { }
        public virtual void Dispose() { }
    }
}
