namespace Utils.Fixtures
{
    class AutoResetEventWrapperFactory : IAutoResetEventFactory
    {
        public IAutoResetEvent Create(bool initialState)
        {
            return new AutoResetEventWrapper(initialState);
        }
    }
}
