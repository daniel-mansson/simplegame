namespace SimpleGame.Core.PopupManagement
{
    /// <summary>
    /// Resolves a view component by type from a container of pre-instantiated UI views.
    /// Implemented by UnityViewContainer via GetComponentInChildren&lt;T&gt;(true),
    /// which finds components even on inactive child GameObjects.
    /// </summary>
    public interface IViewResolver
    {
        T Get<T>() where T : class;
    }
}
