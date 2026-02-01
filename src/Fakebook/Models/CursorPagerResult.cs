using System;

namespace Fakebook.Models;

public class CursorPagerResult<T, TCursor>
{
  public IEnumerable<T> Items { get; set; } = [];
  public TCursor? Next { get; set; } = default;
}

public class DateTimeOffsetCursorPagerResult<T> : CursorPagerResult<T, DateTimeOffset?>
{
}
