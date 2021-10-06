### cs_wpf_property_generator

##### A simple app that generates properties for use in a WPF application.

0.  Overview

    WPF model classes typically need to implement the `INotifyPropertyChanged` interface, and sometimes the `IChangeTracking` interface too.  The code is tedious to write, and abstractions don't help much.  A code generator can create the required code with a simple configuration file.

    Given one or more configuration files (described below), this utility outputs one `.cs` file per configuration file.  Each generated `.cs` file contains a serializable partial class, with property setters that contain code for the specified interfaces.  `IEnumerable<T>`-derived properties are handled, as well as `IEnumerable<T>`-derived properties that contain instances of `IChangeTracking`-derived objects.

1.  Building

    This application is a vanilla C# project.  There are no dependencies required to build it.

2.  Configuration File

    Instead of XML or JSON, the configuration file is a rudimentary, bare-bones text file.  The configuration file's extension doesn't matter.

    The configuration file has six sections: `[namespace]`, `[classname]`, `[interfaces]`, `[properties]`, `[outputfilename]`, and `[usings]`.  The section name is wrapped in square brackets, and all section names are case-insensitive.  For example:

    ```
    # The order of the sections doesn't matter.

    [namespace] # Required.
    MyApp.Library

    [classname] # Required.
    Project

    [interfaces] # Optional, case-insensitive.
    IChangeTracking
    INotifyPropertyChanged

    [properties] # Required.
    String Name
    Int32 Version
    MyCustomType CustomType
    x List<String> Names # The 'x' means the property implements IEnumberable<T>.

    [outputfilename] # Required, case-insensitive.
    %temp%\properties.cs

    [usings]
    System.Xml
    ```

    Most of the sections should be self-explanatory.  The `[interfaces]` section can contain either `IChangeTracking` or `INotifyPropertyChanged`, or both (case-insensitive).  This controls the code generated for each property setter.  You can leave the `[interfaces]` section empty, or remove it altogether, to just generate normal properties.

    The `[properties]` section contains one or more lines, each describing a property's type and name.  Optionally, an "x" (case-insensitive) may be placed at the beginning of a property line to indicate the property implements IEnumerable.

    Any environment variables in the `[outputfilename]` entry will be expanded.  See `properties.cfg` in the Examples folder.

    The generated code's `usings` statement includes `System`, `System.ComponentModel`, `System.Collections.Generic`, and `System.Linq`.  If your code needs other namespaces, list those namespaces in the `[usings]` section of the configuration file (see example above).  The utility filters out duplicate namespaces, so they won't cause an error.

    The `#` sign starts a comment.  The `#` sign, and any text after it up to the end of the line, is ignored.

3.  Usage

    Call the app with the configuration file(s) pathnames as command line parameters:

    ```
     cs_wpf_property_generator "c:\temp\myapp.cfg" "c:\projects\otherappconfig.txt"
     ```

    To process the example configuration, open this project's properties and go to the `Debug` page.  Put `..\..\..\Examples\properties.cfg` in the `Command line arguments` textbox and run the program.  The generated code will be written to the location specified by the `[outputfilename]` section, which in this case is the `%temp%` folder.  It's usually something like `C:\Users\<username>\AppData\Local\Temp\properties.cs`.

    The generated code is an ordinary C# class.  You can create an instance by calling the class's parameterless constructor.  You can also create partial classes to add functionality to the generated code.

    Note that if the class implements `IChangeTracking`, it's a good idea to call the `AcceptChanges()` method after you save the instance's data (e.g. persisting it to a database, writing it to an XML file, etc.).  `AcceptChanges()` resets an internal data structure used by the `IsChanged` property to detect if the instance has any 'dirty' data.  Immediately after calling `AcceptChanges()`, `IsChanged` will always return false.

4.  Messages: Info, Warnings, and Errors

    As code is generated, various messages may be generated.  With two exceptions, all of the messages will be placed in a header comment at the top of the generated code file.  The messages should guide you as to what's wrong if the generated code isn't what you expected.

    The first exception is if a non-existent configuration file path is given to this app.  The second exception is if the `[outputfilename]` section is either missing, has no value, or the output file cannot be created.  In both of these cases the app will raise an exception and exit.

5.  Output

    The above configuration file generates this C# code, and saves it to the file specified in the config file's `[outputfilename]` section.  `[outputfilename]` is overwritten if it already exists:

    ```csharp
    /*

    This code was generated by cs_wpf_property_generator.exe from
    configuration file 'C:\Temp\properties.txt'.
    on 11/19/2019 4:17:21 PM.
 
    Changes to this file may cause incorrect behavior and
    will be lost if the code is regenerated.  Modify the
    above listed configuration file instead and regenerate the code.

    INFO:

    A parameterless constructor has been generated.
    This constructor calls the object's IChangeTracking.AcceptChanges() method,
    which initializes an internal data structure needed to implement correct change-tracking logic.
    Note that another parameterless constructor cannot be created in a partial class of this class.

    */

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Xml;

    namespace MyApp.Library
    {
      [Serializable]
      public partial class Project : IChangeTracking, INotifyPropertyChanged
      {

        public Project() : base() => this.AcceptChanges();

        private String _name = "";
        public String Name
        {
          get
          {
            return this._name;
          }
          set
          {
            if (this._name == value)
              return;
        
            this._name = value;
        
            if (this._originalValues.ContainsKey(nameof(this.Name)))
              this.IsChanged = (value != (String) this._originalValues[nameof(this.Name)]);
            else
              this.IsChanged = true;
        
            OnPropertyChanged(nameof(this.Name));
          }
        }
    
        private Int32 _version = default;
        public Int32 Version
        {
          get
          {
            return this._version;
          }
          set
          {
            if (this._version == value)
              return;
        
            this._version = value;
        
            if (this._originalValues.ContainsKey(nameof(this.Version)))
              this.IsChanged = (value != (Int32) this._originalValues[nameof(this.Version)]);
            else
              this.IsChanged = true;
        
            OnPropertyChanged(nameof(this.Version));
          }
        }
    
        private MyCustomType _customType = default;
        public MyCustomType CustomType
        {
          get
          {
            return this._customType;
          }
          set
          {
            if (this._customType == value)
              return;
        
            this._customType = value;
        
            if (this._originalValues.ContainsKey(nameof(this.CustomType)))
              this.IsChanged = (value != (MyCustomType) this._originalValues[nameof(this.CustomType)]);
            else
              this.IsChanged = true;
        
            OnPropertyChanged(nameof(this.CustomType));
          }
        }
    
        public List<String> Names1 { get; } = new List<String>();
    
        public List<String> Names2 { get; } = new List<String>();
    
        public List<String> Names3 { get; } = new List<String>();

        private readonly Dictionary<String, Object> _originalValues = new Dictionary<String, Object>();
    
        #region IChangeTracking
        private Boolean _isChanged = false;
        public Boolean IsChanged
        {
          get
          {
            return
              this._isChanged ||
              this.Names1.Where(p => p is IChangeTracking).Any(p => (p as IChangeTracking).IsChanged) ||
              this.Names2.Where(p => p is IChangeTracking).Any(p => (p as IChangeTracking).IsChanged) ||
              this.Names3.Where(p => p is IChangeTracking).Any(p => (p as IChangeTracking).IsChanged);
          }
          set
          {
            this._isChanged = value;
          }
        }
    
        public void AcceptChanges()
        {
          this.IsChanged = false;
    
          /* Reset original values. */
          this._originalValues[nameof(this.CustomType)] = this._customType;
          this._originalValues[nameof(this.Name)] = this._name;
          this._originalValues[nameof(this.Version)] = this._version;
      
          foreach (var quux in this.Names1.Where(q => q is IChangeTracking))
            (quux as IChangeTracking).AcceptChanges();
      
          foreach (var quux in this.Names2.Where(q => q is IChangeTracking))
            (quux as IChangeTracking).AcceptChanges();
      
          foreach (var quux in this.Names3.Where(q => q is IChangeTracking))
            (quux as IChangeTracking).AcceptChanges();
        }
        #endregion
    
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
    
        protected void OnPropertyChanged(String name)
        {
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

      }
    }
    ```
