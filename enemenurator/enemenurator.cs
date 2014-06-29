using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Diagnostics;
// http://msdn.microsoft.com/en-us/library/aa288468%28v=vs.71%29.aspx
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using System.Management;
using System.Net;

#region Bootstrap Application

namespace SystemTrayApp
{

public class App
{

private NotifyIcon appIcon = new NotifyIcon();

private int isStateone = 0;
private Icon IdleIcon;
private Icon BusyIcon;

// Define the menu.
private ContextMenu sysTrayMenu = new ContextMenu();
// TODO: offer options
private MenuItem runNowMenuItem = new MenuItem("Run Now");


private MenuItem exitApp = new MenuItem("Exit");

// private DialogHunter worker = new DialogHunter();
// private ArrayList newDialogs = new ArrayList();

static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

static int nScanCounter = 1;
static bool exitFlag = false;

// This is the method to run when the timer is raised.
private void TimerEventProcessor(Object myObject,
				 EventArgs myEventArgs)
{
	myTimer.Stop();
	nScanCounter++;
	Console.Write("{0}\r", nScanCounter.ToString( ));
	isStateone = 1 - isStateone;
	appIcon.Visible = false;
	if (isStateone == 1)
		appIcon.Icon = BusyIcon;
	else
		appIcon.Icon = IdleIcon;
	appIcon.Visible = true;
	// Change the background image to the next image.
	DialogDetector Worker = new DialogDetector();
	Worker.Perform();
	// Thread.Sleep (1000);
	isStateone = 1 - isStateone;
	appIcon.Visible = false;
	if (isStateone == 1)
		appIcon.Icon = BusyIcon;
	else
		appIcon.Icon = IdleIcon;
	appIcon.Visible = true;
	// restart Timer.
	myTimer.Start();

}


public void Start()
{


	IdleIcon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("enemenurator.IdleIcon.ico"));
	BusyIcon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("enemenurator.BusyIcon.ico"));

	appIcon.Icon = IdleIcon;
	appIcon.Text = "Popup Hunter Tool";

	// Place the menu items in the menu.
	sysTrayMenu.MenuItems.Add(runNowMenuItem);
	sysTrayMenu.MenuItems.Add(exitApp);
	appIcon.ContextMenu = sysTrayMenu;

	myTimer.Tick += new EventHandler(TimerEventProcessor);

	// Sets the timer interval to 1 hour.
	// TODO -  read config file:
	myTimer.Interval = 3600000;
	myTimer.Start();

	// Show the system tray icon.
	appIcon.Visible = true;

	// Attach event handlers.
	runNowMenuItem.Click += new EventHandler(runNow);
	exitApp.Click += new EventHandler(ExitApp);

}

private void runNow(object sender, System.EventArgs e)
{
	TimerEventProcessor(sender, e);
}
private void ExitApp(object sender, System.EventArgs e)
{
	// No components to dispose:
	// need to Displose individual resources
	Debug.Assert(exitFlag != true);
	appIcon.Dispose();
	IdleIcon.Dispose();
	BusyIcon.Dispose();

	Application.Exit();
}


public static void Main()
{


#if DEBUG
	Console.WriteLine("Debug version." );
#endif

	App app = new App();
	app.Start();
	// No forms are being displayed,
	// next statement to prevent the application from automatically ending.
	Application.Run();
}



}

}
#endregion





#region Configuration Processor

public class ConfigRead {

private static string _ConfigFileName = "config.xml";
public static string ConfigFileName {  get { return _ConfigFileName; } set { _ConfigFileName = value; } }

public static bool Debug {  get { return DEBUG; } set { DEBUG = value; } }
static bool DEBUG = false;
private ArrayList _PatternArrayList;
private string sDetectorExpression;
public string DetectorExpression {  get { return sDetectorExpression; }         }

public void  LoadConfiguration(string Section, string Column)
{

	_PatternArrayList = new ArrayList();

	XmlDocument xmlDoc = new XmlDocument();
	xmlDoc.PreserveWhitespace =  true;
	Assembly CurrentlyExecutingAssembly = Assembly.GetExecutingAssembly();

	FileInfo CurrentlyExecutingAssemblyFileInfo = new FileInfo(CurrentlyExecutingAssembly.Location);
	string ConfigFilePath = CurrentlyExecutingAssemblyFileInfo.DirectoryName;
	try {
		xmlDoc.Load(ConfigFilePath + @"\" + ConfigFileName);
	} catch (Exception e) {
		Console.WriteLine(e.Message);
		//  Environment.Exit(1);
		Console.WriteLine("Loading embedded resource");
		// While Loading - note : embedded resource Logical Name is overriden at the project level.
		xmlDoc.Load(CurrentlyExecutingAssembly.GetManifestResourceStream(ConfigFileName));

	}
	// see http://forums.asp.net/p/1226183/2209639.aspx
	//
	if (DEBUG)
		Console.WriteLine("Loading: Section \"{0}\" Column \"{1}\"", Section, Column);

	XmlNodeList nodes = xmlDoc.SelectNodes(Section);
	foreach (XmlNode node in nodes) {

		XmlNode DialogTextNode = node.SelectSingleNode(Column);
		string sInnerText = DialogTextNode.InnerText;
		if (!String.IsNullOrEmpty(sInnerText)) {
			_PatternArrayList.Add( sInnerText );
			if (DEBUG)
				Console.WriteLine("Found \"{0}\"", sInnerText);
		}
	}
	if (0 == _PatternArrayList.Count) {
		if (Debug)
			Console.WriteLine("Invalid Configuration:\nReview Section \"{0}\" Column \"{1}\"", Section, Column);

		MessageBox.Show(
			String.Format( "Invalid Configuration file:\nReview \"{0}/{1}\"", Section, Column),
			CurrentlyExecutingAssembly.GetName().ToString(),
			MessageBoxButtons.OK,
			System.Windows.Forms.MessageBoxIcon.Exclamation);
		Environment.Exit(1);
	}
	try {
		sDetectorExpression   =  String.Join( "|", (string [] )_PatternArrayList.ToArray( typeof(string)));
	} catch (Exception e) {
		Console.WriteLine("Internal error processing Configuration");
		System.Diagnostics.Debug.Assert(e != null );
		Environment.Exit(1);

	}
	if (DEBUG)
		Console.WriteLine(sDetectorExpression);
}

}


#endregion




#region Configuration XPATH Processor

public class XMLDataExtractor {


private bool DEBUG = false;
public bool Debug {  get { return DEBUG; } set { DEBUG = value; } }
// see http://support.microsoft.com/kb/308333 about XPathNavigator

private XPathNavigator nav;
private XPathDocument docNav;
private XPathNodeIterator NodeIter;
private XPathNodeIterator NodeResult;

public XMLDataExtractor(string sFile)
{
	try {
		// Open the XML.
		docNav = new XPathDocument(sFile);

	} catch (Exception e ) {
		// don't do anything.
		Trace.Assert(e != null); // keep the compiler happy
	}

	if (docNav != null)
		// Create a navigator to query with XPath.
		nav = docNav.CreateNavigator();

}

public String[] ReadAllNodes(String sNodePath, String sFieldPath)
{

	// Select the node and place the results in an iterator.
	NodeIter = nav.Select(sNodePath);

	ArrayList _DATA = new ArrayList(1024);

	// Iterate through the results showing the element value.

	while (NodeIter.MoveNext()) {
		XPathNavigator here = NodeIter.Current;

		if (DEBUG) {
			try {
				Type ResultType = here.GetType();
				Console.WriteLine("Result type: {0}", ResultType);
				foreach (PropertyInfo oProperty in ResultType.GetProperties()) {
					string sProperty = oProperty.Name.ToString();
					Console.WriteLine("{0} = {1}",
							  sProperty,
							  ResultType.GetProperty(sProperty).GetValue(here, new Object[] {})

					                  /* COM  way:
					                     ResultType.InvokeMember(sProperty,
					                                             BindingFlags.Public |
					                                             BindingFlags.Instance |
					                                             BindingFlags.Static |
					                                             BindingFlags.GetProperty,
					                                             null,
					                                             here,
					                                             new Object[] {},
					                                             new CultureInfo("en-US", false))
					                   */
							  );
				}
				;
			} catch (Exception e) {
				// Fallback to system formatting
				Console.WriteLine("Result:\n{0}", here.ToString());
				Trace.Assert(e != null); // keep the compiler happy
			}
		} // DEBUG

		// collect the caller requested data
		NodeResult = null;

		try { NodeResult = here.Select(sFieldPath); }  catch (Exception e) {
			// Fallback to system formatting
			Console.WriteLine(e.ToString());
			throw /* ??? */;
		}

		if (NodeResult != null) {
			while (NodeResult.MoveNext())
				_DATA.Add(NodeResult.Current.Value);

		}


	}
	;
	String []  res =   (String[])_DATA.ToArray(typeof(string));
	if (DEBUG)
		Console.WriteLine(String.Join(";", res));
	return res;

}

public void ReadSingleNode( String sNodePath)
{
	// http://msdn2.microsoft.com/en-us/library/system.xml.xmlnode.selectsinglenode(VS.71).aspx
	// Select the node and place the results in an iterator.
	NodeIter = nav.Select(sNodePath);
	// Iterate through the results showing the element value.
	while (NodeIter.MoveNext())
		Console.WriteLine("Book Title: {0}", NodeIter.Current.Value);
	;
}


}

#endregion


#region Top Worker class

public class DialogDetector  {

static public bool DialogDetected = false;
static private String CommandLine = String.Empty;
static bool DEBUG = false;
public static bool Debug {  get { return DEBUG; } set { DEBUG = value; } }

public void Perform( )
{

	Process[] myProcesses;
	myProcesses = Process.GetProcesses();

	ConfigRead x = new ConfigRead();
	x.LoadConfiguration("Configuration/ProcessDetection/Process", "ProcessName");
	string s  = x.DetectorExpression;
	Regex r = new Regex( s, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase );

	foreach (Process myProcess in myProcesses) {

		string res =  String.Empty;
		string sProbe = myProcess.ProcessName;
		//  myProcess.StartInfo.FileName - not accessible
		if (Debug) Console.WriteLine( "Process scan: {0}",  s  ); MatchCollection m = r.Matches( sProbe);
		if ( sProbe != null && m.Count != 0 ) {

			try {
				DialogDetected = true;
				ProcessCommandLine z =  new ProcessCommandLine(myProcess.Id.ToString());

				if (Debug) Console.WriteLine( "{0}{1}",  myProcess.Id.ToString(),  z.CommandLine  ); CommandLine =  z.CommandLine;
				// CommandLine = myProcess.ProcessName;
				Console.WriteLine("{0} {1} {2} {3}", sProbe, myProcess.ProcessName, myProcess.Id, DateTime.Now -  myProcess.StartTime);
			} catch (Win32Exception e) { System.Diagnostics.Trace.Assert(e !=  null); }
		}
	}
}

}

#endregion




#region WMI Data processor

public class ProcessCommandLine {

static bool DEBUG = false;
public static bool Debug {  get { return DEBUG; } set { DEBUG = value; } }

private String _CommandLine  = String.Empty;

public String CommandLine { get { return _CommandLine; } }
public ProcessCommandLine (String PID )
{

	ManagementClass mc = new ManagementClass( @"root/cimv2:Win32_Process" );
	ManagementObjectCollection mobjects = mc.GetInstances( );

	if (DEBUG) Console.WriteLine("{0}", PID  ); foreach ( ManagementObject mo in mobjects ) {
		if (DEBUG)
			Console.WriteLine(mo ["ProcessID"].ToString( ));
		if ( PID  ==  mo ["ProcessID"].ToString( ))
			_CommandLine = mo ["CommandLine"].ToString( );

	}
}
}



// Discovery / integration with Plan B Jobs

#endregion

#region PlanB DOS Drive Discovery

public class DosDriveInventory { // originally an MSBuild Task.


private ArrayList _MappedDriveLettersArrayList = new ArrayList(24);
private ArrayList _UnusedDriveLettersArrayList = new ArrayList(24);

private string _MappedDriveLetters = "";

private string _UnusedDriveLetters = "";
static bool DEBUG = false;

public string UnusedDriveLetters {  get { return _UnusedDriveLetters; }         }
public string MappedDriveLetters {  get { return _MappedDriveLetters;  }         }


[DllImport("kernel32.dll")]
public static extern uint QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

[DllImport("kernel32.dll")]
public static extern long GetDriveType(string driveLetter);

private string FmyProperty;

private Hashtable Unused = new Hashtable();
private Hashtable Used = new Hashtable();

private Encoding ascii  =  Encoding.ASCII;
private String[] x   = new String[24];

public string MyProperty {

	get { return FmyProperty; }
	set { FmyProperty = value; }
}


public static void Main()
{

	DEBUG =  ( System.Environment.GetEnvironmentVariable("DEBUG")  == null) ? false : true;
	DosDriveInventory x = new DosDriveInventory();
	x.Execute();


	string SampleCommand   = @"""C:\Program Files\Wise for Windows Installer\wfwi.exe"" /c N:\foobar""X:\src\layouts\msi\MergeModules\mf\mf_lang\x64\retail\es\MF_LANG.wsm"" /o ""x64\retail\es\MF_LANG.msm"" /s /v /l ""x64\retail\es\MF_LANG_msm.log""";


	Console.WriteLine(x.ReportMappedDosDrives(SampleCommand   ) );

}
public bool Execute()
{


	byte cnt;

	// Internal Drive letter  hash table .
	for (cnt = 0; cnt != x.Length; cnt++ ) {
		String z = String.Format("{0}:\\", ascii.GetString( new byte[] { (byte)( cnt + 67 ) }));
		x[cnt] = z;
		Unused.Add(z, 1);
		Used.Add(z, 1);
	}


	string[] aDrives = Environment.GetLogicalDrives();


	for (cnt = 0; cnt != aDrives.Length; cnt++ ) {

		//


		String sDriveRoot =  aDrives[cnt];
		String aRealDriveRootPath =  GetRealPath( sDriveRoot  );
		int iDriveTypeResult = (int)GetDriveType(sDriveRoot);

		// http://www.entisoft.com/ESTools/WindowsAPI_DRIVEConstantToString.HTML
		/*
		    from WinBase.h:


		   #define DRIVE_UNKNOWN     0
		   #define DRIVE_NO_ROOT_DIR 1
		   #define DRIVE_REMOVABLE   2
		   #define DRIVE_FIXED       3
		   #define DRIVE_REMOTE      4
		   #define DRIVE_CDROM       5
		   #define DRIVE_RAMDISK     6
		 */
		// Only interested in DRIVE_FIXED   drives.

		// Another option is to utilize WMI  like:
		/*
		     ManagementClass manager = new ManagementClass("Win32_LogicalDisk");
		     ManagementObjectCollection drives = manager.GetInstances();
		     foreach( ManagementObject drive in drives )
		     {
		                Console.WriteLine ("{0}  is a {1}", drive["Caption"] ,  drive["Description"] );

		     }

		 */
		// http://www.artima.com/forums/flat.jsp?forum=76&thread=3997

		if (3 == iDriveTypeResult  ) {
			// Do not return   trivial information .
			if (0 != String.Compare(sDriveRoot, aRealDriveRootPath, true )  ) {
				if (DEBUG) {
					Console.WriteLine("GetDriveType({0}) =  {1}",   sDriveRoot, iDriveTypeResult);
					Console.WriteLine("GetRealPath({0}) = {1}", sDriveRoot,  aRealDriveRootPath );
				}
			}
		}

		if (  Unused.Contains( aDrives[cnt]))
			Unused[aDrives[cnt]] =  0;

		if (  Used.Contains( aDrives[cnt])) {
			Used[aDrives[cnt]] =  aRealDriveRootPath;
			_MappedDriveLettersArrayList.Add(aDrives[cnt]);
		}

	}

	for (cnt = 0; cnt !=  x.Length; cnt++ ) {

		if ( Unused[(x[cnt])].ToString() ==  "1" )
			_UnusedDriveLettersArrayList.Add(x[cnt]);

	}

	_MappedDriveLetters = String.Join(";", (string [] )_MappedDriveLettersArrayList.ToArray( typeof(string)));
	_UnusedDriveLetters = String.Join(";", (string [] )_UnusedDriveLettersArrayList.ToArray( typeof(string)));
	return true;
}

// Sample Code for second signature:
// http://www.pinvoke.net/default.aspx/kernel32.QueryDosDevice


private static string GetRealPath(string path)

{

	string realPath;

	StringBuilder pathInformation = new StringBuilder(250);

	// Get the drive letter of the
	string driveLetter = Path.GetPathRoot(path).Replace("\\", "");

	QueryDosDevice(driveLetter, pathInformation, 250);

	if (DEBUG)
		Console.WriteLine(pathInformation.ToString());

	// If drive is substed, the result will be in the format of "\??\C:\RealPath\".
	if (pathInformation.ToString().Contains("\\??\\")) {
		// Strip the \??\ prefix.
		string realRoot = pathInformation.ToString().Remove(0, 4);
		//Combine the paths.
		realPath = Path.Combine(realRoot, path.Replace(Path.GetPathRoot(path), ""));
	}     else{
		if (pathInformation.ToString().Contains("\\Device\\LanmanRedirector\\")) {
			string realRoot = pathInformation.ToString().Remove(0, 26);
			realPath = realRoot;
		}else
			realPath = path;
	}

	return realPath;

}

public int DosDriveCount()
{
	return _UnusedDriveLettersArrayList.Count;
}

public String DosDriveRealPath(string sDosDriveLetterAlias )
{
	return Used [ sDosDriveLetterAlias].ToString();
}




public String   ReportMappedDosDrives( String sCommandLine)
{


	ArrayList _Report = new ArrayList(24);
	String sDosDriveLetters = this.MappedDriveLetters;


	if (DEBUG )
		Console.WriteLine(
			String.Format("Mapped DOS Drive Letters={0}\n", sDosDriveLetters ));

	if (DEBUG ) {
		string[] items = sDosDriveLetters.Split(new char[] { ';' });
		for (int cnt = 0; cnt != items.Length; cnt++ ) {
			String sDrive = items[cnt];
			String sRealPath = this.DosDriveRealPath(sDrive );
			if (String.Compare(sRealPath, sDrive, true)  != 0 )
				_Report.Add(  String.Format("{0} is subst for {1}\n", sDrive, sRealPath ));
		}

	}
	if (DEBUG)
		Console.WriteLine("Number of Free DOS Drive Letters={0}\n", this.DosDriveCount());


	if (DEBUG)
		Console.WriteLine("Parsing Command \"{0}\"\n", sCommandLine);


	string sPatternString = @"(?<driveletter>[" +
				String.Join("", sDosDriveLetters.Split(";".ToCharArray())).Replace(":\\", "") +
				"]:\\\\)";
	if (DEBUG)
		Console.WriteLine("Parsing Pattern\"{0}\"\n", sPatternString );

	string res =  String.Empty;
	Regex r = new Regex( sPatternString,
			     RegexOptions.ExplicitCapture |
			     RegexOptions.IgnoreCase );
	MatchCollection m = r.Matches(  sCommandLine);
	if ( m != null ) {
		for ( int i = 0; i < m.Count; i++ ) {
			string sDrive = m[i].Groups["driveletter"].Value.ToString();
			string sRealPath = this.DosDriveRealPath(sDrive);
			// Only report 'subst' drives.
			if (String.Compare(sRealPath, sDrive, true)  != 0 ) {
				res = String.Format("{0} = \"{1}\"", sDrive,  sRealPath );
				_Report.Add(res);
				if (DEBUG)
					Console.WriteLine(res);
			}
		}

	}

	return String.Join("\n",
			   (string [] )_Report.ToArray( typeof(string)));

}




}




#endregion

