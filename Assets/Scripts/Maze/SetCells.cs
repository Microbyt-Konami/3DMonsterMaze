using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SetCells : IEquatable<SetCells>
{
    public int setId;
    public int nCells;
    public bool used;

    public bool Equals(SetCells other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return setId == other.setId;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((SetCells)obj);
    }

    public override int GetHashCode() => setId.GetHashCode();
}