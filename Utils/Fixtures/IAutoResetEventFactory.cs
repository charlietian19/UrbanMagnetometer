namespace Utils.Fixtures
{
    public interface IAutoResetEventFactory
    {
        IAutoResetEvent Create(bool initialState);
    }
}
