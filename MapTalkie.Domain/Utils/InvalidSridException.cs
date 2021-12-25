using System;
using System.Linq;

namespace MapTalkie.Domain.Utils;

public class InvalidSridException : Exception
{
    public InvalidSridException(int[] expected, int got)
        : base($"Invalid SRID. Expected {string.Join(", ", expected.Select(v => v.ToString()))}, got {got}")
    {
    }
}