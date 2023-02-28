using System.Diagnostics;

namespace BuddySave.System;

internal class ProcessProvider : IProcessProvider
{
    public Process Start(ProcessStartInfo processStartInfo)
    {
        return Process.Start(processStartInfo);
    }

    public async Task WaitForExitAsync(Process process)
    {
        await process.WaitForExitAsync();
    }
}