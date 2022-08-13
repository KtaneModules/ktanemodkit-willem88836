
/// <summary>
/// Base class for objects subscribed 
/// to the <c>ButtonPublisher</c> object updates.
/// </summary>
public interface IButtonSubscriber
{
    void OnOkButtonClicked();
    void OnUpButtonClicked();
    void OnDownButtonClicked();
}
