### cs_wpf_property_generator

##### A simple app that generates properties for use in a WPF application.

0.  Overview

    WPF model classes typically need to implement the INotifyPropertyChanged interface, and sometimes the IChangeTracking interface too.  The code is tedious to write, and abstractions don't help much.  A code generator can create the required code with a simple configuration file.

    Given one or more configuration files (described below), this utility outputs one `.cs` file per configuration file.  Each generated `.cs` file contains a serializable partial class, with property setters that contain code for the specified interfaces.  `IEnumerable`-derived properties are handled, as well as `IEnumerable`-derived properties that contain instances of `IChangeTracking`-derived objects.

1.  Building

    This application is a vanilla C# project.  There are no dependencies required to build it.

2.  Configuration File

    Instead of XML or JSON, the configuration file is a rudimentary, bare-bones text file.  The configuration file's extension doesn't matter.

    The configuration file has six sections: namespace, classname, interfaces, properties, outputfilename, and usings.  The sections are separated by one or more blank lines, and all section names are case-insensitive.  For example:

    ```
    # The order of the sections doesn't matter.

    namespace # Required.
    MyApp.Library

    classname # Required.
    Project

    interfaces # Optional, case-insensitive.
    IChangeTracking
    INotifyPropertyChanged

    properties # Required.
    String Name
    Int32 Version
    MyCustomType CustomType
    x List<String> Names # The 'x' means the property implements IEnumberable<T>.

    outputfilename # Required, case-insensitive.
    c:\temp\properties.cs

    usings
    System.Xml
    ```

    Most of the sections should be self-explanatory.  The `interfaces` section can contain either `IChangeTracking` or `INotifyPropertyChanged`, or both (case-insensitive).  This controls the code generated for each property setter.  You can leave the `interfaces` section empty, or remove it altogether, to just generate normal properties.

    The `properties` section contains one or more lines, each describing a property's type and name.  Optionally, an "x" (case-insensitive) may be placed at the beginning of a property line to indicate the property implements IEnumerable.  That affects a property setter's code generation for the `IChangeTracking` interface.

    The generated code's `usings` statement includes `System`, `System.ComponentModel`, `System.Collections.Generic`, and `System.Linq`.  If your code needs other namespaces, list those namespaces in the `usings` section of the configuration file (see example above).  The utility filters out duplicate namespaces, so they won't cause an error.

    The `#` sign starts a comment.  The `#` sign, and any text after it up to the end of the line, is ignored.

3.  Usage

    Just call the app with the configuration file(s) pathnames as command line parameters:

    ```
    cs_wpf_property_generator "c:\temp\myapp.cfg" "c:\projects\otherappconfig.txt"
    ```

4.  Output

    The above configuration file generates this C# code, and saves it to the file specified in the config file's `outputfilename` section.  `outputfilename` is overwritten if it already exists:

    ```csharp
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

    namespace MyApp.Library
    {
      [Serializable]
      public partial class Project : IChangeTracking, INotifyPropertyChanged
      {
        private String _name;
        public String Name
        {
          get
          {
            return this._name;
          }
          set
          {
            this._name = value;
        
            if (!this._originalValues.ContainsKey(nameof(this.Name)))
              this._originalValues[nameof(this.Name)] = this._name;
        
            this.IsChanged = (value != (String) this._originalValues[nameof(this.Name)]);
        
            OnPropertyChanged(nameof(this.Name));
          }
        }
    
        private Int32 _version;
        public Int32 Version
        {
          get
          {
            return this._version;
          }
          set
          {
            this._version = value;
        
            if (!this._originalValues.ContainsKey(nameof(this.Version)))
              this._originalValues[nameof(this.Version)] = this._version;
        
            this.IsChanged = (value != (Int32) this._originalValues[nameof(this.Version)]);
        
            OnPropertyChanged(nameof(this.Version));
          }
        }
    
        private MyCustomType _customType;
        public MyCustomType CustomType
        {
          get
          {
            return this._customType;
          }
          set
          {
            this._customType = value;
        
            if (!this._originalValues.ContainsKey(nameof(this.CustomType)))
              this._originalValues[nameof(this.CustomType)] = this._customType;
        
            this.IsChanged = (value != (MyCustomType) this._originalValues[nameof(this.CustomType)]);
        
            OnPropertyChanged(nameof(this.CustomType));
          }
        }
    
        public List<String> Names { get; } = new List<String>();

        private Dictionary<String, Object> _originalValues = new Dictionary<String, Object>();
    
        #region IChangeTracking
        private Boolean _isChanged = false;
        public Boolean IsChanged
        {
          get
          {
            return
              this._isChanged ||
              this.Names.Where(p => p is IChangeTracking).Any(p => (Boolean) p.GetType().GetProperty("IsChanged").GetValue(p));
          }
          set
          {
            this._isChanged = value;
          }
        }
    
        public void AcceptChanges() => this.IsChanged = false;
        #endregion
    
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
    
        protected void OnPropertyChanged(string name)
        {
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
      }
    }
    ```
