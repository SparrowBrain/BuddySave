using System.Diagnostics;

namespace BuddySave.System;

public interface IProcessProvider
{
    Process Start(ProcessStartInfo processStartInfo);

    Task WaitForExitAsync(Process process);
}