using System;
using System.Threading.Tasks;

namespace AzureNetQ
{
    public static class TaskHelpers
    {
        public static Task ExecuteSynchronously(Action action)
        {
            var tcs = new TaskCompletionSource<NullStruct>();
            try
            {
                action();
                tcs.SetResult(new NullStruct());
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }

            return tcs.Task;
        }

        private struct NullStruct{}
    }
}