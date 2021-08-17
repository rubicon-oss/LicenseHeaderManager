/* Copyright (c) rubicon IT GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LicenseHeaderManager.Core
{
  /// <summary>
  ///   Detects a comment block at the beginning of a text block and returns it (if any).
  /// </summary>
  internal class CommentParser : ICommentParser
  {
    private readonly string _beginComment;
    private readonly string _beginRegion;
    private readonly string _endComment;
    private readonly string _endRegion;
    private readonly string _lineComment;

    private int _position;
    private Stack<int> _regionStarts;
    private bool _started;

    private string _text;

    /// <summary>
    ///   Initializes a new <see cref="CommentParser" /> instance with a language-specific comment syntax.
    /// </summary>
    /// <param name="lineComment">The string denoting a comment for a single line.</param>
    /// <param name="beginComment">The string denoting the start of a comment spanning multiple lines ("block comment").</param>
    /// <param name="endComment">The string denoting the end of a comment spanning multiple lines ("block comment").</param>
    /// <param name="beginRegion">
    ///   The string representing the start of a (collapsible) region, if available in the given
    ///   language, otherwise <see langword="null" />.
    /// </param>
    /// <param name="endRegion">
    ///   The string representing the end of a (collapsible) region, if available in the given language,
    ///   otherwise <see langword="null" />.
    /// </param>
    public CommentParser (string lineComment, string beginComment, string endComment, string beginRegion, string endRegion)
    {
      Contract.Requires (string.IsNullOrWhiteSpace (beginComment) == string.IsNullOrWhiteSpace (endComment));
      Contract.Requires (string.IsNullOrWhiteSpace (beginRegion) == string.IsNullOrWhiteSpace (endRegion));
      Contract.Requires (!(string.IsNullOrWhiteSpace (lineComment) && string.IsNullOrWhiteSpace (beginComment)));

      _lineComment = string.IsNullOrEmpty (lineComment) ? null : lineComment;
      _beginComment = string.IsNullOrEmpty (beginComment) ? null : beginComment;
      _endComment = string.IsNullOrEmpty (endComment) ? null : endComment;
      _beginRegion = string.IsNullOrEmpty (beginRegion) ? null : beginRegion;
      _endRegion = string.IsNullOrEmpty (endRegion) ? null : endRegion;
    }

    public string Parse (string text)
    {
      Contract.Requires (text != null);

      _started = false;
      _position = 0;
      _text = text;
      _regionStarts = new Stack<int>();

      for (var token = GetToken(); HandleToken (token); token = GetToken())
      {
      }

      if (!_started)
        return string.Empty; //don't return any empty lines at the begin of the file which would normally be part of the header

      while (_regionStarts.Count > 1)
        _regionStarts.Pop();

      if (_regionStarts.Count > 0)
        _position = _regionStarts.Pop();

      return _text.Substring (0, _position);
    }

    private string GetToken ()
    {
      if (SkipWhiteSpaces())
        return null;

      var start = _position;
      for (; _position < _text.Length && !char.IsWhiteSpace (_text, _position); _position++)
      {
      }

      return _text.Substring (start, _position - start);
    }

    private bool SkipWhiteSpaces ()
    {
      var start = _position;

      //move to next real character
      for (; _position < _text.Length && char.IsWhiteSpace (_text, _position); _position++)
      {
      }

      //end of file
      if (_position >= _text.Length)
        return true;

      //if the header has already started and we're not in an open region, check if there was more than one NewLine
      if (!_started || _regionStarts.Count != 0)
        return false;

      var firstNewLine = NewLineManager.NextLineEndPositionInformation (_text, start, _position - start);
      if (firstNewLine == null)
        return false;

      var afterFirstNewLine = firstNewLine.Index + firstNewLine.LineEndLength;
      var nextNewLine = NewLineManager.NextLineEndPosition (_text, afterFirstNewLine, _position - afterFirstNewLine);
      //more than one NewLine (= at least one empty line)
      return nextNewLine > 0;
    }

    private bool HandleToken (string token)
    {
      if (token == null)
        return false;

      if (_lineComment != null && token.StartsWith (_lineComment))
      {
        SetStarted();

        //proceed to end of line
        _position = NewLineManager.NextLineEndPosition (_text, _position - token.Length + _lineComment.Length);

        UpdatePositionIfEndOfFile();

        return true;
      }

      if (_beginComment != null && token.StartsWith (_beginComment))
      {
        SetStarted();

        _position = _text.IndexOf (_endComment, _position - token.Length + _beginComment.Length, StringComparison.Ordinal);
        if (_position < 0)
          throw new ParseException();
        _position += _endComment.Length;

        return true;
      }

      if (_beginRegion != null && token == _beginRegion)
      {
        SetStarted();

        _regionStarts.Push (_position - _beginRegion.Length);


        _position = NewLineManager.NextLineEndPosition (_text, _position);

        UpdatePositionIfEndOfFile();

        return true;
      }

      if (_endRegion != null && token == _endRegion)
      {
        SetStarted();

        if (_regionStarts.Count == 0)
          throw new ParseException();

        _regionStarts.Pop();

        _position = NewLineManager.NextLineEndPosition (_text, _position);

        UpdatePositionIfEndOfFile();


        return true;
      }

      if (_endRegion != null && _endRegion.Contains (token))
      {
        SetStarted();

        var firstPart = token;
        token = GetToken();

        if (firstPart + " " + token == _endRegion)
        {
          if (_regionStarts.Count == 0)
            throw new ParseException();

          _regionStarts.Pop();
        }

        _position = NewLineManager.NextLineEndPosition (_text, _position);

        UpdatePositionIfEndOfFile();


        return true;
      }

      _position -= token.Length;
      return false;
    }

    private void UpdatePositionIfEndOfFile ()
    {
      if (_position < 0)
        _position = _text.Length; //end of file
    }

    private void SetStarted ()
    {
      _started = true;
    }
  }
}
