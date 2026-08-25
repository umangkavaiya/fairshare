using System;
using System.Collections.Generic;
using System.Text;

namespace FairShare.Application.Exceptions;

public class ConcurrencyConflictException : Exception
{
    public string CurrentVersion { get; }

    public ConcurrencyConflictException(string currentVersion)
        : base("Record has been modified by another user.")
    {
        CurrentVersion = currentVersion;
    }
}