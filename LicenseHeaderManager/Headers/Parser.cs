#region copyright
// Copyright (c) 2011 rubicon IT GmbH

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LicenseHeaderManager.Headers
{
  public class Parser
  {
    private bool _started;
    private int _position;
    private Stack<int> _regionStarts;

    private string _text;

    public string LineComment { get; private set; }
    public string BeginComment { get; private set; }
    public string EndComment { get; private set; }
    public string BeginRegion { get; private set; }
    public string EndRegion { get; private set; }

    public Parser (string lineComment, string beginComment, string endComment, string beginRegion, string endRegion)
    {
      Contract.Requires (string.IsNullOrWhiteSpace (beginComment) == string.IsNullOrWhiteSpace (endComment));
      Contract.Requires (string.IsNullOrWhiteSpace (beginRegion) == string.IsNullOrWhiteSpace (endRegion));
      Contract.Requires (!(string.IsNullOrWhiteSpace (lineComment) && string.IsNullOrWhiteSpace (beginComment)));

      LineComment = string.IsNullOrEmpty(lineComment) ? null : lineComment;
      BeginComment = string.IsNullOrEmpty (beginComment) ? null : beginComment;
      EndComment = string.IsNullOrEmpty(endComment) ? null : endComment;
      BeginRegion = string.IsNullOrEmpty(beginRegion) ? null : beginRegion;
      EndRegion = string.IsNullOrEmpty(endRegion) ? null : endRegion;
    }

    public string Parse (string text)
    {
      Contract.Requires (text != null);

      _started = false;
      _position = 0;
      _text = text;

      _regionStarts = new Stack<int> ();

      for (string token = GetToken (); HandleToken (token); token = GetToken ()) { }

      if (!_started)
        return string.Empty; //don't return any empty lines at the begin of the file which would normally be part of the header

      while (_regionStarts.Count > 1)
        _regionStarts.Pop ();

      if (_regionStarts.Count > 0)
        _position = _regionStarts.Pop();

      return _text.Substring (0, _position);
    }

    private string GetToken ()
    {
      if (SkipWhiteSpaces ())
        return null;

      int start = _position;
      for (; _position < _text.Length && !char.IsWhiteSpace (_text, _position); _position++) { }
      return _text.Substring (start, _position - start);
    }

    private bool SkipWhiteSpaces ()
    {
      int start = _position;

      //move to next real character
      for (; _position < _text.Length && char.IsWhiteSpace (_text, _position); _position++) { }
      
      //end of file
      if (_position >= _text.Length)
        return true;

      //if the header has already started and we're not in an open region, check if there was more than one NewLine
      if (_started && _regionStarts.Count == 0)
      {
        int firstNewLine = _text.IndexOf (Environment.NewLine, start, _position - start);
        if (firstNewLine >= 0)
        {
          int afterFirstNewLine = firstNewLine + Environment.NewLine.Length;
          int nextNewLine = _text.IndexOf (
              Environment.NewLine, afterFirstNewLine, _position - afterFirstNewLine);
          
          //more than one NewLine (= at least one empty line)
          if (nextNewLine > 0)
            return true;
        }
      }

      return false;
    }

    private bool HandleToken (string token)
    {
      if (token == null)
        return false;

      if (LineComment != null && token.StartsWith(LineComment))
      {
        if (!_started)
          _started = true;

        //proceed to end of line
        _position = _text.IndexOf (Environment.NewLine, _position - token.Length + LineComment.Length);
        if (_position < 0)
          _position = _text.Length; //end of file
          
        return true;
      }

      else if (BeginComment != null && token.StartsWith (BeginComment))
      {
        if (!_started)
          _started = true;

        _position = _text.IndexOf (EndComment, _position - token.Length + BeginComment.Length);
        if (_position < 0)
          throw new ParseException ();
        else
          _position += EndComment.Length;

        return true;
      }

      else if (BeginRegion != null && token == BeginRegion)
      {
        if (!_started)
          _started = true;

        _regionStarts.Push(_position - BeginRegion.Length);

        _position = _text.IndexOf (Environment.NewLine, _position);
        if (_position < 0)
          _position = _text.Length; //end of file

        return true;
      }

      else if (EndRegion != null && token == EndRegion)
      {
        if (!_started)
          _started = true;

        if (_regionStarts.Count == 0)
          throw new ParseException ();

        _regionStarts.Pop ();

        _position = _text.IndexOf (Environment.NewLine, _position);
        if (_position < 0)
          _position = _text.Length; //end of file

        return true;
      }

      else
      {
        _position -= token.Length;
        return false;
      }
    }
  }
}
