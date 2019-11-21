using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace cs_wpf_property_generator
{
  public static class Extensions
  {
    /// <summary>
    /// Return 'value' repeated 'count' times.
    /// <para>
    /// <ul>
    ///   <li>Value cannot be null</li>
    ///   <li>A count of zero or a negative count returns an empty string</li>
    ///   <li>A count of one returns value.</li>
    ///   <li>A count of more than one returns value repeated count times.</li>
    ///   <li>If (value.Length * count) > Int32.Max, an OverflowException is thrown.</li>
    /// </ul>
    /// </para>
    /// </summary>
    public static String Repeat(this String value, Int32 count)
    {
      return (count < 1) ? "" : (new StringBuilder(value.Length * count)).Insert(0, value, count).ToString();
    }

    private static readonly Regex _indentTextRegex = new Regex("(\r\n|\n)");

    /// <summary>
    /// Treat 'value' as a multiline string, where each string is separated either by
    /// a carriage return/linefeed combo, or just a linefeed.
    /// <para>
    /// Indent each line in 'value' by 'indent' spaces and return the modified string.
    /// </para>
    /// <para>
    /// 'value' must be non-null, and 'indent' must be greater than zero.
    /// </para>
    /// </summary>
    public static String Indent(this String value, Int32 indent)
    {
      var indentString = " ".Repeat(indent);
      return indentString + _indentTextRegex.Replace(value, "$1" + indentString);
    }

    public static String Join(this IEnumerable<String> values, String separator)
    {
      return String.Join(separator, values);
    }
  }
}
