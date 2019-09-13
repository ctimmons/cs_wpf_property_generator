using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace cs_wpf_property_generator
{
  public class Program
  {
    public static void Main(String[] args)
    {
      if (!args.Any())
      {
        Console.WriteLine("Please see README.md for usage.");
        return;
      }

      foreach (var arg in args.Select(Path.GetFullPath))
      {
        if (!File.Exists(arg))
          throw new Exception($"The configuration file '{arg}' does not exist.");

        var project = Project.GetProject(arg);

        var output = $@"{GetUsings(project)}

namespace {project.Namespace}
{{
  [Serializable]
  public partial class {project.Classname}{GetInterfaceIdentifiers(project)}
  {{
{GetProperties(project).Indent(4)}

{GetInterfaceImplementations(project).Indent(4)}
  }}
}}";

        project.Save(output);
      }
    }

    private static String GetUsings(Project project)
    {
      return
        project
        .Usings
        .OrderBy(u => u)
        .Distinct()
        .Select(u => $"using {u};")
        .Join("\n");
    }

    private static String GetInterfaceIdentifiers(Project project)
    {
      var interfaceIdentifiers = new List<String>();

      if (project.ShouldImplementIChangeTracking)
        interfaceIdentifiers.Add("IChangeTracking");

      if (project.ShouldImplementINotifyPropertyChanged)
        interfaceIdentifiers.Add("INotifyPropertyChanged");

      var result = interfaceIdentifiers.Join(", ");

      return result.Any() ? " : " + result : "";
    }

    private static String GetIEnumerableChangedExpressions(IEnumerable<Property> properties)
    {
      var result = new List<String>() { "this._isChanged" };

      result.AddRange(
        properties
        .Where(p => p.IsIEnumerable)
        .Select(p => $"this.{p.Name}.Where(p => p is IChangeTracking).Any(p => (Boolean) p.GetType().GetProperty(\"IsChanged\").GetValue(p))"));

      return result.Join(" ||\n");
    }

    private static String GetInterfaceImplementations(Project project)
    {
      var result = new List<String>();

      if (project.ShouldImplementIChangeTracking)
        result.Add($@"private readonly Dictionary<String, Object> _originalValues = new Dictionary<String, Object>();

#region IChangeTracking
private Boolean _isChanged = false;
public Boolean IsChanged
{{
  get
  {{
    return
{GetIEnumerableChangedExpressions(project.Properties).Indent(6)};
  }}
  set
  {{
    this._isChanged = value;
  }}
}}

public void AcceptChanges() => this.IsChanged = false;
#endregion");

      if (project.ShouldImplementINotifyPropertyChanged)
        result.Add($@"#region INotifyPropertyChanged
public event PropertyChangedEventHandler PropertyChanged;

protected void OnPropertyChanged(string name)
{{
  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}}
#endregion");

      return result.Join("\n\n");
    }

    private static String GetPropertySetter(Project project, Property property)
    {
      var result = new List<String>() { $"this.{property.BackingStoreName} = value;" };

      if (project.ShouldImplementIChangeTracking)
        result.Add($@"if (!this._originalValues.ContainsKey(nameof(this.{property.Name})))
  this._originalValues[nameof(this.{property.Name})] = this.{property.BackingStoreName};

this.IsChanged = (value != ({property.Type}) this._originalValues[nameof(this.{property.Name})]);");

      if (project.ShouldImplementINotifyPropertyChanged)
        result.Add($"OnPropertyChanged(nameof(this.{property.Name}));");

      return result.Join("\n\n");
    }

    private static String GetProperty(Project project, Property property)
    {
      if (property.IsIEnumerable)
        return $"public {property.Type} {property.Name} {{ get; }} = new {property.Type}();";
      else
        return
$@"private {property.Type} {property.BackingStoreName};
public {property.Type} {property.Name}
{{
  get
  {{
    return this.{property.BackingStoreName};
  }}
  set
  {{
{GetPropertySetter(project, property).Indent(4)}
  }}
}}";
    }

    private static String GetProperties(Project project)
    {
      return project.Properties.Select(property => GetProperty(project, property)).Join("\n\n");
    }
  }

  public class Project
  {
    public String Namespace { get; private set; }
    public String Classname { get; private set; }
    public Boolean ShouldImplementIChangeTracking { get; private set; } = false;
    public Boolean ShouldImplementINotifyPropertyChanged { get; private set; } = false;
    public List<Property> Properties { get; } = new List<Property>();
    public String OutputFilename { get; private set; }
    public List<String> Usings { get; } = new List<String>() { "System", "System.ComponentModel", "System.Collections.Generic", "System.Linq" };

    private Project()
      : base()
    {
    }

    public void Save(String output)
    {
      Directory.CreateDirectory(Path.GetDirectoryName(this.OutputFilename));
      File.WriteAllText(this.OutputFilename, output);
    }

    /* Remove comments and trim whitespace from line. */
    private static String GetSanitizedLine(String line)
    {
      var indexOfHash = line.IndexOf('#');

      return
        ((indexOfHash < 0)
        ? line
        : line.Substring(0, indexOfHash)).Trim();
    }

    private enum State { Start, Namespace, Classname, Interfaces, Properties, OutputFilename, Usings }

    public static Project GetProject(String filename)
    {
      const Boolean caseInsensitiveComparison = true;
      var result = new Project();

      using (var inputfile = File.OpenText(filename))
      {
        String line = null;
        var state = State.Start;

        while ((line = inputfile.ReadLine()) != null)
        {
          line = GetSanitizedLine(line);

          if (String.IsNullOrWhiteSpace(line))
            continue;

          if (Enum.TryParse(line, caseInsensitiveComparison, out State newState))
          {
            state = newState;
            continue;
          }

          switch (state)
          {
            case State.Namespace:
              result.Namespace = line;
              break;
            case State.Classname:
              result.Classname = line;
              break;
            case State.Interfaces:
              if (line.ToLower() == "ichangetracking")
                result.ShouldImplementIChangeTracking = true;
              else if (line.ToLower() == "inotifypropertychanged")
                result.ShouldImplementINotifyPropertyChanged = true;
              else
                throw new Exception($"'{line}' is an unknown interface identifier in 'interfaces' section.  The only valid values are 'IChangeTracking' and 'INotifyPropertyChanged' (case insensitive).");

              break;
            case State.Properties:
              result.Properties.Add(new Property(line));
              break;
            case State.OutputFilename:
              result.OutputFilename = Path.GetFullPath(line.Replace("\"", ""));
              break;
            case State.Usings:
              result.Usings.Add(line);
              break;
          }
        }
      }

      return result;
    }
  }

  public class Property
  {
    public Boolean IsIEnumerable { get; private set; }
    public String Type { get; private set; }
    public String Name { get; private set; }
    public String BackingStoreName { get; private set; }

    public Property(String typeAndName)
      : base()
    {
      var parts = typeAndName.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length == 3)
      {
        var isEnumerableFlag = parts[0].ToLower();

        if (isEnumerableFlag != "x")
          throw new Exception($"This property specification has three parts, but the first part isn't an 'x': {typeAndName}");

        this.IsIEnumerable = true;
        this.Type = parts[1].Trim();
        this.Name = parts[2].Trim();
      }
      else if (parts.Length == 2)
      {
        this.IsIEnumerable = false;
        this.Type = parts[0].Trim();
        this.Name = parts[1].Trim();
      }
      else
      {
        throw new Exception($"Don't know how to handle this property specification: {typeAndName}");
      }

      var firstLetterOfName = Char.ToLower(this.Name[0]);
      var restOfName = this.Name.Substring(1);
      this.BackingStoreName = $"_{firstLetterOfName}{restOfName}";
    }
  }
}
