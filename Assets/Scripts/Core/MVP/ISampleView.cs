using System;

namespace SimpleGame.Core.MVP
{
    public interface ISampleView : IView
    {
        event Action OnButtonClicked;
        void UpdateLabel(string text);
    }
}
