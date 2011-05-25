using System;
using System.Diagnostics.Contracts;

namespace LicenseHeaderManager.Headers
{
  public class Parser
  {
    private bool _started;
    private int _position;
    private int _regions;
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

      LineComment = lineComment;
      BeginComment = beginComment;
      EndComment = endComment;
      BeginRegion = beginRegion;
      EndRegion = endRegion;
    }

    public string Parse (string text)
    {
      Contract.Requires (text != null);

      _started = false;
      _position = 0;
      _regions = 0;
      _text = text;  

      for (string token = GetToken (); HandleToken (token); token = GetToken ()) { }

      if (!_started)
        return string.Empty; //don't return any empty lines at the begin of the file which would normally be part of the header

      if (_regions != 0)
        throw new ParseException ();

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
      if (_started && _regions == 0)
      {
        int firstNewLine = _text.IndexOf (Environment.NewLine, start, _position - start);
        if (firstNewLine >= 0)
        {
          int nextNewLine = _text.IndexOf (
              Environment.NewLine, firstNewLine + Environment.NewLine.Length, _position - firstNewLine + Environment.NewLine.Length);
          
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

        _regions++;

        _position = _text.IndexOf (Environment.NewLine, _position);
        if (_position < 0)
          _position = _text.Length; //end of file

        return true;
      }

      else if (EndRegion != null && token == EndRegion)
      {
        if (!_started)
          _started = true;

        _regions--;
        if (_regions < 0)
          throw new ParseException ();

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
