/* 2> NUL|| goto :COMPILE
:COMPILE
@echo OFF
set NETFXHOMEDIR=C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322
%NETFXHOMEDIR%\csc.exe /NOLOGO  -r:%NETFXHOMEDIR%\System.dll /out:%TEMP%\%~n0.exe "%~dpf0"
call %TEMP%\%~n0.exe %*
del /q %TEMP%\%~n0.exe
goto :EOF
*/
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Translations.SAM
{

/// <summary>
/// This class provides Metods to convert between SID & object name (user, group) 
/// </summary>

public class SidTranslator
{

static void Main(){

       ParseArgs p  = new ParseArgs(System.Environment.CommandLine);
       string sUser =  p.GetMacro("user");
       if (sUser.Equals(String.Empty)){
            sUser = "Remote Desktop Users"; 
       } 
       
       string sAnswer = null;

       string sSID =  p.GetMacro("sid");

       if (!sSID.Equals(String.Empty)){
       try{
       Console.WriteLine("SID=\"{0}\"", sSID);

              sAnswer =  GetName(sSID);
       } catch (Exception e){
              Console.WriteLine(e.Message);
              sAnswer = "unknown";
       }

       Console.WriteLine("{0} is {1}", sSID , sAnswer);
       }


       if (!sUser.Equals(String.Empty)){
       try{
       Console.WriteLine("User=\"{0}\"", sUser);
        
              sAnswer =  GetSid(sUser);
       } catch (Exception e){
              Console.WriteLine(e.Message);
              sAnswer = "unknown";
       }

       Console.WriteLine("{0} is {1}", sUser , sAnswer);
       }

}


[DllImport( "advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
private static extern bool 
LookupAccountSid([In,MarshalAs(UnmanagedType.LPTStr)] string systemName, 
IntPtr sid, [Out,MarshalAs(UnmanagedType.LPTStr)] StringBuilder name, ref 
int cbName, StringBuilder referencedDomainName, ref int 
cbReferencedDomainName, out int use );

[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
public static extern bool 
LookupAccountName([In,MarshalAs(UnmanagedType.LPTStr)] string systemName, 
[In,MarshalAs(UnmanagedType.LPTStr)] string accountName, IntPtr sid, ref int 
cbSid, StringBuilder referencedDomainName, ref int cbReferencedDomainName, 
out int use);

[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
internal static extern bool ConvertSidToStringSid(IntPtr sid, 
[In,Out,MarshalAs(UnmanagedType.LPTStr)] ref string pStringSid);

[DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
internal static extern bool ConvertStringSidToSid([In, 
MarshalAs(UnmanagedType.LPTStr)] string pStringSid, ref IntPtr sid);

/// <summary>
/// The method converts object name (user, group) into SID string.
/// </summary>
/// <param name="name">Object name in form domain\object_name.</param>
/// <returns>SID string.</returns>

public static string GetSid(string name)
{
	IntPtr _sid = IntPtr.Zero;	//pointer to binary form of SID string.
	int _sidLength = 0;	//size of SID buffer.
	int _domainLength = 0;//size of domain name buffer.
	int _use;	//type of object.
	StringBuilder _domain = new StringBuilder();	//stringBuilder for domain name.
	int _error = 0;
	string _sidString = "";

	//first call of the function only returns the sizes of buffers (SDI, domain name)
	LookupAccountName(null, name, _sid, ref _sidLength, _domain, ref _domainLength, out _use);
	_error = Marshal.GetLastWin32Error();

	if (_error != 122) 
        // error 122 (The data area passed to a system call is too small) - normal behaviour.
	{
            throw(new Exception(new Win32Exception(_error).Message));
	}
	else
	{
_domain = new StringBuilder(_domainLength); //allocates memory for domain name
_sid = Marshal.AllocHGlobal(_sidLength);	//allocates memory for SID
bool _rc = LookupAccountName(null, name, _sid, ref _sidLength, _domain, ref 
_domainLength, out _use);

if (_rc == false)
{
	_error = Marshal.GetLastWin32Error();
	Marshal.FreeHGlobal(_sid);
	throw(new Exception(new Win32Exception(_error).Message));
}
else
{
	// converts binary SID into string
	_rc = ConvertSidToStringSid(_sid, ref _sidString);

	if (_rc == false)
	{
_error = Marshal.GetLastWin32Error();
Marshal.FreeHGlobal(_sid);
throw(new Exception(new Win32Exception(_error).Message));
	}
	else
	{
Marshal.FreeHGlobal(_sid);
return _sidString;
	}
}
	}

}

/// <summary>
/// The method converts SID string (user, group) into object name.
/// </summary>
/// <param name="name">SID string.</param>
/// <returns>Object name in form domain\object_name.</returns>
public static string GetName(string sid)
{
	IntPtr _sid = IntPtr.Zero;	//pointer to binary form of SID string.
	int _nameLength = 0;//size of object name buffer
	int _domainLength = 0;//size of domain name buffer
	int _use;	//type of object
	StringBuilder _domain = new StringBuilder();	//domain name variable
	int _error = 0;
	StringBuilder _name = new StringBuilder();//object name variable

	bool _rc0 = ConvertStringSidToSid(sid, ref _sid);	//converts SID string into the binary form

	if (_rc0 == false)
	{
          _error = Marshal.GetLastWin32Error();
          Marshal.FreeHGlobal(_sid);
          throw(new Exception(new Win32Exception(_error).Message));
	}

	//first call of method returns the size of domain name and object name buffers
	bool _rc = LookupAccountSid(null, _sid, _name, ref _nameLength, _domain, ref _domainLength, out _use);
	_domain = new StringBuilder(_domainLength);
        //allocates memory for domain name
	_name = new StringBuilder(_nameLength);
        //allocates memory for object name
	_rc = LookupAccountSid(null, _sid,  _name, ref _nameLength, _domain, ref _domainLength, out _use);

	if (_rc == false)
	{
          _error = Marshal.GetLastWin32Error();
          Marshal.FreeHGlobal(_sid);
         throw(new Exception(new Win32Exception(_error).Message));
	}
	else
	{
         Marshal.FreeHGlobal(_sid);
         return _domain.ToString() + "\\" + _name.ToString();
	}

}

}
}




public class ParseArgs{

  private StringDictionary _MACROS;

  private StringDictionary AllMacros{
       get {return _MACROS;}
  }

  private bool DefinedMacro(string sMacro){
       return (_MACROS.ContainsKey(sMacro));
  }


  public string GetMacro(string sMacro) {

  if (DefinedMacro(sMacro) ){
         return _MACROS[sMacro];
     } else {
         return String.Empty;
     }
  }

  public string SetMacro(string sMacro, string sValue) {
      _MACROS[sMacro]  = sValue;
      return _MACROS[sMacro] ;
  }

  public ParseArgs(string sLine){

      _MACROS = new StringDictionary();

      string s = @"(\s|^)(?<token>(/|-{1,2})(\S+))";
      Regex r = new Regex( s, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase );
      MatchCollection m = null; 
      try {
         m =  r.Matches( sLine);
       } catch (Exception e){
              Console.WriteLine(e.Message);
       }
        if ( m != null ){

            for ( int i=0; i < m.Count; i++ ){
                   string sToken = m[i].Groups["token"].Value.ToString();
                   // Console.WriteLine("{0}", sToken);
                   ParseSwithExpression(sToken);
                  }
            }
       return;
}

private void ParseSwithExpression(string sToken){

      /* 

      string w = @"[^\=\:]+";
      string s = @"(/|\-{1,2})(?<macro>" + w +  @")([\=\:](?<value>" + w + @"))*";
         will get System.ArgumentException
        _message=(0x04a795bc) "parsing 
        "(?<macro>[0-9a-z_-\@]+)([\=\:](?<value>[0-9a-z_-\@]+))*" 
        - [x-y] range in reverse order."

      */ 

      string s = @"(/|\-{1,2})(?<macro>[a-z0-9_\-\\\@]+)([\=\:](?<value>[a-z0-9_\\\-\@]+))*";

      Regex r = new Regex( s,
            RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase );
        MatchCollection m = r.Matches( sToken);
        if ( m != null ){
            for ( int i=0; i < m.Count; i++ ){
                 string sMacro = m[i].Groups["macro"].Value.ToString();

                 string sValue = m[i].Groups["value"].Value;
                 if ( sValue == "") {
                       sValue = "true";
                 }
                  SetMacro(sMacro, sValue);
                  Console.WriteLine("{0} = \"{1}\"", sMacro,  GetMacro(sMacro));
                 }
        }
    return;

}

}


