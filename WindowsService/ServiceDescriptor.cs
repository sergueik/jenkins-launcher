using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Xml;

namespace WindowsService {
    public class ServiceDescriptor {
        protected readonly XmlDocument dom = new XmlDocument();

        public readonly string BasePath;
        public readonly string BaseName;

        public virtual string ExecutablePath {
            get {
                // this returns the executable name as given by the calling process, so
                // it needs to be absolutized.
                string p = Environment.GetCommandLineArgs()[0];
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, p);

            }
        }

        public ServiceDescriptor() {
            // find co-located configuration xml. We search up to the ancestor directories to simplify debugging,
            // as well as trimming off ".vshost" suffix (which is used during debugging)
            string p = ExecutablePath;
            string baseName = Path.GetFileNameWithoutExtension(p);
            baseName  = @"C:\developer\sergueik\jenkins-launcher\WindowsService\winsw";
            if (baseName.EndsWith(".vshost")) baseName = baseName.Substring(0, baseName.Length - 7);
            while (true) {
                p = Path.GetDirectoryName(p);
                if (File.Exists(Path.Combine(p, baseName + ".xml")))
                    break;
            }

            BaseName = baseName;
            BasePath = Path.Combine(p, BaseName);

            dom.Load(BasePath + ".xml");

            // register the base directory as environment variable so that future expansions can refer to this.
            Environment.SetEnvironmentVariable("BASE", p);
            // ditto for ID
            Environment.SetEnvironmentVariable("SERVICE_ID", Id);
            Environment.SetEnvironmentVariable("WINSW_EXECUTABLE", ExecutablePath);
        }

        public ServiceDescriptor(XmlDocument dom) {
            this.dom = dom;
        }

        public static ServiceDescriptor FromXML(string xml) {
            var dom = new XmlDocument();
            dom.LoadXml(xml);
            return new ServiceDescriptor(dom);
        }

        private string SingleElement(string tagName) {
            return SingleElement(tagName, false);
        }

        private string SingleElement(string tagName, Boolean optional) {
            var n = dom.SelectSingleNode("//" + tagName);
            if (n == null && !optional) throw new InvalidDataException("<" + tagName + "> is missing in configuration XML");
            return n == null ? null : Environment.ExpandEnvironmentVariables(n.InnerText);
        }

        private int SingleIntElement(XmlNode parent, string tagName, int defaultValue) {
            var e = parent.SelectSingleNode(tagName);
            return (e == null) ? defaultValue : int.Parse(e.InnerText);
        }

        private TimeSpan SingleTimeSpanElement(XmlNode parent, string tagName, TimeSpan defaultValue) {
            var e = parent.SelectSingleNode(tagName);
            return (e == null) ? defaultValue : ParseTimeSpan(e.InnerText);
        }

        private TimeSpan ParseTimeSpan(string v) {
            v = v.Trim();
            foreach (var s in SUFFIX) {
                if (v.EndsWith(s.Key)) {
                    return TimeSpan.FromMilliseconds(int.Parse(v.Substring(0, v.Length - s.Key.Length).Trim()) * s.Value);
                }
            }
            return TimeSpan.FromMilliseconds(int.Parse(v));
        }

        private static readonly Dictionary<string,long> SUFFIX = new Dictionary<string,long> {
            { "ms",     1 }, 
            { "sec",    1000L },
            { "secs",   1000L },
            { "min",    1000L*60L },
            { "mins",   1000L*60L },
            { "hr",     1000L*60L*60L },
            { "hrs",    1000L*60L*60L },
            { "hour",   1000L*60L*60L },
            { "hours",  1000L*60L*60L },
            { "day",    1000L*60L*60L*24L },
            { "days",   1000L*60L*60L*24L }
        };

        public string Executable {
            get {
                return SingleElement("executable");
            }
        }

        public string StopExecutable {
            get {
                return SingleElement("stopexecutable");
            }
        }

        public string Arguments {
            get {
                string arguments = AppendTags("argument");

                if (arguments == null) {
                    var argumentsNode = dom.SelectSingleNode("//arguments");

                    if (argumentsNode == null) {
                        return "";
                    }

                    return Environment.ExpandEnvironmentVariables(argumentsNode.InnerText);
                } else {
                    return arguments;
                }
            }
        }

        public string Startarguments {
            get {
                return AppendTags("startargument");
            }
        }

        public string Stoparguments {
            get {
                return AppendTags("stopargument");
            }
        }

        public string WorkingDirectory {
            get {
                var wd = SingleElement("workingdirectory", true);
                return String.IsNullOrEmpty(wd) ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) : wd;
            }
        }

        private string AppendTags(string tagName) {
            XmlNode argumentNode = dom.SelectSingleNode("//" + tagName);

            if (argumentNode == null) {
                return null;
            } else {
                string arguments = "";

                foreach (XmlElement argument in dom.SelectNodes("//" + tagName))                 {
                    string token = Environment.ExpandEnvironmentVariables(argument.InnerText);

                    if (token.StartsWith("\"") && token.EndsWith("\"")) {
                        // for backward compatibility, if the argument is already quoted, leave it as is.
                        // in earlier versions we didn't handle quotation, so the user might have worked
                        // around it by themselves
                    } else {
                        if (token.Contains(" ")) {
                            token = '"' + token + '"';
                        }
                    }
                    arguments += " " + token;
                }

                return arguments;
            }
        }

        public string LogDirectory {
            get {
                XmlNode loggingNode = dom.SelectSingleNode("//logpath");

                if (loggingNode != null) {
                    return Environment.ExpandEnvironmentVariables(loggingNode.InnerText);
                } else {
                    return Path.GetDirectoryName(ExecutablePath);
                }
            }
        }

        public string[] ServiceDependencies {
            get {
                System.Collections.ArrayList serviceDependencies = new System.Collections.ArrayList();

                foreach (XmlNode depend in dom.SelectNodes("//depend")) {
                    serviceDependencies.Add(depend.InnerText);
                }
                return (string[])serviceDependencies.ToArray(typeof(string));
            }
        }

        public string Id {
            get {
                return SingleElement("id");
            }
        }

        public string Caption {
            get {
                return SingleElement("name");
            }
        }

        public string Description {
            get {
                return SingleElement("description");
            }
        }

        public bool BeepOnShutdown {
            get {
                return dom.SelectSingleNode("//beeponshutdown") != null;
            }
        }


        public TimeSpan WaitHint {
            get {
                return SingleTimeSpanElement(dom, "waithint", TimeSpan.FromSeconds(15));
            }
        }
        
        public TimeSpan SleepTime {
            get {
                return SingleTimeSpanElement(dom, "sleeptime", TimeSpan.FromSeconds(1));
            }
        }

        public bool Interactive {
            get {
                return dom.SelectSingleNode("//interactive") != null;
            }
        }

        public Dictionary<string, string> EnvironmentVariables {
            get {
                Dictionary<string, string> map = new Dictionary<string, string>();
                foreach (XmlNode n in dom.SelectNodes("//env")) {
                    string key = n.Attributes["name"].Value;
                    string value = Environment.ExpandEnvironmentVariables(n.Attributes["value"].Value);
                    map[key] = value;

                    Environment.SetEnvironmentVariable(key, value);
                }
                return map;
            }
        }

    }
}
