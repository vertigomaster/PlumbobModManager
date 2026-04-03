using System;
using System.Collections.Generic;

public struct DynamicBoolean
{
    public enum Method { AND, OR }
    public Method method { get; set; }
    private List<Func<bool>> checks { get; set; }

    public static implicit operator bool(DynamicBoolean b)
    {
        if (b.checks == null || b.checks.Count == 0) return false;

        foreach(Func<bool> check in b.checks)
        {
            switch (b.method)
            {
                case Method.AND:
                    {
                        if (!check.Invoke()) return false;
                        break;
                    }
                case Method.OR:
                    {
                        if (check.Invoke()) return true;
                        break;
                    }
            }
        }

        switch (b.method)
        {
            case Method.AND:
                {
                    return true;
                }
            case Method.OR:
            default:
                {
                    return false;
                }
        }
    }

    public void AddCheck(Func<bool> check)
    {
        if(checks == null)
        {
            checks = new List<Func<bool>>();
        }

        checks.Add(check);
    }

    public void AddCheck(IEnumerable<Func<bool>> check)
    {
        if (checks == null)
        {
            checks = new List<Func<bool>>();
        }

        checks.AddRange(check);
    }
}
