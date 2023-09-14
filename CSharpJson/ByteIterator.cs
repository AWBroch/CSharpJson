namespace CSharpJson;
internal class ByteIterator
{
  public int Index { get; set; }
  byte[] Data { get; }

  public ByteIterator(byte[] data)
  {
    Index = 0;
    Data = data;
  }

  public byte? Next()
  {
    var v = this[0];
    if (v.HasValue)
      Index++;
    return v;
  }

  public byte[]? NextRange(int range)
  {
    byte[]? vs = this[0..range];
    if (vs != null)
      Index += range;
    return vs;
  }

  public byte? this[Index index]
  {
    get
    {
      try
      {
        var offset = index.GetOffset(RemainingLength());
        return offset < Data.Length ? Data[offset + Index] : null;
      }
      catch
      {
        return null;
      }
    }
  }

  public byte[]? this[Range index]
  {
    get
    {
      try
      {
        (int offset, int length) = index.GetOffsetAndLength(RemainingLength());
        offset += Index;
        return offset >= 0 && (offset + length) <= Data.Length ? Data[new Range(offset, offset + length)] : null;
      }
      catch
      {
        return null;
      }
    }
  }

  public int RemainingLength()
  {
    return Data.Length - Index;
  }

  public void IgnoreWhitespace()
  {
    while (IsWhitespace(Data[Index]))
    {
      Index++;
    }
  }

  static bool IsWhitespace(byte b)
  {
    return b == 0x0020 || b == 0x000A || b == 0x000D || b == 0x0009;
  }
}