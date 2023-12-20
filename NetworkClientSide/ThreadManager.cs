public static class ThreadManager
{
    public static bool ProgramActive;
    public static void StartThreads()
    {
        ProgramActive = true;
        _mainThreadReference.Start();
        _applicationReference.Start();
        Console.WriteLine($"Main Thread Started");
    }
    public static void StopThreads()
    {
        ProgramActive = false;
    }
    // Everything from the network is moved to the Main Thread to prevent UDP packets from skipping
    #region  Main Thread
    private static Thread _mainThreadReference = new Thread(new ThreadStart(_mainThread));
    private static List<Action> _toExecuteOnMainThread = new List<Action>();
    public static List<Action> ExecuteOnMainThread { set { lock(_toExecuteOnMainThread) {_toExecuteOnMainThread.AddRange(value);} } }
    private static void _mainThread()
    {
        while (ProgramActive)
        {
            lock(_toExecuteOnMainThread)
            {
                while (_toExecuteOnMainThread.Count > 0)
                {
                    _toExecuteOnMainThread[0].Invoke();
                    _toExecuteOnMainThread.RemoveAt(0);
                }
            }
            Thread.Sleep(10);
        }
    }
    private static Thread _applicationReference = new Thread(new ThreadStart(_applicationThread));
    private static List<Action> _toExecuteOnApplicationThread = new List<Action>();
    public static List<Action> ExecuteOnApplicationThread { set { lock(_toExecuteOnApplicationThread) {_toExecuteOnApplicationThread.AddRange(value);} } }
    private static void _applicationThread()
    {
        while (ProgramActive)
        {
            lock(_toExecuteOnApplicationThread)
            {
                while (_toExecuteOnApplicationThread.Count > 0)
                {
                    _toExecuteOnApplicationThread[0].Invoke();
                    _toExecuteOnApplicationThread.RemoveAt(0);
                }
            }
            Thread.Sleep(10);
        }
    }
    #endregion
}