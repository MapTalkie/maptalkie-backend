using System;

namespace MapTalkie.Utils.Cache;

public class CacheKey
{
    private readonly object[] _parts;

    public CacheKey(params object[] parts)
    {
        _parts = parts;
    }

    public CacheKey this[object firstSubKey, params object[] subKeys]
    {
        get
        {
            var newParts = new object[subKeys.Length + _parts.Length + 1];
            Array.Copy(_parts, newParts, _parts.Length);
            Array.Copy(subKeys, 0, newParts, _parts.Length + 1, subKeys.Length);
            newParts[_parts.Length] = firstSubKey;
            return new CacheKey(newParts);
        }
    }

    public static bool operator ==(CacheKey left, CacheKey right)
    {
        if (left._parts.Length != right._parts.Length)
            return false;

        for (var i = 0; i < left._parts.Length; i++)
            if (left._parts[i] != right._parts[i])
                return false;
        return true;
    }

    public static bool operator !=(CacheKey left, CacheKey right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        foreach (var o in _parts) hashCode.Add(o);

        return hashCode.ToHashCode();
    }
}